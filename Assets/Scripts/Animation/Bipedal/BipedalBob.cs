using UnityEngine;

/// <summary>
/// 골반의 수직 위치. 두 가지를 겹쳐 쓴다 — 걸음이 <b>요구하는</b> 주저앉음과, 걸음이 <b>만드는</b> 흔들림.
///
/// <b>주저앉음</b>은 연출이 아니라 기하학이다. 발이 옆으로 벌어진 만큼 다리는 비스듬해지고,
/// 비스듬한 다리는 수직으로 짧아진다. 그만큼 골반이 내려와야 발이 땅에 닿는다.
/// 직각삼각형 하나로 정확히 나오므로 눈으로 맞출 값이 아니다.
///
/// 중요한 건 <b>지금</b> 벌어진 정도를 본다는 것이다. 한 주기 중 가장 벌어지는 순간을 기준으로
/// 계속 내려가 있으면 몸이 내내 주저앉아 보인다. 매 순간 필요한 만큼만 내려가면 저절로
/// 오르내리게 되고 — 두 다리가 갈라지는 순간 낮고 한 다리가 수직인 순간 높다 —
/// 그 오르내림이 곧 걷기의 역진자 운동이다. 서 있으면 벌어짐이 0이라 0이 된다.
///
/// <b>흔들림</b>은 그 위에 얹는 성격이다. 달리기는 중간 지지에서 다리가 눌려 더 낮아지고
/// 체공에서 높아지는데, 이는 기하학이 아니라 스프링이라 따로 더한다. 부호는 이미
/// <see cref="BipedalGait.Airborneness"/>에 들어 있다 — 걷기에서 음수, 달리기에서 양수,
/// 경계에서 0. 그래서 걷기용 코드도 달리기용 코드도 없고, 곱하기 한 번으로 넘어간다.
///
/// 다른 것이 쓰지 않는 노드에 붙인다. 루트 모션을 끈 Mesh가 보통 그 자리다.
/// </summary>
public class BipedalBob : MonoBehaviour
{
    [SerializeField] BipedalGait _gait;
    [SerializeField] BipedalFoot _left;
    [SerializeField] BipedalFoot _right;

    [Space(10f)]
    [Tooltip("다리 길이. 뼈 두 개의 길이 합이다 — 눈으로 맞추는 값이 아니라 리그에서 재는 값이다.")]
    [SerializeField] float _legLength = 0.7f;

    [Space(10f)]
    [Tooltip("스프링 성격의 진폭(m). 부호는 걸음이 정한다. 오르내림의 대부분은 이미 주저앉음이 만든다.")]
    [SerializeField] float _amplitude = 0.03f;

    [Tooltip("이 속도까지는 흔들림이 함께 자란다. 서 있을 때 흔들리지 않게 한다.")]
    [SerializeField] float _fadeInSpeed = 1.5f;

    Vector3 _rest;

    /// <summary>지금 걸음이 요구하는 주저앉음. 조율과 디버깅에 쓰라고 열어둔다.</summary>
    public float Crouch
    {
        get
        {
            float splay = Mathf.Max(_left.Splay, _right.Splay);
            float vertical = Mathf.Sqrt(Mathf.Max(0f, _legLength * _legLength - splay * splay));
            return _legLength - vertical;
        }
    }

    void Start()
    {
        _rest = transform.localPosition;
    }

    void LateUpdate()
    {
        // 주기당 두 번 오르내린다 — 다리가 둘이고 반 주기 어긋나 있으므로.
        float wave = -Mathf.Cos(4f * Mathf.PI * _gait.Phase);
        float grown = Mathf.Clamp01(_gait.Speed / _fadeInSpeed);
        float spring = _gait.Airborneness * _amplitude * grown * wave;

        transform.localPosition = _rest + Vector3.up * (spring - Crouch);
    }
}
