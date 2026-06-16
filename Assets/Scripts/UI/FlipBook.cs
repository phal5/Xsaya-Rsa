using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FlipBook : MonoBehaviour
{
    [SerializeField] UnityEvent onSetBook;
    [SerializeField] UnityEvent onDialogueNull;
    [Space(10f)]
    [SerializeField] TextMeshProUGUI speaker;
    [SerializeField] TextMeshProUGUI dialogue;
    [SerializeField] TextMeshProUGUI midScreen;
    [Space(10f)]
    [SerializeField] private Book _book;
    public static FlipBook Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else Debug.LogError($"Duplicate instance of FlipBook found attached to {gameObject.name}.");
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
        Page? nullablePage = _book.GetNextPage();
        if (nullablePage == null)
        {
            dialogue.text = midScreen.text = speaker.text = string.Empty;
            onDialogueNull.Invoke();
            return;
        }
        Page page = nullablePage.Value;
        speaker.text = page.FooterSpeaker;
        dialogue.text = page.FooterText;
        midScreen.text = page.MidScreenText;
    }

    public void SetBook(Book book)
    {
        if (_book == book) return;
        _book?.ClearEvents();
        _book = book;
        Next();
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

    [SerializeField] private List<Page> _pages = new();
    [SerializeField] private List<PageEvent> _pageEvents = new();
    [field: SerializeField] public int page { get; private set; } = -1;

    public Book() { }

    public Book(List<Page> pages) { _pages = pages; }

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
