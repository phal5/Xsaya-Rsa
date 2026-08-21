using UnityEngine;

/// <summary>
/// 멀쩡할 때의 서브머신. 잠들고, 다가가고, 때리고, 물러나는 것이 전부 여기 안에서 일어난다.
///
/// 루트의 기본 상태다. 예전에는 이 상태들이 루트에 직접 달려 있었는데,
/// 그러면 "싸우는 중"과 "맞아서 끊긴 중"이 같은 층에 놓여 서로를 가로막는지 아닌지가 흐려진다.
/// 한 겹 내려두면 피격은 <b>이 머신 전체를 통째로 밀어내는 것</b>이 되어, 관계가 배치로 읽힌다.
///
/// 피격 구독을 여기서 하는 것도 같은 이유다. 맞아서 이 머신을 떠나면 구독이 함께 풀리므로,
/// 경직 중에 또 맞아 경직이 무한히 갱신되는 일이 구조적으로 생기지 않는다.
///
/// Initial State: Agnostos_Sleep
/// </summary>
public class Agnostos_Sober : FiniteStateMachine
{
    AgnostosManager Agnostos => manager as AgnostosManager;

    public override void Bootstrap()
    {
        if (Agnostos == null)
            Debug.LogError($"[{name}] manager에 AgnostosManager가 연결되어 있지 않습니다.", this);
    }

    public override void Enter()
    {
        if (Agnostos != null && Agnostos.agnostosEnemy != null)
            Agnostos.agnostosEnemy.AddDamageListener(OnDamaged);

        base.Enter();
    }

    public override void Exit()
    {
        if (Agnostos != null && Agnostos.agnostosEnemy != null)
            Agnostos.agnostosEnemy.RemoveDamageListener(OnDamaged);

        base.Exit();
    }

    // 피격은 OnTriggerEnter에서 올라오므로 갱신 주기 밖이다.
    // 콜백은 표시만 남기고 전이는 Transitions()에서 한다.
    bool _hitRequested;

    void OnDamaged(float _)
    {
        _hitRequested = true;
    }

    public override void Transitions()
    {
        if (!_hitRequested) return;
        _hitRequested = false;

        // 경직이냐 사망이냐는 여기서 가리지 않는다. 피격 머신이 골라준다.
        fsm?.TransitTo<Agnostos_Damage>();
    }
}
