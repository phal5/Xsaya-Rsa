using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public abstract class BaseCharacterState : IState
{
    protected CharacterManager characterManager;
    protected FiniteStateMachine fsm;

    public void Init(EntityManager characterManager, FiniteStateMachine machine)
    {
        if (characterManager.GetType() != typeof(CharacterManager)) return;
        this.characterManager = (CharacterManager)characterManager;
        this.fsm = machine;
        Bootstrap();
    }

    public virtual void Bootstrap() { }

    public virtual void Enter() { Debug.Log($"Entered {this.GetType().Name}"); }

    public virtual void UpdateState() { }

    public virtual void FixedUpdateState() { }

    public virtual void Exit() { }

    public virtual void Transitions() { }
}
/*
    public override void Bootstrap()
    {

    }

    public override void Enter()
    {

    }

    public override void UpdateState()
    {

    }

    public override void FixedUpdateState()
    {

    }

    public override void Exit()
    {

    }

    public override void Transitions()
    {

    }
 */