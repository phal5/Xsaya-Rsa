using UnityEngine;

/// <summary>
/// 기본 근접 공격. 이제 FSM 상태로 동작한다.
///
/// 콤보가 부르던 Attack() 진입점은 그대로 유지했다. UnityEvent 배선을 건드릴 필요가 없다.
/// 예전에는 자체 Update 타이머로 돌아 이동/점프와 무관하게 나갔지만,
/// 지금은 Character_Execution 안의 상태라 스킬 중 이동 규칙이 상태로 관리된다.
/// </summary>
public class BasicAttack : Character_SkillBase
{
    [Header("Weapon")]
    [SerializeField] MeleeWeapon _weapon;

    [Tooltip("공격하지 않는 동안 무기 오브젝트를 통째로 꺼둔다.")]
    [SerializeField] bool _disableWeaponObject;

    void Awake()
    {
        if (_disableWeaponObject && _weapon != null) _weapon.gameObject.SetActive(false);
    }

    /// <summary>콤보 UnityEvent가 부르는 이름. 유지 목적으로 남겨둔다.</summary>
    public void Attack()
    {
        Request();
    }

    protected override void OnBegin()
    {
        if (_weapon == null) return;

        if (_disableWeaponObject) _weapon.gameObject.SetActive(true);
        _weapon.StartAttack();
    }

    protected override void OnEnd()
    {
        if (_weapon == null) return;

        _weapon.EndAttack();
        if (_disableWeaponObject) _weapon.gameObject.SetActive(false);
    }
}
