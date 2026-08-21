using UnityEngine;

/// <summary>
/// 다리 하나. IK 타겟을 매 프레임 놓는다.
///
/// 자기 시계를 갖지 않는다. <see cref="BipedalGait.Phase"/>에 자기 몫의 오프셋을 더해
/// 읽을 뿐이다. 두 다리가 어긋날 자유도가 없으므로 맞춰줄 배선도 필요 없다 —
/// 걸음걸이는 오프셋 두 개(0과 0.5)로 <b>표현</b>되지, 이벤트로 <b>유지</b>되지 않는다.
///
/// 지지와 스윙도 갈래로 나누지 않는다. <see cref="Swing"/>은 접지 구간 내내 0에
/// 클램프되므로, 같은 식이 그대로 "발이 착지점에 가만히 있다"가 된다.
/// 그래서 발이 갱신되지 않는 순간이 존재하지 않는다.
/// </summary>
public class BipedalFoot : MonoBehaviour
{
    [SerializeField] BipedalGait _gait;

    [Tooltip("한쪽은 0, 다른 쪽은 0.5. 두 다리가 반 주기 어긋난다는 말이 이 값이다.")]
    [Range(0f, 1f)][SerializeField] float _phaseOffset;

    [Space(10f)]
    [Tooltip("다리 뿌리. 착지점을 여기 아래에서 예측한다.")]
    [SerializeField] Transform _hip;
    [Tooltip("IK가 겨눌 타겟. 몸을 따라가지 않는 월드 공간에 두어야 발이 땅에 박힌다.")]
    [SerializeField] Transform _target;

    [Space(10f)]
    [Tooltip("발목 본이 지면에서 뜨는 높이. 타겟은 접지점이 아니라 발목이 갈 자리다 — IK의 Tip이 발목이므로.")]
    [SerializeField] float _ankleHeight = 0.1f;
    [Tooltip("한 걸음 거리에 대한 발 들림 비율. 멀리 내딛을수록 높이 든다 — 제자리에서는 들지 않는다. "
           + "속도가 아니라 걸음 거리에 매다는 것이 요점이다. 자세를 회복하는 마지막 한 걸음도 들려야 하므로.")]
    [SerializeField] float _liftPerStep = 0.25f;
    [SerializeField] float _maxLift = 0.2f;

    [Space(10f)]
    [SerializeField] LayerMask _ground;
    [SerializeField] float _rayDistance = 2f;
    [Tooltip("레이캐스트를 쏘아 내릴 높이. 뿌리보다 위에서 쏜다.")]
    [SerializeField] float _rayHeight = 0.2f;

    /// <summary>주기 안의 내 위치.</summary>
    public float Phase => Mathf.Repeat(_gait.Phase + _phaseOffset, 1f);

    /// <summary>
    /// 스윙 진행도. 접지 구간에서는 0이다 — 갈래가 아니라 클램프가 나눈다.
    /// duty가 움직이면 지지와 스윙의 경계도 같이 움직이고, 그것이 곧 걷기와 달리기의 차이다.
    /// </summary>
    public float Swing => Mathf.InverseLerp(_gait.Duty, 1f, Phase);

    /// <summary>지금 발이 들려 있는 높이. 디디고 있으면 0이다.</summary>
    public float Lift { get; private set; }

    /// <summary>
    /// 다리 뿌리에서 접지점까지의 수평 거리. 다리가 옆으로 벌어진 정도다.
    /// 벌어진 만큼 다리는 수직으로 짧아지므로, 몸이 얼마나 내려앉아야 하는지가 여기서 나온다.
    /// </summary>
    public float Splay { get; private set; }

    /// <summary>직전 착지점. 이 스크립트에서 과거를 기억하는 유일한 값이다.</summary>
    Vector3 _plant;

    /// <summary>이번 프레임 발이 놓인 지면 위치. 발목 높이와 들림을 더하기 전 값이다.</summary>
    Vector3 _contact;

    float _previousPhase;

    void Start()
    {
        _plant = Ground(_hip.position);
        _contact = _plant;
        _target.position = _plant + Vector3.up * _ankleHeight;
        _previousPhase = Phase;
    }

    void Update()
    {
        float phase = Phase;

        // 위상이 한 바퀴 돌았다 = 방금 디뎠다. 여기서 다음 스윙의 출발점을 잡는다.
        // 시계에서 파생되는 판정이지 따로 들고 있는 상태가 아니다 — 틀릴 자유도가 없다.
        if (phase < _previousPhase) _plant = _contact;
        _previousPhase = phase;

        float swing = Mathf.InverseLerp(_gait.Duty, 1f, phase);
        Vector3 landing = Landing(swing);

        Lift = Arc(swing) * Mathf.Min(_liftPerStep * Vector3.Distance(_plant, landing), _maxLift);
        _contact = Vector3.Lerp(_plant, landing, swing);
        _target.position = _contact + Vector3.up * (_ankleHeight + Lift);

        Vector3 spread = _contact - _hip.position;
        spread.y = 0f;
        Splay = spread.magnitude;
    }

    /// <summary>
    /// 이 스윙이 끝날 때 발이 닿을 자리.
    ///
    /// 남은 스윙 동안 뿌리가 나아갈 거리에, 착지 시점의 선행 거리를 더한다.
    ///
    /// 선행 거리는 <c>0.5 · duty · Stride</c>다. 한 발이 딛고 있는 동안 몸은
    /// <c>duty · Stride</c>만큼 나아가므로, 발이 그 절반만큼 앞에 떨어져야
    /// 디딤이 끝날 때 같은 거리만큼 뒤에 남는다 — 앞뒤가 대칭이어야 밀어낼 수 있다.
    /// 여기에 <c>0.5 · Stride</c>를 쓰면 duty와 무관하게 두 배가 되어, 발이 다리가
    /// 닿는 것보다 멀리 떨어지고 착지 순간 다리가 뻗은 채 굳는다.
    ///
    /// 시간으로 풀면 주파수로 나누게 되어 정지에서 무한대가 된다. 거리로 풀면 나눗셈이
    /// 약분되어 사라지고, 정지에서는 보폭이 0이라 발이 그냥 뿌리 아래에 선다.
    /// </summary>
    Vector3 Landing(float swing)
    {
        float travel = (1f - swing) * (1f - _gait.Duty) + 0.5f * _gait.Duty;
        return Ground(_hip.position + _gait.Direction * (travel * _gait.Stride));
    }

    Vector3 Ground(Vector3 around)
    {
        Vector3 from = around + Vector3.up * _rayHeight;
        if (Physics.Raycast(from, Vector3.down, out RaycastHit hit, _rayHeight + _rayDistance, _ground))
            return hit.point;

        return from + Vector3.down * (_rayHeight + _rayDistance);
    }

    /// <summary>스윙 중간에 1이 되는 포물선. 접지 구간에서는 swing이 0이라 함께 0이다.</summary>
    static float Arc(float x) => 4f * x * (1f - x);

    void OnDrawGizmosSelected()
    {
        if (_gait == null || _hip == null || _target == null) return;

        Gizmos.color = Color.Lerp(Color.cyan, Color.red, Swing);
        Gizmos.DrawWireSphere(_target.position, 0.05f);
        Gizmos.DrawLine(_hip.position, _target.position);
    }
}
