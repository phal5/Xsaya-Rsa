using System.Collections;
using UnityEngine;

/// <summary>
/// 주변 상호작용 대상을 OverlapSphere로 훑어 후보를 모으고, 가장 가까운 하나를 고른다.
/// 최우선 후보가 바뀔 때마다 이전 대상에 Unfocus, 새 대상에 Focus를 보낸다.
///
/// 매 프레임 훑으면 비싸므로 AutoTarget과 같이 주기적으로만 검사한다.
/// </summary>
public class InteractionDetector : MonoBehaviour
{
    [SerializeField] LayerMask _layer = ~0;
    [SerializeField] float _radius = 2f;
    [SerializeField] float _interval = 0.15f;

    public IInteractable Current { get; private set; }

    public bool HasTarget => Alive(Current) && Current.Available;

    readonly Collider[] _buffer = new Collider[16];

    void OnEnable()
    {
        StartCoroutine(ScanLoop());
    }

    void OnDisable()
    {
        SetCurrent(null);
    }

    IEnumerator ScanLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(_interval);

        while (enabled)
        {
            Scan();
            yield return wait;
        }
    }

    void Scan()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _radius, _buffer, _layer);

        IInteractable nearest = null;
        float nearestSqr = float.MaxValue;
        Vector3 origin = transform.position;

        // 후보군을 훑어 거리 우선순위로 하나를 고른다.
        for (int i = 0; i < count; i++)
        {
            Collider c = _buffer[i];
            if (c == null) continue;
            if (!c.TryGetComponent(out IInteractable candidate)) continue;
            if (!candidate.Available) continue;

            float sqr = (c.ClosestPoint(origin) - origin).sqrMagnitude;
            if (sqr >= nearestSqr) continue;

            nearestSqr = sqr;
            nearest = candidate;
        }

        SetCurrent(nearest);
    }

    void SetCurrent(IInteractable next)
    {
        if (ReferenceEquals(Current, next)) return;

        if (Alive(Current)) Current.Unfocus();

        Current = next;

        if (Alive(Current)) Current.Focus();
    }

    /// <summary>
    /// 인터페이스로 들고 있으면 Unity의 파괴된 오브젝트 판정이 안 먹는다.
    /// Component로 되돌려 확인한다.
    /// </summary>
    static bool Alive(IInteractable target)
    {
        return target is Component c && c != null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
