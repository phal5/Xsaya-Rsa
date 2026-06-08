using UnityEngine;

public class BaseEntityState<managerType> : IState where managerType : EntityManager
{
    protected managerType manager;
    protected FiniteStateMachine fsm;

    public void Init(EntityManager entityManager, FiniteStateMachine machine)
    {
        if (entityManager.GetType() != typeof(managerType)) return;
        this.manager = (managerType)entityManager;
        this.fsm = machine;
        Bootstrap();
    }

    public virtual void Bootstrap() { }

    public virtual void Enter() { }

    public virtual void UpdateState() { }

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

    public override void Exit()
    {

    }

    public override void Transitions()
    {

    }
 */
