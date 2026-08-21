using UnityEngine;

/// <summary>올라서기. 설 자리를 향해 클립이 몸을 올리고, 도착하면 축에 넘긴다.</summary>
public class Ledge_Climb : LedgeState
{
    public override void Enter()
    {
        base.Enter();

        Begin(Grab.Climb(Ledge.Braced));
    }

    public override void FixedUpdateState()
    {
        Follow(Ledge.Anchor.stand);
        Transitions();
    }

    public override void Transitions()
    {
        if (!Held) return;

        Ledge.Finish();
    }
}
