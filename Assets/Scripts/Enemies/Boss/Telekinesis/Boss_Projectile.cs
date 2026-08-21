using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 날아가는 일만 맡는다. 무엇이 이걸 던졌는지, 던진 물건이 나중에 어떻게 되는지는 모른다.
///
/// 속도 벡터를 받지 않고 <b>발사 위치와 타겟</b>을 들고 있다가 발사 시점에 스스로 겨눈다.
/// 그래서 조준 규칙(높이 보정·산포·유도)이 스킬마다 흩어지지 않고 여기 한 곳에 모인다.
/// 타겟을 좌표가 아니라 Transform으로 쥐는 덕에, 쏜 뒤에도 따라갈 수 있고
/// 쳐냈을 때는 <see cref="Redirect"/>로 겨냥만 갈아끼우면 된다.
///
/// 때리는 방식도 여기 있다. 날아가며 때리는 것과 스치면 때리는 것(공전 고리)은
/// 같은 반경 검사를 쓰므로, 둘을 갈라두면 같은 코드가 두 벌이 된다.
///
/// 콜라이더끼리 부딪히게 두지 않고 <see cref="Physics.OverlapSphere"/>로 직접 찾는 이유는,
/// 물건마다 콜라이더 구성이 제각각이라 부딪힘에 기대면 어떤 물건은 맞고 어떤 물건은 통과하기 때문이다.
/// </summary>
public class Boss_Projectile : MonoBehaviour
{
    [Header("Flight")]
    [Tooltip("날아가는 속도(m/s). 스킬이 따로 지정하면 그쪽이 이긴다.")]
    [SerializeField, Min(1f)] float _speed = 18f;

    [Tooltip("날아가는 동안 걸리는 중력. 0이면 직선으로 간다.")]
    [SerializeField] float _gravity = 9.8f;

    [Tooltip("이 시간이 지나면 스스로 끝낸다. 벽 너머로 날아가 영영 남는 걸 막는다.")]
    [SerializeField, Min(0.5f)] float _maxFlightTime = 4f;

    [Header("Aim")]
    [Tooltip("타겟의 발치가 아니라 이만큼 위를 노린다.")]
    [SerializeField] float _aimHeight = 1f;

    [Tooltip("겨냥이 흩어지는 각도. 0이면 정확히 타겟으로 간다.")]
    [SerializeField, Min(0f)] float _spread = 4f;

    [Tooltip("쏜 뒤에도 타겟을 따라가는 정도(도/초). 0이면 발사 순간의 방향으로 곧게 간다.")]
    [SerializeField, Min(0f)] float _homingTurnRate;

    [Header("Hit")]
    [Tooltip("이 반경 안에 든 대상을 때린다.")]
    [SerializeField, Min(0.1f)] float _hitRadius = 0.5f;

    [Tooltip("맞고도 계속 갈지. 끄면 한 번 맞히고 끝난다.\n" +
             "창처럼 꿰뚫는 것은 켜둔다 — 맞은 자리에 서지 않고 그대로 지나간다.")]
    [SerializeField] bool _pierce = true;

    [Tooltip("스치면 때리는 모드에서, 같은 대상을 다시 갈기까지의 간격.")]
    [SerializeField, Min(0.05f)] float _rehitDelay = 0.8f;

    [Header("Embed - 바닥에 박힌다")]
    [Tooltip("지형에 닿으면 그 자리에 박혀 멈춘다. 끄면 지형을 통과한다.")]
    [SerializeField] bool _embedOnImpact = true;

    [Tooltip("창끝이 표면을 이만큼 지나 박힌다. 0이면 표면에 딱 붙는다.")]
    [SerializeField, Min(0f)] float _embedDepth = 0.25f;

    /// <summary>비행이 끝났다. 시간이 다 됐거나, 관통하지 않고 맞았거나.</summary>
    public event Action onSpent;

    /// <summary>지형에 박혔다. 그냥 끝난 것과 달리, 박힌 것은 다시 쳐낼 수 있다.</summary>
    public event Action onEmbedded;

    public bool Flying { get; private set; }

    /// <summary>겨눈 대상. 쳐내면 이게 바뀐다.</summary>
    public Transform Target { get; private set; }

    /// <summary>발사 위치. 겨냥은 여기서 타겟을 향해 잡는다.</summary>
    public Vector3 Origin { get; private set; }

    Transform _ignore;
    float _damage;
    float _speedOverride;

    Vector3 _velocity;
    float _flightTimer;

    bool _contact;
    float _rehitTimer;

    readonly List<IDamageable> _alreadyHit = new List<IDamageable>();

    float Speed => _speedOverride > 0f ? _speedOverride : _speed;

    #region Arming

    /// <summary>
    /// 쏠 준비를 한다. 아직 날아가지 않는다 —
    /// 들려 있는 동안 조준만 잡아두고 발사는 나중에 하는 경우가 있기 때문이다.
    /// </summary>
    public void Arm(Vector3 origin, Transform target, Transform ignore, float damage, float speedOverride = 0f)
    {
        Origin = origin;
        Target = target;
        _ignore = ignore;
        _damage = damage;
        _speedOverride = speedOverride;
    }

    /// <summary>겨눈 대로 쏜다.</summary>
    public void Fire()
    {
        if (Flying) return;

        Launch(AimDirection());
    }

    /// <summary>
    /// 표적 없이 정해진 방향으로 쏜다. 고리를 풀어 떨어뜨릴 때처럼
    /// 겨눌 대상이 없고 방향만 있는 경우에 쓴다.
    /// 이렇게 두면 떨어지는 것도 날아가는 것과 같은 길을 지나 땅에 박힌다 — 물리가 필요 없다.
    /// </summary>
    public void FireAlong(Vector3 direction)
    {
        if (Flying) return;
        if (direction.sqrMagnitude < 0.0001f) direction = Vector3.down;

        Launch(direction.normalized);
    }

    void Launch(Vector3 direction)
    {
        Flying = true;
        _velocity = direction * Speed;
        _flightTimer = _maxFlightTime;
        _alreadyHit.Clear();
    }

    /// <summary>
    /// 날아가는 도중 겨냥을 갈아끼운다. 쳐내기가 이걸 쓴다.
    /// 무시 대상까지 함께 바꾸므로, 되날아간 것은 원래 던진 쪽을 때릴 수 있게 된다.
    /// </summary>
    public void Redirect(Transform target, Transform ignore, float damage, float speedOverride = 0f)
    {
        Origin = transform.position;
        Target = target;
        _ignore = ignore;
        _damage = damage;
        _speedOverride = speedOverride;

        Flying = true;
        _velocity = AimDirection() * Speed;
        _flightTimer = _maxFlightTime;
        _alreadyHit.Clear();
    }

    /// <summary>
    /// 날지 않고 스치기만 해도 때리게 한다. 공전 고리가 그 자체로 위험해지는 것.
    /// 맞고도 사라지지 않으므로 재타격 간격으로 조절한다.
    /// </summary>
    public void SetContactDamage(bool on, float damage, float rehitDelay)
    {
        _contact = on;
        _damage = damage;
        _rehitDelay = Mathf.Max(0.05f, rehitDelay);
        _rehitTimer = 0f;
    }

    /// <summary>모든 겨냥과 판정을 내린다. 물건이 원위치로 돌아갈 때.</summary>
    public void Disarm()
    {
        Flying = false;
        _contact = false;
        Target = null;
        _ignore = null;
        _velocity = Vector3.zero;
        _alreadyHit.Clear();
    }

    /// <summary>이 발사체를 무시할 대상. 던진 쪽이 자기 것에 맞지 않게 한다.</summary>
    public void SetIgnore(Transform ignore) { _ignore = ignore; }

    #endregion

    void FixedUpdate()
    {
        if (Flying) { Fly(); return; }
        if (_contact) Contact();
    }

    #region Flight

    void Fly()
    {
        Steer();

        Vector3 from = transform.position;

        _velocity += Vector3.down * (_gravity * Time.fixedDeltaTime);
        transform.position += _velocity * Time.fixedDeltaTime;

        // 날아가는 방향을 바라보게 둔다. 창끝이 진행 방향을 가리켜야 박히는 자세가 맞는다.
        if (_velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(_velocity.normalized, Vector3.up);

        // 맞히는 것과 멈추는 것은 별개다. 사람은 꿰뚫고 지나가고, 지형에만 박힌다.
        if (Strike() && !_pierce) { Spend(); return; }

        if (_embedOnImpact && TryEmbed(from)) return;

        _flightTimer -= Time.fixedDeltaTime;
        if (_flightTimer <= 0f) Spend();
    }

    /// <summary>
    /// 이번 스텝에 지형을 지나쳤는지 본다. 지나쳤으면 그 자리에 박고 멈춘다.
    ///
    /// 위치만 보지 않고 <b>지나온 선분</b>을 훑는다 — 빠른 창은 한 스텝에 벽 두께를 넘어
    /// 위치 검사만으로는 그냥 통과해 버린다.
    ///
    /// 레이어로 지형을 가리지 않는다. 맞을 수 있는 것과 다른 발사체를 걸러내고 남는 것이
    /// 곧 지형이라, 프로젝트의 레이어 구성에 기대지 않아도 된다.
    /// </summary>
    bool TryEmbed(Vector3 from)
    {
        Vector3 step = transform.position - from;
        float distance = step.magnitude;
        if (distance <= 0.0001f) return false;

        Vector3 direction = step / distance;

        RaycastHit[] hits = Physics.RaycastAll(from, direction, distance, ~0, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (IsSelfOrIgnored(hit.collider)) continue;
            if (hit.collider.GetComponentInParent<Boss_Throwable>() != null) continue;
            if (hit.collider.TryGetComponent(out IDamageable _)) continue;   // 사람은 뚫고 지나간다

            Embed(hit);
            return true;
        }

        return false;
    }

    void Embed(RaycastHit hit)
    {
        // 창끝이 표면을 _embedDepth 만큼 지나도록 몸을 뒤로 물린다.
        transform.position = hit.point - transform.forward * (HalfLengthForward() - _embedDepth);

        Flying = false;
        _velocity = Vector3.zero;
        onEmbedded?.Invoke();
    }

    /// <summary>진행축으로 잰 반길이. 회전한 상자의 월드 바운즈를 앞 방향에 투영해 얻는다.</summary>
    float HalfLengthForward()
    {
        Collider self = GetComponent<Collider>();
        if (self == null) return 0f;

        Vector3 e = self.bounds.extents;
        Vector3 f = transform.forward;

        return Mathf.Abs(e.x * f.x) + Mathf.Abs(e.y * f.y) + Mathf.Abs(e.z * f.z);
    }

    bool IsSelfOrIgnored(Collider c)
    {
        if (c.transform.IsChildOf(transform)) return true;
        return _ignore != null && c.transform.IsChildOf(_ignore);
    }

    /// <summary>타겟을 쥐고 있으므로 쏜 뒤에도 겨냥을 고칠 수 있다. 0이면 아무것도 하지 않는다.</summary>
    void Steer()
    {
        if (_homingTurnRate <= 0f || Target == null) return;

        Vector3 want = AimPoint() - transform.position;
        if (want.sqrMagnitude < 0.0001f) return;

        float speed = _velocity.magnitude;
        Vector3 turned = Vector3.RotateTowards(
            _velocity.normalized, want.normalized,
            _homingTurnRate * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);

        _velocity = turned * speed;
    }

    void Contact()
    {
        _rehitTimer -= Time.fixedDeltaTime;
        if (_rehitTimer > 0f) return;

        // 간격이 지났으면 맞은 기록을 지운다. 그래야 같은 대상을 다시 갈 수 있다.
        _alreadyHit.Clear();
        if (Strike()) _rehitTimer = _rehitDelay;
    }

    void Spend()
    {
        Flying = false;
        onSpent?.Invoke();
    }

    #endregion

    #region Aiming

    Vector3 AimPoint()
    {
        if (Target == null) return Origin + transform.forward * 10f;
        return Target.position + Vector3.up * _aimHeight;
    }

    Vector3 AimDirection()
    {
        Vector3 direction = AimPoint() - Origin;

        if (direction.sqrMagnitude < 0.0001f) direction = transform.forward;
        direction.Normalize();

        if (_spread > 0f)
            direction = Quaternion.Euler(
                UnityEngine.Random.Range(-_spread, _spread),
                UnityEngine.Random.Range(-_spread, _spread),
                0f) * direction;

        return direction;
    }

    #endregion

    /// <summary>반경 안에서 맞을 것을 찾아 때린다. 하나라도 때렸으면 참.</summary>
    bool Strike()
    {
        bool hit = false;
        Collider[] found = Physics.OverlapSphere(transform.position, _hitRadius);

        foreach (Collider c in found)
        {
            if (_ignore != null && c.transform.IsChildOf(_ignore)) continue;
            if (!c.TryGetComponent(out IDamageable target)) continue;

            // 발사체끼리는 서로를 때리지 않는다.
            // 던져지는 물건도 쳐낼 수 있는 대상이라, 막지 않으면 날아가던 것이 공전 중인 것을
            // 쳐내 보스 쪽으로 돌려보낸다 — 고리가 제풀에 무너진다.
            //
            // 타입이 아니라 부모를 보는 이유는 받아치기 영역 때문이다. 그건 별도 부품이지만
            // 창에 딸린 것이므로, 이름을 하나씩 나열하면 부품이 늘 때마다 빠뜨리게 된다.
            if (c.GetComponentInParent<Boss_Throwable>() != null) continue;

            if (_alreadyHit.Contains(target)) continue;

            _alreadyHit.Add(target);
            target.TakeDamage(_damage);
            hit = true;

            if (!_pierce) break;
        }

        return hit;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _hitRadius);

        if (Target == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, Target.position + Vector3.up * _aimHeight);
    }
#endif
}
