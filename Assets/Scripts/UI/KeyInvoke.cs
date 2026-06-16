using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class KeyInvoke : MonoBehaviour
{
    [SerializeField] private Key _key;
    [SerializeField] private UnityEvent _event;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[_key].wasPressedThisFrame)
        {
            _event.Invoke();
        }
    }
}
