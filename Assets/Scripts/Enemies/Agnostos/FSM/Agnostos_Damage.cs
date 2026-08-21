using UnityEngine;

/// <summary>
/// 맞았을 때 무엇이 될지 고르는 머신. 행동 트리의 셀렉터와 같은 일을 한다.
///
/// 위에서부터 훑어 먼저 성립하는 것을 고른다.
///   체력이 남았나 → 경직
///   다 깎였나     → 사망
/// 조건이 늘면 목록에 한 줄을 더할 뿐, 부르는 쪽은 달라지지 않는다.
/// Agnostos_Sober는 "맞았다"만 알리고 무엇이 될지는 묻지 않는다.
///
/// <b>사망은 여기 소속이 아니다.</b> 고르기만 하고 루트로 넘긴다 —
/// 경직은 지나가는 구간이지만 사망은 돌아오지 않는 끝이라, 둘을 같은 층에 두면
/// "피격이 끝나면 원래대로"라는 이 머신의 약속이 사망에서만 거짓이 된다.
///
/// Initial State는 쓰지 않는다. 진입 시 Enter가 직접 고른다.
/// </summary>
public class Agnostos_Damage : FiniteStateMachine
{
    AgnostosManager Agnostos => manager as AgnostosManager;

    public override void Enter()
    {
        if (Agnostos == null || Agnostos.agnostosEnemy == null)
        {
            Complete();
            return;
        }

        if (Agnostos.agnostosEnemy.CurrentHealth <= 0f)
        {
            fsm?.TransitTo<Agnostos_Dead>();
            return;
        }

        TransitTo<Agnostos_Stun>();
    }

    /// <summary>경직이 끝났다. 멀쩡한 쪽으로 되돌린다.</summary>
    public void Complete()
    {
        fsm?.TransitTo<Agnostos_Sober>();
    }
}
