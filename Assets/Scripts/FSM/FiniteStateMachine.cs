using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; // Moved inside the preprocessor directive!
#endif

[System.Serializable]
public class FiniteStateMachine : MonoBehaviour, IState
{
    [SerializeField] protected EntityManager manager;
    [Space(10f)]
    [Header("Initial State - only one of these are applied, top to down.")]

#if UNITY_EDITOR
    [SerializeField] protected MonoScript _initialStateScript;
#endif

    // We use this hidden string to save the type so the runtime build can read it.
    [SerializeField, HideInInspector] protected string _initialStateTypeName;

    [SerializeField] protected FiniteStateMachine _machine;
    [Space(10f)]
    [Header("Component States - IState를 구현한 컴포넌트는 여기에 등록한다.")]
    [Tooltip("등록된 컴포넌트는 이 FSM이 직접 구동하므로 Awake에서 비활성화된다.")]
    [SerializeField] protected MonoBehaviour[] _componentStates;
    [Space(10f)]
    [Header("Current State Check Window")]
    [SerializeField] string _currentStateName;
    public Type _currentStateType { get { return _state?.GetType(); } }

    protected FiniteStateMachine fsm;
    protected Dictionary<Type, IState> _states;            // 이미 Init된 살아있는 상태들
    protected Dictionary<Type, MonoBehaviour> _statePool;  // 아직 쓰이지 않은 컴포넌트 상태들
    protected Type _initialStateType;
    protected IState _state;
    protected bool _stateExited;   // 부모가 이 머신을 떠나며 하위에 Exit을 내려보낸 상태인지

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_initialStateScript != null)
        {
            // GetClass() returns the actual script's Type, not the MonoScript type
            Type scriptType = _initialStateScript.GetClass();

            if (scriptType != null && typeof(IState).IsAssignableFrom(scriptType))
            {
                // Save the AssemblyQualifiedName as a string so it survives into the build
                _initialStateTypeName = scriptType.AssemblyQualifiedName;
            }
            else
            {
                Debug.LogWarning("Assigned script does not implement IState!");
                _initialStateScript = null;
                _initialStateTypeName = string.Empty;
            }
        }
        else
        {
            _initialStateTypeName = string.Empty;
        }
    }
#endif

    protected void Awake()
    {
        // 컴포넌트 상태는 이 FSM이 UpdateState/FixedUpdateState로 직접 구동한다.
        // Unity가 자체 Update를 함께 돌려 이중 구동되지 않도록 Start 이전에 꺼둔다.
        Silence(_machine);
        if (_componentStates == null) return;
        foreach (MonoBehaviour component in _componentStates) Silence(component);
    }

    private static void Silence(MonoBehaviour component)
    {
        if (component != null) component.enabled = false;
    }

    protected void Start()
    {
        // 루트 머신은 Init을 거치지 않으므로 여기서 Bootstrap을 불러준다.
        // 하위 머신은 Init에서 이미 불렸고 이 Start는 (비활성이라) 돌지 않는다.
        InitializeState();
        Bootstrap();
    }

    protected void Update()
    {
        UpdateState();
    }

    protected void FixedUpdate()
    {
        FixedUpdateState();
    }

    #region Sub-State Initialization

    protected void InitializeState()
    {
        _states = new Dictionary<Type, IState>();
        _statePool = new Dictionary<Type, MonoBehaviour>();

        PoolComponentStates();

        // 초기 상태가 없어도 그냥 둔다. Enter()에서 첫 상태를 직접 고르는 머신이 많고,
        // 잘못 비워둔 경우는 인스펙터의 Current State Check Window가 비어 있는 것으로 드러난다.
        _initialStateType = ResolveInitialStateType();
        if (_initialStateType != null) TransitTo(_initialStateType);
    }

    /// <summary>
    /// 컴포넌트 상태를 풀에 담아둔다. 실제 Init은 처음 사용될 때(Resolve) 이루어진다.
    /// </summary>
    private void PoolComponentStates()
    {
        Pool(_machine);     // 구버전 슬롯. Component States와 동일하게 취급한다.

        if (_componentStates == null) return;
        foreach (MonoBehaviour component in _componentStates) Pool(component);
    }

    private void Pool(MonoBehaviour component)
    {
        if (component == null) return;

        if (component is not IState)
        {
            Debug.LogError($"[{gameObject.name}] {component.GetType().Name}은(는) IState를 구현하지 않습니다. Component States에서 제거하세요.");
            return;
        }

        if (!_statePool.TryAdd(component.GetType(), component))
        {
            Debug.LogError($"[{gameObject.name}] {component.GetType().Name}이(가) 중복 등록되었습니다.");
        }
    }

    private Type ResolveInitialStateType()
    {
        // Check the string instead of the MonoScript!
        if (!string.IsNullOrEmpty(_initialStateTypeName))
        {
            Type named = Type.GetType(_initialStateTypeName);
            if (named != null) return named;

            Debug.LogError($"[{gameObject.name}] 초기 상태 타입을 찾을 수 없습니다: {_initialStateTypeName}");
        }

        // 초기 상태 스크립트가 없으면 구버전처럼 서브머신을 초기 상태로 삼는다.
        return _machine != null ? _machine.GetType() : null;
    }

    #endregion

    #region Sub-State Instancing

    /// <summary>
    /// 타입에 해당하는 상태를 돌려준다. 처음 요청된 상태는 여기서 한 번만 Init된다.
    /// 컴포넌트 상태는 풀에서 꺼내고, 그 외에는 새로 생성한다.
    /// </summary>
    protected IState Resolve(Type type)
    {
        if (type == null) return null;
        if (_states.TryGetValue(type, out IState live)) return live;

        if (!typeof(IState).IsAssignableFrom(type))
        {
            Debug.LogError($"[{gameObject.name}] {type.Name}은(는) IState를 구현하지 않습니다.");
            return null;
        }

        IState state;

        if (_statePool.TryGetValue(type, out MonoBehaviour component))
        {
            state = (IState)component;
        }
        else if (typeof(MonoBehaviour).IsAssignableFrom(type))
        {
            // new로 만들면 Unity가 껍데기 객체를 만들어버린다. 여기서 끊는다.
            Debug.LogError($"[{gameObject.name}] {type.Name}은(는) 컴포넌트 상태입니다. Component States 목록에 등록하세요.");
            return null;
        }
        else
        {
            state = (IState)Activator.CreateInstance(type);
        }

        _states[type] = state;   // 재진입 대비. Init보다 먼저 등록한다.
        state.Init(manager, this);
        return state;
    }

    #endregion

    #region Sub-State Transition

    protected void Transit(IState nextState)
    {
        if (!_stateExited) _state?.Exit();

        _state = nextState;
        _stateExited = false;
        _state.Enter();

        _currentStateName = nextState.GetType().Name;
    }

    // new() 제약을 두면 MonoBehaviour 상태가 컴파일은 통과하고 런타임에만 터진다.
    // 해소는 Resolve 한 곳으로 모은다.
    public bool TransitTo<T>() where T : IState => TransitTo(typeof(T));

    public bool TransitTo(Type type)
    {
        IState nextState = Resolve(type);
        if (nextState == null) return false;

        Transit(nextState);
        return true;
    }

    #endregion

    #region State Logics: FSM as State (for inheritance)

    public void Init(EntityManager entityManager, FiniteStateMachine machine)
    {
        this.manager = entityManager;
        this.fsm = machine;
        InitializeState();
        Bootstrap();
    }

    public virtual void Bootstrap() { }

    public virtual void Enter()
    {
        // 떠났던 자리에서 재개한다. 하위 상태도 Enter를 다시 받아야 구독 등이 복구된다.
        if (_stateExited)
        {
            _stateExited = false;
            _state?.Enter();
        }
    }

    public void UpdateState()
    {
        _state?.UpdateState();
        Transitions();
    }

    public void FixedUpdateState()
    {
        _state?.FixedUpdateState();
        Transitions();
    }

    /// <summary>
    /// 부모가 이 머신을 떠날 때, 안에서 돌던 하위 상태에도 Exit을 내려보낸다.
    /// 이게 없으면 스킬이 반응/피격에 끊겼을 때 히트박스나 속도가 남는다.
    /// </summary>
    public virtual void Exit()
    {
        if (_stateExited) return;

        _state?.Exit();
        _stateExited = true;
    }

    public virtual void Transitions() { }

    #endregion

    private void OnDestroy()
    {
        if (!_stateExited) _state?.Exit();
        OnDestroyed();
    }

    /// <summary>
    /// 파생 머신의 정리 훅.
    /// OnDestroy를 직접 선언하면 유니티가 가장 하위 것만 불러 base의 Exit 전파가 사라진다.
    /// 구독 해제 같은 정리는 여기서 한다.
    /// </summary>
    protected virtual void OnDestroyed() { }
}