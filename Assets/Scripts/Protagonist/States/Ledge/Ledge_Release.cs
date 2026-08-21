using UnityEngine;

/// <summary>
/// 놓기. 중력에 넘기고 곧바로 공중 축으로 보낸다.
///
/// 붙들지 않는다. 손을 뗀 뒤는 그냥 낙하이고, 낙하도 공중 도약도 공중 축의 것이다.
/// 넘기는 일을 다음 물리 프레임에 하는 이유는 <see cref="Ledge_WallJump"/>와 같다.
/// </summary>
public class Ledge_Release : LedgeState
{
    public override void Enter()
    {
        base.Enter();

        // 붙어 있는 동안 껐던 것을 되돌린다. 이제부터 몸은 물리가 가져간다.
        characterManager.Movement.SetGravity(true);
    }

    public override void FixedUpdateState()
    {
        Transitions();
    }

    public override void Transitions()
    {
        Ledge.Drop();
    }
}
