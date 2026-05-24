using System.Collections;
using UnityEngine;

public abstract class BaseState : IState
{
    protected CharacterManager characterManager;
    protected FiniteStateMachine fsm;

    public void Init(CharacterManager characterManager, FiniteStateMachine machine)
    {
        this.characterManager = characterManager;
        this.fsm = machine;
        Bootstrap();
    }

    public virtual void Bootstrap() { }

    public virtual void Enter() { }

    public virtual void UpdateState() { }

    public virtual void Exit() { }

    public virtual void Transitions() { }
}
