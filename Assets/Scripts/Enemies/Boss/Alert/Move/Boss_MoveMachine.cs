using UnityEngine;

/// <summary>
/// 이동 서브머신. 거리에 따라 접근과 후퇴를 오가고,
/// 사거리에 들어오고 쿨다운이 풀리면 스킬 서브머신으로 넘긴다.
///
/// Initial State: Boss_Approach
/// </summary>
public class Boss_MoveMachine : FiniteStateMachine
{
    BossManager _boss;

    BossManager Boss
    {
        get
        {
            if (_boss == null) _boss = manager as BossManager;
            return _boss;
        }
    }

    public override void Transitions()
    {
        ToSkill();
    }

    void ToSkill()
    {
        if (Boss == null || !Boss.HasPlayer) return;

        // 너무 붙어 있으면 먼저 거리를 벌린다. 스킬은 그 다음이다.
        if (Boss.DistanceToPlayer() < Boss.retreatRadius) return;

        // 어느 슬롯이 쓸 수 있는지는 스킬 머신만 안다. 여기서 판단하지 않는다.
        if (fsm is Boss_Alert alert && alert.AnySkillReady()) alert.ToSkill();
    }
}
