using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// '일어나기' 키를 누르면 이벤트를 쏜다.
///
/// <see cref="KeyInvoke"/>와 달리 키보드를 직접 읽지 않고 <see cref="InputManager.move_raise"/>
/// 액션을 듣는다. 리바인딩과 게임패드가 그대로 따라오고, 키가 무엇인지 아는 곳이
/// InputManager 한 곳으로 남는다.
///
/// 매 프레임 묻지 않는다 — 액션이 눌린 순간에만 불린다.
/// </summary>
public class RaiseInvoke : MonoBehaviour
{
    [Tooltip("'일어나기' 키를 눌렀을 때.")]
    [SerializeField] private UnityEvent _onRaise;

    bool _subscribed;

    /// <summary>
    /// 여기서 한 번, <see cref="Start"/>에서 한 번 시도한다.
    ///
    /// 씬이 올라오는 프레임에는 InputManager의 Awake가 이 OnEnable보다 늦을 수 있다 —
    /// 오브젝트 사이의 순서는 정해져 있지 않기 때문이다. 반면 모든 Awake는 모든 Start보다
    /// 먼저 도므로, 그때는 반드시 잡힌다. 껐다 켜는 경우는 이쪽에서 바로 잡힌다.
    /// </summary>
    void OnEnable() => Subscribe();

    void Start() => Subscribe();

    void OnDisable() => Unsubscribe();

    void Subscribe()
    {
        if (_subscribed || InputManager.instance == null) return;

        if (InputManager.instance.move_raise == null)
        {
            Debug.LogError($"[{name}] InputManager.move_raise가 비어 있어 '일어나기'를 들을 수 없습니다.", this);
            return;
        }

        InputManager.instance.move_raise.action.performed += OnRaise;
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_raise.action.performed -= OnRaise;
        _subscribed = false;
    }

    void OnRaise(InputAction.CallbackContext _)
    {
        _onRaise.Invoke();
    }
}
