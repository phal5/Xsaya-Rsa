using UnityEngine;

public class Agnostos_Attack : BaseEntityState<AgnostosManager>
{
    float timer;

    public override void Bootstrap()
    {

    }

    public override void Enter()
    {

    }

    public override void UpdateState()
    {

        Transitions();
    }

    public override void Exit()
    {

    }

    public override void Transitions()
    {
        ToApproach();
    }

    private void ToApproach()
    {
        Vector3 disparity = PlayerManager.instance.player.position - manager.character.position;
        float r = manager.attackRadius;
        if (disparity.sqrMagnitude > r * r)
        {
            fsm.TransitTo<Agnostos_Approach>();
        }
    }
}
