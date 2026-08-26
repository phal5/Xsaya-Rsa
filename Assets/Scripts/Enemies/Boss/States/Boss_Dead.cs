using UnityEngine;

/// <summary>
/// 흡수 상태. 한 번 들어오면 나가지 않는다.
///
/// 물리를 걷어내는 것과 쓰러지는 자세를 재생하는 것은 <b>여기서 하지 않는다</b>.
/// 둘 다 DamagableBase의 On Death에 물려 있다 — 체력이 0이 되는 그 순간이 정확한 시점이고,
/// FSM이 이 상태까지 도달하는지와 무관하게 걷혀야 하기 때문이다.
///
/// 여기에 남는 것은 상태 기계의 몫뿐이다. 전투 종료 처리와 소멸 타이머, 그리고 명부에 이름을 올리는 일.
/// </summary>
public class Boss_Dead : BaseEntityState<BossManager>
{
    float _despawnAt;

    public override void Enter()
    {
        manager.isDead = true;

        // 명부에 올린다. 여기가 보스가 확실히 죽은 유일한 지점이다.
        // DamagableBase.Die()에 두지 않는 것은 그쪽을 잡몹과 공유하기 때문이다.
        BossGraveyard.Bury(manager.bossId);

        if (manager.weapon != null) manager.weapon.EndAttack();

        // 죽은 뒤 방어 배율이 남지 않도록 되돌린다.
        manager.guarding = false;
        if (manager.health != null) manager.health.SetDamageScale(1f);

        // 전투가 끝났으니 화면을 <b>들어오기 전으로</b> 되돌린다. 2D를 박아두면
        // 3D 스테이지에서 2D로 싸우는 보스를 잡는 순간 스테이지가 납작한 채로 굳는다.
        if (manager.combatView != BossCombatView.Keep && PlayerManager.instance != null)
            PlayerManager.instance.Set2D(manager.viewBefore);

        // 전투가 끝났다고 알린다. Boss_Idle의 NotifyCombatStart와 짝을 이룬다.
        // 소멸까지 기다리지 않는 것은, 그 사이 빈 체력바가 남으면 전투가 안 끝난 것처럼 보이기 때문이다.
        manager.NotifyCombatEnd();

        _despawnAt = Time.time + manager.despawnDelay;
    }

    /// <summary>
    /// 소멸 시각이 됐는지 본다.
    ///
    /// <b>0 이하는 "지우지 않는다"는 뜻이다.</b> 시신을 남기는 보스가 그것이고, 값 하나가 곧 의도가 된다 —
    /// 큰 수를 적어두는 방식은 언젠가 지워지는 데다 왜 그 수인지가 값에 남지 않는다.
    /// Boss_Throwable의 회수 시간이나 겨누는 시간이 0을 같은 뜻으로 쓰는 것과 같은 규약이다.
    ///
    /// 사라지는 <b>모습</b>은 여기서 만들지 않는다. 그건 사망 이벤트에 물린 Disintegrate의 몫이고,
    /// 여기는 다 흩어진 껍데기를 거두는 일만 한다. 그래서 남기기로 해도 흩어지는 연출은 그대로 돈다.
    /// </summary>
    public override void UpdateState()
    {
        if (manager.despawnDelay <= 0f) return;

        if (Time.time < _despawnAt) return;

        Object.Destroy(manager.gameObject);
    }

    public override void Transitions() { }
}
