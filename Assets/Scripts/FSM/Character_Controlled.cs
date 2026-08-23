using UnityEngine;

/// <summary>
/// 조작 가능한 동안의 서브머신. 이동 축(Ground/Airborne)과 스킬 실행을 갈아끼운다.
///
/// Initial State  : Character_Ground
/// ComponentStates: Character_Ground, Character_Airborne, Character_Execution
/// </summary>
public class Character_Controlled : FiniteStateMachine
{
    CharacterManager Character => manager as CharacterManager;

    bool _subscribed;

    // 입력 콜백이 세우고 Transitions()가 소비한다.
    bool _interactRequested;
    bool _dodgeRequested;

    // 상호작용 입력은 축 전환(Ground↔Airborne)과 무관해야 한다.
    // Enter/Exit에 묶으면 축이 바뀔 때마다 붙었다 떨어져 입력을 흘린다.
    // 컴포넌트는 계속 살아 있으므로 Bootstrap에서 한 번만 걸고 유지한다.

    public override void Bootstrap()
    {
        Subscribe();
    }

    /// <summary>
    /// 조작을 돌려받는 지점. 조작이 없던 동안 쌓인 입력을 여기서 버린다.
    ///
    /// 상호작용 구독은 Bootstrap에 걸려 있고 콤보는 FSM 밖의 옵저버라,
    /// 피격·UI로 조작을 잃은 동안에도 표시는 계속 쌓인다.
    /// 그대로 두면 스턴이 풀리는 순간 눌러둔 것들이 한꺼번에 터진다.
    /// </summary>
    public override void Enter()
    {
        _interactRequested = false;
        _dodgeRequested = false;
        Execution?.Discard();

        base.Enter();
    }

    protected override void OnDestroyed()
    {
        Unsubscribe();
    }

    #region Interact Input

    void Subscribe()
    {
        if (_subscribed) return;

        if (InputManager.instance == null)
        {
            Debug.LogError($"[{name}] InputManager가 아직 없어 상호작용 입력을 구독하지 못했습니다.", this);
            return;
        }

        InputManager.instance.move_interact.action.performed += OnInteract;
        InputManager.instance.move_dash.action.performed += OnDodge;
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_interact.action.performed -= OnInteract;
        InputManager.instance.move_dash.action.performed -= OnDodge;
        _subscribed = false;
    }

    void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        _interactRequested = true;
    }

    void OnDodge(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        _dodgeRequested = true;
    }

    /// <summary>
    /// 이동 축이 회피 입력을 가져간다. <b>축마다 구독하지 않는 이유</b>가 여기 있다.
    ///
    /// 축은 Enter에서 붙고 Exit에서 떨어지므로, 축이 아닌 곳(스킬 실행)에 있는 동안의 대시는
    /// 들리지도 않는다. 그러면 스킬을 대시로 끊을 방법이 없다.
    /// 상호작용 입력을 이 머신이 상시로 들고 있는 것과 같은 이유다.
    /// </summary>
    public bool ConsumeDodge()
    {
        if (!_dodgeRequested) return false;

        _dodgeRequested = false;
        return true;
    }

    public override void Transitions()
    {
        // 콜백이 남긴 표시를 여기서 소비한다.
        if (_interactRequested)
        {
            _interactRequested = false;
            ToInteract();
            return;
        }

        // 스킬 요청도 같은 규칙으로 처리한다.
        if (Execution != null && Execution.ConsumeRequest()) { ToExecution(); return; }

        Cancel();
    }

    /// <summary>
    /// 대시로 스킬을 끊는다.
    ///
    /// 이 표시는 <b>축이 먼저</b> 가져간다 — 하위 머신의 Transitions가 이보다 먼저 돌기 때문이다.
    /// 여기까지 남아 있다는 것은 지금 축이 쓰지 못했다는 뜻이고, 스킬 중이라면 그것이 곧 캔슬이다.
    ///
    /// 표시를 지우지 않고 넘긴다. 이동 축으로 돌아간 다음 갱신에서 그 축이 가져가 회피로 잇는다.
    /// 이동과 점프가 스킬을 끊지 못하는 것은 막아서가 아니다 — 그 둘은 축의 입력이고,
    /// 스킬이 도는 동안 축은 돌지 않는다.
    /// </summary>
    void Cancel()
    {
        if (!_dodgeRequested) return;

        if (_currentStateType != typeof(Character_Execution))
        {
            // 회피를 쓸 수 없는 축(턱 등)에 남은 표시는 흘린다. 두면 축이 바뀌는 순간 되살아난다.
            _dodgeRequested = false;
            return;
        }

        ToLocomotion();
    }

    Character_Execution _execution;
    Character_Ledge _ledge;

    Character_Execution Execution => Find(ref _execution);

    Character_Ledge Ledge => Find(ref _ledge);

    /// <summary>등록된 컴포넌트 상태 중 그 타입인 것을 찾아 기억해둔다.</summary>
    T Find<T>(ref T cache) where T : MonoBehaviour
    {
        if (cache != null || _componentStates == null) return cache;

        foreach (MonoBehaviour component in _componentStates)
            if (component is T found) { cache = found; break; }

        return cache;
    }

    public void ToInteract()
    {
        // 구독이 상시라 UI·피격 중에도 입력이 들어온다.
        // 이 머신이 루트의 현재 상태가 아니면 조작 불가 상태이므로 무시한다.
        if (fsm != null && fsm._currentStateType != typeof(Character_Controlled)) return;

        if (_currentStateType == typeof(Character_Interact)) return;

        // 대상이 없으면 헛되이 멈추지 않는다.
        if (Character == null || Character.Interactor == null || !Character.Interactor.HasTarget) return;

        TransitTo<Character_Interact>();
    }

    #endregion

    // 축 전환은 멱등이어야 한다. 이미 그 축이면 아무것도 하지 않는다.
    //
    // 하위 머신이 축을 넘긴 뒤에도 자기 Transitions()가 같은 프레임에 한 번 더 도는데,
    // 그때 자기 전이가 일어나면 Exit/Enter가 재실행되고 Steering.Ground/Airborne이
    // 속도 공간 통합을 다시 돌려 수직 속도가 누적된다.

    public void ToGround()
    {
        if (_currentStateType == typeof(Character_Ground)) return;
        TransitTo<Character_Ground>();
    }

    public void ToAirborne()
    {
        if (_currentStateType == typeof(Character_Airborne)) return;
        TransitTo<Character_Airborne>();
    }

    /// <summary>
    /// 스킬을 실행한다. <b>이미 스킬 중이어도 다시 들어간다</b> — 그 재진입이 곧 캔슬이다.
    ///
    /// 다른 축들과 달리 멱등하게 두지 않았다. 멱등이 필요했던 것은 하위 머신이 축을 넘긴 뒤
    /// 제 Transitions가 같은 프레임에 한 번 더 도는 경우인데, 여기로 오는 길은 켜졌다 꺼지는
    /// 요청 표시 하나뿐이라 저절로 한 번만 돈다.
    /// </summary>
    public void ToExecution()
    {
        TransitTo<Character_Execution>();
    }

    public void ToLedge()
    {
        if (_currentStateType == typeof(Character_Ledge)) return;
        TransitTo<Character_Ledge>();
    }

    /// <summary>
    /// 앞에 잡을 턱이 있으면 잡는다.
    ///
    /// 판정과 자리 계산은 턱 축이 한다. 이동 축은 "잡혔나"만 알면 되고,
    /// 그래서 회피 티켓이나 스킬 요청과 같은 모양이 된다 — 물어보고, 참이면 넘긴다.
    /// </summary>
    public bool TryLedgeGrab(Character_Ledge.Entry entry)
    {
        Character_Ledge ledge = Ledge;
        if (ledge == null || !ledge.TryGrab(entry)) return false;

        ToLedge();
        return true;
    }

    // Heal / Swap은 아직 전용 입력 액션이 없다.
    // KeyInvoke 컴포넌트나 UI 버튼의 UnityEvent에서 이 메서드를 직접 부르면 된다.

    public void ToHeal()
    {
        if (_currentStateType == typeof(Character_Heal)) return;

        // 남은 횟수 판정은 Character_Heal.Enter()가 소모와 함께 한다. 여기서 미리 보지 않는다.
        TransitTo<Character_Heal>();
    }

    public void ToSwap()
    {
        if (_currentStateType == typeof(Character_Swap)) return;
        TransitTo<Character_Swap>();
    }

    /// <summary>
    /// 벽을 찬 방향으로 돌라고 공중 축에 이른다.
    ///
    /// 축들은 저마다 다른 오브젝트에 붙어 있어 GetComponent로는 잡히지 않는다.
    /// 등록된 컴포넌트 상태에서 찾는다.
    /// </summary>
    public void TurnAwayFromWall(Vector3 outward)
    {
        Character_Airborne airborne = Find(ref _airborne);

        if (airborne != null) airborne.TurnAwayFrom(outward);
    }

    Character_Airborne _airborne;

    /// <summary>스킬이 끝난 뒤 접지 여부에 맞는 이동 축으로 돌려준다.</summary>
    public void ToLocomotion()
    {
        CharacterManager character = Character;

        if (character != null && character.GroundCaster != null && character.GroundCaster.Cast(out _))
            ToGround();
        else
            ToAirborne();
    }
}
