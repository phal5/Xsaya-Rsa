using UnityEngine;

/// <summary>
/// 애니메이터가 자식에 있을 때 루트 모션을 몸체(Rigidbody)로 옮겨준다.
///
/// 그냥 applyRootMotion만 켜면 애니메이터가 붙은 자식 오브젝트가 움직여서
/// 메시만 콜라이더를 두고 떠나간다. OnAnimatorMove를 구현하면 유니티가 자동 적용을 멈추므로,
/// 여기서 이동량을 받아 루트 Rigidbody에 넘긴다.
///
/// 위치가 아니라 <b>속도</b>로 넘긴다. MovePosition은 사실상 순간이동이라 충돌 응답을 건너뛰어,
/// 상시 이동에 쓰면 벽이나 다른 캐릭터를 밀고 지나간다. 속도로 넘기면 물리가 그대로 살아있다.
/// 수직 속도는 건드리지 않아 중력과 점프가 계속 담당한다.
///
/// 회전도 넘기지 않는다. SingularFlatMovement가 매 FixedUpdate에 조준 회전을 덮어쓰기 때문에
/// 같이 넘기면 서로 싸운다.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RootMotionRelay : MonoBehaviour
{
    [SerializeField] Rigidbody _body;

    [Tooltip("켜져 있는 동안만 애니메이션 이동이 몸체에 반영된다. 스킬이 필요할 때만 켠다.")]
    [SerializeField] bool _active;

    Animator _animator;

    public bool Active
    {
        get => _active;
        set => _active = value;
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void OnAnimatorMove()
    {
        if (!_active || _body == null || _animator == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 delta = _animator.deltaPosition;
        Vector3 v = _body.linearVelocity;

        _body.linearVelocity = new Vector3(delta.x / dt, v.y, delta.z / dt);
    }
}
