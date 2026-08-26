using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FlipBook : MonoBehaviour
{
    [SerializeField] UnityEvent onSetBook;
    [SerializeField] UnityEvent onDialogueNull;

    [Tooltip("대사를 못 다 본 채로 강제로 닫을 때. 패널을 접는 연출만 물린다 — " +
             "onDialogueNull과 달리 조작 잠금 해제는 여기 물리지 않는다. 이미 다른 경로가 조작을 가져간 뒤일 수 있다.")]
    [SerializeField] UnityEvent onForceClose;
    [Space(10f)]
    [SerializeField] TextMeshProUGUI speaker;
    [SerializeField] TextMeshProUGUI dialogue;

    [Tooltip("화면 중앙 문구. TMP를 직접 잡지 않는다 — 표시 여부는 MessageUI가 정한다.")]
    [SerializeField] MessageUI midScreen;
    [Space(10f)]
    [SerializeField] private Book _book;
    public static FlipBook Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) Debug.LogError($"Duplicate instance of FlipBook found attached to {gameObject.name}.");
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        _book?.ClearEvents();
    }

    public void Next()
    {
        // 넘길 책이 없으면 조용히 지나간다. Enter 키가 상시 여기 물려 있어,
        // ForceClose가 책을 거둔 직후에도 그 프레임에 남은 입력이 이리로 새어 들어올 수 있다.
        if (_book == null) return;

        Page? nullablePage = _book.GetNextPage();
        if (nullablePage == null)
        {
            dialogue.text = speaker.text = string.Empty;
            midScreen.SetText(string.Empty);
            onDialogueNull.Invoke();
            RemoveBook();
            return;
        }
        Page page = nullablePage.Value;
        speaker.text = page.FooterSpeaker;
        dialogue.text = page.FooterText;

        // 빈 문구면 MessageUI가 알아서 숨긴다. 중앙 문구 페이더를 여기서 부르지 않는 이유다.
        midScreen.SetText(page.MidScreenText);
    }

    public void SetBook(Book book)
    {
        if (_book == book) return;
        RemoveBook();
        _book = book;

        // 되감을지는 <b>책이 정한다.</b> 페이지 커서는 책이 들고 있고 닫을 때 되돌아가지 않아,
        // 되감지 않는 책은 한 번 다 본 뒤로 열어도 곧장 닫힌다 — 그것이 한 번만 보여줄 서적의 동작이다.
        // 상점처럼 되풀이해 여는 자리만 책 쪽에서 되감기를 켠다.
        //
        // 여기서 조건을 보지 않는 이유는 입구가 하나가 아니어서다 — BookHolder로도 BookTrigger로도
        // 열리는데, 판단을 여는 쪽에 두면 같은 책이 입구에 따라 다르게 열린다.
        book?.Rewind();

        // <b>여는 것을 먼저 알린다.</b> Next()가 앞서면 페이지가 없는 책에서 순서가 뒤집힌다 —
        // Next()가 onDialogueNull로 먼저 닫고, 뒤이은 onSetBook이 다시 여는 꼴이 되어
        // 아무 대사도 없이 조작만 잠긴 채 남는다. (다 본 책을 다시 밟으면 그렇게 된다)
        onSetBook.Invoke();
        Next();
    }

    private void RemoveBook()
    {
        _book?.ClearEvents();
        _book = null;
    }

    /// <summary>지금 열려 있는 책이 이것인지. 남의 대사를 잘못 닫지 않으려면 부르는 쪽이 먼저 확인해야 한다.</summary>
    public bool IsShowing(Book book) => _book == book;

    /// <summary>
    /// 책이 끝나는 것을 듣는다. <b>씬을 넘어 이 이벤트에 닿는 유일한 길이다</b> —
    /// 배경 씬의 관문은 UI 씬의 이 오브젝트를 인스펙터에 꽂을 수 없다.
    ///
    /// 건 쪽이 <see cref="RemoveDialogueEndListener"/>로 반드시 거둔다.
    /// 관문은 스테이지와 함께 파괴되므로 남겨두면 죽은 대상을 부르게 된다.
    /// </summary>
    public void AddDialogueEndListener(UnityAction call)
    {
        onDialogueNull.AddListener(call);
    }

    public void RemoveDialogueEndListener(UnityAction call)
    {
        onDialogueNull.RemoveListener(call);
    }

    /// <summary>
    /// 지금 열려 있는 책을 <b>못 다 본 채로</b> 닫는다. 그 책을 연 관문이 씬과 함께 사라졌을 때 쓴다.
    ///
    /// <see cref="Next"/>가 끝에 닿아 스스로 닫는 정상 경로와 갈리는 지점은 <b>조작 잠금</b>이다.
    /// onDialogueNull은 ControlLock.Release도 함께 물고 있는데, 대사가 끊긴 것은 대개 사망처럼
    /// 다른 경로가 이미 조작을 가져간 뒤라 — 여기서 그것까지 풀면 되찾아간 조작을 도로 빼앗는다.
    /// 그래서 패널을 접는 연출만 <see cref="onForceClose"/>에 따로 물려 그쪽만 부른다.
    ///
    /// <b>다른 책이 이미 열려 있으면 아무것도 하지 않는다.</b> 부르는 쪽이 자기 책이 아직도
    /// 열려 있는지 매번 확인할 필요 없이, 늦게 도착한 호출이 남의 대사를 끊지 않도록 여기서 막는다.
    /// </summary>
    public void ForceClose(Book book)
    {
        if (_book != book) return;

        dialogue.text = speaker.text = string.Empty;
        midScreen.SetText(string.Empty);
        onForceClose.Invoke();
        RemoveBook();
    }
}

[Serializable]
public struct PageEvent
{
    public int pageIndex;
    public UnityEvent action;
}

[Serializable]
public class Book
{
#nullable enable

    [Tooltip("다시 열 때 첫 장부터 되감을지. <b>꺼두면 한 번 다 본 뒤로는 열어도 곧장 닫힌다</b> - " +
             "한 번만 보여줄 서적이 그것이다. 상점처럼 되풀이해 여는 자리만 켠다.")]
    [SerializeField] private bool _rewind;

    [SerializeField] private List<Page> _pages = new();
    [SerializeField] private List<PageEvent> _pageEvents = new();
    int page = -1;

    public Book() { }

    public Book(List<Page> pages) { _pages = pages; }

    /// <summary>
    /// 띄울 것이 있는지. 인스펙터에 비워둔 책을 여는 것과 아예 열지 않는 것을 구별하는 데 쓴다.
    ///
    /// 빈 책도 <see cref="FlipBook.SetBook"/>에 넣으면 열자마자 닫히므로 결과는 비슷하지만,
    /// 그 사이에 조작이 한 번 잠기고 풀려 화면이 깜빡인다. 그 창을 아예 만들지 않는 쪽이 낫다.
    /// </summary>
    public bool HasPages => _pages != null && _pages.Count > 0;

    /// <summary>
    /// 여는 김에 첫 장으로 되돌린다. <b><see cref="_rewind"/>를 켜둔 책만 되돌아간다</b> —
    /// 끄고 둔 책은 그대로 두어, 한 번 다 본 뒤로는 열어도 곧장 닫히는 지금 동작을 지킨다.
    ///
    /// 되감을지를 여는 쪽이 아니라 책이 정하는 이유. 여는 입구가 하나가 아니다 —
    /// <see cref="BookHolder"/>로도 열리고 <see cref="BookTrigger"/>로도 열리는데,
    /// 그 둘에 각각 표시를 두면 같은 책이 입구에 따라 다르게 열린다.
    /// </summary>
    public void Rewind()
    {
        if (!_rewind) return;

        page = -1;
    }

    public Page? GetNextPage()
    {
        ++page;
        return GetPage();
    }

    public Page? GetPreviousPage()
    {
        --page;
        return GetPage();
    }

    public Page? GoToPage(int page)
    {
        this.page = page;
        return GetPage();
    }

    public Page? GetPage()
    {
        if (page >= 0 && page < _pages.Count)
        {
            foreach (var pageEvent in _pageEvents)
            {
                if (pageEvent.pageIndex == page)
                {
                    pageEvent.action?.Invoke();
                }
            }
            return _pages[page];
        }
        else return null;
    }
    public void ClearEvents()
    {
        if (_pageEvents == null) return;

        foreach (var pageEvent in _pageEvents)
        {
            pageEvent.action?.RemoveAllListeners();
        }
    }

#nullable disable
}

[Serializable]
public struct Page
{
    public string MidScreenText;
    public string FooterSpeaker;
    public string FooterText;
}
