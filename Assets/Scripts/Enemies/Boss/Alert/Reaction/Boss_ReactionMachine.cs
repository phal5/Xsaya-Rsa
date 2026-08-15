using UnityEngine;

/// <summary>
/// 반응 서브머신. 진입 판정("플레이어 공격 감지")은 Boss_Alert가 이미 끝냈고,
/// 여기서는 회피와 방어 중 무엇을 할지만 고른다.
///
/// Initial State는 지정하지 않는다. 진입 시 Enter가 직접 고른다.
/// </summary>
public class Boss_ReactionMachine : FiniteStateMachine
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
        if (Boss == null)
        {
            Complete();
            return;
        }

        // 붙어 있으면 피할 공간이 없으니 막고, 여유가 있으면 빠진다.
        if (Boss.DistanceToPlayer() <= Boss.guardPreferenceRadius) TransitTo<Boss_Guard>();
        else TransitTo<Boss_Dodge>();
    }

    /// <summary>회피/방어가 끝나면 이동으로 되돌린다.</summary>
    public void Complete()
    {
        if (fsm is Boss_Alert alert) alert.ToMovement();
    }
}
