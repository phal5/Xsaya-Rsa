using UnityEngine;
using UnityEngine.Events;

public class DamagableBase : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [Space(10f)]
    [SerializeField] private UnityEvent<float> onDamage;
    [SerializeField] private UnityEvent onDestroy;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        
        Debug.Log($"{gameObject.name}took {damageAmount:F1} damage!");

        if (currentHealth <= 0f)
        {
            Die();
        }

        onDamage.Invoke(damageAmount);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} Destroyed!");
        onDestroy.Invoke();
        Destroy(gameObject);
    }

    public void AddDamageListener(UnityAction<float> call)
    {
        onDamage.AddListener(call);
    }

    public void RemoveDamageListener(UnityAction<float> call)
    {
        onDamage.RemoveListener(call);
    }
}
