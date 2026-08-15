using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private float damage = 25;
    [SerializeField] private Collider weaponCollider;
    [SerializeField] private Collider weilder;

    private List<IDamageable> alreadyHitTargets = new List<IDamageable>();

    /// <summary>공격이 시작되는 순간 발신된다. 적이 이걸 보고 회피/방어를 판정한다.</summary>
    public event System.Action onAttackStart;

    private void Awake()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void StartAttack()
    {
        alreadyHitTargets.Clear();
        weaponCollider.enabled = true;
        onAttackStart?.Invoke();
    }

    /// <summary>스킬마다 위력이 다른 경우를 위해 발동 직전에 갈아끼운다.</summary>
    public void SetDamage(float value)
    {
        damage = value;
    }

    public void EndAttack()
    {
        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == weilder) return;
        if (other.TryGetComponent<IDamageable>(out IDamageable target))
        {
            if (!alreadyHitTargets.Contains(target))
            {
                target.TakeDamage(damage);
                alreadyHitTargets.Add(target);
            }
        }
    }
}
