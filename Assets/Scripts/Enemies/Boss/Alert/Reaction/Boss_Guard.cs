using UnityEngine;

/// <summary>
/// 제자리에서 막는다. 방어 중에는 피해가 경감되고 경직되지 않는다.
/// Exit에서 반드시 원복하므로, 피격이나 사망에 끊겨도 배율이 남지 않는다.
/// </summary>
public class Boss_Guard : BaseEntityState<BossManager>
{
    float _timer;

    public override void Enter()
    {
        _timer = manager.guardTime;

        manager.Stop();
        manager.guarding = true;
        if (manager.health != null) manager.health.SetDamageScale(manager.guardDamageScale);

        manager.PlayAnimation(manager.guardTrigger);
    }

    public override void UpdateState()
    {
        _timer -= Time.deltaTime;
        Transitions();
    }

    public override void Exit()
    {
        manager.guarding = false;
        if (manager.health != null) manager.health.SetDamageScale(1f);
    }

    public override void Transitions()
    {
        if (_timer > 0f) return;

        if (fsm is Boss_ReactionMachine machine) machine.Complete();
    }
}
