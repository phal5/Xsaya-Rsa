using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class FiniteStateMachine : MonoBehaviour, IState
{
    [SerializeField] private Dictionary<Type, IState> _states;
    [SerializeField] private CharacterManager _characterManager;
    [Space(10f)]
    [Header("Initial State - only one of these are applied, top to down.")]
    [SerializeField] MonoScript _initialStateScript;
    [SerializeField] FiniteStateMachine _machine;

    private IState _state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _states = new();

        if (_initialStateScript != null)
        {
            InitializeFromMonoScript(_initialStateScript);
            return;
        }
        if (_machine != null)
        {
            _state = _machine;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
    }

    #region Initial State Definition

    private void InitializeFromMonoScript(MonoScript initialState)
    {
        Type t = initialState.GetClass();

        IState state = CreateInstance(t);
        _states.TryAdd(t, state);
        TransitTo(t);
    }

    #endregion

    #region State Instancing

    private T CreateInstance<T>() where T : IState, new()
    {
        if (_states.ContainsKey(typeof(T))) Debug.LogError($"Multiple instances of a State [{typeof(T).Name}] has been initialized in a single State Machine under [{gameObject.name}]");

        T nextState = new T();
        nextState.Init(_characterManager, this);
        return nextState;
    }

    private IState CreateInstance(Type type)
    {
        if (!typeof(IState).IsAssignableFrom(type)) return null;
        if (_states.ContainsKey(type)) return null;

        IState state = (IState)Activator.CreateInstance(type, _characterManager);
        state.Init(_characterManager, this);
        return state;
    }

    private void AddState<T>(T state) where T : IState
    {
        Type type = typeof(T);
        _states.TryAdd(type, (T)state);
    }

    #endregion

    #region State Transition

    public void Transition(IState nextState)
    {
        if (_state != null) _state.Exit();
        _state = nextState;
        _state.Enter();
    }

    public bool TransitTo<T>() where T : IState, new()
    {
        Type type = typeof(T);
        if (_states.TryGetValue(type, out IState state))
        {
            Transition(state);
        }
        else
        {
            IState nextState = CreateInstance<T>();
            _states.TryAdd(type, nextState);
            Transition(nextState);
        }
        return true;
    }

    public bool TransitTo(Type type)
    {
        if (_states.ContainsKey(type))
        {
            IState nextState = _states[type];
            Transition(nextState);
            return true;
        }
        else return false;
    }

    #endregion

    #region State Logics: FSM as State (for inheritance)

    public virtual void Enter() { }

    public void UpdateState()
    {
        _state.UpdateState();
        Transitions();
    }

    public void Exit() { }

    public virtual void Transitions() { }

    #endregion
}