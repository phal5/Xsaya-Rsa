using UnityEngine;

public class CapsuleCaster : MonoBehaviour
{
    [SerializeField] CapsuleCollider _capsuleCollider;
    [SerializeField] float _distance;

    Vector3 center;
    float radius;
    float halfHeightMinusRadius;

    bool _hit;

    private void Awake()
    {
        radius = _capsuleCollider.radius;
        halfHeightMinusRadius = _capsuleCollider.height - radius;
        halfHeightMinusRadius *= transform.lossyScale.y;
    }

    public bool Cast(out RaycastHit hit, Vector3 direction)
    {
        Vector3 point1 = Vector3.up * halfHeightMinusRadius;
        Vector3 point2 = Vector3.down * halfHeightMinusRadius;
        _hit = Physics.CapsuleCast(point1 + transform.position, point2 + transform.position, radius, direction, out hit, _distance);
        return _hit;
    }
}