using UnityEngine;

/// <summary>이동이 놓이는 평면. 한 축을 통째로 쓰지 않는 개체가 있다.</summary>
public enum MovementPlane
{
    /// <summary>3D. 축 제한 없음.</summary>
    Free,

    /// <summary>Z축 이동을 버린다. Rigidbody 제약과 조향 양쪽에 걸린다.</summary>
    LockZ,
}

/// <summary>
/// 적의 이동. 이 Rigidbody에 직접 쓰는 <b>유일한</b> 컴포넌트다.
///
/// 다른 컴포넌트는 여기에 의도만 제출한다 — 루트 모션도, 중력 배율도, 순간 속도도.
/// 합성은 FixedUpdate 한 곳에서 하고 linearVelocity는 스텝당 한 번만 쓴다.
/// 주인이 여럿이면 쓰는 페이즈가 서로 달라(애니메이션 / Update / FixedUpdate)
/// 한 스텝에서 누가 마지막이었는지가 프레임마다 바뀌고, 그 차이가 드리프트로 보인다.
/// </summary>
public class SingularFlatMovement : MonoBehaviour, IMovement
{
#nullable enable
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] RayCaster _groundCaster;
    [SerializeField] float _maxAccel;
    [SerializeField] float _maxTilt;
    [SerializeField] Vector3 localEyeOffset;

    Vector3 targetVelocity;
    [SerializeField]Transform? lookTarget;

    [Tooltip("수직을 형태 지을 모델. 비워두면 유니티 기본 중력만 걸린다.")]
    [SerializeField] VerticalMotion? _vertical;

    [Tooltip("Free는 3D. LockZ는 Z축 이동을 버린다 — 그 축을 쓰지 않는 개체 전용.")]
    [SerializeField] MovementPlane _plane = MovementPlane.Free;

    [Tooltip("모델의 정면이 로컬 어느 축인지. 대부분 +Z(0,0,1)지만 다르게 만들어진 모델이 있다.")]
    [SerializeField] Vector3 _modelForward = Vector3.forward;

    // 모델 정면을 +Z로 돌려놓는 보정. LookRotation이 +Z를 목표에 맞추므로 뒤에 곱한다.
    Quaternion _forwardFix = Quaternion.identity;

    // 이번 스텝에만 유효한 의도들. 합성이 끝나면 비운다.
    Vector3 _rootMotionDelta;
    bool _rootMotionReceived;
    Vector3 _horizontalImpulse;
    bool _hasHorizontalImpulse;

    /// <summary>
    /// 수평을 루트 모션이 만든다. RootMotionRelay가 켜고 끈다.
    /// 켜져 있는 동안에는 Move()의 목표도 순간 속도 지정도 수평에 닿지 못한다.
    /// </summary>
    public bool RootMotionDriven { get; set; }

    /// <summary>몸이 지금 들고 있는 Y 속도. 아직 반영되지 않은 수직 입력은 VerticalMotion이 안다.</summary>
    public float VerticalVelocity => _rigidbody.linearVelocity.y;

    void Awake()
    {
        _forwardFix = _modelForward.sqrMagnitude < 0.0001f
            ? Quaternion.identity
            : Quaternion.FromToRotation(_modelForward.normalized, Vector3.forward);
    }

    void Start()
    {
        // 제약과 조향을 한 곳에서 건다. 따로 두면 한쪽만 걸렸을 때
        // "위치는 안 새는데 속도가 이상하다" 같은 부분 증상이 나와 원인 찾기가 고약하다.
        if (_plane == MovementPlane.LockZ)
            _rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
    }

    void FixedUpdate()
    {
        Vector3 normal = GroundNormal();

        _rigidbody.linearVelocity = Compose(normal, _rigidbody.linearVelocity);
        Rotate(normal);

        Consume();
    }

    /// <summary>
    /// 이번 스텝의 속도를 만든다. 수평의 주인은 셋 중 하나뿐이고, 아래 순서가 곧 우선순위다.
    /// 루트 모션이 도는 동안 순간 속도나 목표 속도가 끼어들지 못하는 것도 이 순서로 표현된다.
    /// </summary>
    private Vector3 Compose(Vector3 normal, Vector3 velocity)
    {
        if (RootMotionDriven)
        {
            // 변위가 안 들어온 스텝은 애니메이션이 안 돈 것이다. 직전 수평 속도를 유지한다.
            if (_rootMotionReceived)
            {
                Vector3 rootVelocity = _rootMotionDelta / Time.fixedDeltaTime;
                velocity.x = rootVelocity.x;
                velocity.z = rootVelocity.z;
            }
        }
        else if (_hasHorizontalImpulse)
        {
            velocity.x = _horizontalImpulse.x;
            velocity.z = _horizontalImpulse.z;
        }
        else
        {
            velocity += Acceleration(normal, velocity);
        }

        // 수직은 해석하지 않는다. 모델이 돌려준 날것의 속도를 그대로 쓴다.
        if (_vertical != null) velocity.y = _vertical.Resolve(velocity.y, Time.fixedDeltaTime);

        // 어느 경로로도 잠긴 축이 새지 않는다는 보장.
        return Flatten(velocity);
    }

    private void Consume()
    {
        _rootMotionDelta = Vector3.zero;
        _rootMotionReceived = false;
        _hasHorizontalImpulse = false;
    }

    /// <summary>잠긴 축을 버린다. Free면 그대로 돌려준다.</summary>
    private Vector3 Flatten(Vector3 v)
    {
        if (_plane == MovementPlane.LockZ) v.z = 0f;
        return v;
    }

    #region Intake

    //called every frame on motion.
    public void Move(Vector3 velocity)
    {
        // 목표부터 평면 안에 넣는다. 결과만 깎으면 Acceleration이 닿지 못할 축을 향해
        // 계속 밀어서, maxAccel 예산의 일부가 그 축으로 새어나간다.
        targetVelocity = Flatten(velocity);
    }

    public void LookTowards(Transform? target)
    {
        lookTarget = target;
    }

    /// <summary>
    /// 이번 스텝의 루트 모션 변위. OnAnimatorMove는 렌더 프레임마다 도므로
    /// 한 물리 스텝에 여러 번 들어올 수 있다. 누적해 두었다가 fixedDeltaTime으로 나눈다.
    /// </summary>
    public void SubmitRootMotion(Vector3 delta)
    {
        _rootMotionDelta += delta;
        _rootMotionReceived = true;
    }

    /// <summary>
    /// 수평 속도를 이번 스텝에 즉시 지정한다. 도약처럼 목표를 향해 붙일 시간이 없을 때 쓴다.
    /// Acceleration이 다음 스텝에 되돌리지 않도록 목표 속도도 같이 맞춘다.
    /// </summary>
    public void SetHorizontalVelocity(Vector3 horizontal)
    {
        horizontal.y = 0f;
        horizontal = Flatten(horizontal);

        _horizontalImpulse = horizontal;
        _hasHorizontalImpulse = true;
        targetVelocity = horizontal;
    }

    #endregion

    private Vector3 Acceleration(Vector3 normal, Vector3 velocity)
    {
        Vector3 flatCurrentVelocity = CustomMath.CleanRemove(normal, velocity);
        Vector3 flatTarget = CustomMath.PreservativeRemove(normal, targetVelocity);

        if (flatCurrentVelocity == flatTarget) return Vector3.zero;

        Vector3 accel = flatTarget - flatCurrentVelocity;
        return Mathf.Min(accel.magnitude, _maxAccel * Time.fixedDeltaTime) * accel.normalized;
    }

    /// <summary>
    /// 볼 방향이 없으면 회전에 손대지 않는다.
    /// 예전에는 그 자리에서 transform.forward를 목표로 삼았는데, 이 컴포넌트가 몸과
    /// 다른 오브젝트에 붙어 있으면(Agnostos: Movement / Agnostos_Mesh) 그 forward는
    /// 몸을 따라오지 않는 고정 방향이라, 몸을 매 프레임 그쪽으로 끌어당긴다.
    /// </summary>
    private void Rotate(Vector3 up)
    {
        Vector3 facing = LookDirection(up);
        if (facing.sqrMagnitude < 0.0001f) return;

        _rigidbody.MoveRotation(Quaternion.LookRotation(facing, up) * _forwardFix);
    }

    private Vector3 GroundNormal()
    {
        return (_groundCaster.Cast(out RaycastHit hit, 1 << 3)) ? hit.normal : Vector3.up;
    }

    /// <summary>바라볼 방향. 의견이 없으면 영벡터를 돌려주고, 그때 Rotate는 회전을 건너뛴다.</summary>
    private Vector3 LookDirection(Vector3 groundNormal)
    {
        if (lookTarget == null) return Vector3.zero;

        Vector3 direction = lookTarget.position - (_rigidbody.position + _rigidbody.transform.TransformDirection(localEyeOffset));

        // 몸도 평면 밖을 보지 않는다. 이걸 빼면 루트 모션이 클립 변위를 잠긴 축으로
        // 밀어내고, 그 몫이 솔버에서 버려져 걸음이 클립보다 느려진다.
        direction = Flatten(direction);

        Vector3 flatDirection = CustomMath.CleanRemove(groundNormal, direction).normalized;

        // 잠긴 축 위에 정확히 겹치면 방향이 사라진다. 그때도 회전하지 않는다.
        if (flatDirection.sqrMagnitude < 0.0001f) return Vector3.zero;

        float tiltAmount = Mathf.Clamp(CustomMath.GetComponentSizeFrom(groundNormal, direction), -_maxTilt, _maxTilt);
        Vector3 tilt = tiltAmount * groundNormal;
        return flatDirection + tilt;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_rigidbody.position, _rigidbody.position + GroundNormal());
    }
}
