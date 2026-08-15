using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 지상 축 서브머신. Idle · Run/Walk · Jump · Dodge를 담는다.
///
/// 다이어그램의 화살표 종류를 그대로 옮겼다.
///   Idle ↔ Run/Walk : 실선 = 매 프레임 판정 (하위 상태가 입력을 폴링)
///   Jump · Dodge    : 점선 = Invoke 시 판정 (이 머신이 입력 이벤트로 밀어넣는다)
///
/// 접지 판정과 코요테 타임은 축 전체의 관심사라 여기서 관리한다.
///
/// Initial State: Ground_Idle
/// </summary>
public class Character_Ground : FiniteStateMachine
{
    RaycastHit _hit;
    float _groundedUntil;
    bool _subscribed;

    // 입력 콜백이 세우고 Transitions()가 소비한다.
    bool _jumpRequested;
    bool _dodgeRequested;

    public CharacterManager Character => manager as CharacterManager;

    /// <summary>하위 상태가 경사면 이동에 쓰는 지면 노멀.</summary>
    public RaycastHit GroundHit => _hit;

    public override void Enter()
    {
        Character.Steering.Ground();

        // 코요테 타임은 누적이 아니라 만료 시각으로 잡는다.
        // Transitions()가 Update와 FixedUpdate 양쪽에서 불려 누적식은 두 배로 깎인다.
        _groundedUntil = Time.time + Character.CoyoteTime;

        Subscribe();

        // base.Enter()의 "떠났던 자리에서 재개"를 쓰면 안 된다.
        // 축을 떠날 때 Dodge 중이었으면 돌아오자마자 되살아난다. 축에 들어오면 항상 Idle/Move부터.
        ToLocomotion();
    }

    public override void Exit()
    {
        Unsubscribe();
        base.Exit();
    }

    public override void Transitions()
    {
        Probe();

        // 입력 콜백은 표시만 남긴다. 실제 처리는 상태 기계의 갱신 주기 안에서 한다.
        if (Consume(ref _jumpRequested)) Jump();

        if (Consume(ref _dodgeRequested))
        {
            if (_currentStateType != typeof(Ground_Dodge)) { TransitTo<Ground_Dodge>(); return; }
        }

        ToAirborne();
    }

    static bool Consume(ref bool flag)
    {
        if (!flag) return false;
        flag = false;
        return true;
    }

    #region Ground Probe

    void Probe()
    {
        if (Character.GroundCaster.Cast(out _hit))
            _groundedUntil = Time.time + Character.CoyoteTime;
    }

    void ToAirborne()
    {
        if (Time.time <= _groundedUntil) return;

        if (fsm is Character_Controlled controlled) controlled.ToAirborne();
    }

    /// <summary>
    /// 지상 점프. 속도만 싣고 축은 건드리지 않는다.
    ///
    /// 몸이 떠오르면 지면 캐스트가 자연히 빗나가고, 코요테가 만료되며 ToAirborne이 넘긴다.
    /// 그때까지 Mutual이 계속 돌아 sampler의 점프가 rigidbody에 반영되므로,
    /// Singular 통합이 온전한 값을 가져간다. 여기서 축을 강제로 바꾸면 그 창이 사라진다.
    /// </summary>
    void Jump()
    {
        Character.Steering.Jump(Character.JumpSpeed);
    }

    #endregion

    #region Input - 점선 화살표

    void Subscribe()
    {
        if (_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_jump.action.performed += OnJump;
        InputManager.instance.move_dash.action.performed += OnDodge;
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_jump.action.performed -= OnJump;
        InputManager.instance.move_dash.action.performed -= OnDodge;
        _subscribed = false;
    }

    void OnJump(InputAction.CallbackContext _) { _jumpRequested = true; }

    void OnDodge(InputAction.CallbackContext _) { _dodgeRequested = true; }

    #endregion

    /// <summary>하위 상태가 이동 축으로 되돌아올 때 쓴다.</summary>
    public void ToLocomotion()
    {
        if (InputManager.CharacterMove.sqrMagnitude > 0.01f) TransitTo<Ground_Move>();
        else TransitTo<Ground_Idle>();
    }
}
