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

    public override void UpdateState()
    {
        if (manager.HasPlayer) Approach();
        else manager.Stop();

        Locomotion();

        Transitions();
    }

    /// <summary>
    /// 걷는 그림을 지금 내는 속도에 맞춘다. <b>매 프레임 본다</b> — 진입할 때 한 번만 걸어두면,
    /// 그 한 번이 흘러가거나 도중에 멈춰 서도 되찾을 길이 없다.
    ///
    /// 멈춰 있으면 걷지 않는다. 속도에 그대로 비례시키면 멈추기 직전에 다리가 기어간다.
    /// </summary>
    void Locomotion()
    {
        float speed = manager.HasPlayer ? manager.approachSpeed : 0f;

        if (speed < manager.walkThreshold)
        {
            manager.PlayIfNot(manager.idleTrigger);
            return;
        }

        manager.PlayIfNot(manager.walkTrigger);
        manager.SetMoveSpeed(speed);
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
