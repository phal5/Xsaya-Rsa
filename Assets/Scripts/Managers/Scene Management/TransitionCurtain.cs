using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 스테이지가 갈리는 동안 화면을 덮는 검은 막. 덮은 자리에 다음 지역의 이름을 띄운다.
///
/// <b>내려가지 않는 씬에 둔다.</b> 배경에 두면 커튼째 사라져 화면이 벗겨진다.
///
/// director가 이 자리를 찾아올 수 있도록 <b>스스로 등록한다.</b> 커튼은 캐릭터 씬 밖에 있어
/// director가 인스펙터로 꽂을 수 없다 — 씬을 넘는 참조는 유니티가 저장하지 못한다.
/// 자리가 하나뿐인 것은 "페이더"가 아니라 <b>화면을 덮는 역할</b>이 하나이기 때문이라,
/// 다른 페이더들과 부딪히지 않는다.
///
/// <b>모든 시간을 벽시계로 잰다.</b> 시간 배속이 체력을 따라가는 설계라, 죽어서 배속이 바닥일 때
/// 스케일 시계로 페이드를 돌리면 영영 끝나지 않는다.
/// </summary>
[DisallowMultipleComponent]
public class TransitionCurtain : MonoBehaviour
{
    public static TransitionCurtain Current { get; private set; }

    [Tooltip("덮는 막 전체. 알파를 여기서 움직인다.")]
    [SerializeField] CanvasGroup _group;

    [Tooltip("다음 지역 이름을 띄울 곳. 비워두면 이름 없이 덮기만 한다.")]
    [SerializeField] TMP_Text _label;

    [Tooltip("덮는 데 걸리는 시간.")]
    [SerializeField, Min(0f)] float _coverTime = 0.25f;

    [Tooltip("덮은 채로 머무는 최소 시간. 로드가 이보다 빨라도 이름을 읽을 틈은 준다.")]
    [SerializeField, Min(0f)] float _holdTime = 0.6f;

    [Tooltip("걷는 데 걸리는 시간.")]
    [SerializeField, Min(0f)] float _revealTime = 0.5f;

    void Awake()
    {
        Current = this;

        if (_group == null)
        {
            Debug.LogError($"[{name}] CanvasGroup이 꽂혀 있지 않습니다. 커튼이 동작하지 않습니다.", this);
            return;
        }

        // 시작은 걷힌 상태다. 켜둔 채 저장해도 게임이 가려진 채 시작하지 않게.
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    /// <summary>
    /// 화면을 덮는다. <b>돌아올 때는 이미 한 프레임 이상 덮인 채로 그려진 뒤다.</b>
    ///
    /// 그 보장이 필요한 이유는 뒤따르는 로드가 동기이기 때문이다 — 알파만 올려두고 곧바로
    /// 로드에 들어가면 덮인 화면이 한 번도 그려지지 않은 채로 프레임이 얼어붙는다.
    /// </summary>
    public IEnumerator Cover(string region)
    {
        if (_group == null) yield break;

        if (_label != null) _label.text = region;

        _group.blocksRaycasts = true;

        float from = _group.alpha;
        for (float t = 0f; t < _coverTime; t += Time.unscaledDeltaTime)
        {
            _group.alpha = Mathf.Lerp(from, 1f, t / _coverTime);
            yield return null;
        }

        _group.alpha = 1f;

        // 이 프레임이 덮인 채로 그려진 다음에 돌아간다.
        yield return null;
    }

    /// <summary>덮은 채로 최소 시간만큼 머문다. 로드가 끝난 뒤에 부른다.</summary>
    public IEnumerator Hold()
    {
        if (_holdTime > 0f) yield return new WaitForSecondsRealtime(_holdTime);
    }

    /// <summary>막을 걷는다.</summary>
    public IEnumerator Reveal()
    {
        if (_group == null) yield break;

        float from = _group.alpha;
        for (float t = 0f; t < _revealTime; t += Time.unscaledDeltaTime)
        {
            _group.alpha = Mathf.Lerp(from, 0f, t / _revealTime);
            yield return null;
        }

        _group.alpha = 0f;
        _group.blocksRaycasts = false;

        if (_label != null) _label.text = string.Empty;
    }
}
