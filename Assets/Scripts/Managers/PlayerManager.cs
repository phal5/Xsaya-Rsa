using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [field: SerializeField] public Transform player {  get; private set; }
    [field: SerializeField] public DamagableBase playerDamagable {  get; private set; }

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
