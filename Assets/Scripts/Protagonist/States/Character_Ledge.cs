using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 턱 축 서브머신. Catch · Hang · Climb · Release · WallJump를 담는다.
///
/// 축을 가르는 기준은 "어떤 운동 모델이 몸을 소유하는가"다.
///   Ground   Mutual  + 중력
///   Airborne Singular + 중력
///   Ledge    Singular + <b>중력 없음</b>, 위치는 sampler에 못박힌다
/// 세 번째 모델이라 세 번째 축이다.
///
/// <b>Braced냐 Freehang이냐는 상태가 아니라 이 축의 성질이다.</b>
/// 올라가기·놓기·벽점프가 저마다 두 갈래로 갈리는데, 그때마다 상태를 둘로 쪼개면
/// 같은 전이가 여덟 벌이 된다. 여기 하나로 들고 각 상태가 클립을 고르게 한다.
///
/// Initial State: 없음. Enter가 진입 방식에 따라 직접 고른다.
/// </summary>
public class Character_Ledge : FiniteStateMachine
{
    public CharacterManager Character => manager as CharacterManager;

    /// <summary>어디서 들어왔는지. 공중에서 문 것과 가장자리에서 내려간 것은 첫 동작이 다르다.</summary>
    public enum Entry { Air, Ground }

    /// <summary>잡을 자리. 무는 순간 재두고, 매달린 동안 다시 재지 않는다.</summary>
    public LedgeGrab.Anchor Anchor { get; private set; }

    /// <summary>발을 디디고 있는지. 지상 진입은 거짓으로 시작해 Catch가 참으로 바꾼다.</summary>
    public bool Braced { get; private set; }

    /// <summary>매달린 뒤 이어서 틀 전환 동작이 있는지. 지상 진입에서 발 디딜 곳을 찾았을 때 선다.</summary>
    public bool BracingPending { get; private set; }

    /// <summary>어디서 들어왔는지. 끝까지 가야 하는 동작인지를 가르는 데 쓰인다.</summary>
    public Entry Origin => _entry;

    /// <summary>
    /// 턱을 <b>실제로 잡고 있는지.</b> 진입 동작이 도는 동안은 아직 아니다 — 손을 뻗어 가는 중이다.
    ///
    /// 오르기·놓기·벽점프는 전부 "잡고 있다"를 전제로 하는 동작이라, 잡기 전에는 뜻이 없다.
    /// </summary>
    public bool Hanging => _currentStateType != typeof(Ledge_Catch);

    Entry _entry;
    bool _subscribed;

    /// <summary>Compose가 준 그대로. 자세가 바뀔 때 여기서 다시 옮긴다 — 옮긴 것을 또 옮기지 않으려고 원본을 든다.</summary>
    LedgeGrab.Anchor _raw;

    /// <summary>
    /// 잡을 권리. 한 번 잡으면 소모된다.
    ///
    /// 되돌아오는 방식이 나가는 방식에 따라 다르다.
    ///   <b>뛰어서</b> 나가면 그 자리에서 돌아온다 — 속도가 몸을 데려가므로 같은 턱을 도로 물 일이 없다.
    ///   <b>놓아서</b> 나가면 잡을 곳이 없어질 때까지 기다린다 — 놓은 자리에 그대로 있어
    ///   매 프레임 탐지가 성립하므로, 그때 권리가 있으면 손을 뗀 그 프레임에 도로 물린다.
    ///
    /// 처음에는 채워져 있다. 게임을 시작하자마자 벽 앞에 서 있을 수도 있기 때문이다.
    /// </summary>
    bool _charged = true;

    // 입력 콜백이 세우고 하위 상태가 소비한다.
    bool _upRequested;
    bool _downRequested;
    bool _jumpRequested;

    #region Grab

    /// <summary>
    /// 잡을 턱이 있는지 묻는다. 잡히면 자리와 자세를 재두므로, 부른 쪽은 이 축으로 넘기기만 하면 된다.
    /// 쿨다운도 여기서 본다 — 놓자마자 같은 턱을 다시 무는 것은 이 머신의 사정이다.
    /// </summary>
    public bool TryGrab(Entry entry)
    {
        if (Character == null || Character.Ledge == null) return false;

        LedgeGrab grab = Character.Ledge;
        Transform body = Character.Body.transform;

        // 탐지 수치는 발밑 기준이고 몸의 원점은 캡슐 중앙에 있다. 그 차이를 여기서 넘긴다.
        float feet = Character.FootOffset;

        bool found = entry == Entry.Air
            ? grab.Probe(body, feet, out LedgeGrab.Anchor anchor)
            : grab.ProbeFromTop(body, feet, out anchor);

        // 잡을 곳이 없다. 놓고 나온 뒤 권리가 돌아오는 자리다.
        if (!found)
        {
            _charged = true;
            return false;
        }

        if (!_charged) return false;

        _charged = false;

        _entry = entry;

        bool canBrace = grab.IsBraced(anchor);

        // 공중에서 문 것은 그 순간 자세가 정해진다.
        // 가장자리에서 내려가는 것은 언제나 철봉처럼 매달린 채 시작하고, 디딜 곳이 있으면 이어서 바꾼다.
        Braced = entry == Entry.Air && canBrace;
        BracingPending = entry == Entry.Ground && canBrace;

        _raw = anchor;
        Anchor = grab.Pose(_raw, Braced);

        return true;
    }

    /// <summary>전환 동작이 끝났다. 이제부터 발을 디딘 것으로 친다.</summary>
    public void Brace()
    {
        Braced = true;
        BracingPending = false;

        // 자세가 바뀌면 손이 벽에 닿는 자리도 바뀐다. 팔을 접는 만큼 몸이 벽에서 물러난다.
        Anchor = Character.Ledge.Pose(_raw, true);
    }

    #endregion

    public override void Enter()
    {
        Character.Steering.Airborne();

        // 중력을 끄는 것이 먼저다. 위치를 붙드는 것만으로는 속도가 계속 쌓인다.
        Character.Movement.SetGravity(false);

        // 클립이 몸을 옮기는 구간은 여기뿐이다. 여기서만 받는다.
        //
        // 옮기는 것은 위치뿐이다. 방향은 턱 법선이 정하고 아래에서 한 번 세운다 —
        // 진입 클립의 회전은 배우가 공중에서 몸을 트는 몫이라, 세워둔 방향 위에 또 실으면 두 번 돈다.
        Character.Animation.CaptureRootMotion(true);

        // 붙어 있는 동안은 물리가 몸을 밀지 못하게 한다.
        //
        // 매달린 자세는 손이 벽면에 닿아야 해서 몸이 벽보다 안쪽에 오기도 한다.
        // 그대로 두면 솔버가 매 프레임 밀어내고 우리가 도로 끌어와 눈에 보이는 떨림이 된다.
        // 콜라이더는 그대로 남으므로 피격 판정은 잃지 않는다.
        Character.Body.isKinematic = true;

        // 자리와 방향을 정하는 것은 여기 한 번뿐이다. 중력이 꺼져 있으니 그대로 있는다.
        // 매 프레임 다시 놓으면 물리가 밀어낸 것을 도로 끌어와 떨림이 된다.
        Character.Movement.Pin(Anchor.hang);
        Character.Body.rotation = Anchor.facing;

        _upRequested = false;
        _downRequested = false;
        _jumpRequested = false;
        Subscribe();

        // 다른 축과 같은 이유로 재개하지 않는다. 무는 방식이 첫 동작을 정한다.
        if (CatchClip().IsSet) TransitTo<Ledge_Catch>();
        else TransitTo<Ledge_Hang>();
    }

    /// <summary>
    /// 무는 순간 틀 동작.
    ///
    /// 가장자리에서 내려가는 경우는 자세와 무관하게 언제나 매달리는 동작부터 시작한다 —
    /// 발 디딜 곳이 있으면 Catch가 이어서 자세를 바꾼다.
    /// 이름이 비어 있으면 진입 동작 없이 곧장 대기 자세로 간다.
    /// </summary>
    public LedgeGrab.LedgeClip CatchClip()
    {
        LedgeGrab grab = Character.Ledge;

        if (_entry == Entry.Ground) return grab.dropToFree;

        return Braced ? grab.catchBraced : grab.catchFree;
    }

    /// <summary>
    /// 복구 지점은 여기 한 곳뿐이다.
    ///
    /// 매달린 채 맞으면 루트가 이 축을 통째로 밀어낸다. 중력 복구를 하위 상태에 두면
    /// 그때 Hang의 Exit만 돌거나 아예 돌지 않아, 중력이 꺼진 채로 새어나간다.
    /// </summary>
    public override void Exit()
    {
        Unsubscribe();

        Character.Animation.CaptureRootMotion(false);

        Character.Body.isKinematic = false;
        Character.Movement.SetGravity(true);

        base.Exit();
    }

    #region Requests

    /// <summary>
    /// 오르기·놓기·벽점프는 <b>축 전체의 관심사</b>다. 특정 상태에 맡기지 않는다.
    ///
    /// 매달려 있는 동안이면 진입 동작이 돌고 있든 쉬고 있든 같은 뜻이어야 하는데,
    /// 대기 상태에만 두면 진입 클립이 끝나기를 기다리는 동안 입력이 통째로 죽는다.
    /// 여기 두면 그 창이 없어지고, Update와 FixedUpdate 양쪽에서 확인되어 반응도 빨라진다.
    ///
    /// <b>턱 동작에는 끝까지 가야 하는 것이 없다.</b> 어느 구간이든 다음 입력이 이긴다 —
    /// 구간마다 입력을 받을지 말지를 따로 두면, 눌렀는데 아무 일도 안 일어나는 창이 생긴다.
    /// </summary>
    public override void Transitions()
    {
        if (!Hanging)
        {
            // 아직 잡은 것이 아니다. 잡은 뒤에만 뜻이 있는 조작이므로 흘려보낸다 —
            // 남겨두면 진입이 끝나는 순간 터져서, 누른 적 없는 동작이 저절로 나간다.
            _upRequested = false;
            _downRequested = false;

            // 점프만 다르다. 잡기 전의 점프는 턱 조작이 아니라 공중 도약이다.
            if (Consume(ref _jumpRequested)) Abandon();
            return;
        }

        if (Consume(ref _upRequested)) { Switch(typeof(Ledge_Climb)); return; }

        if (Consume(ref _downRequested)) { Switch(typeof(Ledge_Release)); return; }

        // 벽을 찰 발판이 없으면 뛰지 못한다. 그때 점프는 놓기가 된다 —
        // 어차피 내려가는 길이 그것뿐인데 키를 죽여두면 입력이 먹힌 것처럼 보인다.
        if (Consume(ref _jumpRequested))
            Switch(Braced ? typeof(Ledge_WallJump) : typeof(Ledge_Release));
    }

    /// <summary>이미 그 상태면 아무것도 하지 않는다. 연타로 같은 동작이 처음부터 다시 돌지 않게.</summary>
    void Switch(System.Type next)
    {
        if (_currentStateType != next) TransitTo(next);
    }

    static bool Consume(ref bool flag)
    {
        if (!flag) return false;
        flag = false;
        return true;
    }

    #endregion

    #region Exits

    /// <summary>
    /// 잡으려던 것을 포기하고 공중 도약한다. <b>진입 중에만 부른다.</b>
    ///
    /// 권리가 없으면 아무 일도 없고 진입이 이어진다 — 못 뛰는데 손까지 놓을 이유는 없다.
    /// 잡은 뒤라면 이 길로 오지 않는다. 그때 점프는 벽을 차는 것이다.
    /// </summary>
    void Abandon()
    {
        if (fsm is not Character_Controlled controlled) return;
        if (!controlled.ConsumeJump()) return;

        // 뛰어서 나간다. 속도가 데려가므로 그 자리에서 권리를 돌려도 도로 물리지 않는다.
        _charged = true;

        // 속도는 넘기기 전에 싣는다. 외력 몸은 키네마틱이 아니라 지금 실어도 남는다.
        Character.Steering.Jump(Character.JumpSpeed);

        Drop();
    }

    /// <summary>턱을 놓는다. 중력은 이 축의 Exit이 되돌린다.</summary>
    public void Drop()
    {
        if (fsm is Character_Controlled controlled) controlled.ToAirborne();
    }

    /// <summary>
    /// 벽을 찬다. 축은 아직 넘기지 않는다 —
    /// 차는 클립을 끝까지 보여주려면 날아가는 동안에도 이쪽이 재생을 쥐고 있어야 한다.
    ///
    /// <b>루트 모션은 쓰지 않는다.</b> 그건 오르기 하나에만 허락된 예외다.
    /// 클립은 자세만 주고, 어디로 얼마나 날아가는지는 이 속도와 중력이 정한다.
    /// </summary>
    public void Launch()
    {
        LedgeGrab grab = Character.Ledge;
        Vector3 outward = -(Anchor.facing * Vector3.forward);

        // 뛰어서 나간다. 속도가 데려가므로 그 자리에서 권리를 돌려도 도로 물리지 않는다.
        _charged = true;

        // 붙어 있는 동안 껐던 것을 되돌린다. 이제부터 몸은 물리가 가져간다.
        Character.Movement.SetGravity(true);

        // 수평은 벽을 밀어내는 몫만 실어 둔다. 방향키가 들어오면 그 위를 공중 조종이 이어받는다.
        Character.Movement.SetSamplerVelocity(outward * grab.wallJumpOut);

        // 수직은 지상 점프와 같은 길로 넣는다. 도약이 수직만 정하고 수평은 조종이 정하는 것이 이 게임의 점프다.
        Character.Steering.Jump(grab.wallJumpUp);

        // sampler에 실은 것을 몸이 이번 프레임 안에 받게 한다. 물리 적분을 기다리면 첫 프레임이 멈춰 보인다.
        Character.Movement.ClearMovement();

        // 조향 목표를 지금 속도에 맞춰 둔다. ClearMovement가 목표를 0으로 두고 가는데,
        // Character_Movement.FixedUpdate가 상태보다 먼저 돌아 Coast가 걸리기 전 한 프레임을 제동한다 —
        // 공중 가속도가 100이라 그 한 프레임에 수평이 절반으로 깎인다.
        Character.Steering.Coast();
    }

    /// <summary>
    /// 다 올라섰다.
    ///
    /// 세 가지를 <b>같은 순간에</b> 한다 — 설 자리에 놓고, 다음 자세로 섞기 시작하고, 축을 넘긴다.
    /// 섞이는 동안 이쪽에 더 붙들고 있으면 그만큼 조작을 잃고, 자세만 먼저 바꾸면 몸이 뒤늦게 따라와 튄다.
    ///
    /// 자리를 여기서 정하는 이유는 오르기 클립이 끝나는 곳이 설 자리와 다르기 때문이다.
    /// 상승량이 매달린 깊이에 못 미치고, 앞으로 가는 몫은 오르는 내내 몸이 턱보다 아래라 절벽 면에 막혀 먹힌다.
    ///
    /// 섞이는 시간을 길게 주는 것은 오르기 자세와 선 자세가 너무 달라서다.
    /// 기본값으로는 한 프레임 만에 갈아타 순간이동처럼 보인다.
    /// </summary>
    public void Finish()
    {
        _charged = true;

        LedgeGrab grab = Character.Ledge;

        Character.Movement.Pin(Anchor.stand);
        Character.Animation.Play(grab.mount.state, grab.Seconds(grab.mount));

        if (fsm is Character_Controlled controlled) controlled.ToLocomotion();
    }

    #endregion

    #region Input - 점선 화살표

    void Subscribe()
    {
        if (_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_climbUp.action.performed += OnUp;
        InputManager.instance.move_climbDown.action.performed += OnDown;
        InputManager.instance.move_jump.action.performed += OnJump;
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_climbUp.action.performed -= OnUp;
        InputManager.instance.move_climbDown.action.performed -= OnDown;
        InputManager.instance.move_jump.action.performed -= OnJump;
        _subscribed = false;
    }

    void OnUp(InputAction.CallbackContext _) { _upRequested = true; }

    void OnDown(InputAction.CallbackContext _) { _downRequested = true; }

    void OnJump(InputAction.CallbackContext _) { _jumpRequested = true; }

    #endregion
}
