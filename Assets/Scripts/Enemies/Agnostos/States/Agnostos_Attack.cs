using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Agnostos_Attack : BaseEntityState<AgnostosManager>
{
    bool _ready = true;

    public override void Bootstrap()
    {

    }

    public override void Enter()
    {
        _ready = true;
    }

    public override void UpdateState()
    {
        UpdateLoop();
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

    private void UpdateLoop()
    {
        if (_ready)
        {
            _ready = false;
            Ready();
        }
    }

    private void Ready()
    {
        manager.footTargets[0].Transit(manager.AttackReadyTargets[0], manager.attackReadyTime, (x) => { return x; }, Attack);
    }

    private void Attack()
    {
        manager.weapon.StartAttack();
        manager.AttackTarget.position = PlayerManager.instance.player.position;
        manager.footTargets[0].Transit(manager.AttackTarget, manager.attackTime, (x) => { return x; }, ImpactFrame);
    }

    private void ImpactFrame()
    {
        manager.weapon.EndAttack();
        manager.footTargets[0].StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(manager.attackCooldown);
        _ready = true;
    }
}
