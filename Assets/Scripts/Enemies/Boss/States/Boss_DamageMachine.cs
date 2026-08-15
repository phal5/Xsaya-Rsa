using UnityEngine;

/// <summary>
/// 피격 서브머신. 진입 조건("피해를 입음")은 BossRoot가 판정하고,
/// 여기서는 경직이냐 사망이냐만 고른다. Boss_ReactionMachine과 같은 역할 분담이다.
///
/// 치명타로 재진입할 수 있다. 경직 중에 죽으면 Enter가 다시 돌면서 사망으로 내려간다.
///
/// Initial State는 지정하지 않는다. 진입 시 Enter가 직접 고른다.
/// </summary>
public class Boss_DamageMachine : FiniteStateMachine
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

    public override void Enter()
    {
        if (Boss == null || Boss.health == null)
        {
            Complete();
            return;
        }

        if (Boss.health.CurrentHealth <= 0f) TransitTo<Boss_Dead>();
        else TransitTo<Boss_Staggered>();
    }

    /// <summary>경직이 끝나면 전투로 되돌린다. 사망은 여기를 부르지 않는다.</summary>
    public void Complete()
    {
        if (fsm is BossRoot root) root.ToAlert();
    }
}
