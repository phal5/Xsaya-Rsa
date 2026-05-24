using UnityEngine;

public class SphereCaster : MonoBehaviour
{
    [SerializeField] Vector3 _direction;
    [SerializeField] float _radius;
    [SerializeField] float _distance;

    Vector3 center;
    bool _hit;

    public bool Cast(out RaycastHit hit)
    {
        _hit = Physics.SphereCast(transform.position, _radius, _direction, out hit, _distance);
        if(_hit) center = hit.point + hit.normal * _radius;
        return _hit;
    }

    private void OnDrawGizmos()
    {
        if (_hit)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, center);
            Gizmos.DrawSphere(center, _radius);
            _hit = false;
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _direction * _distance + transform.position);
        }
    }
}
