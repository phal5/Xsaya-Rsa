using UnityEngine;

/// <summary>
/// 보스의 최상위 상태기계. 피격과 사망은 어느 상태에서든 끊고 들어와야 하므로
/// 구독을 여기 한 곳에 두고, 하위 머신은 평상시 행동에만 집중한다.
///
/// Initial State  : Boss_Idle
/// ComponentStates: Boss_Alert
/// </summary>
public class BossRoot : FiniteStateMachine
{
    BossManager _boss;

    public override void Bootstrap()
    {
        _boss = manager as BossManager;

        if (_boss == null)
        {
            Debug.LogError($"[{name}] BossRoot의 manager에 BossManager가 연결되어 있지 않습니다.", this);
            return;
        }

        if (_boss.health == null)
        {
            Debug.LogError($"[{name}] BossManager.health가 비어 있어 피격/사망을 감지할 수 없습니다.", this);
            return;
        }

        _boss.health.AddDamageListener(OnDamaged);
    }

    // 피격은 OnTriggerEnter에서 올라와 갱신 주기 밖이다.
    // 콜백은 표시만 남기고 전이는 Transitions()에서 한다.
    bool _hitRequested;

    void OnDamaged(float _)
    {
        _hitRequested = true;
    }

    public override void Transitions()
    {
        if (!_hitRequested) return;
        _hitRequested = false;

        if (_boss == null || _boss.isDead) return;

        bool lethal = _boss.health.CurrentHealth <= 0f;

        // 죽는 타격이 아니라면 방어 중이거나 경직 쿨다운일 때 흘린다. 무한 경직 방지.
        if (!lethal && !_boss.CanStagger) return;

        // 경직/사망 중 무엇을 할지는 피격 머신이 정한다.
        // 이미 피격 머신 안이어도 재진입시켜서, 경직 중에 죽으면 사망으로 내려가게 한다.
        TransitTo<Boss_DamageMachine>();
    }

    /// <summary>경직/기상 후 전투로 복귀할 때 하위 상태들이 부른다.</summary>
    public void ToAlert()
    {
        TransitTo<Boss_Alert>();
    }

    public void ToIdle()
    {
        TransitTo<Boss_Idle>();
    }
}
