using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class Agnostos_Idle : BaseEntityState<AgnostosManager>
{
    public override void UpdateState()
    {
        Transitions();
    }

    public override void Transitions()
    {
        ToApproach();
    }

    public void ToApproach()
    {
        if (PlayerManager.instance.player.IsDestroyed()) return;

        Vector3 disparity = manager.character.position - PlayerManager.instance.player.position;
        float r = manager.awakeRadius;
        if (disparity.sqrMagnitude <= r * r)
        {
            fsm.TransitTo<Agnostos_Approach>();
        }
    }
}
