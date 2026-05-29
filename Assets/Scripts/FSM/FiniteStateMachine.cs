using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class FiniteStateMachine : MonoBehaviour, IState
{
    [SerializeField] protected CharacterManager characterManager;
    [Space(10f)]
    [Header("Initial State - only one of these are applied, top to down.")]
    [SerializeField] protected MonoScript _initialStateScript;
    [SerializeField] protected FiniteStateMachine _machine;
    [Space(10f)]
    [Header("Current State Check Window")]
    [SerializeField] string _currentState;

    protected Dictionary<Type, IState> _states;
    protected FiniteStateMachine fsm;
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
            _machine.Init(characterManager, this);
            return;
        }
    }

    #endregion

    #region Sub-State Instancing

    protected T CreateInstance<T>() where T : IState, new()
    {
        if (_states.ContainsKey(typeof(T))) Debug.LogError($"Multiple instances of a State [{typeof(T).Name}] has been initialized in a single State Machine under [{gameObject.name}]");

        T nextState = new T();
        nextState.Init(characterManager, this);
        return nextState;
    }

    protected IState CreateInstance(Type type)
    {
        if (!typeof(IState).IsAssignableFrom(type)) return null;
        if (_states.ContainsKey(type)) return null;

        IState state = (IState)Activator.CreateInstance(type, characterManager);
        state.Init(characterManager, this);
        return state;
    }

    protected void AddState<T>(T state) where T : IState
    {
        Type type = typeof(T);
        _states.TryAdd(type, (T)state);
    }

    #endregion

    #region Sub-State Transition - remind you, the FSM is a state itself - thus, sub-states.

    protected void Transit(IState nextState)
    {
        if (_state != null) _state.Exit();
        _state = nextState;
        _state.Enter();

        _currentState = nextState.GetType().Name;
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
        if (_states.ContainsKey(type))
        {
            IState nextState = _states[type];
            Transit(nextState);
            return true;
        }
        else return false;
    }

    #endregion

    #region State Logics: FSM as State (for inheritance)

    public void Init(CharacterManager characterManager, FiniteStateMachine machine)
    {
        this.characterManager = characterManager;
        this.fsm = machine;
        InitializeState();
        Bootstrap();
    }

    public virtual void Bootstrap() { }

    public virtual void Enter() { }

    public void UpdateState()
    {
        _state.UpdateState();
        Transitions();
    }

    public virtual void Exit() { }

    public virtual void Transitions() { }

    #endregion
}