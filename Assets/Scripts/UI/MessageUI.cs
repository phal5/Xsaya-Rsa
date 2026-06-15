using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _text;

    public void SetText(string text)
    {
        _text.text = text;
    }

    public void SetVisibility(bool show)
    {
        gameObject.SetActive(show);
    }
}
