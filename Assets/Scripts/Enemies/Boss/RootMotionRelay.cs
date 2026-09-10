using UnityEngine;

/// <summary>
/// 애니메이터가 자식에 있을 때 루트 모션을 나눠 넘겨준다. 이동은 몸에, 회전은 루트에.
///
/// 그냥 applyRootMotion만 켜면 애니메이터가 붙은 자식 오브젝트가 움직여서
/// 메시만 콜라이더를 두고 떠나간다. OnAnimatorMove를 구현하면 유니티가 자동 적용을 멈추므로,
/// 여기서 이동량을 받아 넘긴다.
///
/// Rigidbody에 직접 쓰지 않는다. 쓰는 쪽은 SingularFlatMovement 하나뿐이고,
/// 여기는 그 스텝의 변위를 <b>제출</b>만 한다. OnAnimatorMove는 렌더 프레임마다,
/// 물리는 고정 주기로 도는 탓에 직접 쓰면 한 스텝에 마지막 갱신분만 남고 나머지가 버려진다.
///
/// <b>회전은 몸이 아니라 이 Transform(루트)에 얹는다.</b> 몸의 회전은 SingularFlatMovement가
/// 매 FixedUpdate에 조준으로 덮어쓰므로, 같은 곳에 쓰면 서로 싸운다. 루트는 몸의 자식이라
/// 둘은 다른 Transform에 쓰고, 보이는 방향은 조준 × 클립 회전이 된다 — 회전 베기가 몸의 조준 안에서 돈다.
/// 몸이 언제 조준할지는 <see cref="Boss_Skill"/>의 구간이 정한다.
/// 이동 변위는 유니티가 이 Transform의 현재 회전으로 재어 주므로, 도는 동안의 발놀림도 따라 돈다.
///
/// 수평 회전(Y)만 얹는다. 기울기까지 받으면 루트가 기운 채로 몸에 남는다.
///
/// 꺼지면 루트를 제자리로 되돌린다. 회전 베기는 360°의 배수로 끝나지 않아(442°면 82°가 남는다)
/// 그대로 두면 다음 걸음이 옆을 보고 걷는다. 한 번에 되돌리면 그만큼 툭 튀므로 초당 각도로 서서히 돌린다.
/// 애니메이터와 같은 시계를 쓴다 — 느려지는 구간에서는 도는 것도 돌아오는 것도 함께 느려진다.
///
/// 수직은 여기로 오지 않는다. 클립들이 Root Transform Position (Y)를 Bake Into Pose로
/// 임포트하고 있어 상하 움직임이 포즈 안에 들어 있다. 몸의 높이는 물리가 정한다.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RootMotionRelay : MonoBehaviour
{
    [Tooltip("변위를 넘겨받아 실제로 몸을 움직일 이동 컴포넌트.")]
    [SerializeField] SingularFlatMovement _movement;

    [Tooltip("켜져 있는 동안만 애니메이션 이동과 회전이 반영된다. 스킬이 필요할 때만 켠다.")]
    [SerializeField] bool _active;

    [Tooltip("꺼진 뒤 루트가 제자리로 돌아오는 속도(도/초). 회전 베기가 남긴 각도를 이 속도로 걷어낸다.")]
    [SerializeField, Min(0f)] float _returnSpeed = 360f;

    Animator _animator;

    /// <summary>루트의 제자리. 처음 놓인 로컬 회전이다.</summary>
    Quaternion _rest;

    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            Publish();
        }
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rest = transform.localRotation;

        // 인스펙터에서 켠 채로 시작하면 setter를 거치지 않는다. 여기서 한 번 맞춘다.
        Publish();
    }

    /// <summary>수평의 주인이 누구인지 이동 컴포넌트에 알린다.</summary>
    void Publish()
    {
        if (_movement != null) _movement.RootMotionDriven = _active;
    }

    void OnAnimatorMove()
    {
        if (_animator == null) return;

        if (!_active)
        {
            Return();
            return;
        }

        if (_movement != null) _movement.SubmitRootMotion(_animator.deltaPosition);

        transform.localRotation *= Yaw(_animator.deltaRotation);
    }

    /// <summary>제자리로 서서히 돌아간다.</summary>
    void Return()
    {
        if (transform.localRotation == _rest) return;

        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, _rest, _returnSpeed * Time.deltaTime);
    }

    /// <summary>회전에서 수평(Y) 성분만 떼어낸다.</summary>
    static Quaternion Yaw(Quaternion rotation)
    {
        Vector3 forward = rotation * Vector3.forward;

        return Quaternion.AngleAxis(Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg, Vector3.up);
    }
}
