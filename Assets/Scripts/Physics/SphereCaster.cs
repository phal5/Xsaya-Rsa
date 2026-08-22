using UnityEngine;

public class SphereCaster : MonoBehaviour
{
    [SerializeField] string _purpose;
    [SerializeField] Vector3 _direction;
    [SerializeField] LayerMask _layerMask = -1;
    [SerializeField] float _radius;
    [SerializeField] float _distance;

    [Tooltip("트리거도 맞힌 것으로 칠지. 꺼두면 무시한다.")]
    [SerializeField] bool _hitTriggers;

    Vector3 center;
    bool _hit;

    /// <summary>
    /// 트리거를 맞힌 것으로 칠지. <b>기본은 무시</b>다.
    ///
    /// 지금 이 캐스터를 쓰는 곳은 전부 디딜 것을 찾는 자리다. 트리거는 딛을 수 없으므로
    /// 그것을 바닥으로 읽으면 킬 볼륨 위에 서거나 상호작용 볼륨 위에 착지한다.
    /// 프로젝트 설정(Queries Hit Triggers)이 켜져 있어 인자를 넘기지 않으면 그렇게 된다.
    ///
    /// 형제인 CapsuleCaster는 처음부터 무시하고 있었다. 이쪽만 빠져 있던 것을 맞춘 것이고,
    /// 트리거를 잡아야 하는 쓰임이 생기면 그때 켜면 된다.
    /// </summary>
    QueryTriggerInteraction Triggers => _hitTriggers
        ? QueryTriggerInteraction.Collide
        : QueryTriggerInteraction.Ignore;

    public bool Cast(out RaycastHit hit)
    {
        _hit = Physics.SphereCast(transform.position, _radius, _direction, out hit, _distance - _radius, _layerMask, Triggers);
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
