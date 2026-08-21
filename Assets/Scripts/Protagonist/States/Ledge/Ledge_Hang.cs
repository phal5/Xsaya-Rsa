using UnityEngine;

/// <summary>
/// 매달려 기다린다.
///
/// 중력을 끄고 속도를 턴 것은 축(<see cref="Character_Ledge"/>)이 이미 했고,
/// 오르기·놓기·벽점프 버튼도 축이 받는다. 여기 남은 것은 <b>방향 입력으로 놓는 일</b> 하나다.
///
/// 방향 쪽만 여기 있는 이유는 진입 동작 중에는 받으면 안 되기 때문이다.
/// 가장자리에서 걸어 나가며 내려온 참이면 그 방향키를 아직 쥐고 있을 수 있어,
/// 축에 두면 매달리자마자 도로 손을 놓는다.
/// </summary>
public class Ledge_Hang : LedgeState
{
    public override void Enter()
    {
        base.Enter();

        Begin(Grab.Hang(Ledge.Braced));
    }

    public override void FixedUpdateState()
    {
        // 손이 턱에 박혀 있고 몸이 흔들리는 클립이다. 몸을 클립에 맡겨야 그 관계가 지켜진다 —
        // 붙들어 두면 흔들림이 갈 곳이 손밖에 없어 손이 턱을 떠난다.
        Follow();
        Transitions();
    }

    /// <summary>벽 반대쪽으로 밀면 손을 놓는다. 버튼과 달리 누르고 있는 동안 계속 성립한다.</summary>
    public override void Transitions()
    {
        Vector3 pushing = WorldMove();
        if (pushing == Vector3.zero) return;

        // facing이 벽을 보고 있으므로 그 앞이 벽 쪽이다. 반대로 미는 만큼이 음수로 나온다.
        Vector3 toWall = Ledge.Anchor.facing * Vector3.forward;
        if (Vector3.Dot(pushing, toWall) > -Grab.releasePush) return;

        fsm.TransitTo<Ledge_Release>();
    }

    /// <summary>
    /// 조종 입력을 월드 방향으로 바꾼다.
    /// <see cref="InputManager.CharacterMove"/>는 카메라 공간이라 그대로 벽 법선과 견줄 수 없다.
    /// </summary>
    static Vector3 WorldMove()
    {
        Vector3 input = InputManager.CharacterMove;
        if (input.sqrMagnitude < 0.01f) return Vector3.zero;

        Camera camera = Camera.main;
        Vector3 world = camera != null ? camera.transform.TransformVector(input) : input;

        world = CustomMath.RemoveY(world);
        return world.sqrMagnitude < 0.0001f ? Vector3.zero : world.normalized;
    }
}
