using UnityEngine;

/// <summary>
/// 애니메이터가 자식에 있을 때 루트 모션을 몸체의 이동 컴포넌트로 넘겨준다.
///
/// 그냥 applyRootMotion만 켜면 애니메이터가 붙은 자식 오브젝트가 움직여서
/// 메시만 콜라이더를 두고 떠나간다. OnAnimatorMove를 구현하면 유니티가 자동 적용을 멈추므로,
/// 여기서 이동량을 받아 넘긴다.
///
/// Rigidbody에 직접 쓰지 않는다. 쓰는 쪽은 SingularFlatMovement 하나뿐이고,
/// 여기는 그 스텝의 변위를 <b>제출</b>만 한다. OnAnimatorMove는 렌더 프레임마다,
/// 물리는 고정 주기로 도는 탓에 직접 쓰면 한 스텝에 마지막 갱신분만 남고 나머지가 버려진다.
///
/// 회전도 넘기지 않는다. SingularFlatMovement가 매 FixedUpdate에 조준 회전을 덮어쓰기 때문에
/// 같이 넘기면 서로 싸운다.
///
/// 수직은 여기로 오지 않는다. 클립들이 Root Transform Position (Y)를 Bake Into Pose로
/// 임포트하고 있어 상하 움직임이 포즈 안에 들어 있다. 몸의 높이는 물리가 정한다.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RootMotionRelay : MonoBehaviour
{
    [Tooltip("변위를 넘겨받아 실제로 몸을 움직일 이동 컴포넌트.")]
    [SerializeField] SingularFlatMovement _movement;

    [Tooltip("켜져 있는 동안만 애니메이션 이동이 몸체에 반영된다. 스킬이 필요할 때만 켠다.")]
    [SerializeField] bool _active;

    Animator _animator;

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
        if (!_active || _movement == null || _animator == null) return;

        _movement.SubmitRootMotion(_animator.deltaPosition);
    }
}
