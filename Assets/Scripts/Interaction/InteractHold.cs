using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 붙들고 있어야 발동하는 상호작용. 단발 입력을 <b>붙든 시간</b>으로 바꿔 넘긴다.
///
/// Input System의 Hold 인터랙션을 쓰지 않는다 — 그건 move_interact 액션 하나에 걸려
/// 게임의 모든 상호작용에 똑같이 적용된다. 대상마다 다른 시간을 주려면, 그 액션이 발화하는
/// 시점 자체를 늦출 수는 없으니 대상 쪽에서 직접 눌림 상태를 읽고 재는 수밖에 없다.
///
/// 그래서 <see cref="InputManager.CharacterInteract"/>(액션의 performed/canceled가 세우는
/// 정적 눌림 플래그)를 매 프레임 읽는다. 시간을 재는 시계는 여기 하나뿐이다.
///
/// <b>지금 조준된 대상이 나인지</b>는 InteractionDetector.Current와 견주어 안다.
/// 그러지 않으면 근처의 홀드 대상이 전부 함께 차오른다.
/// </summary>
public class InteractHold : MonoBehaviour
{
    [Tooltip("붙들고 있어야 하는 시간(초).")]
    [SerializeField, Min(0.01f)] float _holdTime = 1f;

    [Tooltip("채울 이미지. Image Type을 Filled로 두어야 fillAmount가 먹는다. 비워두면 시각화만 없을 뿐 판정은 그대로 돈다.")]
    [SerializeField] Image _fill;

    [Tooltip("다 채웠을 때. Checkpoint.Activate() 같은 실제 발동 메서드를 여기 물린다.")]
    [SerializeField] UnityEvent _onHoldComplete;

    [Tooltip("판정 대상. 비워두면 같은 오브젝트에서 찾는다.")]
    [SerializeField] InteractableEvent _target;

    float _held;
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

    void Awake()
    {
        if (_target == null) _target = GetComponent<InteractableEvent>();
    }

    void OnEnable() => Render(0f);

    void OnDisable()
    {
        _held = 0f;
        Render(0f);
    }

    void Update()
    {
        InteractionDetector detector = Detector;

        // 지금 조준된 것이 나인가. 아니면 남의 붙듦이다.
        bool mine = _target != null && detector != null && ReferenceEquals(detector.Current, _target);

        if (mine && InputManager.CharacterInteract)
        {
            _held += Time.deltaTime;

            if (_held >= _holdTime)
            {
                _held = 0f;
                Render(0f);
                _onHoldComplete.Invoke();
                return;
            }
        }
        else
        {
            // 놓거나 조준이 벗어나면 처음부터 다시. 이어서 채우는 게 아니다.
            _held = 0f;
        }

        Render(_held / _holdTime);
    }

    void Render(float amount)
    {
        if (_fill != null) _fill.fillAmount = amount;
    }
}
