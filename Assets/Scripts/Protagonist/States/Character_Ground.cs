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
    bool _climbDownRequested;

    public CharacterManager Character => manager as CharacterManager;

    Character_Controlled Owner => fsm as Character_Controlled;

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
        //
        // 착지 마무리는 축에 들어오는 이 순간에만 따진다.
        // 하위 상태가 ToLocomotion으로 되돌아오는 경우(회피 종료 등)는 착지가 아니다.
        if (Landing()) TransitTo<Ground_Land>();
        else ToLocomotion();
    }

    /// <summary>
    /// 방금 점프로 착지했고, 마무리 동작을 볼 여유가 있는지.
    ///
    /// 착지 신호를 공중 축에서 따로 넘겨받지 않는다 — 점프 클립이 하강 자세로 멈춰 있다는 사실
    /// 자체가 신호다. 공중에서 회피나 스킬이 끼어들어 클립이 갈렸으면 저절로 거짓이 된다.
    /// </summary>
    bool Landing()
    {
        if (InputManager.CharacterMove.sqrMagnitude > 0.01f) return false;
        return Character.Jump.TailPending(Character.Animation);
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

        // 회피 입력은 조작 머신이 상시로 들고 있다. 축마다 구독하면 축 밖(스킬 실행)의 대시를 흘린다.
        if (Owner != null && Owner.ConsumeDodge())
        {
            if (_currentStateType != typeof(Ground_Dodge)) { TransitTo<Ground_Dodge>(); return; }
        }

        // 가장자리에서 내려가 매달리기. 잡을 곳이 없으면 아무 일도 일어나지 않는다.
        if (Consume(ref _climbDownRequested) && ToLedge()) return;

        ToAirborne();
    }

    /// <returns>가장자리를 잡아 축을 넘겼는지.</returns>
    bool ToLedge()
    {
        return fsm is Character_Controlled controlled
            && controlled.TryLedgeGrab(Character_Ledge.Entry.Ground);
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
        InputManager.instance.move_climbDown.action.performed += OnClimbDown;
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_jump.action.performed -= OnJump;
        InputManager.instance.move_climbDown.action.performed -= OnClimbDown;
        _subscribed = false;
    }

    void OnJump(InputAction.CallbackContext _) { _jumpRequested = true; }

    void OnClimbDown(InputAction.CallbackContext _) { _climbDownRequested = true; }

    #endregion

    /// <summary>하위 상태가 이동 축으로 되돌아올 때 쓴다.</summary>
    public void ToLocomotion()
    {
        if (InputManager.CharacterMove.sqrMagnitude > 0.01f) TransitTo<Ground_Move>();
        else TransitTo<Ground_Idle>();
    }
}
