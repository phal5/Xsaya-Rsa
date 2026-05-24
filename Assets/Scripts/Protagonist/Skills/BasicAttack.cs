using UnityEngine;

public class BasicAttack : MonoBehaviour
{
    [SerializeField] MeleeWeapon _weapon;
    [SerializeField] float _duration;
    [SerializeField] bool _disableWeaponObject;

    bool _onAttack;
    float _endTime;

    private void Awake()
    {
        if (_disableWeaponObject) _weapon.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (_onAttack)
        {
            if (Time.time > _endTime)
            {
                _onAttack = false;
                _weapon.EndAttack();
                if (_disableWeaponObject) _weapon.gameObject.SetActive(false);
            }
        }
    }

    public void Attack()
    {
        if (_weapon == null) return;

        _onAttack = true;
        _weapon.StartAttack();
        if (_disableWeaponObject) _weapon.gameObject.SetActive(true);
        _endTime = Time.time + _duration;
    }
}
