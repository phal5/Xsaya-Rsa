using UnityEngine;

/// <summary>
/// 이족 보행의 시계. 개체마다 하나다. 걸음에 필요한 값을 전부 속도 하나에서 뽑는다.
///
/// <b>걷기와 달리기를 가르지 않는다.</b> 둘의 차이는 종류가 아니라 값 하나가
/// 0.5를 지나가는 것이다 — 한 주기 중 발이 땅에 붙어 있는 비율(<see cref="Duty"/>).
///
///   duty &gt; 0.5 : 두 발이 동시에 닿아 있는 구간이 생긴다. 그것을 걷기라 부른다.
///   duty &lt; 0.5 : 두 발이 동시에 떠 있는 구간이 생긴다. 그것을 달리기라 부른다.
///
/// 그래서 전이도, 블렌드 트리도, 임계값을 넘는 순간도 없다. 속도가 duty를 밀 뿐이고
/// 걷기와 달리기는 그 연속선 위의 두 구간에 붙인 <b>이름</b>이다.
///
/// <b>주파수가 먼저고 보폭이 나중이다.</b> 반대로 두면 — 보폭을 정하고 주파수를
/// 속도에서 나누면 — 멈추는 순간 주파수가 0이 되어 시계가 걸음 중간에 선다.
/// 그 자세로 얼어붙는 것이 "자세가 안 돌아오는" 증상이었다. 순서를 뒤집으면
/// 멈춰도 시계는 돌고 보폭만 0으로 줄어, 발이 스스로 뿌리 아래로 모인다.
/// 자세 회복은 따로 붙인 기능이 아니라 이 나눗셈의 방향이다.
/// </summary>
public class BipedalGait : MonoBehaviour
{
    [Tooltip("걸음 속도를 잴 몸통. 위치 변화로 재므로 무엇이 몸을 움직이든 상관없다.")]
    [SerializeField] Transform _body;

    [Space(10f)]
    [Tooltip("1m/s에서의 보폭. 이 상수가 속도와 회전수의 관계를 정한다.")]
    [SerializeField] float _baseStride = 1.1f;

    [Tooltip("보폭의 상한. 반이 곧 한 걸음이므로 다리가 닿을 수 있는 거리의 두 배를 넘으면 안 된다. "
           + "여기 걸리면 남은 속도가 보폭 대신 회전수로 간다 — 다리가 짧은 몸이 빨리 달릴 때 실제로 그렇다.")]
    [SerializeField] float _maxStride = 1.5f;

    [Tooltip("멈춰도 시계가 도는 최소 속도. 걸음이 중간에 얼지 않고 끝까지 가서 자세가 돌아온다. "
           + "회복에 걸리는 시간이 이 값의 역수다.")]
    [SerializeField] float _idleFrequency = 0.6f;

    [Space(10f)]
    [Tooltip("멈춰 있을 때의 접지 비율. 0.5보다 커야 걷기가 된다.")]
    [Range(0.5f, 0.9f)][SerializeField] float _standingDuty = 0.68f;
    [Tooltip("전력 질주에서의 접지 비율. 0.5보다 작아야 달리기가 된다.")]
    [Range(0.1f, 0.5f)][SerializeField] float _sprintDuty = 0.32f;
    [Tooltip("이 속도에서 sprintDuty에 닿는다. 걷기/달리기 경계를 옮기는 손잡이다 — TransitionSpeed 참조.")]
    [SerializeField] float _sprintSpeed = 4f;

    /// <summary>주기 안의 위치. 0에서 1을 돌고 다시 0이다. 이 개체의 유일한 걸음 상태다.</summary>
    public float Phase { get; private set; }

    /// <summary>수평 속도. 뛰어오르거나 떨어지는 것은 걸음이 아니므로 수직은 버린다.</summary>
    public Vector3 Velocity { get; private set; }
    public Vector3 Direction { get; private set; }
    public float Speed { get; private set; }

    /// <summary>
    /// 초당 주기 수. 세 요구를 모두 만족하는 가장 느린 값이다.
    /// 셋 다 "적어도 이만큼은 빨라야 한다"는 형태라 최댓값 하나로 합쳐진다.
    ///
    ///   √v / baseStride : 속도에 어울리는 자연스러운 회전수
    ///   v / maxStride   : 보폭이 다리가 닿는 거리를 넘지 않게
    ///   idleFrequency   : 멈춰도 시계가 서지 않게
    /// </summary>
    public float CycleFrequency => Mathf.Max(
        Mathf.Max(Mathf.Sqrt(Speed) / _baseStride, Speed / _maxStride), _idleFrequency);

    /// <summary>
    /// 한 주기에 나아가는 거리. 속도를 회전수로 나눈 것이라 <b>정의상 발이 미끄러지지 않는다</b>.
    /// 회전수가 0이 될 수 없으니 나눗셈을 지킬 것도 없고, 멈추면 0이 되어 발이 뿌리 아래로 모인다.
    /// </summary>
    public float Stride => Speed / CycleFrequency;

    /// <summary>한 걸음의 길이.</summary>
    public float StepLength => 0.5f * Stride;

    /// <summary>한 주기 중 발 하나가 땅에 붙어 있는 비율. 이 값 하나가 걷기와 달리기를 가른다.</summary>
    public float Duty => Mathf.Lerp(_standingDuty, _sprintDuty, Mathf.Clamp01(Speed / _sprintSpeed));

    /// <summary>한 주기 중 두 발이 모두 떠 있는 비율. 걷기에서는 0이다.</summary>
    public float FlightFraction => Mathf.Max(0f, 1f - 2f * Duty);

    /// <summary>한 주기 중 두 발이 모두 닿아 있는 비율. 달리기에서는 0이다.</summary>
    public float DoubleSupportFraction => Mathf.Max(0f, 2f * Duty - 1f);

    /// <summary>
    /// 달리기다움. -1은 완전한 걷기, +1은 완전한 달리기, 0은 그 경계다.
    /// <c>FlightFraction - DoubleSupportFraction</c>과 같다 — 둘 중 하나는 늘 0이므로.
    /// </summary>
    public float Airborneness => 1f - 2f * Duty;

    /// <summary>
    /// duty가 0.5를 지나는 속도. 설정하는 값이 아니라 위 세 값에서 따라 나온다.
    /// 경계를 S로 옮기고 싶으면 sprintSpeed를 <c>S · (standingDuty - sprintDuty) / (standingDuty - 0.5)</c>로 둔다.
    /// </summary>
    public float TransitionSpeed =>
        _sprintSpeed * Mathf.Clamp01((_standingDuty - 0.5f) / Mathf.Max(0.0001f, _standingDuty - _sprintDuty));

    Vector3 _previousPosition;

    void Start()
    {
        _previousPosition = _body.position;
    }

    void FixedUpdate()
    {
        Vector3 movement = _body.position - _previousPosition;
        _previousPosition = _body.position;

        movement.y = 0f;
        Velocity = movement / Time.fixedDeltaTime;
        Speed = Velocity.magnitude;
        Direction = Velocity.normalized;
    }

    void Update()
    {
        Phase = Mathf.Repeat(Phase + CycleFrequency * Time.deltaTime, 1f);
    }
}
