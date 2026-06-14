using UnityEngine;

public class Agnostos_Stun : BaseEntityState<AgnostosManager>
{
    float _timer = 0;

    public override void Enter()
    {
        _timer = manager.stunRecoveryTime;
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
