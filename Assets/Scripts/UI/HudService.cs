using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// 바깥 씬이 UI에 말을 거는 <b>유일한 창구</b>.
///
/// UI는 씬을 따로 쓰므로 인스펙터로는 꽂을 수 없다 — 씬을 넘는 참조는 유니티가 저장하지 못한다.
/// 그래서 <see cref="FlipBook"/>·<see cref="SceneDirector"/>와 같은 방식으로 자기를 등록하고,
/// 바깥은 <see cref="Instance"/>로 런타임에 찾는다.
///
/// <b>안쪽 구성을 밖에 알리지 않는다.</b> HUD를 올리는 데 페이더가 몇 개 필요한지는
/// 여기서만 안다. 묶음이 늘거나 줄어도 고칠 곳은 이 목록 하나다.
///
/// 캔버스 루트에 둔다.
/// </summary>
public class HudService : MonoBehaviour
{
    public static HudService Instance { get; private set; }

    #region Inspector Fields

    [Header("Prompt")]
    [Tooltip("키 하나를 기다리는 안내. 같은 프리팹 안에 있으므로 여기서 꽂는다.")]
    [SerializeField] Instructor _instructor;

    [Tooltip("화면 중앙 문구의 주인. 안내를 걷을 때 비우는 데 쓴다.")]
    [SerializeField] MessageUI _message;

    [Header("HUD")]
    [Tooltip("한 덩어리로 올리고 내릴 묶음. Header와 키 가이드들.")]
    [SerializeField] List<UIGroupFader> _hudGroups = new();

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else Debug.LogError($"Duplicate instance of HudService found attached to {gameObject.name}.", this);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #endregion

    #region Public API

    /// <summary>
    /// 키 하나를 기다리는 안내를 띄운다. 그 키가 눌리면 <paramref name="onDone"/>이 발동한다.
    ///
    /// <paramref name="onDone"/>은 <b>런타임 리스너</b>로 붙는다. 인스펙터로 꽂아둔 것은
    /// 지워지지 않으므로, UI가 스스로 하기로 한 일은 그대로 살아 있다.
    /// </summary>
    public void Prompt(Key key, string message, UnityEvent onDone)
    {
        if (_instructor == null)
        {
            Debug.LogError($"[{name}] Instructor가 꽂혀 있지 않아 안내를 띄울 수 없습니다.", this);
            return;
        }

        if (string.IsNullOrEmpty(message))
        {
            // 빈 문구로 열면 아무것도 안 적힌 칸만 뜬다. 여는 쪽의 실수다.
            Debug.LogWarning($"[{name}] 문구가 비어 있는 안내를 띄우려 했습니다.", this);
        }

        _instructor.Set(key, message, onDone);
    }

    /// <summary>
    /// 기다리던 안내를 취소한다. 연 쪽이 사라질 때 부른다 —
    /// 그냥 두면 파괴된 대상을 부르는 리스너만 남는다.
    /// </summary>
    public void ClearPrompt()
    {
        if (_instructor != null) _instructor.enabled = false;
        if (_message != null) _message.SetText(string.Empty);
    }

    /// <summary>HUD 묶음을 통째로 올리거나 내린다. 같은 상태로 다시 불러도 안전하다.</summary>
    public void SetHudVisible(bool visible)
    {
        foreach (UIGroupFader group in _hudGroups)
        {
            if (group == null) continue;

            if (visible) group.FadeIn();
            else group.FadeOut();
        }
    }

    #endregion
}
