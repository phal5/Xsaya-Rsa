using UnityEngine;

public class AgnostosManager : EntityManager
{
    [field: SerializeField] public SingularFlatMovement movement { get; private set; }
    [field: SerializeField] public Transform character { get; private set; }
    [field: SerializeField] public float speed {  get; private set; }
    [field: SerializeField] public float attackRadius {  get; private set; }
    [field: SerializeField] public Transform[] legRoots { get; private set; }
}