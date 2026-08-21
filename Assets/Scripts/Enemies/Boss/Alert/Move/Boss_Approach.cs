using UnityEngine;

/// <summary>
/// 플레이어를 향해 접근한다. 너무 가까워지면 후퇴로 넘긴다.
///
/// 이동은 approachSpeed가 만든다. 걷기 클립의 상하 움직임은 Bake Into Pose로
/// 포즈 안에 들어 있으므로 루트 모션을 쓰지 않는다.
/// 클립의 보폭과 approachSpeed가 어긋나면 발이 미끄러지므로 값을 맞춰야 한다.
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
    }

    public override void UpdateState()
    {
        if (manager.HasPlayer) Approach();
        else manager.Stop();

        Transitions();
    }

    public override void Exit()
    {
        // 속도는 상태 밖에 남으므로 반드시 거둔다.
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
