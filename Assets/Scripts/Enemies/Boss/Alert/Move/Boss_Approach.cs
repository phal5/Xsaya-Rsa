using UnityEngine;

/// <summary>
/// 플레이어를 향해 접근한다. 너무 가까워지면 후퇴로 넘긴다.
///
/// 이동은 루트 모션이 만든다. 걷기 클립이 앞으로 나아가고, 몸이 플레이어를 향해 회전하므로
/// 결과적으로 플레이어 쪽으로 다가간다. 속도는 클립이 정하고 발은 미끄러지지 않는다.
/// 중계기가 없는 경우에만 approachSpeed로 밀어주는 예전 방식으로 되돌아간다.
/// </summary>
public class Boss_Approach : BaseEntityState<BossManager>
{
    public override void Bootstrap()
    {
        manager.LookTowards(manager.Player);
    }

    public override void Enter()
    {
        manager.PlayAnimation(manager.walkTrigger);
        manager.SetRootMotion(true);
    }

    public override void UpdateState()
    {
        // 대상이 사라지면 루트 모션을 끊어야 한다. 안 그러면 걷기 클립이 계속 몸을 밀어낸다.
        bool hasTarget = manager.HasPlayer;
        manager.SetRootMotion(hasTarget);

        if (!hasTarget) manager.Stop();
        else if (!manager.HasRootMotion) Approach();

        Transitions();
    }

    public override void Exit()
    {
        // 루트 모션을 끄고, 속도는 상태 밖에 남으므로 반드시 거둔다.
        manager.SetRootMotion(false);
        manager.Stop();
    }

    public override void Transitions()
    {
        ToRetreat();
    }

    void Approach()
    {
        if (!manager.HasPlayer) { manager.Stop(); return; }

        manager.Move(manager.DirectionToPlayer() * manager.approachSpeed);
    }

    void ToRetreat()
    {
        if (!manager.HasPlayer) return;

        if (manager.DistanceToPlayer() < manager.retreatRadius)
        {
            fsm.TransitTo<Boss_Retreat>();
        }
    }
}
