using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// 외부에서 주입해줄 콤보 - 이벤트 구조체
[System.Serializable]
public struct ComboEvent
{
    public Key[] keySequence;
    public UnityEvent onExecute;

    public ComboEvent(Key[] sequence, UnityEvent action)
    {
        this.keySequence = sequence;
        this.onExecute = action;
    }
}

// 트리 검색을 위한 트라이(Trie) 노드 클래스
public class ComboTrieNode
{
    public Dictionary<Key, ComboTrieNode> Children { get; } = new Dictionary<Key, ComboTrieNode>();
#nullable enable
    public UnityEvent? ComboAction { get; set; } = null;
#nullable disable
}