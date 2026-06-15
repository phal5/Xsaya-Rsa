using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character_Airborne : BaseCharacterState
{
    uint _jumpTicket = 1;
    uint _dashTicket = 1;
    bool _jump = false;

    private void ResetValues()
    {
        _jumpTicket = 1;
        _dashTicket = 1;
        _jump = false;
    }

    public override void Bootstrap()
    {
        Debug.Log("Character_Airborne Bootstrap Complete!");
    }

    public override void Enter()
    {
        characterManager.steering.Airborne();
        InputManager.instance.move_jump.action.performed += RegisterJump;
        InputManager.instance.move_jump.action.canceled += CancelJump;
        ResetValues();
    }

    public override void Exit()
    {
        InputManager.instance.move_jump.action.performed -= RegisterJump;
        InputManager.instance.move_jump.action.canceled -= CancelJump;
    }

    public override void UpdateState()
    {
        Transitions();
        Move();
        if (_jump) Jump();
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
        characterManager.steering.Move(steering * speed, Vector3.up);
    }

    private void Jump()
    {
        if (_jumpTicket <= 0) return;
        _jumpTicket--;
        characterManager.steering.Jump(characterManager.jumpSpeed);
    }

    private void RegisterJump(InputAction.CallbackContext _)
    {
        _jump = true;
    }

    private void CancelJump(InputAction.CallbackContext _)
    {
        if(_jump) _jump = false;
        characterManager.steering.Drop();
    }
}
