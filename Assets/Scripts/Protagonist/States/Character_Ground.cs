using UnityEngine;
using UnityEngine.InputSystem;

public class Character_Ground : BaseCharacterState
{
    RaycastHit _raycastHit;
    float coyoteTimer;

    public override void Bootstrap()
    {
        Debug.Log("Character_Ground Bootstrap complete!");
    }

    public override void Enter()
    {
        InputManager.instance.move_jump.action.performed += Jump;
        InputManager.instance.move_jump.action.canceled += CancelJump;
        characterManager.Steering.Ground();
    }

    public override void UpdateState()
    {
        Move();
        Transitions();
    }

    public override void Exit()
    {
        InputManager.instance.move_jump.action.performed -= Jump;
        InputManager.instance.move_jump.action.canceled -= CancelJump;
    }

    public override void Transitions()
    {
        ToAirborne();
    }

    private void ToAirborne()
    {
        if (!characterManager.Caster.Cast(out _raycastHit))
            coyoteTimer -= Time.deltaTime;
        else coyoteTimer = characterManager.CoyoteTime;

        if (coyoteTimer < 0) fsm.TransitTo<Character_Airborne>();
    }

    private void Move()
    {
        Vector3 steering = InputManager.CharacterMove;
        float speed = characterManager.GroundSpeed;
        characterManager.Steering.Move(steering * speed, _raycastHit.normal);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        characterManager.Steering.Jump(characterManager.JumpSpeed);
    }

    private void CancelJump(InputAction.CallbackContext _)
    {
        characterManager.Steering.Drop();
    }
}