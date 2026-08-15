using UnityEngine;

public class Agnostos_Sober : FiniteStateMachine
{
    [SerializeField] Agnostos agnostos;

    public override void Enter()
    {
        agnostos.AddDamageListener(ToStun);
        base.Enter();
    }

    public override void Exit()
    {
        agnostos.RemoveDamageListener(ToStun);
        base.Exit();
    }

    private void ToStun(float _)
    {
        // 치명타에서는 곧바로 파괴되므로 스턴에 들어가지 않는다.
        if (agnostos.CurrentHealth <= 0f) return;

        // 컴포넌트 상태는 부모 FSM이 구동하므로 enabled는 항상 false다.
        // 활성 여부는 Enter/Exit의 구독-해제가 이미 보장한다.
        fsm.TransitTo<Agnostos_Stun>();
    }
}
