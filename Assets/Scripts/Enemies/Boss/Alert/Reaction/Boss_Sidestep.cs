using UnityEngine;

/// <summary>
/// 옆으로 빠진다. 되날아오는 창을 피할 때 쓴다.
///
/// <see cref="Boss_Dodge"/>와 다르다. 그쪽은 플레이어 <b>반대</b>로 물러나는데,
/// 날아오는 것은 보스와 플레이어를 잇는 선을 타고 오므로 뒤로 빠져봐야 계속 그 선 위에 있다.
/// 옆으로 나가야 선에서 벗어난다.
///
/// 어느 쪽으로 갈지는 재보고 정한다. 벽을 등지고 있을 때 벽 쪽으로 뛰면
/// 제자리에서 버둥거리다 그대로 맞는다 — 피하는 시늉만 하고 결과가 같으면 회피가 아니다.
/// </summary>
public class Boss_Sidestep : BaseEntityState<BossManager>
{
    float _timer;
    Vector3 _direction;

    public override void Enter()
    {
        _timer = manager.dodgeTime;
        _direction = ChooseSide();

        manager.PlayAnimation(manager.dodgeTrigger);

        // 옆으로 빠지면서도 플레이어를 계속 본다. 등을 보이면 다음 대응이 늦는다.
        manager.LookTowards(manager.Player);
        manager.Move(_direction * manager.dodgeSpeed);
    }

    public override void UpdateState()
    {
        // 속도는 상태 밖에 남으므로 매 프레임 다시 밀어준다.
        manager.Move(_direction * manager.dodgeSpeed);

        _timer -= Time.deltaTime;
        Transitions();
    }

    public override void Exit()
    {
        manager.Stop();
    }

    public override void Transitions()
    {
        if (_timer > 0f) return;

        if (fsm is Boss_ReactionMachine machine) machine.Complete();
    }

    #region Choosing a side

    /// <summary>더 트인 쪽. 양쪽이 같으면 무작위로 고른다.</summary>
    Vector3 ChooseSide()
    {
        Vector3 right = CustomMath.RemoveY(manager.character.right).normalized;
        if (right.sqrMagnitude < 0.0001f) right = Vector3.right;

        float toRight = Clearance(right);
        float toLeft = Clearance(-right);

        if (Mathf.Abs(toRight - toLeft) < 0.1f)
            return Random.value < 0.5f ? right : -right;

        return toRight > toLeft ? right : -right;
    }

    /// <summary>
    /// 그 방향으로 얼마나 갈 수 있는지. 막힌 것이 없으면 재려던 거리를 그대로 돌려준다.
    ///
    /// 자기 몸과 창과 플레이어는 장애물로 세지 않는다.
    /// 창은 지나갈 것이고, 플레이어는 밀고 지나가면 되며, 자기 콜라이더는 언제나 발밑에 있다.
    /// 레이어에 기대지 않고 이 셋만 걸러내면 남는 것이 곧 지형이다.
    /// </summary>
    float Clearance(Vector3 direction)
    {
        float distance = manager.sidestepDistance;
        Vector3 origin = manager.character.position;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin, manager.sidestepProbeRadius, direction, distance, ~0, QueryTriggerInteraction.Ignore);

        float nearest = distance;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(manager.character)) continue;
            if (hit.collider.GetComponentInParent<Boss_Throwable>() != null) continue;
            if (hit.collider.TryGetComponent(out IDamageable _)) continue;

            if (hit.distance < nearest) nearest = hit.distance;
        }

        return nearest;
    }

    #endregion
}
