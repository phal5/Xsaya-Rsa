using UnityEngine;
using UnityEngine.Events;

public class DamagableBase : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [field: SerializeField] public float MaxHealth { get; private set; } = 100f;
    [field: SerializeField] public float CurrentHealth { get; private set; }
    [Space(10f)]
    [Tooltip("꺼두면 체력이 0이 되어도 오브젝트를 파괴하지 않는다. 다운 후 부활하는 플레이어용.")]
    [SerializeField] private bool _destroyOnDeath = true;
    [Space(10f)]
    [SerializeField] private UnityEvent<float> onDamage;
    [SerializeField] private UnityEvent onDestroy;

    private float _damageScale = 1f;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }

    /// <summary>방어 자세처럼 피해를 경감시키는 상태가 잠시 걸어두는 배율. 해제 시 1로 되돌린다.</summary>
    public void SetDamageScale(float scale)
    {
        _damageScale = scale;
    }

    /// <summary>
    /// 켜져 있는 동안 피격 판정을 아예 받지 않는다. 회피(대시)의 무적 프레임에 쓴다.
    /// 컴포넌트를 꺼서는 막을 수 없다. MeleeWeapon이 TryGetComponent로 찾는데
    /// 그건 비활성 컴포넌트도 잡아내기 때문이다.
    /// </summary>
    public bool Invulnerable { get; set; }

    public void TakeDamage(float damageAmount)
    {
        // 배율 0과 달리 onDamage 자체를 발신하지 않는다. 피격 상태가 헛돌지 않게.
        if (Invulnerable) return;

        damageAmount *= _damageScale;
        CurrentHealth -= damageAmount;

        Debug.Log($"{gameObject.name} took {damageAmount:F1} damage!");

        // 파괴 전에 알린다. Die()가 먼저 돌면 리스너가 이미 파괴된 오브젝트를 건드리게 된다.
        onDamage.Invoke(damageAmount);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} Destroyed!");
        onDestroy.Invoke();

        // 플레이어처럼 다운 후 부활하는 대상은 파괴하지 않는다.
        if (_destroyOnDeath) Destroy(gameObject);
    }

    /// <summary>체력을 회복시킨다. 최대치를 넘지 않는다.</summary>
    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }

    /// <summary>다운 상태에서 복귀시킨다.</summary>
    public void Revive()
    {
        CurrentHealth = MaxHealth;
        Invulnerable = false;
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
