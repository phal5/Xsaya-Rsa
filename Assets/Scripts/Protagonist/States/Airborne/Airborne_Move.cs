using UnityEditor;
using UnityEngine;

/// <summary>공중 조종(Midair Move). 입력이 끊기면 Fall로 돌아간다.</summary>
public class Airborne_Move : BaseCharacterState
{
    public override void FixedUpdateState()
    {
        Vector3 input = InputManager.CharacterMove;
        characterManager.Steering.Move(input * characterManager.AirborneSpeed, Vector3.up);
        Transitions();
    }

    public override void Enter()
    {
        Debug.Log("Entered Airborne_Jump.");
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
