using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class FiniteStateMachine : MonoBehaviour, IState
{
    [SerializeField] protected EntityManager manager;
    [Space(10f)]
    [Header("Initial State - only one of these are applied, top to down.")]
    [SerializeField] protected MonoScript _initialStateScript;
    [SerializeField] protected FiniteStateMachine _machine;
    [SerializeField] protected bool _resetOnEnter = false;
    [Space(10f)]
    [Header("Current State Check Window")]
    [SerializeField] string _currentStateName;
    public Type _currentStateType { get { return _state.GetType(); } }

    protected FiniteStateMachine fsm;
    protected Dictionary<Type, IState> _states;
    protected Type _initialStateType;
    protected IState _state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        InitializeState();
    }

    // Update is called once per frame
    protected void Update()
    {
        UpdateState();
    }

    public Type StateType => _state.GetType();

    #region Sub-State Initialization

    protected void InitializeFromMonoScript(MonoScript initialState, out Type t, out IState state)
    {
        t = initialState.GetClass();
        state = CreateInstance(t);
    }

    protected void InitializeState()
    {
        _states = new();

        if (_initialStateScript != null)
        {
            InitializeFromMonoScript(_initialStateScript, out Type t, out IState state);
            _states.TryAdd(t, state);
            TransitTo(t);
            return;
        }
        if (_machine != null)
        {
            _state = _machine;
            // Manual initialization(since we're not initializing the machine)
            _machine.Init(manager, this);
            return;
        }

        _initialStateType = _state.GetType();
    }

    #endregion

    #region Sub-State Instancing

    protected T CreateInstance<T>() where T : IState, new()
    {
        if (_states.ContainsKey(typeof(T))) Debug.LogError($"Multiple instances of a State [{typeof(T).Name}] has been initialized in a single State Machine under [{gameObject.name}]");

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
        _states.TryAdd(type, (T)state);
    }

    #endregion

    #region Sub-State Transition - remind you, the FSM is a state in itself - thus, sub-states.

    protected void Transit(IState nextState)
    {
        if (_state != null) _state.Exit();
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
        else return false;
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
        if (_resetOnEnter) TransitTo(_initialStateType);
    }

    /// <summary>
    /// Called every frame before transition check.
    /// </summary>
    public void UpdateState()
    {
        _state.UpdateState();
        Transitions();
    }

    public virtual void Exit() { }

    /// <summary>
    /// Called every frame after state update.
    /// </summary>
    public virtual void Transitions() { }

    #endregion
}

/*

    public override void Bootstrap()
    {

    }

    public override void Exit()
    {

    }

    public override void Transitions()
    {

    }

 */