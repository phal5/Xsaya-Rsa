using UnityEngine;

public class RayCaster : MonoBehaviour
{
    [SerializeField] Vector3 _direction;
    [SerializeField] float _distance;

    Vector3 hitPoint;
    bool _hit;

    public bool Cast(out RaycastHit hit, int layerMask = -1)
    {
        _hit = Physics.Raycast(transform.position, _direction, out hit, _distance);
        if (_hit) hitPoint = hit.point;
        return _hit;
    }

    private void OnDrawGizmos()
    {
        if (_hit)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, hitPoint);
            _hit = false;
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _direction * _distance + transform.position);
        }
    }
}
