using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ComboEvaluator
{
    private ComboTrieNode root;

    // Generate Trie on Instantiation
    public ComboEvaluator(List<ComboEvent> combos)
    {
        BuildTrie(combos);
    }

    // 
    private void BuildTrie(List<ComboEvent> combos)
    {
        root = new ComboTrieNode();
        foreach (var combo in combos)
        {
            if (combo.keySequence == null || combo.keySequence.Length == 0) continue;
            ComboTrieNode current = root;

            for (int i = combo.keySequence.Length - 1; i >= 0; i--)
            {
                Key key = combo.keySequence[i];
                if (!current.Children.ContainsKey(key))
                {
                    current.Children[key] = new ComboTrieNode();
                }
                current = current.Children[key];
            }
            current.ComboAction = combo.onExecute;
        }
    }

    public bool EvaluateAndInvoke(List<Key> inputRecords)
    {
        if (inputRecords == null || inputRecords.Count == 0) return false;

        ComboTrieNode current = root;
        ComboTrieNode best = null;

        // Search Trie in Flipped Order
        for (int i = inputRecords.Count - 1; i >= 0; i--)
        {
            Key currentKey = inputRecords[i];

            if (current.Children.TryGetValue(currentKey, out ComboTrieNode nextNode))
            {
                current = nextNode;
                if (current.ComboAction != null) best = current;
            }
            else
            {
                // no children (with matching key)
                break;
            }
        }
        // execute action
        if (best == null) return false; //No match found.
        if (best.ComboAction == null) //This is not supposed to happen
        {
            Debug.LogError("This can't be happening");
            return false;
        }
        best.ComboAction.Invoke();
        //Flushing is entitled entirely to each skill envoked
        //This is to ap[ply skillchains later on(if possible - sth like ^X>X<X).
        return true;
    }

#nullable enable

    private bool SearchStep(ref ComboTrieNode current, ref ComboTrieNode best, Key currentKey)
    {
        bool exit = false;
        if (current.Children.TryGetValue(currentKey, out ComboTrieNode nextNode))
        {
            current = nextNode;
            if (current.ComboAction != null) best = current;

            exit = false;
        }
        else
        {
            exit = true;
        }
        return exit;
    }

#nullable disable
}
