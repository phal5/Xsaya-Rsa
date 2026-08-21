using UnityEngine;

/// <summary>지상 대기. 이동 입력이 들어오면 Run/Walk로 넘긴다.</summary>
public class Ground_Idle : BaseCharacterState
{
    public override void Enter()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
        characterManager.Animation.Play("Idle");
    }

    public override void FixedUpdateState()
    {
        // 속도는 상태 밖(Character_Movement)에 남으므로 계속 0을 눌러준다.
        characterManager.Steering.Move(Vector3.zero, Ground.GroundHit.normal);
        Transitions();
    }

    public override void Transitions()
    {
        if (InputManager.CharacterMove.sqrMagnitude > 0.01f) fsm.TransitTo<Ground_Move>();
    }

    Character_Ground Ground => fsm as Character_Ground;
}
