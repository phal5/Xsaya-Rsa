using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlipBook : MonoBehaviour
{
    Book book;

    
}

[Serializable]
public class Book
{
#nullable enable

    [SerializeField] private List<Page> _pages;
    int _page = 0;

    public Page? GetNextPage()
    {
        return GetPage(_page++);
    }

    public Page? GoToPage(int page)
    {
        _page = page;
        return GetPage(_page++);
    }

    private Page? GetPage(int page)
    {
        if (page < _pages.Count) return _pages[page];
        else return null;
    }

#nullable disable
}

[Serializable]
public class Page
{
    public string MidScreenText;
    public string FooterSpeaker;
    public string FooterText;
    public UnityEvent _event;
}
