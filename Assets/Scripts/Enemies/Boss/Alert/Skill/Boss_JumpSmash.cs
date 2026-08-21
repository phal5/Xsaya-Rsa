using UnityEngine;

/// <summary>
/// 뛰어올라 낙하하며 내리꽂는 스킬.
///
/// 궤적은 물리가 아니라 <b>두 점 사이의 보간</b>이다. 중력을 흉내 내면 높이·중력 배율·하강 속도가
/// 서로 얽혀 착지 시각이 흔들리는데, 여기서는 시간이 곧 진행도라 착지 시각이 정확히 정해진다.
/// 대신 밀려나거나 걸리는 일이 없다 — 이 스킬이 도는 동안 몸은 스크립트가 완전히 쥔다.
///
/// 네 단계로 진행한다. 애니메이션은 캐스팅 한 클립의 재생 속도만 조작해서 만든다.
///
///   1 준비 : 크라우치를 정방향으로 재생. 아직 뛰지 않는다.        [Windup]
///   2 도약 : 재생이 끝나면 도약을 시작하며 캐스팅 25프레임으로 블렌드.
///   3 내려치기 : 절반 지점(정점)에서 그때의 플레이어 자리를 착지점으로 굳히고,
///            착지 시각에 64프레임(타격)이 오도록 재생을 푼다.      [Active]
///   4 마무리 : 90프레임에서 배속을 1로 낮추고 클립이 끝나면 종료.  [Recovery]
///
/// 골격의 구간 대응
///   windupTime   = 크라우치 길이
///   activeTime   = 체공 시간 (도약 → 착지)
///   recoveryTime = 착지 후 남은 재생 시간
/// </summary>
public class Boss_JumpSmash : Boss_SkillBase
{
    [Header("Jump Smash")]
    [Tooltip("포물선의 최고 높이(미터). 궤적의 생김새만 정한다.")]
    [SerializeField, Min(0f)] float _arcHeight = 5f;

    [Tooltip("도약으로 덮을 수 있는 최대 수평 거리. 이보다 먼 곳은 여기까지만 간다.")]
    [SerializeField, Min(0f)] float _maxLeapDistance = 8f;

    [Tooltip("착지점을 굳히는 시점. 0.5면 체공 절반(정점)에서 그때의 플레이어 자리를 노린다.")]
    [SerializeField, Range(0f, 1f)] float _commitAt = 0.5f;

    [Header("Jump Smash - Animation")]
    [Tooltip("준비 동작(크라우치) 트리거.")]
    [SerializeField] string _crouchTrigger = "Jump";

    [Tooltip("캐스팅 트리거. 컨트롤러에서 25프레임 지점으로 진입하도록 offset을 걸어둔다.")]
    [SerializeField] string _slamTrigger = "JumpSmash";

    [Tooltip("JumpSlam 상태의 Speed Multiplier로 걸어둔 Float 파라미터 이름.")]
    [SerializeField] string _slamSpeedParameter = "SlamSpeed";

    [Header("Jump Smash - Animation Frames")]
    [Tooltip("캐스팅 클립의 프레임레이트.")]
    [SerializeField, Min(1f)] float _clipFrameRate = 30f;

    [Tooltip("캐스팅 클립의 총 프레임 수. AnimatorStateInfo.length는 배속으로 나뉘어 배속 0에서 무한대가 된다.")]
    [SerializeField, Min(1f)] float _clipTotalFrames = 144f;

    [Tooltip("체공 중 붙잡아 둘 프레임.")]
    [SerializeField, Min(0f)] float _holdFrame = 25f;

    [Tooltip("착지 겸 타격이 일어날 프레임.")]
    [SerializeField, Min(0f)] float _impactFrame = 64f;

    [Tooltip("이 프레임에 닿으면 배속을 1로 되돌린다.")]
    [SerializeField, Min(0f)] float _settleFrame = 90f;

    [Tooltip("내려치는 구간의 배속.")]
    [SerializeField, Min(0.1f)] float _slamSpeed = 2f;

    // 궤적
    Vector3 _start;
    Vector3 _target;
    float _flightStart;
    bool _flying;
    bool _committed;

    // 애니메이션 진행 표시
    bool _slamStarted;
    bool _settled;

    float FlightTime => Profile.activeTime;

    /// <summary>홀드에서 타격까지 실제로 걸리는 시간. 착지에서 이만큼 거슬러 올라가 재생을 푼다.</summary>
    float SlamLead => (_impactFrame - _holdFrame) / _clipFrameRate / _slamSpeed;

    public override void Enter()
    {
        _flying = false;
        _committed = false;
        _slamStarted = false;
        _settled = false;

        SetSlamSpeed(0f);   // 캐스팅에 들어가면 곧바로 멈춰 있도록 미리 걸어둔다
        base.Enter();
    }

    #region 1. 준비 — 아직 뛰지 않는다

    protected override void OnWindup()
    {
        manager.LookTowards(manager.Player);
        PlayAnimation(_crouchTrigger);
    }

    #endregion

    #region 2. 도약

    protected override void OnActivate()
    {
        _start = manager.character.position;
        _target = AimPoint();
        _flightStart = Time.time;
        _flying = true;

        // 캐스팅으로 넘어간다. 배속 0이라 25프레임에서 멈춘 채 체공한다.
        PlayAnimation(_slamTrigger);
    }

    /// <summary>
    /// 지금 플레이어가 선 자리. 미래 위치를 예측하지 않으므로 굳힌 뒤에 움직이면 피할 수 있다.
    /// 너무 멀면 갈 수 있는 데까지만 간다.
    /// </summary>
    Vector3 AimPoint()
    {
        if (!manager.HasPlayer) return _start;

        Vector3 flat = CustomMath.RemoveY(manager.Player.position - _start);
        float distance = flat.magnitude;

        if (distance > _maxLeapDistance) flat = flat / distance * _maxLeapDistance;

        return new Vector3(_start.x + flat.x, _start.y, _start.z + flat.z);
    }

    #endregion

    #region 3-4. 비행과 마무리

    public override void UpdateState()
    {
        base.UpdateState();
        if (!_flying) return;

        float t = Mathf.Clamp01((Time.time - _flightStart) / FlightTime);

        // 절반 지점에서 착지점을 굳힌다. 그 전까지는 플레이어를 계속 따라간다.
        if (!_committed && t >= _commitAt)
        {
            _committed = true;
            _target = AimPoint();
        }
        else if (!_committed) _target = AimPoint();

        Fly(t);

        // 착지 SlamLead초 전에 재생을 푼다.
        if (!_slamStarted && (1f - t) * FlightTime <= SlamLead)
        {
            _slamStarted = true;
            SetSlamSpeed(_slamSpeed);
        }

        if (t >= 1f) Land();

        // 지정 프레임에 닿으면 배속을 되돌린다.
        if (_slamStarted && !_settled && ReachedFrame(_settleFrame))
        {
            _settled = true;
            SetSlamSpeed(1f);
        }
    }

    /// <summary>수평은 직선 보간, 수직은 사인 한 봉우리. 시간이 곧 진행도다.</summary>
    void Fly(float t)
    {
        Vector3 position = Vector3.Lerp(_start, _target, t);
        position.y += _arcHeight * Mathf.Sin(t * Mathf.PI);

        manager.character.position = position;
    }

    void Land()
    {
        _flying = false;

        manager.character.position = _target;
        manager.Stop();
    }

    /// <summary>캐스팅이 지정 프레임을 지났는지. 다른 상태로 넘어가 있으면 거짓.</summary>
    bool ReachedFrame(float frame)
    {
        if (manager.animator == null) return false;

        AnimatorStateInfo info = manager.animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName("JumpSlam")) return false;

        // info.length를 쓰면 안 된다. 배속으로 나눈 값이라 SlamSpeed=0에서 무한대가 된다.
        return info.normalizedTime * _clipTotalFrames >= frame;
    }

    void SetSlamSpeed(float speed)
    {
        if (manager.animator == null || string.IsNullOrEmpty(_slamSpeedParameter)) return;
        manager.animator.SetFloat(_slamSpeedParameter, speed);
    }

    #endregion

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        float lead = (_impactFrame - _holdFrame) / _clipFrameRate / _slamSpeed;
        if (lead > Profile.activeTime)
            Debug.LogWarning(
                $"[{name}] 내려치기에 필요한 {lead:0.00}초가 체공 시간 {Profile.activeTime:0.00}초보다 깁니다. " +
                $"activeTime을 늘리거나 배속을 올려주세요.", this);
    }
#endif
}
