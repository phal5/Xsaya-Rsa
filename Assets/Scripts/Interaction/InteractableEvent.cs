using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UnityEvent를 물려 쓰는 범용 상호작용 대상.
/// 문(SceneLoader.LoadScene), 살펴보기(FlipBook.SetBook) 같은 걸 스크립트 없이 붙일 수 있다.
///
/// 이벤트 셋
///   onFocus    : 최우선 후보가 됨 → 표시 띄우기
///   onUnfocus  : 후보에서 벗어남 → 표시 내리기
///   onInteract : 실제 상호작용
/// </summary>
public class InteractableEvent : MonoBehaviour, IInteractable
{
    [Tooltip("UI에 띄울 안내 문구.")]
    [SerializeField] string _prompt = "상호작용";

    [Tooltip("한 번 쓰면 더 이상 반응하지 않는다.")]
    [SerializeField] bool _once;

    [Header("Events")]
    [Tooltip("상호작용 가능 표시를 띄울 때.")]
    [SerializeField] UnityEvent _onFocus;

    [Tooltip("표시를 내릴 때.")]
    [SerializeField] UnityEvent _onUnfocus;

    [SerializeField] UnityEvent _onInteract;

    bool _used;
    bool _focused;

    public string Prompt => _prompt;

    public bool Available => enabled && (!_once || !_used);

    public void Focus()
    {
        if (_focused) return;

        _focused = true;
        _onFocus.Invoke();
    }

    public void Unfocus()
    {
        if (!_focused) return;

        _focused = false;
        _onUnfocus.Invoke();
    }

    public virtual void Interact(CharacterManager character)
    {
        if (!Available) return;

        _used = true;
        _onInteract.Invoke();

        // 한 번만 쓰는 대상은 즉시 표시를 내린다.
        if (_once) Unfocus();
    }

    void OnDisable()
    {
        Unfocus();
    }
}
