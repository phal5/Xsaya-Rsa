using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private float damage = 25;
    [SerializeField] private Collider weaponCollider;
    [SerializeField] private Collider weilder;

    private List<IDamageable> alreadyHitTargets = new List<IDamageable>();

    private void Awake()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void StartAttack()
    {
        alreadyHitTargets.Clear();
        weaponCollider.enabled = true;
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
