using UnityEngine;
using UnityEngine.Events;

public class DamagableBase : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [field: SerializeField] public float MaxHealth { get; private set; } = 100f;
    [field: SerializeField] public float CurrentHealth { get; private set; }
    [Space(10f)]
    [SerializeField] private UnityEvent<float> onDamage;
    [SerializeField] private UnityEvent onDestroy;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        
        Debug.Log($"{gameObject.name}took {damageAmount:F1} damage!");

        if (CurrentHealth <= 0f)
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
