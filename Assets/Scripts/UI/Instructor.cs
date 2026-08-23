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
            // 남의 오브젝트가 꺼지는 것에 기대지 않고 직접 멈춘다.
            // 예전에는 SetVisibility(false)가 이 오브젝트째로 껐지만,
            // 다른 쪽이 다시 켜면 예전 배선을 든 채로 되살아났다.
            enabled = false;

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
