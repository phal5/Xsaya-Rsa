using UnityEngine;

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

    #region Runtime Flags

    /// <summary>방어 중에는 경직되지 않는다.</summary>
    public bool guarding { get; set; }

    /// <summary>사망은 흡수 상태다. 이후 들어오는 피격은 전부 무시된다.</summary>
    public bool isDead { get; set; }

    float _staggerReadyTime;
    float _reactionReadyTime;

    public bool CanStagger => !guarding && Time.time >= _staggerReadyTime;
    public bool ReactionReady => Time.time >= _reactionReadyTime;

    public void StartStaggerCooldown() { _staggerReadyTime = Time.time + staggerCooldown; }
    public void StartReactionCooldown() { _reactionReadyTime = Time.time + reactionCooldown; }

    #endregion

    #region Special Condition

    /// <summary>
    /// TODO: 특수기 발동 조건. 직접 지정하실 조건이 정해지면 여기를 채우면 된다.
    /// 예) 체력 50% 이하 / 특정 페이즈 진입 / 플레이어가 일정 시간 회피만 반복 등.
    /// 지금은 항상 false라 특수기가 선택되지 않는다.
    /// </summary>
    public bool SpecialCondition => false;

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
    public bool HasRootMotion => rootMotion != null;

    public void SetRootMotion(bool on)
    {
        if (rootMotion != null) rootMotion.Active = on;
    }

    #endregion

    public void PlayAnimation(string trigger)
    {
        if (animator == null || string.IsNullOrEmpty(trigger)) return;
        animator.SetTrigger(trigger);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Agnostos에서 반경 대소관계가 뒤집혀 공격 전이가 불가능해진 적이 있다. 같은 사고를 막는다.
        if (retreatRadius >= meleeRadius)
            Debug.LogWarning($"[{name}] retreatRadius({retreatRadius}) >= meleeRadius({meleeRadius}). 근접 판정 구간이 사라져 속공이 나가지 않는다.", this);

        if (meleeRadius >= awakeRadius)
            Debug.LogWarning($"[{name}] meleeRadius({meleeRadius}) >= awakeRadius({awakeRadius}). 깨어나는 즉시 사거리 안이라 접근 구간이 사라진다.", this);
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
