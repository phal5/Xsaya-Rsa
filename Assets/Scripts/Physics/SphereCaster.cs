using UnityEngine;

public class SphereCaster : MonoBehaviour
{
    [SerializeField] string _purpose;
    [SerializeField] Vector3 _direction;
    [SerializeField] LayerMask _layerMask = -1;
    [SerializeField] float _radius;
    [SerializeField] float _distance;

    Vector3 center;
    bool _hit;

    public bool Cast(out RaycastHit hit)
    {
        _hit = Physics.SphereCast(transform.position, _radius, _direction, out hit, _distance - _radius, _layerMask);
        if(_hit) center = hit.point + hit.normal * _radius;
        return _hit;
    }

    private void OnDrawGizmos()
    {
        Cast(out _);
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
