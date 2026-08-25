using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 싸우는 동안의 화면. 보스마다 다르다.
///
/// Keep이 있는 이유는, 화면을 바꾸지 않는 보스와 "2D로 바꾸는 보스"가 다르기 때문이다.
/// 둘을 bool 하나로 뭉개면, 3D 스테이지에 놓인 중간보스가 깨어나며 화면을 납작하게 만든다.
/// </summary>
public enum BossCombatView
{
    /// <summary>건드리지 않는다. 들어온 화면 그대로 싸운다.</summary>
    Keep,

    /// <summary>납작한 2D로 싸운다.</summary>
    Flat2D,

    /// <summary>Z가 열린 3D로 싸운다.</summary>
    Full3D,
}

/// <summary>
/// 보스의 데이터 허브. 상태들은 로직만 갖고, 참조와 수치는 전부 여기서 읽는다.
/// AgnostosManager와 같은 역할이다.
/// </summary>
public class BossManager : EntityManager
{
    [Header("References")]
    [field: SerializeField] public Transform character { get; private set; }
    [Tooltip("상태들이 실제로 호출하는 이동 계층. 점프/접지 판정도 여기 있다.")]
    [field: SerializeField] public Boss_Steering steering { get; private set; }
    [Tooltip("Steering이 쓰는 백엔드. 상태에서 직접 부르지 않는다.")]
    [field: SerializeField] public SingularFlatMovement movement { get; private set; }
    [field: SerializeField] public DamagableBase health { get; private set; }
    [field: SerializeField] public MeleeWeapon weapon { get; private set; }
    [Tooltip("비워두면 애니메이션 트리거는 무시된다. 모델을 받은 뒤 연결하면 된다.")]
    [field: SerializeField] public Animator animator { get; private set; }
    [Tooltip("루트 모션을 몸체로 전달하는 중계기. 애니메이터와 같은 오브젝트에 있다.")]
    [field: SerializeField] public RootMotionRelay rootMotion { get; private set; }

    [Header("Combat Events")]
    [Tooltip("전투가 열릴 때. 이 보스를 지켜보는 것들을 여기 꽂는다 — 체력바, 음악, 관문 잠그기 등. " +
             "판마다 정확히 한 번만 발동한다.")]
    [SerializeField] UnityEvent onCombatStart;

    [Tooltip("전투가 끝날 때. 사망으로 닫히는 지점이다.")]
    [SerializeField] UnityEvent onCombatEnd;

    [Header("Radii - retreat < melee < awake 를 지켜야 한다")]
    [Tooltip("이 안에 들어오면 잠에서 깬다.")]
    [field: SerializeField] public float awakeRadius { get; private set; } = 12f;
    [Tooltip("이 안이면 근접으로 판단해 속공을 고른다.")]
    [field: SerializeField] public float meleeRadius { get; private set; } = 3.5f;
    [Tooltip("이보다 가까우면 거리를 벌린다.")]
    [field: SerializeField] public float retreatRadius { get; private set; } = 1.5f;

    [Header("Movement")]
    [field: SerializeField] public float approachSpeed { get; private set; } = 3f;
    [field: SerializeField] public float retreatSpeed { get; private set; } = 2f;

    [Header("Leash - 이탈")]
    [Tooltip("끄면 한 번 깨어난 뒤 다시 잠들지 않는다. 아레나 보스 기본값.")]
    [field: SerializeField] public bool useLeash { get; private set; } = false;
    [Tooltip("awakeRadius의 배수. 경계에서 Idle↔Alert가 진동하지 않도록 여유를 둔다.")]
    [field: SerializeField, Min(1f)] public float leashMultiplier { get; private set; } = 1.5f;

    public float LeashRadius => awakeRadius * leashMultiplier;

    [Header("Reaction - 회피 / 방어")]
    [Tooltip("플레이어 공격을 감지했을 때 반응할 확률.")]
    [field: SerializeField, Range(0f, 1f)] public float reactionChance { get; private set; } = 0.4f;
    [Tooltip("이 거리 밖에서 들어온 공격은 무시한다.")]
    [field: SerializeField] public float reactionRadius { get; private set; } = 4.5f;
    [Tooltip("반응 후 다음 반응까지의 최소 간격.")]
    [field: SerializeField] public float reactionCooldown { get; private set; } = 1.5f;
    [Tooltip("이보다 가까우면 방어, 멀면 회피를 고른다.")]
    [field: SerializeField] public float guardPreferenceRadius { get; private set; } = 2.5f;

    [Header("Dodge")]
    [field: SerializeField] public float dodgeSpeed { get; private set; } = 9f;
    [field: SerializeField] public float dodgeTime { get; private set; } = 0.35f;

    [Header("Sidestep - 되날아오는 것을 옆으로 피한다")]
    [Tooltip("옆으로 빠질 때 앞을 재는 거리. 이만큼 트여 있으면 그쪽으로 간다.")]
    [field: SerializeField] public float sidestepDistance { get; private set; } = 4f;

    [Tooltip("옆을 잴 때 쓰는 구의 반지름. 몸통 폭보다 약간 크게 잡는다.")]
    [field: SerializeField] public float sidestepProbeRadius { get; private set; } = 0.6f;

    [Header("Guard")]
    [field: SerializeField] public float guardTime { get; private set; } = 0.8f;
    [Tooltip("방어 중 받는 피해 배율. 0이면 완전 무효.")]
    [field: SerializeField, Range(0f, 1f)] public float guardDamageScale { get; private set; } = 0.2f;

    [Header("Stagger - 피격 경직")]
    [field: SerializeField] public float staggerTime { get; private set; } = 0.4f;
    [Tooltip("경직 후 다시 경직되기까지의 최소 간격. 무한 경직을 막는다.")]
    [field: SerializeField] public float staggerCooldown { get; private set; } = 1.0f;

    [Header("Animator Triggers")]
    [field: SerializeField] public string idleTrigger { get; private set; } = "Idle";
    [field: SerializeField] public string walkTrigger { get; private set; } = "Walk";
    [field: SerializeField] public string retreatTrigger { get; private set; } = "Retreat";
    [field: SerializeField] public string dodgeTrigger { get; private set; } = "Dodge";
    [field: SerializeField] public string guardTrigger { get; private set; } = "Guard";
    [field: SerializeField] public string staggerTrigger { get; private set; } = "Stagger";
    [field: SerializeField] public string deathTrigger { get; private set; } = "Death";

    [Tooltip("쓰러진 뒤 오브젝트가 사라지기까지의 시간. 사망 애니메이션 길이에 맞춘다.")]
    [field: SerializeField] public float despawnDelay { get; private set; } = 3f;

    [Header("Combat View")]
    [Tooltip("싸우는 동안의 화면. 깨어날 때 이쪽으로 바꾸고, 죽으면 들어오기 전으로 되돌린다. " +
             "Keep이면 양쪽 다 하지 않는다.")]
    [field: SerializeField] public BossCombatView combatView { get; private set; } = BossCombatView.Full3D;

    [Tooltip("씬을 다시 불러도 이 보스를 알아보기 위한 이름표. 보스마다 겹치지 않게 적는다. " +
             "오브젝트 이름을 쓰지 않는 이유는, 하이어라키에서 이름을 바꿨다고 죽은 보스가 되살아나면 안 되기 때문이다.")]
    [field: SerializeField] public string bossId { get; private set; } = "";

    #region Runtime Flags

    /// <summary>방어 중에는 경직되지 않는다.</summary>
    public bool guarding { get; set; }

    /// <summary>사망은 흡수 상태다. 이후 들어오는 피격은 전부 무시된다.</summary>
    public bool isDead { get; set; }

    /// <summary>
    /// 싸움을 열기 전의 화면. <see cref="Boss_Idle"/>이 적고 <see cref="Boss_Dead"/>가 되돌린다.
    ///
    /// 여는 쪽이 들고 있지 않는 이유는, 그 상태가 한 번 떠나면 돌아오지 않아
    /// 죽는 시점에 물어볼 수가 없기 때문이다. 두 상태가 모두 보는 자리는 여기뿐이다.
    /// </summary>
    public bool viewBefore { get; set; } = true;

    float _staggerReadyTime;
    float _reactionReadyTime;

    /// <summary>
    /// 스킬이 도는 동안 경직을 흘릴지. 스킬이 Enter에서 세우고 Exit에서 되돌린다.
    /// 방어와 같은 자리에 두어 판정이 한 줄에 모이게 한다.
    /// </summary>
    public bool staggerImmune { get; set; }

    public bool CanStagger => !guarding && !staggerImmune && Time.time >= _staggerReadyTime;
    public bool ReactionReady => Time.time >= _reactionReadyTime;

    public void StartStaggerCooldown() { _staggerReadyTime = Time.time + staggerCooldown; }
    public void StartReactionCooldown() { _reactionReadyTime = Time.time + reactionCooldown; }

    #endregion

    #region Combat Events

    /// <summary>
    /// 전투가 열렸다. <see cref="Boss_Idle"/>이 깨어나는 그 한 번에 부른다.
    ///
    /// 상태기계가 <b>무엇이 듣는지 모르게</b> 두려고 이벤트로 낸다. 체력바를 직접 부르면
    /// 보스 FSM이 UI를 알게 되고, 바를 두지 않은 보스마다 예외를 하나씩 두게 된다.
    /// 듣는 쪽은 인스펙터에서 정한다.
    /// </summary>
    public void NotifyCombatStart() { onCombatStart.Invoke(); }

    /// <summary>전투가 끝났다. <see cref="Boss_Dead"/>가 부른다.</summary>
    public void NotifyCombatEnd() { onCombatEnd.Invoke(); }

    #endregion

    #region Alert Clock

    float _alertedAt = -1f;

    /// <summary>전투에 들어온 뒤 흐른 시간. 스킬 조건이 참조한다.</summary>
    public float AlertedFor => _alertedAt < 0f ? 0f : Time.time - _alertedAt;

    /// <summary>Boss_Alert이 진입·이탈할 때 알린다.</summary>
    public void SetAlerted(bool on) { _alertedAt = on ? Time.time : -1f; }

    #endregion

    #region Player Queries

    public Transform Player => PlayerManager.instance != null ? PlayerManager.instance.player : null;

    public bool HasPlayer => Player != null && character != null;

    public float DistanceToPlayer()
    {
        if (!HasPlayer) return float.MaxValue;
        return Vector3.Distance(Player.position, character.position);
    }

    /// <summary>보스에서 플레이어를 향하는 수평 단위벡터.</summary>
    public Vector3 DirectionToPlayer()
    {
        if (!HasPlayer) return Vector3.zero;
        return CustomMath.RemoveY(Player.position - character.position).normalized;
    }

    #endregion

    #region Movement Facade - 상태는 전부 이 통로만 쓴다

    public void Move(Vector3 velocity)
    {
        if (steering != null) steering.Move(velocity);
        else if (movement != null) movement.Move(velocity);   // Steering 미배선 시 폴백
    }

    public void Stop()
    {
        if (steering != null) steering.Stop();
        else if (movement != null) movement.Move(Vector3.zero);
    }

    public void LookTowards(Transform target)
    {
        if (steering != null) steering.LookTowards(target);
        else if (movement != null) movement.LookTowards(target);
    }

    public bool Grounded => steering == null || steering.Grounded;

    /// <summary>루트 모션 중계기가 붙어 있는지. 없으면 상태들이 속도 이동으로 되돌아간다.</summary>

    #endregion

    /// <summary>
    /// 몸을 물리에서 걷어낸다. DamagableBase의 On Death에 물려 쓴다.
    ///
    /// 리지드바디는 지우지 않고 재우기만 한다 — 지우면 그 프레임에 콜라이더가 정적으로 재계산되며 튄다.
    /// 이동 컴포넌트도 끈다. 매 FixedUpdate에 조준 회전을 덮어쓰기 때문에,
    /// 두면 쓰러진 뒤에도 플레이어를 계속 바라본다.
    /// </summary>
    public void DisableBody()
    {
        foreach (Collider c in GetComponentsInChildren<Collider>(true))
            c.enabled = false;

        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(true))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        foreach (SingularFlatMovement move in GetComponentsInChildren<SingularFlatMovement>(true))
            move.enabled = false;

        foreach (Boss_Steering steer in GetComponentsInChildren<Boss_Steering>(true))
            steer.enabled = false;
    }

    [Tooltip("떠 있던 보스가 죽어 떨어질 때, 땅에 닿기를 기다리는 최대 시간. 이 안에 못 닿으면 그냥 걷어낸다.")]
    [field: SerializeField] public float deathFallTimeout { get; private set; } = 3f;

    /// <summary>
    /// 떠 있는 보스의 죽음. <see cref="DisableBody"/>를 곧바로 부르면 리지드바디가 키네마틱이 되어
    /// 그 자리에 굳는다 — 떠 있던 보스에게는 공중에 못 박히는 것으로 보인다.
    ///
    /// 그래서 순서를 나눈다. 먼저 부양만 놓아 중력을 돌려주고, 땅에 닿은 뒤에 몸을 걷어낸다.
    /// 그동안 콜라이더와 이동 계층은 살아 있어야 실제로 떨어지고 바닥에 멈춘다.
    ///
    /// DamagableBase의 On Death에 이걸 물리면 된다. 지상 보스는 그대로 DisableBody를 쓴다.
    /// </summary>
    public void DropThenDisableBody()
    {
        foreach (Boss_Hover hover in GetComponentsInChildren<Boss_Hover>(true))
            hover.enabled = false;   // OnDisable이 중력 배율을 되돌린다

        StartCoroutine(FallThenDisable());
    }

    System.Collections.IEnumerator FallThenDisable()
    {
        float deadline = Time.time + deathFallTimeout;

        while (Time.time < deadline && !Grounded) yield return new WaitForFixedUpdate();

        DisableBody();
    }

    public void PlayAnimation(string trigger)
    {
        if (animator == null || string.IsNullOrEmpty(trigger)) return;
        animator.SetTrigger(trigger);
    }

    [Tooltip("모든 스킬 상태의 Speed Multiplier에 물려둔 Float 파라미터 이름. 스킬은 한 번에 하나만 돌아 하나면 족하다.")]
    [field: SerializeField] public string skillSpeedParameter { get; private set; } = "SkillSpeed";

    /// <summary>스킬이 자기 애니메이션 구간을 스킬 길이에 맞출 때 쓴다.</summary>
    public void SetSkillSpeed(float speed)
    {
        if (animator == null || string.IsNullOrEmpty(skillSpeedParameter)) return;
        animator.SetFloat(skillSpeedParameter, speed);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Agnostos에서 반경 대소관계가 뒤집혀 공격 전이가 불가능해진 적이 있다. 같은 사고를 막는다.
        if (retreatRadius >= meleeRadius)
            Debug.LogWarning($"[{name}] retreatRadius({retreatRadius}) >= meleeRadius({meleeRadius}). 근접 판정 구간이 사라져 속공이 나가지 않는다.", this);

        if (meleeRadius >= awakeRadius)
            Debug.LogWarning($"[{name}] meleeRadius({meleeRadius}) >= awakeRadius({awakeRadius}). 깨어나는 즉시 사거리 안이라 접근 구간이 사라진다.", this);

        // 이름표가 없으면 죽어도 명부에 오르지 못해, 무대를 다시 부를 때마다 되살아난다.
        if (GetComponent<BossGrave>() != null && string.IsNullOrEmpty(bossId))
            Debug.LogWarning($"[{name}] bossId가 비어 있습니다. 이대로는 죽여도 무대를 다시 부르면 되살아납니다.", this);

        foreach (Boss_SkillBase skill in GetComponentsInChildren<Boss_SkillBase>(true))
            WarnIfSkillRangeCollides(skill);
    }

    /// <summary>
    /// 보스는 retreatRadius ~ meleeRadius 사이를 오가며 거리를 잰다.
    /// 스킬의 minRange가 그 구간 <b>안</b>에 있으면, 보스가 스스로 물러나다 그 지점을 지나는 순간
    /// 스킬이 열린다 — 방금 벌린 거리를 자기가 지운다. 플레이어가 어디 있느냐가 아니라
    /// 자기 발걸음이 발동 조건이 되는 셈이라, 도약기가 3m짜리 근접기로 변한다.
    ///
    /// 그래서 minRange는 구간 밖 어느 한쪽에 있어야 한다.
    ///   구간 아래(≤ retreatRadius) : 구간 내내 쓸 수 있는 근접기
    ///   구간 위(≥ meleeRadius)     : 플레이어가 실제로 거리를 벌렸을 때만 열리는 갭 클로저
    ///
    /// 반경을 고칠 때와 스킬을 고칠 때 모두 걸려야 하므로, 양쪽 OnValidate가 이걸 부른다.
    /// </summary>
    public void WarnIfSkillRangeCollides(Boss_SkillBase skill)
    {
        if (skill == null) return;

        float min = skill.Profile.minRange;
        if (min <= retreatRadius || min >= meleeRadius) return;

        Debug.LogWarning(
            $"[{name}] {skill.GetType().Name}의 minRange({min})가 이동 구간 {retreatRadius} ~ {meleeRadius} 안에 있습니다. " +
            $"보스가 후퇴하다 이 지점을 지나며 스스로 발동시켜, 방금 벌린 거리를 자기가 지웁니다. " +
            $"{retreatRadius} 이하(근접기)나 {meleeRadius} 이상(갭 클로저)으로 옮기세요.", skill);
    }

    void OnDrawGizmosSelected()
    {
        if (character == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(character.position, awakeRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(character.position, meleeRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(character.position, retreatRadius);
    }
#endif
}

/// <summary>
/// 스킬 한 개의 구간 길이와 위력. 준비 → 발동 → 후딜 순으로 진행된다.
/// </summary>
[System.Serializable]
public class BossSkillProfile
{
    public string name = "Skill";
    [Tooltip("히트박스가 켜지기 전 선딜. 이 길이로 스킬의 '읽히는 정도'가 결정된다.")]
    [Min(0f)] public float windupTime = 0.5f;
    [Tooltip("히트박스가 켜져 있는 시간.")]
    [Min(0f)] public float activeTime = 0.15f;
    [Tooltip("히트박스가 꺼진 뒤 움직이지 못하는 시간.")]
    [Min(0f)] public float recoveryTime = 0.5f;
    [Min(0f)] public float cooldown = 3f;
    [Min(0f)] public float damage = 25f;
    [Tooltip("이 스킬이 닿는 거리. 이보다 멀면 선택되지 않고 계속 접근한다.")]
    [Min(0f)] public float range = 3.5f;
    [Tooltip("이 스킬을 쓰기 위한 최소 거리. 근접기는 0, 도약기처럼 붙으면 안 되는 것은 올려 잡는다.")]
    [Min(0f)] public float minRange = 0f;
    [Tooltip("비워두면 애니메이션을 재생하지 않는다.")]
    public string animatorTrigger = "";

    [System.NonSerialized] float _readyTime;

    public BossSkillProfile() { }

    public BossSkillProfile(string name, float windup, float active, float recovery, float cooldown, float damage)
    {
        this.name = name;
        this.windupTime = windup;
        this.activeTime = active;
        this.recoveryTime = recovery;
        this.cooldown = cooldown;
        this.damage = damage;
        this.animatorTrigger = name;
    }

    public bool IsReady => Time.time >= _readyTime;

    public void StartCooldown() { _readyTime = Time.time + cooldown; }

    public void ResetCooldown() { _readyTime = 0f; }
}
