using UnityEngine;

/// <summary>
/// 놓기. 중력에 넘기고, <b>코요테 시간만큼 붙들었다가</b> 공중 축으로 보낸다.
///
/// 붙들고 있는 동안 몸은 이미 물리가 가져가 그냥 떨어진다 — 축만 아직 넘기지 않을 뿐이다.
/// 지상의 코요테와 같은 모양이다: 지상 축도 발이 떨어진 뒤 CoyoteTime 동안은 축을 넘기지 않아
/// 그 사이의 점프가 지상 점프로 나간다.
///
/// 여기서 사는 것은 <b>방금까지 잡고 있었으니 벽을 찬 것으로 친다</b>는 관용이다.
/// 이 창이 없으면 놓자마자 누른 점프가 조용히 공중 도약이 되어, 벽을 차려던 입력이
/// 다른 동작으로 바뀐다. 발판이 없으면 어차피 벽을 못 차므로 이 관용은 Braced에만 뜻이 있다.
/// </summary>
public class Ledge_Release : LedgeState
{
    float _releasedAt;

    public override void Enter()
    {
        base.Enter();

        _releasedAt = Time.time;

        // 붙어 있는 동안 껐던 것을 되돌린다. 이제부터 몸은 물리가 가져간다.
        //
        // 축보다 먼저 되돌리는 것은 붙들고 있는 동안에도 떨어져야 하기 때문이다.
        // 떼어둔 채로 기다리면 놓고도 0.2초 동안 허공에 멈춰 선다.
        characterManager.Movement.Detach(false);
        characterManager.Movement.SetGravity(true);
    }

    public override void FixedUpdateState()
    {
        Transitions();
    }

    public override void Transitions()
    {
        if (Time.time - _releasedAt < characterManager.CoyoteTime) return;

        Ledge.Drop();
    }
}
