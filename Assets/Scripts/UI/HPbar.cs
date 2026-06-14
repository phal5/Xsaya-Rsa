using UnityEngine;
using UnityEngine.UI;

public class HPbar : MonoBehaviour
{
    [SerializeField] Image bar;
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
    }
}
