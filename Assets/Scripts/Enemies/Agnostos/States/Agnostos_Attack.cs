using System;
using UnityEngine;
using UnityEngine.Events;

public class Agnostos_Attack : BaseEntityState<AgnostosManager>
{
    bool _ready;
    bool _hit;

    public override void Bootstrap()
    {

    }

    public override void Enter()
    {
        _ready = false;
        _hit = false;
        Ready();
    }

    public override void UpdateState()
    {
        
        Transitions();
    }

    public override void Exit()
    {
        
    }

    public override void Transitions()
    {
        ToApproach();
    }

    private void ToApproach()
    {
        Vector3 disparity = PlayerManager.instance.player.position - manager.character.position;
        float r = manager.attackRadius;
        if (disparity.sqrMagnitude > r * r)
        {
            fsm.TransitTo<Agnostos_Approach>();
        }
    }

    private void Ready()
    {
        manager.footTargets[0].Transit(manager.AttackReadyTargets[0], manager.attackReadyTime, (x) => { return x; }, Attack);
    }

    private void Attack()
    {
        manager.AttackTarget.position = PlayerManager.instance.player.position;
        manager.footTargets[0].Transit(manager.AttackTarget, manager.attackTime, (x) => { return x; }, Ready);
    }
}
