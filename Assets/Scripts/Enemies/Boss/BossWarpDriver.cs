using UnityEngine;

/// <summary>
/// 보스의 상태를 소용돌이 세기로 옮긴다. 체력이 닳을수록 세계가 일그러지고,
/// 공격을 준비하는 동안 한 번 더 밀린다.
///
///   기본  (1 - 체력비율) x <see cref="_scale"/>
///   시전  거기에 <see cref="_castBonus"/>를 더한다. 스킬이 끝나면 증분만 걷힌다.
///
/// <b>부드럽게 만드는 일은 여기서 하지 않는다.</b> 목표값만 <see cref="MaterialFloatTransition"/>에게
/// 넘기고, 그쪽이 제 시간과 곡선으로 따라간다. 두 곳에서 각자 다듬으면 어느 쪽 속도가 보이는지
/// 알 수 없게 된다.
///
/// <b>값이 바뀔 때만 넘긴다.</b> 매 프레임 넘기면 그쪽 전환 코루틴이 매번 다시 시작해
/// 첫 프레임에서 영영 벗어나지 못한다 - 부드럽게 하려던 것이 오히려 멎는다.
/// </summary>
public class BossWarpDriver : MonoBehaviour
{
    [Tooltip("어느 보스를 따라갈지. 비워두면 자기 위쪽에서 찾는다.")]
    [SerializeField] BossManager _boss;

    [Tooltip("같은 값을 받을 대상들. Warp High / Warp Low / Warp Weapon의 _T를 여기 늘어놓는다.")]
    [SerializeField] MaterialFloatTransition[] _targets;

    [Tooltip("체력이 다 닳았을 때의 세기. 기본값은 (1 - 체력비율)에 곱해진다.")]
    [SerializeField] float _scale = 0.04f;

    [Tooltip("공격을 준비하는 동안 더할 양. 스킬이 끝나면 이 증분만 걷힌다.")]
    [SerializeField] float _castBonus = 0.005f;

    /// <summary>마지막으로 넘긴 값. 처음에는 어떤 값과도 같지 않아야 하므로 NaN으로 둔다.</summary>
    float _pushed = float.NaN;

    void Awake()
    {
        if (_boss == null) _boss = GetComponentInParent<BossManager>(true);
    }

    void Update()
    {
        if (_boss == null || _targets == null) return;

        float value = Value();

        // NaN은 어떤 비교에도 거짓이라 첫 프레임은 반드시 통과한다.
        if (value == _pushed) return;

        _pushed = value;

        foreach (MaterialFloatTransition target in _targets)
            if (target != null) target.SetTargetValue(value);
    }

    float Value()
    {
        float t = 1f - Ratio();

        return t * _scale + (_boss.casting ? _castBonus : 0f);
    }

    /// <summary>
    /// 남은 체력 비율. 체력이 없거나 최대치가 0이면 <b>가득 찬 것으로 본다</b> —
    /// 그래야 잴 수 없는 상황에서 세계가 멋대로 일그러지지 않는다.
    /// </summary>
    float Ratio()
    {
        DamagableBase health = _boss.health;
        if (health == null || health.MaxHealth <= 0f) return 1f;

        return Mathf.Clamp01(health.CurrentHealth / health.MaxHealth);
    }
}
