using UnityEngine;

/// <summary>
/// 뛰어올라 낙하하며 내리꽂는 스킬.
///
/// 골격의 세 구간을 그대로 쓴다.
///   준비(windupTime) = 상승,  발동(activeTime) = 낙하,  후딜(recoveryTime) = 착지 후
///
/// 발동 구간에 골격이 히트박스를 켜주므로, 준비 시간을 상승 시간에 맞추면
/// 정점에서 칼이 켜지고 떨어지는 내내 판정이 살아 있다.
///
/// 조준은 두 번 한다.
///   이륙 시 : 전체 체공 시간 기준으로 대략 겨냥
///   정점 시 : 하강 속도를 실은 뒤 남은 시간으로 다시 겨냥 (여기서 궤도가 확정된다)
/// 정점 이후로는 손대지 않으므로, 늦게 피하면 피할 수 있다.
/// </summary>
public class Boss_JumpSmash : Boss_SkillBase
{
    [Header("Jump Smash")]
    [Tooltip("도달 높이(미터).")]
    [SerializeField, Min(0f)] float _jumpHeight = 8f;

    [Tooltip("정점에서 꽂아 넣을 하강 속도의 배율. 1이면 자유낙하와 같은 속도로 시작한다.")]
    [SerializeField, Min(0f)] float _slamMultiplier = 1f;

    [Tooltip("도약 수평 속도의 상한. 정점 재조준의 보정 폭도 이 값이 제한한다.")]
    [SerializeField, Min(0f)] float _maxLeapSpeed = 12f;

    public override void Enter()
    {
        if (manager.steering != null) manager.steering.onApex += OnApex;
        base.Enter();
    }

    public override void Exit()
    {
        if (manager.steering != null) manager.steering.onApex -= OnApex;
        base.Exit();
    }

    protected override void OnWindup()
    {
        manager.LookTowards(manager.Player);

        Boss_Steering steering = manager.steering;
        if (steering == null) return;

        steering.JumpToHeight(_jumpHeight);
        AimOver(steering, steering.EstimateAirTime(_jumpHeight));
    }

    /// <summary>정점. 하강 속도를 싣고, 그 속도 기준으로 남은 시간을 다시 계산해 조준한다.</summary>
    void OnApex()
    {
        Boss_Steering steering = manager.steering;
        if (steering == null) return;

        // 순서가 중요하다. 먼저 꽂아야 EstimateTimeToLand가 실제 낙하를 반영한다.
        float freeFall = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * _jumpHeight);
        steering.Slam(freeFall * _slamMultiplier);

        AimOver(steering, steering.EstimateTimeToLand());
    }

    /// <summary>주어진 시간 안에 현재 거리를 덮을 수평 속도를 실어준다.</summary>
    void AimOver(Boss_Steering steering, float seconds)
    {
        if (!manager.HasPlayer || seconds <= 0f) return;

        float speed = Mathf.Min(manager.DistanceToPlayer() / seconds, _maxLeapSpeed);
        steering.SetHorizontalVelocity(manager.DirectionToPlayer() * speed);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 준비/발동 구간이 실제 궤도와 어긋나면 정점이 아닌 데서 칼이 켜진다.
        Boss_SkillSlot slot = GetComponent<Boss_SkillSlot>();
        Boss_Steering steering = GetComponentInParent<Boss_Steering>();
        if (slot == null || steering == null) return;

        float rise = steering.EstimateRiseTime(_jumpHeight);
        if (Mathf.Abs(slot.Profile.windupTime - rise) > 0.1f)
        {
            Debug.LogWarning(
                $"[{name}] windupTime({slot.Profile.windupTime:0.00})이 상승 시간({rise:0.00})과 어긋납니다. " +
                $"정점에서 히트박스가 켜지게 하려면 맞춰주세요.", this);
        }
    }
#endif
}
