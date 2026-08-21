using UnityEngine;

/// <summary>
/// 벽을 차고 반대쪽으로 뛴다. 발판이 있어야 차므로 Braced에서만 들어온다.
///
/// 클립은 자세만 주고, 몸을 밀어내는 것은 처음에 실어준 속도와 중력이다.
/// 수직만 도약이 정하고 수평은 방향키가 정한다 — 지상 점프와 같다.
///
/// <b>붙들지 않는다.</b> 클립을 걸고 곧바로 공중 축에 넘긴다 —
/// 차고 난 뒤는 그냥 공중이고, 공중 도약도 조종도 회피도 공중 축이 준다.
/// 여기 머무는 프레임만큼 그것들이 늦어질 뿐이다.
/// </summary>
public class Ledge_WallJump : LedgeState
{
    public override void Enter()
    {
        base.Enter();

        Ledge.Launch();

        Begin(Grab.wallJump);

        // 이 자세는 이미 정해졌다고 표시한다. 없으면 공중 축이 같은 프레임에 제 점프 자세로 덮어
        // 차는 동작이 현재 클립이 되어 보지도 못한다.
        characterManager.Animation.Claim();
    }

    public override void FixedUpdateState()
    {
        Transitions();
    }

    public override void Transitions()
    {
        if (!Held) return;

        Ledge.Drop();
    }
}
