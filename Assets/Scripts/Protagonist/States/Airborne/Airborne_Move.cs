using UnityEngine;

/// <summary>
/// 공중 조종(Midair Move). 입력이 끊기면 Fall로 돌아간다.
///
/// 애니메이션은 걸지 않는다. 이 상태와 Fall은 <b>입력 유무</b>로 갈리는데
/// 점프 클립은 상승·하강으로 갈리므로, 여기서 재생을 건드리면 공중에서 방향키를
/// 넣었다 뗄 때마다 클립이 앞으로 되감긴다. 재생은 Character_Airborne이 통째로 맡는다.
/// </summary>
public class Airborne_Move : BaseCharacterState
{
    public override void FixedUpdateState()
    {
        Vector3 input = InputManager.CharacterMove;
        characterManager.Steering.Move(input * characterManager.AirborneSpeed, Vector3.up);
        Transitions();
    }

    public override void Exit()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void Transitions()
    {
        if (InputManager.CharacterMove.sqrMagnitude <= 0.01f) fsm.TransitTo<Airborne_Fall>();
    }
}
