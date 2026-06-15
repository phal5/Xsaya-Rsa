using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPbar : MonoBehaviour
{
    [SerializeField] Image bar;
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField][Range(0, 1)] float minSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DamagableBase playerDamagable = PlayerManager.instance.playerDamagable;
        float hpRate = playerDamagable.CurrentHealth / playerDamagable.MaxHealth;
        bar.fillAmount = hpRate;
        Time.timeScale = hpRate * (1 - minSpeed) + minSpeed;
        _text.text = (Mathf.Floor(Time.timeScale * 10) * 0.1f).ToString();
    }
}
