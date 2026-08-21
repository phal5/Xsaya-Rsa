using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPbar : MonoBehaviour
{
    [SerializeField] Image bar;
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField][Range(0, 1)] float minSpeed;

    // Update is called once per frame
    void Update()
    {
        DamagableBase playerDamagable = PlayerManager.instance.playerDamagable;
        float hpRate = playerDamagable.CurrentHealth / playerDamagable.MaxHealth;
        bar.fillAmount = hpRate;
        TimeManager.SetScale(hpRate * (1 - minSpeed) + minSpeed);

        // 정지 중에는 Time.timeScale이 0이다. 표시는 체력이 정한 배속을 그대로 읽는다.
        _text.text = (Mathf.Floor(TimeManager.Scale * 10) * 0.1f).ToString();
    }
}
