using UnityEngine;

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

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 normal = GroundNormal();
        Accelerate(normal);
        Rotate(normal);
    }

    //called every frame on motion.
    public void Move(Vector3 velocity)
    {
        targetVelocity = velocity;
    }

    public void LookTowards(Transform? target)
    {
        lookTarget = target;
    }

    public float VerticalVelocity => _rigidbody.linearVelocity.y;

    /// <summary>
    /// 월드 Y 속도를 직접 갈아끼운다. 점프/낙하처럼 접지면과 무관한 동작에 쓴다.
    /// Accelerate는 지면 노멀에 수직인 평면에서만 가속하므로 여기서 넣은 수직 속도를 건드리지 않는다.
    /// </summary>
    public void SetVerticalVelocity(float velocity)
    {
        Vector3 v = _rigidbody.linearVelocity;
        v.y = velocity;
        _rigidbody.linearVelocity = v;
    }

    public void AddVelocity(Vector3 delta)
    {
        _rigidbody.linearVelocity += delta;
    }

    private void Accelerate(Vector3 normal)
    {
        Vector3 v = _rigidbody.linearVelocity;
        Vector3 flatCurrentVelocity = CustomMath.CleanRemove(normal, v);
        Vector3 flatTarget = CustomMath.PreservativeRemove(normal, targetVelocity);

        if (flatCurrentVelocity != flatTarget)
        {
            Vector3 accel = flatTarget - flatCurrentVelocity;
            Vector3 accelDireciton = accel.normalized;
            float accelMagnitude = accel.magnitude;
            _rigidbody.linearVelocity += Mathf.Min(accelMagnitude, _maxAccel * Time.deltaTime) * accelDireciton;
        }
    }

    private void Rotate(Vector3 up)
    {
        Quaternion targetRotation = Quaternion.LookRotation(LookDirection(up), up);
        _rigidbody.MoveRotation(targetRotation);
    }

    private Vector3 GroundNormal()
    {
        return (_groundCaster.Cast(out RaycastHit hit, 1 << 3)) ? hit.normal : Vector3.up;
    }

    private Vector3 LookDirection(Vector3 groundNormal)
    {
        if (lookTarget == null) return transform.forward;

        Vector3 direction = lookTarget.position - (_rigidbody.position + _rigidbody.transform.TransformDirection(localEyeOffset));
        Vector3 flatDirection = CustomMath.CleanRemove(groundNormal, direction).normalized;

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
