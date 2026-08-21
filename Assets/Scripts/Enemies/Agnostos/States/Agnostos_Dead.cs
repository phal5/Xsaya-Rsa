using UnityEngine;

/// <summary>
/// 사망. 한 번 들어오면 나가지 않는 흡수 상태다.
///
/// <b>루트 직계</b>다. 피격을 거쳐 들어오는 것이 보통이지만 피격에 속하지는 않는다 —
/// 경직은 지나가는 구간이고 사망은 끝이라, 같은 층에 두면 "피격이 끝나면 원래대로"가 깨진다.
///
/// 이 상태가 성립하려면 Agnostos(DamagableBase)의 Destroy On Death를 꺼야 한다.
/// 켜져 있으면 체력이 0이 되는 순간 오브젝트가 사라져, 여기 들어올 몸이 남지 않는다.
/// 소멸은 여기서 시간을 두고 직접 한다.
/// </summary>
public class Agnostos_Dead : BaseEntityState<AgnostosManager>
{
    float _despawnAt;

    public override void Enter()
    {
        // 속도는 상태 밖(SingularFlatMovement)에 남으므로 여기서 거둔다.
        // 진행 중이던 공격도 ImpactFrame 콜백을 못 받으니 히트박스를 직접 내린다.
        if (manager.movement != null) manager.movement.Move(Vector3.zero);
        if (manager.weapon != null) manager.weapon.EndAttack();

        DisableBody();

        _despawnAt = Time.time + manager.despawnDelay;
    }

    public override void UpdateState()
    {
        if (Time.time < _despawnAt) return;

        Object.Destroy(manager.gameObject);
    }

    /// <summary>
    /// 몸을 물리와 판정에서 걷어낸다.
    /// 리지드바디는 지우지 않고 재우기만 한다 — 지우면 그 프레임에 콜라이더가 정적으로 재계산되며 튄다.
    /// </summary>
    void DisableBody()
    {
        foreach (Collider c in manager.GetComponentsInChildren<Collider>(true))
            c.enabled = false;

        foreach (Rigidbody body in manager.GetComponentsInChildren<Rigidbody>(true))
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
        }

        foreach (SingularFlatMovement move in manager.GetComponentsInChildren<SingularFlatMovement>(true))
            move.enabled = false;
    }

    /// <summary>나가는 길이 없다. 흡수 상태라는 뜻이 이 빈 몸에 들어 있다.</summary>
    public override void Transitions() { }
}
