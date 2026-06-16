using UnityEngine;

public class CharacterManager : EntityManager
{
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
    [field:SerializeField] public Character_Movement Movement {  get; private set; }
    [field: SerializeField] public Character_Steering Steering { get; private set; }
    [field:SerializeField] public CapsuleCollider CapsuleCollider { get; private set; }
    [Header("Gameplay")]
    [field: SerializeField] public float CoyoteTime { get; private set; } = 0.2f;
    [Header("Horizontal Speed")]
    [field: SerializeField] public float GroundSpeed { get; private set; }
    [field: SerializeField] public float AirborneSpeed { get; private set; }
    [field: SerializeField] public float DashSpeed { get; private set; }
    [Header("Vertical Speed")]
    [field:SerializeField] public float JumpSpeed { get; private set; }
    [Header("Spherecaster")]
    [field:SerializeField] public SphereCaster Caster { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
