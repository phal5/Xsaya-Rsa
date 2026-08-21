using UnityEngine;

/// <summary>올라서기. 클립이 몸을 올리고, 끝나면 축이 설 자리에 놓으며 넘긴다.</summary>
public class Ledge_Climb : LedgeState
{
    public override void Enter()
    {
        base.Enter();

        Begin(Grab.Climb(Ledge.Braced));
    }

    public override void FixedUpdateState()
    {
        Follow();
        Transitions();
    }

    public override void Transitions()
    {
        if (!Held) return;

        Ledge.Finish();
    }
}
