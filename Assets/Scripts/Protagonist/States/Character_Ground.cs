using UnityEngine;
using UnityEngine.InputSystem;

public class Character_Ground : BaseState
{
    Ray _ray;

    public override void Bootstrap()
    {
        _ray.direction = Vector3.down;
        Debug.Log("Character_Ground Bootstrap complete!");
    }

    public override void Enter()
    {
        InputManager.instance.move_jump.action.performed += Jump;
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
        if (!characterManager.caster.Cast(out _)) fsm.TransitTo<Character_Airborne>();
        
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

    private void RegisterJump()
    {

    }
}