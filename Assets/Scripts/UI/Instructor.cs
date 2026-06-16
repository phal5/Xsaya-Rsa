using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Instructor : MonoBehaviour
{
    [SerializeField] Key _key;
    [SerializeField] UnityEvent _event;
    [Space(10f)]
    [SerializeField] MessageUI _messageUI;

    private void Awake()
    {
        if(_messageUI == null) TryGetComponent(out  _messageUI);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[_key].wasPressedThisFrame)
        {
            _messageUI.SetVisibility(false);
            _event.Invoke();
        }
    }

    private void SetMessage(string message)
    {
        _messageUI.SetText(message);
        _messageUI.SetVisibility(true);
    }

    private void SetEvent(UnityEvent onAccomplish)
    {
        _event.RemoveAllListeners();
        if (onAccomplish != null)
        {
            _event.AddListener(onAccomplish.Invoke);
        }
    }

    public void Set(Key key, string message, UnityEvent onAccomplish)
    {
        _key = key;
        SetMessage(message);
        SetEvent(onAccomplish);
        this.enabled = true;
    }
}
