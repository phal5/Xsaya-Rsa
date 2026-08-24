using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// 키 하나를 가르치고 끝나는 안내 한 단계.
///
/// <b>프리팹으로 만들어 아무 씬에나 떨어뜨리면 된다.</b> 씬 바깥을 가리키는 참조가 하나도 없다 —
/// UI는 <see cref="HudService.Instance"/>로 런타임에 찾고, 가르칠 내용은 전부 이 컴포넌트가 들고 있다.
/// 이식할 때 끊길 자리가 없다는 뜻이다.
///
/// UI 씬이 없으면 경고만 남기고 지나간다. 스테이지만 띄워 레벨을 다듬는 경우가 있기 때문이다.
/// </summary>
public class TutorialStep : MonoBehaviour
{
    #region Inspector Fields

    [Header("Lesson")]
    [Tooltip("이 키를 누르면 끝난다.")]
    [SerializeField] Key _key = Key.V;

    [Tooltip("화면 중앙에 띄울 문구.")]
    [SerializeField, TextArea] string _message = "Press [ V ] to Raise";

    [Header("Behaviour")]
    [Tooltip("씬에 올라오자마자 시작한다. 끄면 Begin()을 직접 불러야 한다.")]
    [SerializeField] bool _beginOnStart = true;

    [Tooltip("가르치는 동안 HUD를 내렸다가 끝나면 되돌린다.")]
    [SerializeField] bool _hideHudWhileTeaching = true;

    [Tooltip("한 판에 한 번만. 죽어서 스테이지를 다시 불러와도 두 번 열리지 않는다.")]
    [SerializeField] bool _once = true;

    [Tooltip("'한 번만'을 가리는 이름. 비우면 오브젝트 이름을 쓴다. " +
             "같은 안내를 여러 자리에 두고 하나로 묶고 싶을 때만 손으로 적는다.")]
    [SerializeField] string _id;

    [Header("Events")]
    [Tooltip("끝났을 때. 같은 프리팹 안의 대상만 꽂을 것 — " +
             "씬 오브젝트를 꽂으면 다른 씬으로 옮길 때 끊긴다.")]
    [SerializeField] UnityEvent _onDone;

    #endregion

    #region Internal State

    /// <summary>
    /// 이 판에서 이미 끝낸 단계들. 씬을 넘나들어도 남아야 하므로 static이다.
    ///
    /// static은 재생을 다시 눌러도 (도메인 리로드를 끈 경우) 남는다.
    /// 지난 판에서 끝낸 것이 다음 판까지 따라오지 않도록 아래에서 되돌린다 —
    /// <see cref="TimeManager"/>가 배속을 되돌리는 것과 같은 자리다.
    /// </summary>
    static readonly HashSet<string> _finished = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState() => _finished.Clear();

    bool _pending;

    string Id => string.IsNullOrEmpty(_id) ? name : _id;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Awake가 아니라 Start다. HudService는 Awake에서 자기를 등록하는데,
    /// 함께 올라온 씬들은 모든 Awake가 모든 Start보다 먼저 돈다 —
    /// 여기서 찾으면 씬 순서와 무관하게 이미 있다.
    /// </summary>
    private void Start()
    {
        if (_beginOnStart) Begin();
    }

    private void OnDestroy()
    {
        // 안내를 띄워둔 채 씬이 내려가면, 사라진 이쪽을 부르는 리스너만 UI에 남는다.
        if (_pending && HudService.Instance != null) HudService.Instance.ClearPrompt();
    }

    #endregion

    #region Public API

    /// <summary>
    /// 안내를 연다. 이미 열려 있거나 이 판에서 끝낸 단계면 아무것도 하지 않는다.
    /// 트리거나 대사 이벤트에서 직접 부를 수 있다.
    /// </summary>
    public void Begin()
    {
        if (_pending) return;
        if (_once && _finished.Contains(Id)) return;

        if (HudService.Instance == null)
        {
            Debug.LogWarning($"[{name}] HudService가 없어 안내를 열지 못했습니다. UI 씬이 올라와 있어야 합니다.", this);
            return;
        }

        _pending = true;

        if (_hideHudWhileTeaching) HudService.Instance.SetHudVisible(false);

        // Instructor가 UnityEvent를 받으므로, 끝을 이쪽이 알기 위해 한 겹 둔다.
        UnityEvent relay = new();
        relay.AddListener(Complete);

        HudService.Instance.Prompt(_key, _message, relay);
    }

    #endregion

    #region Core Logic

    void Complete()
    {
        if (!_pending) return;
        _pending = false;

        if (_once) _finished.Add(Id);

        // 내린 쪽이 올린다. UI 프리팹의 배선에 기대지 않아야 어느 씬에 놓아도 같게 동작한다.
        if (_hideHudWhileTeaching && HudService.Instance != null)
            HudService.Instance.SetHudVisible(true);

        _onDone.Invoke();
    }

    #endregion
}
