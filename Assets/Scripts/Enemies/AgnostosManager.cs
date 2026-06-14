using UnityEngine;

public class AgnostosManager : EntityManager
{
    [field: SerializeField] public Agnostos agnostosEnemy {  get; private set; }
    [field: SerializeField] public SingularFlatMovement movement { get; private set; }
    [field: SerializeField] public Transform character { get; private set; }
    [field: SerializeField] public float speed { get; private set; }
    [field: SerializeField] public float awakeRadius { get; private set; }
    [field: SerializeField] public float attackRadius { get; private set; }
    [field: SerializeField] public float attackReadyTime { get; private set; }
    [field: SerializeField] public float attackTime { get; private set; }
    [field: SerializeField] public float stunRecoveryTime { get; private set; }
    [field: SerializeField] public TransformWeightBlender[] footTargets { get; private set; }
    [field: SerializeField] public Transform[] legRoots { get; private set; }
    [field: SerializeField] public Transform[] walkTargets { get; private set; }
    [field: SerializeField] public Transform[] AttackReadyTargets { get; private set; }
    [field: SerializeField] public Transform AttackTarget { get; private set; }
}