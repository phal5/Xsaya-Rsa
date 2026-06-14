using UnityEngine;

public class BasicAttack : MonoBehaviour
{
    [SerializeField] MeleeWeapon _weapon;
    [SerializeField] float _duration;
    [SerializeField] float _cooldown;
    [SerializeField] bool _disableWeaponObject;

    bool _onAttack;
    float _endTime;
    float _cooldownDue;

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
                Reset();
                SetCooldown();
            }
        }
    }

    public void Attack()
    {
        if (_weapon == null) return;
        if (Time.time < _cooldownDue) return;
        if (Time.time < _endTime) return;

        _onAttack = true;
        _weapon.StartAttack();
        if (_disableWeaponObject) _weapon.gameObject.SetActive(true);
        _endTime = Time.time + _duration;
    }

    public void SetCooldown()
    {
        _cooldownDue = Time.time + _cooldown;
    }

    public void Reset()
    {
        _onAttack = false;
        _weapon.EndAttack();
        if (_disableWeaponObject) _weapon.gameObject.SetActive(false);
    }
}
