using UnityEngine;

public class Agnostos_Stun : BaseEntityState<AgnostosManager>
{
    float _timer = 0;

    public override void Enter()
    {
        _timer = manager.stunRecoveryTime;

        // 이동 속도는 상태 밖(SingularFlatMovement.targetVelocity)에 남아 계속 적용된다.
        // 진행 중이던 공격도 ImpactFrame 콜백을 못 받으므로 여기서 직접 거둔다.
        manager.movement.Move(Vector3.zero);
        manager.weapon.EndAttack();
    }

    public override void UpdateState()
    {
        _timer -= Time.deltaTime;
        Transitions();
    }

    public override void Transitions()
    {
        ToSober();
    }

    private void ToSober()
    {
        if(_timer <= 0)
        {
            fsm.TransitTo<Agnostos_Sober>();
        }
    }
}
