using UnityEngine;

public class CharacterManager : EntityManager
{
    [field:SerializeField] public Character_Movement movement {  get; private set; }
    [field: SerializeField] public Character_Steering steering { get; private set; }
    [field:SerializeField] public CapsuleCollider capsuleCollider { get; private set; }
    [Header("Horizontal Speed")]
    [field: SerializeField] public float groundSpeed { get; private set; }
    [field: SerializeField] public float airborneSpeed { get; private set; }
    [field: SerializeField] public float dashSpeed { get; private set; }
    [Header("Vertical Speed")]
    [field:SerializeField] public float jumpSpeed { get; private set; }
    [Header("Spherecaster")]
    [field:SerializeField] public SphereCaster caster { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
