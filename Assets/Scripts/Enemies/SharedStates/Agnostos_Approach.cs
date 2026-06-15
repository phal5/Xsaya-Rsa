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
        for(int i = 0;  i < 4; i++)
        {
            ReturnToWalk(i);
        }
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
        Vector3 disparity = targetPosition - manager.character.position;
        Vector3 direction = disparity.normalized;
        float dist = disparity.magnitude;
        if(dist < manager.retreatRadius) direction = -direction;
        manager.movement.Move(direction * manager.speed);
    }

    private void ToAttack()
    {
        Vector3 disparity = PlayerManager.instance.player.position - manager.character.position;
        float dist = disparity.magnitude;
        if (dist <= manager.attackRadius && dist > manager.retreatRadius)
        {
            fsm.TransitTo<Agnostos_Attack>();
        }
    }

    private void ReturnToWalk(int i)
    {
        manager.footTargets[i].Transit(manager.walkTargets[i], manager.attackReadyTime, (x) => { return x; }, null);
    }
}
