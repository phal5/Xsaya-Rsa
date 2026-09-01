using UnityEngine;

/// <summary>
/// 너무 붙었을 때 거리를 벌린다.
/// 복귀 임계값을 meleeRadius로 둬서 retreatRadius 근처에서 진동하지 않게 한다.
///
/// 이동은 retreatSpeed가 만든다. 플레이어를 계속 바라본 채 뒤로 물러난다.
/// </summary>
public class Boss_Retreat : BaseEntityState<BossManager>
{
    public override void UpdateState()
    {
        if (manager.HasPlayer) Retreat();
        else manager.Stop();

        Locomotion();

        Transitions();
    }

    /// <summary>
    /// 물러나는 그림을 지금 내는 속도에 맞춘다. <see cref="Boss_Approach"/>와 같은 자리다.
    /// 멈춰 있으면 물러나지 않는다.
    /// </summary>
    void Locomotion()
    {
        float speed = manager.HasPlayer ? manager.retreatSpeed : 0f;

        if (speed < manager.walkThreshold)
        {
            manager.PlayIfNot(manager.idleTrigger);
            return;
        }

        manager.PlayIfNot(manager.retreatTrigger);
        manager.SetMoveSpeed(speed);
    }

    public override void Exit()
    {
        manager.Stop();
    }

    public override void Transitions()
    {
        ToApproach();
    }

    void Retreat()
    {
        if (!manager.HasPlayer) { manager.Stop(); return; }

        manager.Move(-manager.DirectionToPlayer() * manager.retreatSpeed);
    }

    void ToApproach()
    {
        if (!manager.HasPlayer) return;

        if (manager.DistanceToPlayer() >= manager.meleeRadius)
        {
            fsm.TransitTo<Boss_Approach>();
        }
    }
}
