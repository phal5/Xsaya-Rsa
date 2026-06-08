using UnityEngine;
using UnityEngine.InputSystem;

public class Character_Ground : BaseCharacterState
{
    RaycastHit _raycastHit;

    public override void Bootstrap()
    {
        Debug.Log("Character_Ground Bootstrap complete!");
    }

    public override void Enter()
    {
        InputManager.instance.move_jump.action.performed += Jump;
        characterManager.movement.SetMovementMode(false);
    }

    public override void UpdateState()
    {
        Move();
        Transitions();
    }

    public override void Exit()
    {
        InputManager.instance.move_jump.action.performed -= Jump;
    }

    public override void Transitions()
    {
        ToAirborne();
    }

    private void ToAirborne()
    {
        if (!characterManager.caster.Cast(out _raycastHit))
            fsm.TransitTo<Character_Airborne>();
    }

    private void Move()
    {
        Vector3 steering = InputManager.CharacterMove;
        float speed = characterManager.groundSpeed;
        characterManager.movement.Move(steering * speed);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        characterManager.movement.SetSamplerYVelocity(characterManager.jumpSpeed);
    }
}