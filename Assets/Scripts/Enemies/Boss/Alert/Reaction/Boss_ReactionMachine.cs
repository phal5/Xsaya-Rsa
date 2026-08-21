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

    /// <summary>
    /// 다음 진입에서 옆으로 빠지라는 표시. 되날아오는 창을 알린 쪽이 세운다.
    ///
    /// 거리로 고르는 평소 규칙을 건너뛴다 — 날아오는 것에는 방어가 소용없고,
    /// 뒤로 물러나는 것도 소용없다. 사선에서 벗어나는 것만이 답이다.
    /// </summary>
    bool _sidestepNext;

    public void RequestSidestep() { _sidestepNext = true; }

    public override void Enter()
    {
        if (Boss == null)
        {
            Complete();
            return;
        }

        if (_sidestepNext)
        {
            _sidestepNext = false;
            TransitTo<Boss_Sidestep>();
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
