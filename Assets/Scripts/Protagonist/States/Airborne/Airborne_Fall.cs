using UnityEngine;

/// <summary>공중 기본 상태. 조종 입력이 들어오면 Midair Move로 넘긴다.</summary>
public class Airborne_Fall : BaseCharacterState
{
    public override void Enter()
    {
        base.Enter();
    }

    public override void FixedUpdateState()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
        Transitions();
    }

    public override void Transitions()
    {
        if (InputManager.CharacterMove.sqrMagnitude > 0.01f) fsm.TransitTo<Airborne_Move>();
    }
}
