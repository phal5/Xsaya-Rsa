using UnityEngine;

public class AgnostosManager : EntityManager
{
    [field: SerializeField] public Agnostos agnostosEnemy {  get; private set; }
    [field: SerializeField] public SingularFlatMovement movement { get; private set; }
    [field: SerializeField] public Transform character { get; private set; }
    [field: SerializeField] public float speed { get; private set; }
    [field: SerializeField] public float awakeRadius { get; private set; }
    [field: SerializeField] public float attackRadius { get; private set; }
    [field: SerializeField] public float retreatRadius { get; private set; } = 4.2f;
    [field: SerializeField] public float attackReadyTime { get; private set; }
    [field: SerializeField] public float attackTime { get; private set; }
    [field: SerializeField] public float attackCooldown { get; private set; } = 0.3f;
    [field: SerializeField] public float stunRecoveryTime { get; private set; }
    [Tooltip("쓰러진 뒤 오브젝트가 사라지기까지의 시간. Agnostos의 Destroy On Death는 꺼두어야 한다.")]
    [field: SerializeField] public float despawnDelay { get; private set; } = 2f;
    [field: SerializeField] public TransformWeightBlender[] footTargets { get; private set; }
    [field: SerializeField] public Transform[] legRoots { get; private set; }
    [field: SerializeField] public Transform[] walkTargets { get; private set; }
    [field: SerializeField] public Transform[] AttackReadyTargets { get; private set; }
    [field: SerializeField] public Transform AttackTarget { get; private set; }
    [field: SerializeField] public MeleeWeapon weapon { get; private set; }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(character.position, awakeRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(character.position, attackRadius);
    }
}