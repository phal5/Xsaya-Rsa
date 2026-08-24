using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// <b>이 대상을</b> 붙들고 있는 동안 차오르는 표시. 상호작용 대상 쪽에 붙인다.
///
/// 시간을 여기서 재지 않는다. Interact 액션에 걸린 Hold 인터랙션이 이미 재고 있고,
/// 다 차면 그쪽이 performed를 쏘아 상호작용을 발동시킨다. 여기서 따로 세면 시계가 둘이 되어
/// 게이지는 다 찼는데 발동하지 않거나 그 반대가 된다. 진행도는 액션에게 묻는다.
///
/// 액션 참조도 들지 않는다. <see cref="InputManager"/>가 이미 그것을 들고 있고,
/// 여기서 또 꽂으면 둘이 다른 액션을 가리키는 날 표시와 동작이 갈린다.
///
/// <b>조준 중인 대상이 자기일 때만</b> 찬다. 탐지기가 고른 최우선 후보와 견주는데,
/// 그러지 않으면 근처의 체크포인트가 <b>전부</b> 함께 차오른다.
/// </summary>
public class InteractHoldBar : MonoBehaviour
{
    [Tooltip("어느 대상의 게이지인지. 비워두면 자기 위쪽에서 찾는다 — 체크포인트 밑에 매단 캔버스면 그걸로 잡힌다.")]
    [SerializeField] InteractableEvent _target;

    [Tooltip("채울 이미지. Image Type을 Filled로 두어야 fillAmount가 먹는다.")]
    [SerializeField] Image _fill;

    [Tooltip("켜고 끌 대상. 이 컴포넌트가 붙은 오브젝트를 넣지 않는다 — 자기를 끄면 다시 못 켠다.")]
    [SerializeField] GameObject _root;

    InteractionDetector _detector;

    /// <summary>
    /// 주인공 쪽 탐지기. 캐릭터 씬에 있어 인스펙터로 꽂을 수 없으므로 런타임에 찾는다.
    /// 캐릭터 씬은 내려가지 않으므로 한 번 찾으면 들고 있는다.
    /// </summary>
    InteractionDetector Detector
    {
        get
        {
            if (_detector != null) return _detector;

            Transform player = PlayerManager.instance != null ? PlayerManager.instance.player : null;
            if (player != null) _detector = player.GetComponentInChildren<InteractionDetector>(true);

            return _detector;
        }
    }

    InputAction Action => InputManager.instance != null && InputManager.instance.move_interact != null
        ? InputManager.instance.move_interact.action
        : null;

    void Awake()
    {
        if (_target == null) _target = GetComponentInParent<InteractableEvent>(true);
    }

    void OnEnable() => Render(0f, false);

    void Update()
    {
        InteractionDetector detector = Detector;
        InputAction action = Action;

        // 지금 조준된 것이 나인가. 아니면 남의 게이지다.
        bool mine = _target != null && detector != null && ReferenceEquals(detector.Current, _target);

        // 다 채운 순간 액션이 완료율을 0으로 되돌린다. 눌려 있지 않으면 그냥 0으로 본다.
        float amount = mine && action != null && action.IsPressed()
            ? action.GetTimeoutCompletionPercentage()
            : 0f;

        Render(amount, mine);
    }

    void Render(float amount, bool visible)
    {
        if (_root != null) _root.SetActive(visible);

        if (_fill != null) _fill.fillAmount = amount;
    }
}
