using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [field: SerializeField] public Transform player {  get; private set; }
    [field: SerializeField] public DamagableBase playerDamagable {  get; private set; }
    [Tooltip("적이 공격 시작 시점을 감지하는 통로. 회피/방어 판정에 쓰인다.")]
    [field: SerializeField] public MeleeWeapon playerWeapon { get; private set; }

    public static PlayerManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(this);
    }
}
