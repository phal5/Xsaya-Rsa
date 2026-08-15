using UnityEngine;

public class CharacterManager : EntityManager
{
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
    [field:SerializeField] public Character_Movement Movement {  get; private set; }
    [field: SerializeField] public Character_Steering Steering { get; private set; }
    [field:SerializeField] public CapsuleCollider CapsuleCollider { get; private set; }
    [Tooltip("회피 무적과 피격 상태가 참조한다.")]
    [field: SerializeField] public DamagableBase Damagable { get; private set; }
    [Tooltip("상호작용 대상 탐지기. 비어 있으면 Interact 상태로 들어가지 않는다.")]
    [field: SerializeField] public InteractionDetector Interactor { get; private set; }
    [Header("Gameplay")]
    [field: SerializeField] public float CoyoteTime { get; private set; } = 0.2f;
    [field: SerializeField] public float JumpBufferTime { get; private set; } = 0.2f;
    [Header("Horizontal Speed")]
    [field: SerializeField] public float GroundSpeed { get; private set; }
    [field: SerializeField] public float AirborneSpeed { get; private set; }
    [field: SerializeField] public float DashSpeed { get; private set; }
    [Tooltip("회피(Dodge)가 지속되는 시간.")]
    [field: SerializeField] public float DashTime { get; private set; } = 0.2f;
    [Header("Vertical Speed")]
    [field:SerializeField] public float JumpSpeed { get; private set; }
    [Header("Hit / Down")]
    [Tooltip("피격 경직이 지속되는 시간.")]
    [field: SerializeField] public float StunTime { get; private set; } = 0.4f;
    [Tooltip("다운 후 부활까지 걸리는 시간.")]
    [field: SerializeField] public float RespawnDelay { get; private set; } = 2f;
    [Tooltip("부활 지점. 비워두면 쓰러진 자리에서 일어난다.")]
    [field: SerializeField] public Transform RespawnPoint { get; private set; }

    [Header("Heal - 자원 소모형")]
    [Tooltip("최대 회복 횟수. 시작 시 이만큼 채워진다.")]
    [field: SerializeField] public int HealChargeMax { get; private set; } = 3;
    [field: SerializeField] public float HealAmount { get; private set; } = 40f;
    [Tooltip("회복 동작이 걸리는 시간. 이 동안 조작을 잃는다.")]
    [field: SerializeField] public float HealTime { get; private set; } = 1.2f;

    [Header("Swap")]
    [field: SerializeField] public float SwapTime { get; private set; } = 0.5f;

    [Header("Interact")]
    [Tooltip("상호작용 동작이 붙드는 시간. 대화가 열리면 UI 상태가 따로 조작을 잠근다.")]
    [field: SerializeField] public float InteractTime { get; private set; } = 0.3f;

    [Header("Spherecaster")]
    [field:SerializeField] public SphereCaster GroundCaster { get; private set; }

    #region Heal Charges

    /// <summary>남은 회복 횟수. 직렬화하지 않고 시작 시 최대치로 채운다.</summary>
    public int HealCharges { get; private set; }

    public bool HasHealCharge => HealCharges > 0;

    void Awake()
    {
        HealCharges = HealChargeMax;
    }

    /// <summary>회복을 시도한다. 남은 횟수가 없으면 실패한다.</summary>
    public bool ConsumeHealCharge()
    {
        if (HealCharges <= 0) return false;

        HealCharges--;
        return true;
    }

    /// <summary>휴식 지점 등에서 회복 횟수를 되돌린다.</summary>
    public void RefillHealCharges()
    {
        HealCharges = HealChargeMax;
    }

    #endregion
}
