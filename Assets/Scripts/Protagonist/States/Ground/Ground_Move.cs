using UnityEngine;

/// <summary>지상 이동(Run/Walk). 입력이 끊기면 Idle로 돌아간다.</summary>
public class Ground_Move : BaseCharacterState
{
    public override void Enter()
    {
        characterManager.Animation.Play("Run");
    }

    public override void FixedUpdateState()
    {
        Move();
        Transitions();
    }

    public override void Exit()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void Transitions()
    {
        if (InputManager.CharacterMove.sqrMagnitude <= 0.01f) fsm.TransitTo<Ground_Idle>();
    }

    void Move()
    {
        Vector3 input = InputManager.CharacterMove;
        characterManager.Steering.Move(input * characterManager.GroundSpeed, Ground.GroundHit.normal);
    }

    Character_Ground Ground => fsm as Character_Ground;
}
