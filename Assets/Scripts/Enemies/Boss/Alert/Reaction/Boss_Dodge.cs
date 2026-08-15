using UnityEngine;

/// <summary>플레이어 반대 방향으로 짧게 빠진다.</summary>
public class Boss_Dodge : BaseEntityState<BossManager>
{
    float _timer;

    public override void Enter()
    {
        _timer = manager.dodgeTime;
        manager.PlayAnimation(manager.dodgeTrigger);

        Vector3 away = -manager.DirectionToPlayer();
        if (away == Vector3.zero) away = -manager.character.forward;

        manager.Move(away * manager.dodgeSpeed);
    }

    public override void UpdateState()
    {
        _timer -= Time.deltaTime;
        Transitions();
    }

    public override void Exit()
    {
        manager.Stop();
    }

    public override void Transitions()
    {
        if (_timer > 0f) return;

        if (fsm is Boss_ReactionMachine machine) machine.Complete();
    }
}
