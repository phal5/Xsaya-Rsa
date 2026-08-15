using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 공중 축 서브머신. Fall · Jump · Dodge · Midair Move를 담는다.
///
/// 스펙의 티켓제를 그대로 옮겼다.
///   JumpTicket = 1 (or 0),  DodgeTicket = 1 (or 0)
/// 공중에 뜰 때 각 1장씩 주어지고, 쓰면 착지 전까지 다시 못 쓴다.
///
/// 점프 버퍼는 착지 직전 입력을 살려주기 위한 것으로, 티켓과 별개로 관리한다.
///
/// Initial State: Airborne_Fall
/// </summary>
public class Character_Airborne : FiniteStateMachine
{
    uint _jumpTicket;
    uint _dodgeTicket;
    float _jumpBufferedUntil;
    bool _subscribed;

    // 입력 콜백이 세우고 Transitions()가 소비한다.
    bool _jumpRequested;
    bool _dodgeRequested;

    public CharacterManager Character => manager as CharacterManager;

    public bool HasDodgeTicket => _dodgeTicket > 0;

    public override void Enter()
    {
        Character.Steering.Airborne();

        _jumpTicket = 1;
        _dodgeTicket = 1;
        _jumpBufferedUntil = -1f;

        Subscribe();

        // 지상 축과 같은 이유로 재개하지 않는다. 공중에 들어오면 항상 Fall/Move부터.
        ToLocomotion();
    }

    public override void Exit()
    {
        Unsubscribe();
        base.Exit();
    }

    public override void Transitions()
    {
        // 입력 콜백은 표시만 남긴다. 티켓 판정과 처리는 여기서 한다.
        if (Consume(ref _jumpRequested)) Jump();

        if (Consume(ref _dodgeRequested))
        {
            if (HasDodgeTicket && _currentStateType != typeof(Airborne_Dodge))
            {
                TransitTo<Airborne_Dodge>();
                return;
            }
        }

        ToGround();
    }

    static bool Consume(ref bool flag)
    {
        if (!flag) return false;
        flag = false;
        return true;
    }

    #region Landing

    void ToGround()
    {
        // 지면 캐스트만 본다. 지상 점프가 축을 강제로 바꾸지 않으므로
        // "떴는데 아직 캐스트에 걸려 있는" 구간 자체가 생기지 않는다.
        if (!Character.GroundCaster.Cast(out _)) return;

        bool buffered = Time.time <= _jumpBufferedUntil;

        if (fsm is Character_Controlled controlled) controlled.ToGround();

        // 착지 직전에 눌린 점프를 살려준다.
        if (buffered) Character.Steering.Jump(Character.JumpSpeed);
    }

    #endregion

    /// <summary>
    /// 공중 점프 전체. 지상과 달리 이미 Singular라 축 전환이 없고, 티켓만 소모하면 된다.
    /// 상태를 바꾸지 않는다 — 뛰든 떨어지든 공중 축의 Fall/Move가 그대로 조종을 맡는다.
    /// </summary>
    void Jump()
    {
        if (_jumpTicket == 0)
        {
            // 티켓이 없으면 착지 직전 입력으로 보고 버퍼에 담는다.
            _jumpBufferedUntil = Time.time + Character.JumpBufferTime;
            return;
        }

        _jumpTicket--;
        Character.Steering.Jump(Character.JumpSpeed);
    }

    #region Tickets

    public bool ConsumeDodgeTicket()
    {
        if (_dodgeTicket == 0) return false;
        _dodgeTicket--;
        return true;
    }

    #endregion

    #region Input - 점선 화살표

    void Subscribe()
    {
        if (_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_jump.action.performed += OnJump;
        InputManager.instance.move_jump.action.canceled += OnJumpReleased;
        InputManager.instance.move_dash.action.performed += OnDodge;
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_jump.action.performed -= OnJump;
        InputManager.instance.move_jump.action.canceled -= OnJumpReleased;
        InputManager.instance.move_dash.action.performed -= OnDodge;
        _subscribed = false;
    }

    void OnJump(InputAction.CallbackContext _) { _jumpRequested = true; }

    void OnJumpReleased(InputAction.CallbackContext _)
    {
        // 전이가 아니라 속도 조작이라 즉시 처리해도 상태가 흔들리지 않는다.
        Character.Steering.Drop();
    }

    void OnDodge(InputAction.CallbackContext _) { _dodgeRequested = true; }

    #endregion

    /// <summary>하위 상태가 이동 축으로 되돌아올 때 쓴다.</summary>
    public void ToLocomotion()
    {
        if (InputManager.CharacterMove.sqrMagnitude > 0.01f) TransitTo<Airborne_Move>();
        else TransitTo<Airborne_Fall>();
    }
}
