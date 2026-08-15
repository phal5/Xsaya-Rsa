using UnityEngine;

/// <summary>
/// 너무 붙었을 때 거리를 벌린다.
/// 복귀 임계값을 meleeRadius로 둬서 retreatRadius 근처에서 진동하지 않게 한다.
///
/// 뒷걸음질 클립(great sword walk (2))이 뒤로 물러나므로, 플레이어를 계속 바라본 채
/// 루트 모션만으로 거리가 벌어진다.
/// </summary>
public class Boss_Retreat : BaseEntityState<BossManager>
{
    public override void Enter()
    {
        manager.PlayAnimation(manager.retreatTrigger);
        manager.SetRootMotion(true);
    }

    public override void UpdateState()
    {
        // 대상이 사라지면 루트 모션을 끊어야 한다. 안 그러면 후진 클립이 계속 몸을 밀어낸다.
        bool hasTarget = manager.HasPlayer;
        manager.SetRootMotion(hasTarget);

        if (!hasTarget) manager.Stop();
        else if (!manager.HasRootMotion) Retreat();

        Transitions();
    }

    public override void Exit()
    {
        manager.SetRootMotion(false);
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
