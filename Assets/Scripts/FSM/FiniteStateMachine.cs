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
    [Header("Initial State")]

#if UNITY_EDITOR
    [SerializeField] protected MonoScript _initialStateScript;
#endif

    // We use this hidden string to save the type so the runtime build can read it.
    [SerializeField, HideInInspector] protected string _initialStateTypeName;

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

        // 컴포넌트 상태를 풀에 담아둔다. 실제 Init은 처음 쓰일 때(Resolve) 이루어진다.
        if (_componentStates != null)
            foreach (MonoBehaviour component in _componentStates) Pool(component);

        // 초기 상태가 없어도 그냥 둔다. Enter()에서 첫 상태를 직접 고르는 머신이 많고,
        // 잘못 비워둔 경우는 인스펙터의 Current State Check Window가 비어 있는 것으로 드러난다.
        _initialStateType = ResolveInitialStateType();
        if (_initialStateType != null) TransitTo(_initialStateType);
    }

    private void Pool(MonoBehaviour component)
    {
        if (component == null) return;

        if (component is not IState)
        {
            Debug.LogError($"[{gameObject.name}] {component.GetType().Name}은(는) IState를 구현하지 않습니다. Component States에서 제거하세요.");
            return;
        }

        // 인스턴스로 가리는 머신은 타입 색인을 만들지 않는다. 같은 타입이 여럿인 게 정상이다.
        if (ComponentStatesAreInstances) return;

        if (!_statePool.TryAdd(component.GetType(), component))
        {
            Debug.LogError($"[{gameObject.name}] {component.GetType().Name}이(가) 중복 등록되었습니다.");
        }
    }

    /// <summary>
    /// 컴포넌트 상태를 타입이 아니라 <b>인스턴스</b>로 가리는 머신인지.
    ///
    /// 기본은 거짓 — 상태 하나에 타입 하나라, TransitTo&lt;T&gt;로 고를 수 있다.
    /// 참이면 같은 부품을 값만 달리해 여러 개 달 수 있는 대신, 타입으로는 고를 수 없고
    /// 머신이 <see cref="ResolveComponent"/>로 직접 고른 인스턴스에 넘겨야 한다.
    /// </summary>
    protected virtual bool ComponentStatesAreInstances => false;

    readonly HashSet<MonoBehaviour> _initializedComponents = new HashSet<MonoBehaviour>();

    /// <summary>
    /// 컴포넌트 상태 인스턴스를 쓸 수 있게 만들어 돌려준다. Init은 인스턴스마다 한 번만 돈다.
    /// 타입 색인을 거치지 않으므로 같은 타입이 여럿이어도 서로를 덮지 않는다.
    /// </summary>
    protected IState ResolveComponent(MonoBehaviour component)
    {
        if (component == null) return null;

        if (component is not IState state)
        {
            Debug.LogError($"[{gameObject.name}] {component.GetType().Name}은(는) IState를 구현하지 않습니다.");
            return null;
        }

        if (_initializedComponents.Add(component)) state.Init(manager, this);

        return state;
    }

    /// <summary>
    /// 초기 상태 타입을 문자열에서 되찾는다. MonoScript는 에디터 전용이라 빌드에 없다.
    ///
    /// 저장된 값이 <see cref="Type.AssemblyQualifiedName"/>이라 네임스페이스나 어셈블리가 바뀌면
    /// 그대로는 못 찾는다. 그 경우까지 <see cref="SerializedType"/>이 감당한다.
    /// </summary>
    private Type ResolveInitialStateType()
    {
        return SerializedType.Resolve(_initialStateTypeName, typeof(IState), this);
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

        // Enter()가 조건 검사에 실패해 곧바로 다른 상태로 넘길 수 있다.
        // 그때 안쪽 Transit이 남긴 이름을 덮지 않도록, 이름은 Enter 전에 적는다.
        _currentStateName = nextState.GetType().Name;

        _state.Enter();
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