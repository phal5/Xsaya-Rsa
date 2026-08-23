using TMPro;
using UnityEngine;

/// <summary>
/// 화면 중앙 문구의 유일한 주인. 텍스트와 표시 여부를 <b>여기서만</b> 정한다.
///
/// 예전에는 이 오브젝트의 활성 상태를 셋이 각자 건드렸다 —
/// 이쪽의 SetActive, Central 페이더의 SetActivity, 그리고 TMP를 직접 잡고 있던 FlipBook.
/// 그래서 대사를 한 번 열면 페이더가 오브젝트를 되살리고, 같이 얹혀 있던 Instructor가
/// 예전 키 배선을 든 채로 부활했다.
///
/// 이제 표시는 전부 페이더를 거친다. SetActive를 직접 부르지 않는다 —
/// 알파와 활성 상태가 서로 어긋날 자리를 없애기 위해서다.
/// </summary>
public class MessageUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _text;

    [Tooltip("이 문구를 담고 있는 페이더. 표시 여부는 전부 이쪽으로 넘긴다.")]
    [SerializeField] UIGroupFader _fader;

    /// <summary>문구를 정한다. 빈 문자열이면 그대로 숨긴다.</summary>
    public void SetText(string text)
    {
        _text.text = text;
        SetVisibility(!string.IsNullOrEmpty(text));
    }

    public void SetVisibility(bool show)
    {
        if (show) _fader.FadeIn();
        else _fader.FadeOut();
    }
}
