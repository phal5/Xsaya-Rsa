using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character_Airborne : BaseState
{
    uint _jumpTicket = 1;
    uint _dashTicket = 1;
    bool _jump = false;

    public override void Bootstrap()
    {
        Debug.Log("Character_Airborne Bootstrap Complete!");
    }

    public override void Enter()
    {
        characterManager.movement.IntegrateVelocitySpace();
        InputManager.instance.move_jump.action.performed += RegisterJump;
        _jumpTicket = 1;
        _dashTicket = 1;
        _jump = false;
        return;
    }

    public override void UpdateState()
    {
        Transitions();
        Move();
        if (_jump) Jump();
    }

    public override void Exit()
    {
        InputManager.instance.move_jump.action.performed -= RegisterJump;
        return;
    }

    public override void Transitions()
    {
        ToGround();
        return;
    }

    private void ToGround()
    {
        if (characterManager.caster.Cast(out _))
        {
            fsm.TransitTo<Character_Ground>();
        }
    }

    private void Move()
    {
        Vector3 steering = InputManager.CharacterMove;
        float speed = characterManager.airborneSpeed;
        characterManager.movement.Move(steering * speed);
    }

    private void Jump()
    {
        if (_jumpTicket <= 0) return;
        _jumpTicket--;
        characterManager.movement.SetSamplerYVelocity(characterManager.jumpSpeed);
    }

    private void RegisterJump(InputAction.CallbackContext _)
    {
        _jump = true;
    }
}
