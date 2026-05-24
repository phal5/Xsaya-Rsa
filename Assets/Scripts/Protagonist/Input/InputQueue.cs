using System; // Required for IDisposable
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities; // Required for .Call() extension

public class InputQueue : MonoBehaviour
{
    [SerializeField] private UnityEvent<List<Key>> comboCheckKeys;
    [SerializeField] private float inputLifetime = 0.5f;
    [SerializeField] private int maxBufferSize = 10;

    private Queue<Key> keyQueue = new Queue<Key>();
    private Queue<float> timestampQueue = new Queue<float>();

    // Store the subscription to clean it up later
    private IDisposable anyButtonListener;

    void OnEnable()
    {
        anyButtonListener = InputSystem.onAnyButtonPress.Call(OnAnyKeyPress);
    }

    void OnDisable()
    {
        // Dispose of the subscription to prevent memory leaks - Important
        anyButtonListener?.Dispose();
    }

    void Update()
    {
        FlushExpired();
    }

    private void OnAnyKeyPress(InputControl control)
    {
        ProcessKeyInput(control);
        InvokeComboCheck();
    }

    private void InvokeComboCheck()
    {
        comboCheckKeys.Invoke(keyQueue.ToList());
    }

    private void ProcessKeyInput(InputControl control)
    {
        if (control is KeyControl keyControl)
        {
            EnqueueKey(keyControl.keyCode);
            //Debug();
        }
    }

    private void EnqueueKey(Key pressedKey)
    {
        if (pressedKey == Key.None) return;

        if (keyQueue.Count >= maxBufferSize)
        {
            keyQueue.Dequeue();
            timestampQueue.Dequeue();
        }

        keyQueue.Enqueue(pressedKey);
        timestampQueue.Enqueue(Time.time);
    }

    private void FlushExpired()
    {
        while (timestampQueue.Count > 0 && (Time.time - timestampQueue.Peek()) > inputLifetime)
        {
            keyQueue.Dequeue();
            timestampQueue.Dequeue();
        }
    }

    public void Debug()
    {
        if (keyQueue.Count > 0)
        {
            string str = "";
            foreach (Key key in keyQueue)
            {
                str += key.ToString() + " ";
            }
            print(str);
        }
        else print("Input is Empty!");
    }
}