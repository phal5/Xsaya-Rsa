using System;
using UnityEngine;

/// <summary>
/// 보스의 이동 동사 계층. 상태는 여기에만 말을 걸고, 실제 속도 조작은 SingularFlatMovement가 한다.
/// Character_Steering이 플레이어에게 하는 역할과 같다.
///
/// 아주 높은 점프를 다루기 위해 네 가지를 책임진다.
///  - 접지 판정      : 긴 체공 동안 지상 로직이 끼어들지 않게 한다
///  - 중력 배율      : 상승과 하강에 다른 배율을 걸어, 높이는 살리면서 체공 시간만 줄인다
///  - 높이 기준 API  : 속도가 아니라 "몇 미터"로 점프를 지시한다
///  - 체공 시간 예측 : 스킬이 자기 구간 길이를 점프에 맞출 수 있게 한다
/// </summary>
public class Boss_Steering : MonoBehaviour
{
    [Header("Backend")]
    [SerializeField] SingularFlatMovement _movement;
    [SerializeField] Rigidbody _rigidbody;
    [Tooltip("접지 판정용. 지면 노멀을 읽는 RayCaster와는 별개다.")]
    [SerializeField] SphereCaster _groundCaster;

    [Header("Air Control")]
    [Tooltip("공중에서 수평 조종이 먹히는 비율. 0이면 이륙 시 궤도가 그대로 유지된다.")]
    [SerializeField, Range(0f, 1f)] float _airControl = 0.3f;

    [Header("Jump")]
    [Tooltip("상승 구간 중력 배율. 낮출수록 같은 초기속도로 더 높이 뜬다.")]
    [SerializeField, Min(0.1f)] float _riseGravityScale = 1f;
    [Tooltip("하강 구간 중력 배율. 높이면 도달 높이는 그대로 두고 떨어지는 시간만 줄일 수 있다.")]
    [SerializeField, Min(0.1f)] float _fallGravityScale = 2.5f;
    [Tooltip("이륙 직후 접지 판정을 무시하는 시간. 발밑 캐스터가 즉시 착지로 오인하는 걸 막는다.")]
    [SerializeField, Min(0f)] float _takeoffGrace = 0.12f;

    #region State

    public bool Grounded { get; private set; }
    public bool Airborne => !Grounded;

    public float VerticalVelocity => _movement != null ? _movement.VerticalVelocity : 0f;

    public bool Rising => Airborne && VerticalVelocity > 0.01f;
    public bool Falling => Airborne && VerticalVelocity < -0.01f;

    /// <summary>이륙 지점 기준 현재 높이. 착지 상태에서는 0.</summary>
    public float HeightAboveTakeoff => Airborne ? _rigidbody.position.y - _takeoffY : 0f;

    /// <summary>점프로 떠 있는 중인지. 낙하(발판에서 걸어 나감)와 구분된다.</summary>
    public bool Jumping { get; private set; }

    #endregion

    #region Events

    public event Action onTakeoff;
    /// <summary>상승이 끝나는 순간. 내려찍기 전환 지점으로 쓴다.</summary>
    public event Action onApex;
    public event Action onLand;

    #endregion

    float _takeoffY;
    float _graceTimer;
    bool _apexReported;

    void Start()
    {
        // 첫 프레임에 착지 이벤트가 헛발질하지 않도록 현재 상태로 초기화한다.
        Grounded = CastGround();
    }

    void FixedUpdate()
    {
        UpdateGrounded();
        ApplyGravityScale();
        ReportApex();
    }

    #region Verbs

    /// <summary>수평 이동 지시. 공중에서는 _airControl 만큼만 먹는다.</summary>
    public void Move(Vector3 velocity)
    {
        if (_movement == null) return;

        _movement.Move(Grounded ? velocity : velocity * _airControl);
    }

    public void Stop()
    {
        if (_movement != null) _movement.Move(Vector3.zero);
    }

    /// <summary>
    /// 수평 속도를 즉시 지정한다. 도약처럼 한 순간에 속도를 실어야 할 때 쓴다.
    /// Move()는 목표만 넘겨 maxAccel로 서서히 붙기 때문에 순간 가속에는 맞지 않는다.
    /// Accelerate가 되돌리지 않도록 목표 속도도 같이 맞춰둔다. 수직 속도는 건드리지 않는다.
    /// </summary>
    public void SetHorizontalVelocity(Vector3 horizontal)
    {
        if (_movement == null || _rigidbody == null) return;

        horizontal.y = 0f;

        Vector3 v = _rigidbody.linearVelocity;
        _rigidbody.linearVelocity = new Vector3(horizontal.x, v.y, horizontal.z);
        _movement.Move(horizontal);
    }

    /// <summary>
    /// 수평 속도를 즉시 0으로 만든다. Stop()은 목표만 0으로 두고 maxAccel로 천천히 줄이므로,
    /// 루트 모션이 이동을 전담하는 동안에는 그 잔여 속도가 발 미끄러짐으로 보인다.
    /// 수직 속도는 중력/점프가 쓰므로 건드리지 않는다.
    /// </summary>
    public void KillHorizontalVelocity()
    {
        if (_movement == null || _rigidbody == null) return;

        _movement.Move(Vector3.zero);

        Vector3 v = _rigidbody.linearVelocity;
        _rigidbody.linearVelocity = new Vector3(0f, v.y, 0f);
    }

    public void LookTowards(Transform target)
    {
        if (_movement != null) _movement.LookTowards(target);
    }

    /// <summary>
    /// 도달 높이를 지정해 점프한다. 속도가 아니라 미터로 지시하므로
    /// "엄청 높이"를 숫자 하나로 조율할 수 있고, 중력 배율을 바꿔도 높이가 유지된다.
    /// </summary>
    public void JumpToHeight(float peakHeight)
    {
        float g = Mathf.Abs(Physics.gravity.y) * _riseGravityScale;
        Jump(Mathf.Sqrt(2f * g * Mathf.Max(peakHeight, 0f)));
    }

    public void Jump(float speed)
    {
        if (_movement == null) return;

        _takeoffY = _rigidbody.position.y;
        _graceTimer = _takeoffGrace;
        Grounded = false;
        Jumping = true;
        _apexReported = false;

        _movement.SetVerticalVelocity(speed);
        onTakeoff?.Invoke();
    }

    /// <summary>공중에서 즉시 내리꽂는다. 점프 후 내려찍기에 쓴다.</summary>
    public void Slam(float downSpeed)
    {
        if (Grounded || _movement == null) return;

        _movement.SetVerticalVelocity(-Mathf.Abs(downSpeed));
    }

    /// <summary>상승을 즉시 끊는다. 점프 도중 경직에 걸렸을 때 등.</summary>
    public void CutRise()
    {
        if (_movement == null) return;
        if (VerticalVelocity > 0f) _movement.SetVerticalVelocity(0f);
    }

    #endregion

    #region Planning

    /// <summary>
    /// 주어진 높이로 점프했을 때의 예상 체공 시간.
    /// 점프형 스킬이 준비/발동 구간을 실제 궤도에 맞출 때 쓴다.
    /// </summary>
    public float EstimateAirTime(float peakHeight)
    {
        if (peakHeight <= 0f) return 0f;

        float g = Mathf.Abs(Physics.gravity.y);
        float up = Mathf.Sqrt(2f * peakHeight / (g * _riseGravityScale));
        float down = Mathf.Sqrt(2f * peakHeight / (g * _fallGravityScale));
        return up + down;
    }

    /// <summary>
    /// 지금 높이와 수직 속도로부터 착지까지 남은 시간.
    /// EstimateAirTime은 자유낙하 기준이라 Slam으로 하강 속도를 실은 뒤에는 맞지 않는다.
    /// 이륙 지점과 착지 지점의 높이가 같다고 가정한다.
    /// </summary>
    public float EstimateTimeToLand()
    {
        float h = HeightAboveTakeoff;
        if (h <= 0f) return 0f;

        float a = Mathf.Abs(Physics.gravity.y) * _fallGravityScale;
        if (a <= 0f) return 0f;

        float v0 = Mathf.Max(-VerticalVelocity, 0f);   // 하강 속도(양수). 상승 중이면 0으로 본다.

        return (-v0 + Mathf.Sqrt(v0 * v0 + 2f * a * h)) / a;
    }

    /// <summary>주어진 높이까지 올라가는 데 걸리는 시간.</summary>
    public float EstimateRiseTime(float peakHeight)
    {
        if (peakHeight <= 0f) return 0f;
        return Mathf.Sqrt(2f * peakHeight / (Mathf.Abs(Physics.gravity.y) * _riseGravityScale));
    }

    #endregion

    #region Internals

    bool CastGround()
    {
        return _groundCaster != null && _groundCaster.Cast(out _);
    }

    void UpdateGrounded()
    {
        if (_graceTimer > 0f)
        {
            _graceTimer -= Time.fixedDeltaTime;
            Grounded = false;
            return;
        }

        bool grounded = CastGround();

        if (grounded && !Grounded)
        {
            Jumping = false;
            _apexReported = false;
            Grounded = true;
            onLand?.Invoke();
            return;
        }

        Grounded = grounded;
    }

    void ApplyGravityScale()
    {
        if (Grounded) return;

        float scale = VerticalVelocity > 0f ? _riseGravityScale : _fallGravityScale;
        if (Mathf.Approximately(scale, 1f)) return;

        // 내장 중력 위에 차이분만 얹는다. Rigidbody.useGravity는 켜둔 채로 둔다.
        _movement.AddVelocity(Physics.gravity * ((scale - 1f) * Time.fixedDeltaTime));
    }

    void ReportApex()
    {
        // 발판에서 걸어 나간 낙하는 정점이 없다. 명시적으로 뛴 경우만 본다.
        if (!Jumping || _apexReported) return;
        if (VerticalVelocity > 0f) return;

        _apexReported = true;
        onApex?.Invoke();
    }

    #endregion
}
