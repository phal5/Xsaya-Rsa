using UnityEngine;

/// <summary>
/// 공중 기본 상태. 조종 입력이 들어오면 Midair Move로 넘긴다.
/// 애니메이션을 걸지 않는 이유는 Airborne_Move와 같다.
///
/// <b>입력이 없을 때 감속시키지 않는다.</b> Move(zero)는 "0까지 줄여라"라서
/// 공중 가속도(100)로 수평 속도를 두 프레임 만에 없앤다 — 벽을 차고 나가든 뛰어서 나가든
/// 손을 떼는 순간 제자리 낙하가 된다. 여기서 할 일은 조종을 놓는 것이지 제동이 아니다.
/// </summary>
public class Airborne_Fall : BaseCharacterState
{
    public override void FixedUpdateState()
    {
        characterManager.Steering.Coast();
        Transitions();
    }

    public override void Transitions()
    {
        if (InputManager.CharacterMove.sqrMagnitude > 0.01f) fsm.TransitTo<Airborne_Move>();
    }
}
