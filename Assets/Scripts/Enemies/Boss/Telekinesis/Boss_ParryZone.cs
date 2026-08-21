using UnityEngine;

/// <summary>
/// 받아치기 판정만 맡는 영역. 창 본체와 <b>따로</b> 둔다.
///
/// 창의 콜라이더는 창의 크기와 정확히 같아야 한다 — 그래야 박히는 자리와 보이는 자리가 맞는다.
/// 그런데 그 크기로는 사람이 쳐낼 수가 없다. 폭 15cm짜리를 검으로 맞히는 건 실질적으로 불가능하다.
///
/// 그래서 맞는 크기와 <b>쳐내는 크기</b>를 갈랐다.
///   본체 콜라이더  창 그대로. 지형에 박히고, 눈에 보이는 것과 일치한다.
///   이 영역        넉넉한 구. 여기에 검이 닿으면 창이 튕겨 나간다.
///
/// 스스로 판단하지 않는다. 맞았다는 사실만 창에게 넘기고, 쳐낼지 말지는 창이 정한다.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class Boss_ParryZone : MonoBehaviour, IDamageable
{
    [Tooltip("비워두면 부모에서 찾는다.")]
    [SerializeField] Boss_Throwable _owner;

    Boss_Throwable Owner
    {
        get
        {
            if (_owner == null) _owner = GetComponentInParent<Boss_Throwable>();
            return _owner;
        }
    }

    public float Radius
    {
        get => Collider != null ? Collider.radius : 0f;
        set { if (Collider != null) Collider.radius = value; }
    }

    SphereCollider _collider;

    SphereCollider Collider
    {
        get
        {
            if (_collider == null) _collider = GetComponent<SphereCollider>();
            return _collider;
        }
    }

    void Awake()
    {
        // 물리적으로 밀지 않는다. 오직 무기의 트리거 판정에만 걸린다.
        if (Collider != null) Collider.isTrigger = true;
    }

    /// <summary>주인공의 무기가 이걸 부른다. 판단은 창이 한다.</summary>
    public void TakeDamage(float damageAmount)
    {
        if (Owner == null) return;
        Owner.TakeDamage(damageAmount);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
#endif
}
