using UnityEngine;
using UnityEngine.UIElements;

public class Agnostos_Approach : BaseEntityState<AgnostosManager>
{
    public override void Bootstrap()
    {
        manager.movement.LookTowards(PlayerManager.instance.player);
    }

    public override void Enter()
    {
        
    }

    public override void UpdateState()
    {
        Approach(PlayerManager.instance.player.position);
        Transitions();
    }

    public override void Exit()
    {
        manager.movement.Move(Vector3.zero);
    }

    public override void Transitions()
    {
        ToAttack();
    }

    private void Approach(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - manager.character.position).normalized;
        manager.movement.Move(direction * manager.speed);
    }

    private void ToAttack()
    {
        Vector3 disparity = PlayerManager.instance.player.position - manager.character.position;
        float r = manager.attackRadius;
        if (disparity.sqrMagnitude <= r * r)
        {
            fsm.TransitTo<Agnostos_Attack>();
        }
    }
}
