using UnityEngine;

public class QuadrupedicTilt : MonoBehaviour
{
    [SerializeField] float _tiltAmount;
    [SerializeField] Rigidbody _rigidBody;
    [Space(10f)]
    [SerializeField] Transform foot1;
    [SerializeField] Transform foot2;
    [SerializeField] Transform foot3;
    [SerializeField] Transform foot4;

    Vector3 up;

    // Update is called once per frame
    void FixedUpdate()
    {
        float tiltCoefficient = _rigidBody.linearVelocity.sqrMagnitude;
        if (_rigidBody.linearVelocity != Vector3.zero) Rotate(Mathf.Clamp01(tiltCoefficient));
    }

    void Rotate(float tiltCoefficient = 1)
    {
        float tiltAllowance = Time.deltaTime * _tiltAmount * tiltCoefficient;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, TargetRotation(), tiltAllowance);
    }

    Vector3 FaceUp(Vector3 v)
    {
        return (v.y > 0) ? v : -v;
    }

    Quaternion TargetRotation()
    {
        Vector3 tilt = Vector3.up * (foot1.position.y + foot4.position.y - foot2.position.y - foot3.position.y) * 0.5f;
        Vector3 forth = transform.parent.forward + tilt;
        Vector3 left = -transform.parent.right - tilt;

        up = FaceUp(Vector3.Cross(left, forth).normalized);
        return Quaternion.LookRotation(up, forth);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + up);
        Gizmos.DrawSphere(transform.position + up, 0.1f);
    }
}
