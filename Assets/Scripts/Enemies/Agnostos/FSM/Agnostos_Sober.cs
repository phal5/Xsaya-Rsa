using UnityEngine;

public class Agnostos_Sober : FiniteStateMachine
{
    [SerializeField] Agnostos agnostos;

    public override void Enter()
    {
        agnostos.AddDamageListener(ToStun);
        base.Enter();
    }

    public override void Exit()
    {
        agnostos.RemoveDamageListener(ToStun);
        base.Exit();
    }

    private void ToStun(float _)
    {
        if(this.isActiveAndEnabled) fsm.TransitTo<Agnostos_Stun>();
    }
}
