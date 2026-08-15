using UnityEngine;

/// <summary>
/// 흡수 상태. 한 번 들어오면 나가지 않는다.
/// 실제 오브젝트 파괴는 DamagableBase.Die()가 담당하므로 여기서는 정리와 연출만 한다.
/// </summary>
public class Boss_Dead : BaseEntityState<BossManager>
{
    public override void Enter()
    {
        manager.isDead = true;

        manager.Stop();
        if (manager.weapon != null) manager.weapon.EndAttack();

        // 죽은 뒤 방어 배율이 남지 않도록 되돌린다.
        manager.guarding = false;
        if (manager.health != null) manager.health.SetDamageScale(1f);

        manager.PlayAnimation(manager.deathTrigger);
    }

    public override void UpdateState() { }

    public override void Transitions() { }
}
