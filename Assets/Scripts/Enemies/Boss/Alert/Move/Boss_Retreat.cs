using UnityEngine;

/// <summary>
/// 너무 붙었을 때 거리를 벌린다.
/// 복귀 임계값을 meleeRadius로 둬서 retreatRadius 근처에서 진동하지 않게 한다.
///
/// 이동은 retreatSpeed가 만든다. 플레이어를 계속 바라본 채 뒤로 물러난다.
/// </summary>
public class Boss_Retreat : BaseEntityState<BossManager>
{
    public override void Enter()
    {
        manager.PlayAnimation(manager.retreatTrigger);
    }

    public override void UpdateState()
    {
        if (manager.HasPlayer) Retreat();
        else manager.Stop();

        Transitions();
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
