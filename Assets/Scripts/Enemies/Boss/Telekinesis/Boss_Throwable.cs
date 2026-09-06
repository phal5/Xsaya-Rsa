using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스가 집어던질 수 있는 물건. 바닥에 미리 놓아두고 이 부품만 붙이면 탄환이 된다.
///
/// 보스가 주변을 훑어 물건을 <b>알아내지</b> 않는다 — 물건이 스스로 명부에 이름을 올린다.
/// 그래서 무엇이 탄환인지는 씬을 보면 바로 알 수 있고, 보스는 명부만 읽는다.
///
/// 여기가 아는 것은 <b>정체와 국면</b>뿐이다 — 지금 집을 수 있는지, 누가 들고 있는지,
/// 쓰이고 나서 언제 돌아오는지. 날아가는 일은 전부 <see cref="Boss_Projectile"/>이 맡는다.
///
///   Idle    놓여 있다. 이때만 집을 수 있다.
///   Held    보스가 들고 있다. 자리는 보스가 매 스텝 지정한다.
///   Aiming  던지기로 정해진 뒤 표적을 겨누는 짧은 구간. 이게 예고다.
///   Flying  발사체가 몰고 간다. 이 부품은 끝나기를 기다릴 뿐이다.
///   Stuck   땅에 박혀 있다. 그 자리가 새 쉼터가 되고, 시간이 지나면 거기서 회수된다.
///
/// 자리는 처음부터 끝까지 이 부품과 발사체가 정한다. 그래서 <b>리지드바디가 필요 없다</b>.
/// </summary>
[RequireComponent(typeof(Boss_Projectile))]
public class Boss_Throwable : MonoBehaviour, IDamageable
{
    /// <summary>
    /// Aiming은 표적을 향해 돌아서고 뒤로 물러나는 구간이다 — "저게 곧 온다"가 자세로 읽힌다.
    /// Stuck은 "쓰이고 사라진 것"이 아니라 <b>땅에 꽂혀 있는 것</b>이다.
    /// 아직 쓸 수 있는 물건이라 쳐내면 되날아가고, 시간이 지나면 그 자리에서 회수된다.
    /// </summary>
    /// <summary>
    /// <b>새 값은 끝에 붙인다.</b> 가운데에 끼우면 이미 저장된 정수가 통째로 한 칸씩 밀린다.
    /// </summary>
    public enum Phase { Idle, Held, Aiming, Flying, Stuck, Captured }

    [Header("Body")]
    [Tooltip("비워두면 자기 자신에서 찾는다. 없으면 위치만 옮긴다.")]
    [SerializeField] Rigidbody _body;

    [Header("Hold")]
    [Tooltip("들려 있을 때 지정된 자리로 따라붙는 빠르기. 스킬이 덮어쓴다.")]
    [SerializeField, Min(0.1f)] float _defaultFollowSpeed = 20f;

    [Header("Telegraph - 나가기 전에 겨눈다")]
    [Tooltip("표적을 겨누고 버티는 시간. 0이면 정해지는 즉시 날아간다.\n" +
             "이 시간이 곧 플레이어가 읽고 피할 수 있는 창이다.")]
    [SerializeField, Min(0f)] float _telegraphTime = 0.35f;

    [Tooltip("겨누는 동안 뒤로 물러나는 거리. 활시위처럼 당겼다 나간다.")]
    [SerializeField, Min(0f)] float _telegraphPullback = 0.8f;

    [Tooltip("표적을 향해 돌아서는 빠르기.")]
    [SerializeField, Min(1f)] float _aimTurnSpeed = 14f;

    [Header("Collision")]
    [Tooltip("탄환끼리 부딪히지 않게 한다.\n" +
             "끄면 바닥에 쌓인 것이 서로를 밀어내고, 날아가는 것이 놓여 있는 것에 걸려 멈춘다.")]
    [SerializeField] bool _ignoreOtherThrowables = true;

    [Header("Recovery - 박힌 뒤 다시 쓰이기까지")]
    [Tooltip("던진 뒤 이만큼 지나면 있는 그 자리에서 다시 집을 수 있게 된다. 0이면 회수하지 않는다.\n" +
             "시계는 손을 떠나는 순간부터 흐른다 — 어디에 어떻게 멈췄는지는 회수 시각을 바꾸지 않는다.")]
    [SerializeField, Min(0f)] float _respawnDelay = 8f;

    [Tooltip("고리를 풀어 떨어뜨릴 때 아래로 쏘는 속도. 물리 대신 이걸로 떨어진다.")]
    [SerializeField, Min(0.5f)] float _dropSpeed = 8f;

    [Header("Parry - 주인공이 쳐낼 때")]
    [Tooltip("이만큼 맞으면 쳐내진다. 집힐 때마다 다시 채워진다.")]
    [SerializeField, Min(1f)] float _maxHealth = 1f;

    [Tooltip("떠 있는 동안(공전 고리) 쳐낼 수 있는지.")]
    [SerializeField] bool _parryWhileHeld = true;

    [Tooltip("날아오는 동안 쳐낼 수 있는지.")]
    [SerializeField] bool _parryWhileFlying = true;

    [Tooltip("땅에 박혀 회수를 기다리는 동안 쳐낼 수 있는지. 켜면 꽂힌 창이 플레이어의 무기가 된다.")]
    [SerializeField] bool _parryWhileStuck = true;

    [Tooltip("바닥에 놓여 있을 때도 쳐낼 수 있는지. 켜면 교전 전에 미리 치워둘 수 있게 된다.")]
    [SerializeField] bool _parryWhileIdle;

    [Tooltip("받아치기 판정 반경. 창 본체는 얇아 검으로 맞힐 수 없으므로, 이 크기의 영역을 따로 둔다.\n" +
             "0이면 영역을 만들지 않는다 — 본체에 직접 맞아야만 쳐낼 수 있게 된다.")]
    [SerializeField, Min(0f)] float _parryRadius = 0.9f;

    [Tooltip("쳐냈을 때 되날아가는 속도. 0이면 발사체의 기본 속도를 쓴다.")]
    [SerializeField, Min(0f)] float _deflectSpeed = 22f;

    [Tooltip("되날아가는 동안의 속도 배수. 가로축은 <b>쳐낸 뒤 흐른 시간(초)</b>, 세로축은 위 속도에 곱할 배수다. " +
             "마지막 키 이후로는 그 값이 그대로 이어진다. 평평한 1이면 지금까지와 같다. " +
             "0을 찍으면 그 순간 추진이 사라져 중력만 남는다 - 멈춰 뜨는 것이 아니라 떨어진다.")]
    [SerializeField] AnimationCurve _deflectSpeedCurve = AnimationCurve.Constant(0f, 1f, 1f);

    [Tooltip("되날아간 것이 주는 피해. 보스가 창을 돌리는 중일 때만 실린다 - " +
             "그렇지 않으면 때리러 가는 것이 아니라 되찾기러 가는 것이라 0으로 나간다.")]
    [SerializeField, Min(0f)] float _deflectDamage = 20f;

    [Tooltip("되날아갈 때의 선회 속도(도/초). 때리러 가든 되찾기러 가든 보스를 따라붙어 반드시 닿게 한다. " +
             "0이면 쳐낸 순간의 방향으로 곧게 날아가 빗나간다.")]
    [SerializeField, Min(0f)] float _deflectHoming = 240f;

    [Tooltip("보스에게 이만큼 가까워지면 흡수된다. 본체 콜라이더보다 넉넉하게 잡는다.")]
    [SerializeField, Min(0.1f)] float _absorbRadius = 1.5f;

    [Header("Capture - 흡수된 뒤 스스로 도는 궤도")]
    [Tooltip("스킬의 고리와 다른 궤도다. 스킬이 돌고 있지 않을 때 흡수된 창이 혼자 도는 자리이고, " +
             "다음 시전이 여기서 집어 간다. 그때부터는 스킬이 적어둔 반지름을 따른다.")]
    [SerializeField, Min(0.5f)] float _captureRadius = 2.5f;

    [Tooltip("보스 발치에서 그 궤도까지의 높이.")]
    [SerializeField] float _captureHeight = 1.5f;

    [Tooltip("도는 각속도(도/초). 음수면 반대로 돈다.")]
    [SerializeField] float _captureSpin = 90f;

    #region Registry - 물건이 스스로 이름을 올린다

    static readonly List<Boss_Throwable> _all = new List<Boss_Throwable>();

    public static IReadOnlyList<Boss_Throwable> All => _all;

    void OnEnable()
    {
        // 서로를 무시하는 짝을 먼저 맺고 명부에 오른다.
        // 그래야 자기 자신과 짝을 맺으려 들지 않는다.
        IgnoreAlreadyRegistered();

        if (!_all.Contains(this)) _all.Add(this);
        if (Projectile != null)
        {
            Projectile.onSpent += OnProjectileSettled;
            Projectile.onEmbedded += OnProjectileSettled;
        }
    }

    void OnDisable()
    {
        _all.Remove(this);
        if (Projectile != null)
        {
            Projectile.onSpent -= OnProjectileSettled;
            Projectile.onEmbedded -= OnProjectileSettled;
        }
    }

    #endregion

    #region Mutual Collision - 탄환끼리는 서로를 통과한다

    Collider[] _colliders;

    Collider[] Colliders
    {
        get
        {
            if (_colliders == null) _colliders = GetComponentsInChildren<Collider>(true);
            return _colliders;
        }
    }

    /// <summary>
    /// 이미 명부에 오른 것들과 충돌을 끊는다. 새로 켜지는 쪽이 매번 이걸 하므로
    /// 몇 개가 언제 들어오든 모든 짝이 한 번씩 맺어진다 — 무시는 양방향이라 한 번이면 족하다.
    /// </summary>
    void IgnoreAlreadyRegistered()
    {
        if (!_ignoreOtherThrowables) return;

        foreach (Boss_Throwable other in _all)
        {
            if (other == null || other == this || !other._ignoreOtherThrowables) continue;

            foreach (Collider mine in Colliders)
            {
                if (mine == null) continue;

                foreach (Collider theirs in other.Colliders)
                {
                    if (theirs == null) continue;
                    Physics.IgnoreCollision(mine, theirs, true);
                }
            }
        }
    }

    #endregion

    public Phase phase { get; private set; } = Phase.Idle;

    /// <summary>지금 집을 수 있는지. 이미 다른 스킬이 들고 있으면 거짓.</summary>
    /// <summary>
    /// 집을 수 있는지. 흡수되어 혼자 도는 것도 포함한다 —
    /// 그 상태의 뜻이 곧 "보스가 다음에 쓸 수 있는 물건"이기 때문이다.
    /// </summary>
    public bool Available => phase == Phase.Idle || phase == Phase.Captured;

    Boss_Projectile _projectile;

    Boss_Projectile Projectile
    {
        get
        {
            if (_projectile == null) _projectile = GetComponent<Boss_Projectile>();
            return _projectile;
        }
    }

    /// <summary>
    /// 지금 창이 <b>쉬고 있는 자리</b>. 처음에는 씬에 놓인 자리이고, 이후로는 박힌 자리로 갱신된다.
    ///
    /// 처음 자리를 영구히 붙들면 회수가 아니라 리스폰이 된다 —
    /// 창이 꽂힌 자리에서 사라졌다가 저 멀리 원래 자리에 다시 나타난다.
    /// 박힌 곳을 새 쉼터로 삼으면, 보스는 눈에 보이는 그 자리에서 끌어올린다.
    /// </summary>
    Vector3 _rest;
    Quaternion _restRotation;

    /// <summary>이 시각이 되면 회수된다. 발사 때 맞춰지고, 멈춘 곳과 무관하게 흐른다.</summary>
    float _recoverAt;
    float _health;

    /// <summary>집어 든 쪽. 쳐냈을 때 되돌려 보낼 방향이다.</summary>
    Transform _thrower;

    // 겨누는 동안 들고 있는 발사 지시. 시간이 다 되면 그대로 쏜다.
    Transform _pendingTarget;
    float _pendingDamage;
    float _pendingSpeed;
    float _telegraphTimer;

    // 들려 있는 동안의 목표 자리. 보스가 매 스텝 갱신한다.
    Vector3 _slot;
    float _followSpeed;

    void Awake()
    {
        if (_body == null) _body = GetComponent<Rigidbody>();

        _rest = transform.position;
        _restRotation = transform.rotation;
        _followSpeed = _defaultFollowSpeed;
        _health = _maxHealth;

        EnsureParryZone();
    }

    /// <summary>
    /// 받아치기 영역을 자식으로 마련한다. 물건마다 손으로 달게 하지 않기 위해서다 —
    /// 이 부품 하나 붙이면 끝이라는 규칙을 여기서도 지킨다.
    /// 이미 달려 있으면 반경만 맞춘다.
    /// </summary>
    void EnsureParryZone()
    {
        if (_parryRadius <= 0f) return;

        Boss_ParryZone zone = GetComponentInChildren<Boss_ParryZone>(true);

        if (zone == null)
        {
            GameObject go = new GameObject("ParryZone");
            go.transform.SetParent(transform, false);
            zone = go.AddComponent<Boss_ParryZone>();
        }

        // 창이 길쭉하게 눌려 있으면 자식도 함께 눌린다. 반경은 월드에서 재야 뜻이 맞는다.
        Vector3 scale = transform.lossyScale;
        float widest = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

        zone.transform.localScale = widest > 0.0001f
            ? new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z)
            : Vector3.one;

        zone.Radius = _parryRadius;
    }

    #region Verbs - 스킬이 부르는 것들

    /// <summary>집어 든다. 물리를 재우고 자리 지정을 받기 시작한다.</summary>
    public void Grab(Transform thrower)
    {
        if (!Available) return;

        // 혼자 돌던 것을 스킬이 가져간다. 중심을 놓아야 Circle이 다시 끼어들지 않는다.
        _absorbTarget = null;

        phase = Phase.Held;
        _thrower = thrower;
        _slot = transform.position;
        _health = _maxHealth;

        Projectile.SetIgnore(thrower);
        SleepBody();
    }

    /// <summary>들려 있는 동안의 목표 자리. 매 스텝 넘긴다.</summary>
    public void SetSlot(Vector3 position, float followSpeed)
    {
        _slot = position;
        _followSpeed = Mathf.Max(0.1f, followSpeed);
    }

    /// <summary>그 자리에서 즉시 잡는다. 고리를 짤 때처럼 미끄러짐 없이 배치할 때 쓴다.</summary>
    public void SnapTo(Vector3 position)
    {
        _slot = position;
        transform.position = position;
    }

    /// <summary>들려 있는 채로 스치면 아프게 한다. 공전 고리가 그 자체로 위험해지는 것.</summary>
    public void SetHarmful(bool on, float damage, float rehitDelay)
    {
        Projectile.SetContactDamage(on, damage, rehitDelay);
    }

    /// <summary>
    /// 던지기로 정한다. 겨냥은 발사체가 자기 규칙으로 잡으므로
    /// 여기서 하는 일은 "이 대상을 향해 쏜다"고 정하는 것뿐이다.
    ///
    /// 곧바로 날리지 않고 <see cref="Phase.Aiming"/>을 한 번 거친다 —
    /// 예고 없이 날아오는 것은 반응이 아니라 암기가 되기 때문이다.
    /// </summary>
    public void Throw(Transform target, float damage, float speedOverride = 0f)
    {
        if (phase == Phase.Aiming || phase == Phase.Flying || phase == Phase.Stuck) return;

        _pendingTarget = target;
        _pendingDamage = damage;
        _pendingSpeed = speedOverride;

        // 겨눌 시간을 두지 않기로 했으면 예전처럼 즉시 나간다.
        if (_telegraphTime <= 0f) { Fire(); return; }

        phase = Phase.Aiming;
        _telegraphTimer = _telegraphTime;

        // 바닥에 놓인 것을 곧바로 던지는 경우를 위해 여기서도 물리를 넘겨받는다.
        SleepBody();
    }

    void Fire()
    {
        phase = Phase.Flying;

        // 회수 시계는 <b>손을 떠나는 순간</b> 시작한다. 어디에 어떻게 멈추든 상관없이 흐른다.
        //
        // 멈춘 뒤부터 재면, 빗나가 멀리 날아간 창은 늦게 멈추는 만큼 늦게 돌아오고
        // 되받아쳐 보스를 지나친 창은 아예 돌아오지 못한다 — 결과가 나쁠수록 손해가 커진다.
        // 발사부터 재면 던진 순간에 이미 다음이 정해지고, 발사가 순차적이므로 회수도 순차가 된다.
        _recoverAt = Time.time + _respawnDelay;

        Projectile.SetContactDamage(false, 0f, 0f);
        Projectile.Arm(transform.position, _pendingTarget, _thrower, _pendingDamage, _pendingSpeed);
        Projectile.Fire();
    }

    /// <summary>
    /// 들고 있던 것을 놓는다. 공전이 끝날 때처럼 던지지 않고 끝낼 때.
    ///
    /// 물리에 맡겨 떨어뜨리지 않고 <b>아래로 쏜다</b>. 떨어지는 것도 날아가는 것과 같은 길을 지나
    /// 땅에 박히므로, 끝나는 모습이 한 가지로 모이고 리지드바디도 필요 없어진다.
    /// </summary>
    public void Release()
    {
        if (phase != Phase.Held) return;

        phase = Phase.Flying;

        Projectile.SetContactDamage(false, 0f, 0f);
        Projectile.Arm(transform.position, null, _thrower, 0f, _dropSpeed);
        Projectile.FireAlong(Vector3.down);
    }

    /// <summary>
    /// 스킬이 중간에 끊겼을 때. 들고 있던 것을 <b>집어 올린 자리</b>로 되돌린다.
    ///
    /// 이미 겨누고 있던 것은 돌려보내지 않고 그대로 쏜다.
    /// 표적을 향해 돌아서고 물러나는 것까지 보여준 뒤에 사라지면, 플레이어가 읽은 예고가 거짓이 된다.
    /// 스킬의 후딜이 짧아 마지막 하나가 겨누는 도중에 끝나는 경우가 바로 이것이다.
    /// </summary>
    public void Cancel()
    {
        if (phase == Phase.Aiming) { Fire(); return; }
        if (phase != Phase.Held) return;

        Projectile.Disarm();
        transform.SetPositionAndRotation(_rest, _restRotation);

        phase = Phase.Idle;
        _thrower = null;
        _health = _maxHealth;
        _followSpeed = _defaultFollowSpeed;
    }

    #endregion

    #region Parry - 주인공이 쳐낸다

    /// <summary>
    /// 지금 쳐낼 수 있는 상태인지. 국면마다 따로 켤 수 있게 해 두었다 —
    /// 날아오는 것만 쳐내게 할지, 도는 고리도 깨게 할지가 이 보스의 난이도를 크게 가른다.
    /// </summary>
    bool Parryable => phase switch
    {
        Phase.Held => _parryWhileHeld,
        // 겨누는 구간은 떠 있는 것과 같이 본다. 예고를 보고 먼저 치는 것이 이 구간의 값어치다.
        Phase.Aiming => _parryWhileHeld,
        Phase.Flying => _parryWhileFlying,
        // 박혀 있는 것도 쳐낼 수 있다. 회수를 기다리는 창이 곧 플레이어의 무기가 된다.
        Phase.Stuck => _parryWhileStuck,
        // 되찾긴 창은 쳐내도 다시 되찾기므로, 떠 있는 것과 같은 취급으로 둔다.
        Phase.Captured => _parryWhileIdle,
        Phase.Idle => _parryWhileIdle,
        _ => false,
    };

    /// <summary>
    /// 주인공의 무기가 이걸 부른다. 다 깎이면 쳐내진다.
    /// 체력은 집힐 때마다 다시 채워지므로, "몇 대 쳐야 깨지는가"는 한 번의 시전 안에서만 유효하다.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (!Parryable) return;

        _health -= damageAmount;
        if (_health > 0f) return;

        Deflect();
    }

    /// <summary>
    /// 집어 든 쪽으로 되돌려 보낸다. <b>무엇이 되어 돌아가는지는 보스가 지금 무엇을 하고 있느냐가 정한다.</b>
    ///
    ///   창을 돌리는 중  때린다. 커밋에 들어간 보스는 손을 뺄 수 없으므로 그대로 맞는다.
    ///   그렇지 않으면    되찾긴다. 궤도를 틀어 보스를 돌기 시작하고, 다음 공격에 같이 나간다.
    ///
    /// 둘 다 보스를 향해 <b>휘어서</b> 간다. 어느 쪽이든 닿는 것이 전제이기 때문이다.
    ///
    /// 이 갈림이 쳐내기의 값어치를 정한다 — 아무 때나 쳐내면 탄을 돌려주는 셈이고,
    /// 보스가 고리를 돌리는 그 구간에 쳐내야 피해가 된다.
    ///
    /// 겨냥만 갈아끼우면 되는 것이 발사체가 타겟을 Transform으로 쥔 덕이다.
    /// 무시 대상도 함께 뒤집혀 쳐낸 사람은 자기 것에 맞지 않는다.
    /// </summary>
    void Deflect()
    {
        Transform player = PlayerManager.instance != null ? PlayerManager.instance.player : null;

        // 지금 물건을 들고 있는 스킬이 있는지가 곧 "창을 돌리고 있는가"다.
        bool spinning = Boss_Telekinesis.Holding != null;

        _absorbTarget = DeflectTarget;
        _absorbing = !spinning && _absorbTarget != null;

        phase = Phase.Flying;
        Projectile.SetContactDamage(false, 0f, 0f);

        // <b>어느 쪽이든 선회를 싣는다.</b> 되찾기는 닿아야 성립하고, 때리는 쪽은 보스가
        // 취약한 그 구간에 노린 것이므로 빗나가면 안 된다 - 커밋에 들어간 보스를 벌하는 것이
        // 이 구간의 값어치인데, 곧게만 날아가면 보스가 조금 움직인 것만으로 그 값어치가 사라진다.
        Projectile.Redirect(_absorbTarget, player != null ? player.root : null,
                            spinning ? _deflectDamage : 0f,
                            _deflectSpeed, _deflectSpeedCurve, _deflectHoming);
    }

    Transform _absorbTarget;
    bool _absorbing;

    /// <summary>
    /// 다 왔는지 본다. 닿는 판정을 발사체의 타격에 맡기지 않는 것은,
    /// 흡수는 맞히는 일이 아니라 도착하는 일이기 때문이다 — 관통 설정이나 콜라이더 모양에 딸리면 안 된다.
    /// </summary>
    void CloseIn()
    {
        if (_absorbTarget == null) { _absorbing = false; return; }

        if ((transform.position - _absorbTarget.position).sqrMagnitude > _absorbRadius * _absorbRadius) return;

        Absorb();
    }

    /// <summary>
    /// 보스에게 되찾긴다.
    ///
    /// 날아오는 사이에 고리가 돌기 시작했다면 그 자리에 합류한다. 아니면 스스로 돈다 —
    /// 떠 있기만 해도 다음 시전이 집어 가지만, 도는 편이 "되찾겼다"가 눈에 읽힌다.
    /// </summary>
    void Absorb()
    {
        _absorbing = false;

        MakeAvailable();

        Boss_Telekinesis holding = Boss_Telekinesis.Holding;
        if (holding != null && holding.Absorb(this)) { _absorbTarget = null; return; }

        Capture();
    }

    /// <summary>
    /// 혼자 도는 궤도에 올린다. 시작 각도는 <b>도착한 자리</b>에서 딴다 —
    /// 0도에서 시작하면 삼켜지자마자 반대편으로 튀는 것이 보인다.
    /// </summary>
    void Capture()
    {
        if (_absorbTarget == null) return;

        phase = Phase.Captured;

        Vector3 offset = transform.position - _absorbTarget.position;
        offset.y = 0f;

        _captureAngle = offset.sqrMagnitude < 0.0001f
            ? 0f
            : Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
    }

    float _captureAngle;

    /// <summary>
    /// 보스 주위를 돈다. 스킬의 고리와 달리 자리를 나눠 가질 상대가 없으므로 각도를 혼자 굴린다.
    /// 보스가 움직이면 중심도 따라 움직인다.
    /// </summary>
    void Circle()
    {
        if (_absorbTarget == null) { phase = Phase.Idle; return; }

        _captureAngle += _captureSpin * Time.fixedDeltaTime;

        float angle = _captureAngle * Mathf.Deg2Rad;
        Vector3 centre = _absorbTarget.position + Vector3.up * _captureHeight;
        Vector3 want = centre + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _captureRadius;

        transform.position = Vector3.Lerp(transform.position, want,
            1f - Mathf.Exp(-_followSpeed * Time.fixedDeltaTime));
    }

    /// <summary>
    /// 되돌려 보낼 곳. 방금 던진 쪽이 있으면 그쪽이다.
    ///
    /// 땅에 박힌 창은 던진 쪽을 이미 잊었으므로 — 회수 대기 중인 창을 누가 던졌는지는
    /// 더 이상 의미가 없다 — 그때는 지금 살아 있는 보스를 찾는다.
    /// 이게 없으면 꽂힌 창을 쳐냈을 때 겨눌 곳이 없어 앞으로만 날아간다.
    /// </summary>
    Transform DeflectTarget
    {
        get
        {
            if (_thrower != null) return _thrower;

            BossManager boss = BossManager.Current;
            return boss != null ? boss.character : null;
        }
    }

    #endregion

    /// <summary>
    /// 비행이 끝났다. 땅에 박혔든 시간이 다 됐든, 창은 그 자리에 그대로 선다.
    ///
    /// 물리를 돌려주지 않는다. 박힌 창은 뽑히기 전까지 움직일 이유가 없고,
    /// 움직이지 않는다면 리지드바디도 필요 없다 — 지금 창에 리지드바디가 없는 이유가 이것이다.
    /// </summary>
    void OnProjectileSettled()
    {
        if (phase != Phase.Flying) return;

        Settle();
    }

    void FixedUpdate()
    {
        if (phase == Phase.Held) Follow();
        else if (phase == Phase.Captured) Circle();
        else if (phase == Phase.Aiming) Aim();
        else if (phase == Phase.Flying && _absorbing) CloseIn();
        else if (phase == Phase.Stuck) CountToRecovery();
    }

    void Follow()
    {
        // 지정한 자리로 부드럽게 따라붙는다. 즉시 옮기면 고리가 딱딱해 보인다.
        transform.position = Vector3.Lerp(transform.position, _slot,
            1f - Mathf.Exp(-_followSpeed * Time.fixedDeltaTime));
    }

    /// <summary>
    /// 표적을 향해 돌아서고 뒤로 물러난다. 이 두 동작이 곧 예고다 —
    /// 어디서 무엇이 오는지가 자세로 읽혀야 피할 수 있다.
    /// </summary>
    void Aim()
    {
        Vector3 direction = AimDirection();

        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(direction, Vector3.up),
            1f - Mathf.Exp(-_aimTurnSpeed * Time.fixedDeltaTime));

        // 고리 자리에서 표적 반대쪽으로 당긴다. 고리가 계속 돌면 이 자리도 따라 돈다.
        Vector3 want = _slot - direction * _telegraphPullback;
        transform.position = Vector3.Lerp(transform.position, want,
            1f - Mathf.Exp(-_followSpeed * Time.fixedDeltaTime));

        _telegraphTimer -= Time.fixedDeltaTime;
        if (_telegraphTimer <= 0f) Fire();
    }

    Vector3 AimDirection()
    {
        if (_pendingTarget == null) return transform.forward;

        Vector3 direction = _pendingTarget.position + Vector3.up - transform.position;
        return direction.sqrMagnitude < 0.0001f ? transform.forward : direction.normalized;
    }

    #region Lifecycle

    /// <summary>
    /// 멈춰 선다. 지금 자리가 곧 새 쉼터다.
    /// 던진 쪽은 지운다 — 다음에 이 창을 집는 것이 같은 보스라는 보장이 없다.
    /// </summary>
    void Settle()
    {
        phase = Phase.Stuck;
        _thrower = null;

        // 흡수되지 못하고 어딘가에 박혔다. 표시를 지워야 다음 비행이 이 시계를 이어받지 않는다.
        _absorbing = false;
        _absorbTarget = null;

        _rest = transform.position;
        _restRotation = transform.rotation;

        Projectile.Disarm();
    }

    /// <summary>
    /// 회수 시각이 됐는지 본다. 시계는 발사 때 이미 맞춰졌으므로 여기서는 보기만 한다.
    /// 늦게 멈춘 창은 그만큼 짧게 꽂혀 있다가 돌아가고, 시각이 이미 지났으면 곧바로 돌아간다.
    /// </summary>
    void CountToRecovery()
    {
        if (_respawnDelay <= 0f) return;
        if (Time.time < _recoverAt) return;

        MakeAvailable();
    }

    /// <summary>
    /// 다시 집을 수 있게 된다. <b>자리는 그대로 둔다</b> —
    /// 박힌 곳에서 보스가 끌어올리는 것이 회수이지, 원래 자리로 되돌리는 것은 리스폰이다.
    /// 끌어올리는 모습은 집힌 뒤 고리로 따라붙는 동안 저절로 만들어진다.
    /// </summary>
    void MakeAvailable()
    {
        phase = Phase.Idle;
        _thrower = null;
        _health = _maxHealth;
        _followSpeed = _defaultFollowSpeed;

        Projectile.Disarm();
    }

    #endregion

    #region Physics - 창은 물리를 쓰지 않는다

    /// <summary>
    /// 리지드바디가 붙어 있으면 재운다. 붙어 있지 않으면 아무 일도 하지 않는다.
    ///
    /// 창의 자리는 처음부터 끝까지 이 부품과 발사체가 정한다 — 떠 있는 동안에도, 날아가는 동안에도,
    /// 박힌 뒤에도. 물리가 손댈 구간이 없으므로 <b>창에는 리지드바디가 필요 없다</b>.
    ///
    /// 그럼에도 남겨둔 것은, 이 부품을 물리로 굴러다니던 소품에 붙이는 경우 때문이다.
    /// 그때 재우지 않으면 중력이 위치를 계속 밀어 스크립트와 다툰다.
    /// 돌려주는 짝이 없는 것은 의도다 — 한 번 창이 된 것은 다시 물리로 돌아가지 않는다.
    /// </summary>
    void SleepBody()
    {
        if (_body == null) return;

        // 키네마틱으로 넘어가기 전에 지운다. 넘어간 뒤에는 속도 대입이 무시된다.
        _body.linearVelocity = Vector3.zero;
        _body.angularVelocity = Vector3.zero;

        _body.useGravity = false;
        _body.isKinematic = true;
    }

    #endregion
}
