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
    [SerializeField] protected bool _resetOnEnter = false;
    [Space(10f)]
    [Header("Current State Check Window")]
    [SerializeField] string _currentStateName;
    public Type _currentStateType { get { return _state?.GetType(); } }

    protected FiniteStateMachine fsm;
    protected Dictionary<Type, IState> _states;
    protected Type _initialStateType;
    protected IState _state;

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

    protected void Start()
    {
        InitializeState();
    }

    protected void Update()
    {
        UpdateState();
    }

    public Type StateType => _state?.GetType();

    #region Sub-State Initialization

    protected void InitializeState()
    {
        _states = new Dictionary<Type, IState>();

        // Check the string instead of the MonoScript!
        if (!string.IsNullOrEmpty(_initialStateTypeName))
        {
            _initialStateType = Type.GetType(_initialStateTypeName);

            if (_initialStateType != null)
            {
                IState state = CreateInstance(_initialStateType);
                if (state != null)
                {
                    _states.TryAdd(_initialStateType, state);
                    TransitTo(_initialStateType);
                    return;
                }
            }
        }

        if (_machine != null)
        {
            _state = _machine;
            _machine.Init(manager, this);
            return;
        }

        if (_state != null)
        {
            _initialStateType = _state.GetType();
        }
    }

    #endregion

    #region Sub-State Instancing

    protected T CreateInstance<T>() where T : IState, new()
    {
        if (_states.ContainsKey(typeof(T)))
            Debug.LogError($"Multiple instances of a State [{typeof(T).Name}] has been initialized in a single State Machine under [{gameObject.name}]");

        T nextState = new T();
        nextState.Init(manager, this);
        return nextState;
    }

    protected IState CreateInstance(Type type)
    {
        if (!typeof(IState).IsAssignableFrom(type)) return null;
        if (_states.ContainsKey(type)) return null;

        IState state = (IState)Activator.CreateInstance(type, manager);
        state.Init(manager, this);
        return state;
    }

    protected void AddState<T>(T state) where T : IState
    {
        Type type = typeof(T);
        _states.TryAdd(type, state);
    }

    #endregion

    #region Sub-State Transition

    protected void Transit(IState nextState)
    {
        _state?.Exit();
        _state = nextState;
        _state.Enter();

        _currentStateName = nextState.GetType().Name;
    }

    public bool TransitTo<T>() where T : IState, new()
    {
        Type type = typeof(T);
        if (_states.TryGetValue(type, out IState state))
        {
            Transit(state);
        }
        else
        {
            IState nextState = CreateInstance<T>();
            _states.TryAdd(type, nextState);
            Transit(nextState);
        }
        return true;
    }

    public bool TransitTo(Type type)
    {
        if (_states.TryGetValue(type, out IState state))
        {
            Transit(state);
            return true;
        }
        return false;
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
        if (_resetOnEnter && _initialStateType != null) TransitTo(_initialStateType);
    }

    public void UpdateState()
    {
        _state?.UpdateState();
        Transitions();
    }

    public virtual void Exit() { }

    public virtual void Transitions() { }

    #endregion

    private void OnDestroy()
    {
        _state?.Exit();
    }
}