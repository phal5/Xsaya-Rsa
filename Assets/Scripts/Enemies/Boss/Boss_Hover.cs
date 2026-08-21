using UnityEngine;

/// <summary>
/// 보스를 땅에서 띄워 둔다. 지상 보스와 같은 부품을 쓰되, 수직만 이쪽이 가져간다.
///
/// <see cref="Boss_Steering"/>을 고치지 않는다. 그쪽이 이미 두 창구를 열어두었기 때문이다.
///   <see cref="Boss_Steering.SetGravityScale"/>(0, 0) 으로 중력을 끄고,
///   <see cref="VerticalMotion.SetVelocity"/> 로 매 스텝 수직 속도를 지정한다.
/// 지정은 그 스텝의 중력 모델을 건너뛰므로, 둘이 서로 밀지 않는다.
///
/// 높이는 <b>땅으로부터</b> 잰다. 절대 높이로 잡으면 지형이 오르내릴 때 파묻히거나 치솟는다.
/// 발밑에 땅이 없으면(낭떠러지 위) 마지막으로 알던 높이를 유지한다 — 허공에서 갑자기 내려앉지 않게.
/// </summary>
[RequireComponent(typeof(Boss_Steering))]
public class Boss_Hover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Boss_Steering _steering;
    [SerializeField] Rigidbody _body;

    [Header("Height")]
    [Tooltip("땅에서 이 높이를 유지한다.")]
    [SerializeField, Min(0f)] float _height = 3.5f;

    [Tooltip("높이를 되찾는 빠르기. 클수록 뻣뻣하게 붙는다.")]
    [SerializeField, Min(0.1f)] float _stiffness = 4f;

    [Tooltip("수직 속도의 상한. 높이 차가 커도 이보다 빨리 오르내리지 않는다.")]
    [SerializeField, Min(0.5f)] float _maxClimbSpeed = 6f;

    [Header("Bob - 제자리에서 일렁이기")]
    [Tooltip("위아래로 일렁이는 폭(미터). 0이면 가만히 떠 있는다.")]
    [SerializeField, Min(0f)] float _bobAmplitude = 0.25f;

    [Tooltip("일렁임 한 번에 걸리는 시간(초).")]
    [SerializeField, Min(0.1f)] float _bobPeriod = 3f;

    [Header("Ground")]
    [Tooltip("발밑을 재는 거리. 부양 높이보다 넉넉히 길어야 한다.")]
    [SerializeField, Min(1f)] float _probeDistance = 30f;

    [Tooltip("땅으로 볼 레이어.")]
    [SerializeField] LayerMask _groundMask = ~0;

    float _lastKnownGroundY;
    bool _hasGround;

    void Reset()
    {
        _steering = GetComponent<Boss_Steering>();
        _body = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (_steering == null) _steering = GetComponent<Boss_Steering>();
        if (_body == null) _body = GetComponent<Rigidbody>();

        // 수직은 전적으로 이쪽이 만든다. 중력이 같이 걸리면 매 스텝 서로를 지운다.
        if (_steering != null) _steering.SetGravityScale(0f, 0f);

        _lastKnownGroundY = _body != null ? _body.position.y - _height : 0f;
    }

    void OnDisable()
    {
        // 부양을 끄면 중력을 돌려준다. 안 그러면 무중력인 채로 남는다.
        if (_steering != null) _steering.ResetGravityScale();
    }

    void FixedUpdate()
    {
        if (_steering == null || _body == null) return;

        float target = GroundY() + _height + Bob();
        float gap = target - _body.position.y;

        // 지정 속도로 넣는다. 힘이 아니라 속도라 지형을 지나며 튀지 않는다.
        _steering.SetVelocity(Mathf.Clamp(gap * _stiffness, -_maxClimbSpeed, _maxClimbSpeed));
    }

    float Bob()
    {
        if (_bobAmplitude <= 0f) return 0f;

        return Mathf.Sin(Time.time * Mathf.PI * 2f / _bobPeriod) * _bobAmplitude;
    }

    /// <summary>발밑의 땅 높이. 못 찾으면 마지막으로 알던 값을 그대로 쓴다.</summary>
    float GroundY()
    {
        Vector3 origin = _body.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _probeDistance, _groundMask, QueryTriggerInteraction.Ignore))
        {
            _lastKnownGroundY = hit.point.y;
            _hasGround = true;
        }

        return _hasGround ? _lastKnownGroundY : _body.position.y - _height;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_body == null) return;

        Gizmos.color = Color.cyan;
        Vector3 ground = new Vector3(_body.position.x, GroundYPreview(), _body.position.z);
        Gizmos.DrawLine(ground, ground + Vector3.up * _height);
        Gizmos.DrawWireSphere(ground + Vector3.up * _height, 0.3f);
    }

    float GroundYPreview()
    {
        Vector3 origin = _body.position + Vector3.up * 0.5f;
        return Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _probeDistance, _groundMask, QueryTriggerInteraction.Ignore)
            ? hit.point.y
            : _body.position.y - _height;
    }
#endif
}
