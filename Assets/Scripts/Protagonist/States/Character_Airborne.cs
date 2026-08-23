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

    public CharacterManager Character => manager as CharacterManager;

    Character_Controlled Owner => fsm as Character_Controlled;


    public override void Enter()
    {
        Character.Steering.Airborne();

        _jumpTicket = 1;
        _dodgeTicket = 1;
        _jumpBufferedUntil = -1f;

        Subscribe();

        // 애니메이션은 축 전체에 걸쳐 있으므로 여기서 건다.
        // 하위 상태는 입력 유무로 갈리지 상승·하강으로 갈리지 않아, 구간을 맡길 수 없다.
        //
        // 다만 넘겨준 축이 이미 자세를 정해 놓았으면 덮지 않는다 — 벽을 차는 동작이 그렇다.
        // DriveJumpAnimation이 남의 재생을 건드리지 않는 것과 같은 규칙을, 들어오는 순간에도 적용한다.
        if (!Character.Animation.Claimed) BeginJumpAnimation();

        // 지상 축과 같은 이유로 재개하지 않는다. 공중에 들어오면 항상 Fall/Move부터.
        ToLocomotion();
    }

    public override void Exit()
    {
        _turnTo = null;

        Unsubscribe();
        base.Exit();
    }

    public override void Transitions()
    {
        Turn();

        DriveJumpAnimation();

        // 입력 콜백은 표시만 남긴다. 티켓 판정과 처리는 여기서 한다.
        if (Consume(ref _jumpRequested)) Jump();

        // 티켓 판정은 Airborne_Dodge.Enter()가 소모와 함께 한다. 여기서 미리 보지 않는다.
        // 회피 입력은 조작 머신이 상시로 들고 있다. 축마다 구독하면 축 밖(스킬 실행)의 대시를 흘린다.
        if (Owner != null && Owner.ConsumeDodge())
        {
            if (_currentStateType != typeof(Airborne_Dodge))
            {
                TransitTo<Airborne_Dodge>();
                return;
            }
        }

        // 착지가 언제나 우선이다. 땅에 닿았는데 옆의 턱을 무는 일이 없어야 한다.
        if (ToGround()) return;

        ToLedge();
    }

    static bool Consume(ref bool flag)
    {
        if (!flag) return false;
        flag = false;
        return true;
    }

    #region Jump Animation - 도약 · 상승 · 하강 세 구간

    /// <summary>클립 위에서 지금 어느 구간에 있는지. 착지 이후는 지상 축이 이어받는다.</summary>
    enum JumpPhase { Launch, RiseHold, Fall, FallHold }

    JumpPhase _phase;

    /// <summary>재생을 푼 시각. 가속을 시간으로 걸기 때문에 기준점이 필요하다.</summary>
    float _resumedAt;

    /// <summary>
    /// 지금 떠오르는 중인지.
    ///
    /// 몸이 아니라 <b>속도의 원본</b>에 묻는다. 벽을 차고 넘어온 참이면 몸은 아직 그 속도를 받지 못했고,
    /// 턱 축에 있는 동안은 키네마틱이라 아예 실리지도 않는다 — 그걸 0으로 읽으면
    /// 차고 오르는 중인데도 떨어지는 중으로 보고 그 자리에서 턱을 다시 문다.
    /// </summary>
    bool Rising => Character.Movement.Velocity.y > 0f;

    float SinceResume => Time.time - _resumedAt;

    /// <param name="eased">
    /// 멈춰 있다 푸는 것이면 참. 갓 섞여 들어온 참이면 거짓 —
    /// 이미 제 속도인 것으로 쳐서 도약이 굼떠지지 않게 한다.
    /// </param>
    void Resume(JumpPhase phase, bool eased)
    {
        _phase = phase;
        _resumedAt = eased ? Time.time : Time.time - Character.Jump.easeInTime;
    }

    void BeginJumpAnimation()
    {
        JumpAnimation jump = Character.Jump;

        if (Rising)
        {
            // 첫 프레임이 아니라 도약 자세로 섞어 들어간다.
            Resume(JumpPhase.Launch, eased: false);
            Character.Animation.PlayFrom(jump.stateName, jump.LaunchOffset);
        }
        else
        {
            // 뛴 게 아니라 걸어 나가 떨어지는 경우. 도약 구간은 건너뛰고 하강부터 시작한다.
            Resume(JumpPhase.Fall, eased: false);
            Character.Animation.PlayFrom(jump.stateName, jump.DropOffset);
        }

        Character.Animation.SetFloat(jump.speedParameter, 1f);
    }

    /// <summary>
    /// 배속을 매 프레임 다시 계산한다. 세울 프레임이 가까우면 줄고, 막 풀렸으면 올라온다.
    /// 클립이 갈렸으면(회피·스킬이 끼어듦) 아무것도 하지 않는다 — 남의 재생을 건드리지 않는다.
    /// </summary>
    void DriveJumpAnimation()
    {
        JumpAnimation jump = Character.Jump;
        if (!jump.TryGetFrame(Character.Animation, out float frame)) return;

        float speed;

        switch (_phase)
        {
            case JumpPhase.Launch:
                if (jump.Reached(frame, jump.riseHoldFrame)) { _phase = JumpPhase.RiseHold; goto case JumpPhase.RiseHold; }
                speed = jump.ApproachSpeed(frame, jump.riseHoldFrame, 1f, SinceResume);
                break;

            case JumpPhase.RiseHold:
                // 떠오르는 동안은 멈춰 선다. 내려가기 시작하면 그때 푼다.
                if (!Rising) { Resume(JumpPhase.Fall, eased: true); goto case JumpPhase.Fall; }
                speed = 0f;
                break;

            case JumpPhase.Fall:
                if (jump.Reached(frame, jump.fallHoldFrame)) { _phase = JumpPhase.FallHold; goto case JumpPhase.FallHold; }
                speed = jump.ApproachSpeed(frame, jump.fallHoldFrame, 1f, SinceResume);
                break;

            case JumpPhase.FallHold:
            default:
                // 착지 마무리는 지상 축이 이어받는다. 그때까지 멈춰 선다.
                speed = 0f;
                break;
        }

        Character.Animation.SetFloat(jump.speedParameter, speed);
    }

    #endregion

    #region Wall Turn

    /// <summary>벽을 차고 넘어온 참이면 향해야 할 방향. 다 돌면 비운다.</summary>
    Quaternion? _turnTo;

    /// <summary>
    /// 벽을 찬 방향으로 돌기 시작한다. 턱 축이 넘겨주면서 한 번 부른다.
    ///
    /// <b>얼마나 도는지가 아니라 어디를 보고 끝나는지가 중요하다.</b> 벽을 차 놓고 벽을 마주 본 채
    /// 날아가면 어색한데, 각도를 더하는 방식이면 어디서 시작했느냐에 따라 끝이 매번 달라진다.
    /// 목표를 향해 돌다 닿으면 멈추므로 끝은 언제나 벽 반대쪽이다.
    ///
    /// 축이 아니라 여기 있는 이유는 턱 축이 한 프레임 만에 넘기기 때문이다 — 붙들고 돌면
    /// 그동안 공중 조종과 도약을 잃는다. 도는 것은 날아가는 내내 이어져야 한다.
    /// </summary>
    public void TurnAwayFrom(Vector3 outward)
    {
        outward = CustomMath.RemoveY(outward);
        if (outward.sqrMagnitude < 0.0001f) return;

        _turnTo = Quaternion.LookRotation(outward.normalized, Vector3.up);
    }

    /// <summary>
    /// 목표를 향해 조금 돌린다. 방향키가 들어오면 그만둔다 — 플레이어가 고른 방향이 이긴다.
    /// </summary>
    void Turn()
    {
        if (_turnTo == null) return;

        if (InputManager.CharacterMove.sqrMagnitude > 0.01f) { _turnTo = null; return; }

        Rigidbody body = Character.Body;
        Quaternion next = Quaternion.RotateTowards(body.rotation, _turnTo.Value, Character.WallTurnSpeed * Time.deltaTime);

        body.MoveRotation(next);

        if (Quaternion.Angle(next, _turnTo.Value) < 0.5f) _turnTo = null;
    }

    #endregion

    #region Landing

    /// <returns>착지해서 축을 넘겼는지.</returns>
    bool ToGround()
    {
        // <b>솟는 중에는 착지하지 않는다.</b> 올라가면서 땅에 닿는다는 것은 말이 되지 않는다.
        //
        // 지면 캐스트는 시작할 때 이미 콜라이더와 겹쳐 있으면 거리 0으로 즉시 히트를 돌려준다.
        // 벽을 차고 나온 참이면 몸이 벽면에서 0.19밖에 안 떨어져 있어 탐지 구가 벽에 박혀 있고,
        // 그것이 착지로 읽혀 지상 축으로 넘어간다 — Mutual이 실어둔 도약 속도를 그 자리에서 지운다.
        // 실측으로 (밖 4, 위 6)이 한 프레임 만에 (0, -0.2)가 되어 벽점프가 통째로 사라졌다.
        if (Rising) return false;

        // 지상 점프가 축을 강제로 바꾸지 않으므로 "떴는데 아직 캐스트에 걸려 있는" 구간은 생기지 않는다.
        if (!Character.GroundCaster.Cast(out _)) return false;

        bool buffered = Time.time <= _jumpBufferedUntil;

        if (fsm is Character_Controlled controlled) controlled.ToGround();

        // 착지 직전에 눌린 점프를 살려준다.
        if (buffered) Character.Steering.Jump(Character.JumpSpeed);

        return true;
    }

    /// <summary>
    /// 앞에 턱이 있으면 잡는다.
    ///
    /// 떨어지는 중에만 본다. 올라가는 길에도 잡으면 점프 궤도가 천장 턱에 걸려 예측할 수 없어지고,
    /// 무엇보다 지상에 서 있는 동안 수직 속도가 0 근처를 오가며 가슴 높이 턱을 무는 일이 생긴다.
    /// </summary>
    void ToLedge()
    {
        if (Rising) return;

        if (fsm is Character_Controlled controlled) controlled.TryLedgeGrab(Character_Ledge.Entry.Air);
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

        // 도약 자세를 처음부터 다시 건다. 남의 클립이 돌고 있었더라도(벽을 차고 들어온 경우)
        // 여기서 되찾아야 두 번째 도약이 자세 없이 지나가지 않는다.
        BeginJumpAnimation();
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
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_jump.action.performed -= OnJump;
        InputManager.instance.move_jump.action.canceled -= OnJumpReleased;
        _subscribed = false;
    }

    void OnJump(InputAction.CallbackContext _) { _jumpRequested = true; }

    void OnJumpReleased(InputAction.CallbackContext _)
    {
        // 전이가 아니라 속도 조작이라 즉시 처리해도 상태가 흔들리지 않는다.
        Character.Steering.Drop();
    }

    #endregion

    /// <summary>하위 상태가 이동 축으로 되돌아올 때 쓴다.</summary>
    public void ToLocomotion()
    {
        if (InputManager.CharacterMove.sqrMagnitude > 0.01f) TransitTo<Airborne_Move>();
        else TransitTo<Airborne_Fall>();
    }
}
