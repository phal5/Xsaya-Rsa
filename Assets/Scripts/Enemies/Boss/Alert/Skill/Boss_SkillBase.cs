using System.Collections.Generic;
using UnityEngine;

/// <summary>스킬 시작 시점 기준의 히트박스 구간. 연타는 이걸 여러 개 늘어놓는다.</summary>
[System.Serializable]
public struct HitWindow
{
    [Tooltip("스킬 시작(Enter) 기준 초.")]
    [Min(0f)] public float start;
    [Min(0f)] public float end;

    public HitWindow(float start, float end)
    {
        this.start = start;
        this.end = end;
    }

    public bool Contains(float t) => t >= start && t < end;
}

/// <summary>
/// 스킬 세 종류가 공유하는 준비 → 발동 → 후딜 진행.
/// 구간 길이와 위력은 꽂힌 슬롯(Boss_SkillSlot)의 프로파일에서 읽는다.
///
/// 히트박스는 두 가지 방식 중 하나로 열린다.
///  - Hit Windows가 비어 있으면: 발동 구간 전체가 하나의 판정 (단타)
///  - Hit Windows가 있으면: 그 구간들이 판정을 지배 (연타)
/// 구간이 열릴 때마다 StartAttack이 다시 불려 피격 목록이 비워지므로, 타마다 새로 맞는다.
/// </summary>
public abstract class Boss_SkillBase : ComponentEntityState<BossManager>
{
    protected enum Phase { Windup, Active, Recovery }

    [Header("Movement Source - 둘 다 켜면 이동이 겹쳐 발이 미끄러진다")]
    [Tooltip("애니메이션의 이동량을 몸체에 반영한다. 전진하며 베는 공격 등.")]
    [SerializeField] bool _useRootMotion;

    [Tooltip("스크립트 속도를 유지한다. 끄면 진입 시 수평 속도를 버리고 그 동안 계속 0으로 눌러둔다.")]
    [SerializeField] bool _useVelocity;

    [Tooltip("스킬 시작 기준 히트박스 구간들. 비워두면 발동 구간 전체가 한 번의 판정이 된다.")]
    [SerializeField] List<HitWindow> _hitWindows = new List<HitWindow>();

    protected Phase phase { get; private set; }

    protected Boss_SkillSlot Slot => fsm as Boss_SkillSlot;

    protected BossSkillProfile Profile => Slot != null ? Slot.Profile : null;

    bool UsesHitWindows => _hitWindows != null && _hitWindows.Count > 0;

    float _timer;
    float _elapsed;
    int _openWindow = -1;

    public override void Enter()
    {
        if (Profile == null)
        {
            Debug.LogError($"{GetType().Name}은(는) Boss_SkillSlot 안에서만 동작합니다.");
            return;
        }

        phase = Phase.Windup;
        _timer = Profile.windupTime;
        _elapsed = 0f;
        _openWindow = -1;

        if (!_useVelocity) manager.Stop();
        manager.PlayAnimation(Profile.animatorTrigger);

        if (manager.rootMotion != null) manager.rootMotion.Active = _useRootMotion;

        OnWindup();
    }

    public override void UpdateState()
    {
        if (Profile == null) return;

        _elapsed += Time.deltaTime;
        _timer -= Time.deltaTime;

        if (_timer <= 0f) Advance();

        // 속도를 안 쓰는 스킬은 잔여 속도를 계속 눌러둔다.
        // Stop()만으로는 maxAccel로 서서히 줄어들어 그 동안 발이 미끄러진다.
        if (!_useVelocity && manager.steering != null) manager.steering.KillHorizontalVelocity();

        UpdateHitWindows();
        Transitions();
    }

    public override void Exit()
    {
        // 끊겼든 끝났든 루트 모션은 반드시 꺼야 한다. 켜둔 채 나가면 이동 상태와 싸운다.
        if (manager.rootMotion != null) manager.rootMotion.Active = false;

        // 반응이나 피격에 중간에 끊겼을 수 있다. 히트박스를 반드시 내린다.
        if (manager.weapon != null) manager.weapon.EndAttack();
        _openWindow = -1;

        // 끊겼든 끝났든 쿨다운은 돌린다. 끊긴 직후 같은 스킬이 즉시 다시 나가는 걸 막는다.
        if (Profile != null) Profile.StartCooldown();
    }

    public override void Transitions() { }

    #region Phases

    void Advance()
    {
        switch (phase)
        {
            case Phase.Windup:
                phase = Phase.Active;
                _timer = Profile.activeTime;
                Activate();
                OnActivate();
                break;

            case Phase.Active:
                phase = Phase.Recovery;
                _timer = Profile.recoveryTime;
                if (!UsesHitWindows && manager.weapon != null) manager.weapon.EndAttack();
                OnRecover();
                break;

            case Phase.Recovery:
                Complete();
                break;
        }
    }

    void Activate()
    {
        // 구간 목록이 있으면 판정은 그쪽이 지배한다.
        if (UsesHitWindows || manager.weapon == null) return;

        manager.weapon.SetDamage(Profile.damage);
        manager.weapon.StartAttack();
    }

    #endregion

    #region Hit Windows

    void UpdateHitWindows()
    {
        if (!UsesHitWindows) return;

        int current = -1;
        for (int i = 0; i < _hitWindows.Count; i++)
        {
            if (_hitWindows[i].Contains(_elapsed)) { current = i; break; }
        }

        if (current == _openWindow) return;

        if (_openWindow >= 0 && manager.weapon != null) manager.weapon.EndAttack();

        _openWindow = current;

        if (current >= 0 && manager.weapon != null)
        {
            manager.weapon.SetDamage(Profile.damage);
            manager.weapon.StartAttack();   // 피격 목록이 비워져 이번 타는 새로 맞는다
        }
    }

    #endregion

    /// <summary>스킬이 정상적으로 끝났을 때 슬롯을 통해 선택기로 돌아간다.</summary>
    protected void Complete()
    {
        if (Slot != null) Slot.Complete();
    }

    #region Hooks - 파생 스킬이 필요할 때만 채운다

    protected virtual void OnWindup() { }

    protected virtual void OnActivate() { }

    protected virtual void OnRecover() { }

    #endregion

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!UsesHitWindows) return;

        Boss_SkillSlot slot = GetComponent<Boss_SkillSlot>();
        if (slot == null) return;

        float total = slot.Profile.windupTime + slot.Profile.activeTime + slot.Profile.recoveryTime;

        for (int i = 0; i < _hitWindows.Count; i++)
        {
            HitWindow w = _hitWindows[i];

            if (w.end <= w.start)
                Debug.LogWarning($"[{name}] Hit Window {i}: end({w.end})가 start({w.start}) 이하라 판정이 열리지 않습니다.", this);
            else if (w.end > total)
                Debug.LogWarning($"[{name}] Hit Window {i}: end({w.end})가 스킬 전체 길이({total:0.00}s)를 넘어 잘립니다.", this);
        }
    }
#endif
}
