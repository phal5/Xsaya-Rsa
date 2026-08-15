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

    // 상호작용 입력은 축 전환(Ground↔Airborne)과 무관해야 한다.
    // Enter/Exit에 묶으면 축이 바뀔 때마다 붙었다 떨어져 입력을 흘린다.
    // 컴포넌트는 계속 살아 있으므로 Bootstrap에서 한 번만 걸고 유지한다.

    public override void Bootstrap()
    {
        Subscribe();
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
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_interact.action.performed -= OnInteract;
        _subscribed = false;
    }

    void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        _interactRequested = true;
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
        if (Execution != null && Execution.ConsumeRequest()) ToExecution();
    }

    Character_Execution _execution;

    Character_Execution Execution
    {
        get
        {
            if (_execution == null && _componentStates != null)
            {
                foreach (MonoBehaviour c in _componentStates)
                    if (c is Character_Execution e) { _execution = e; break; }
            }
            return _execution;
        }
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

    public void ToExecution()
    {
        if (_currentStateType == typeof(Character_Execution)) return;
        TransitTo<Character_Execution>();
    }

    // Heal / Swap은 아직 전용 입력 액션이 없다.
    // KeyInvoke 컴포넌트나 UI 버튼의 UnityEvent에서 이 메서드를 직접 부르면 된다.

    public void ToHeal()
    {
        if (_currentStateType == typeof(Character_Heal)) return;
        if (Character != null && !Character.HasHealCharge) return;   // 헛동작 방지

        TransitTo<Character_Heal>();
    }

    public void ToSwap()
    {
        if (_currentStateType == typeof(Character_Swap)) return;
        TransitTo<Character_Swap>();
    }

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
