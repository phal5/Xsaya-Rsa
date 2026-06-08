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

    public virtual void Enter() { }

    public virtual void UpdateState() { }

    public virtual void Exit() { }

    public virtual void Transitions() { }
}
