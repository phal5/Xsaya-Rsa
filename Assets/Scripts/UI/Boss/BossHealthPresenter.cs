using UnityEngine;

/// <summary>
/// 보스의 체력을 <see cref="BossHealthVM"/>에 옮겨 적는다. 보스 프리팹에 붙인다.
///
/// <b>UI를 모른다.</b> 바가 있든 없든, UI 씬이 아직 안 올라왔든 하는 일이 같다 —
/// 값을 적어둘 뿐이고 그리는 것은 저쪽 사정이다.
///
/// 띄우고 내리는 시점은 이미 짝이 맞은 자리가 있다.
///   Boss_Idle.ToAlert()  각성 — 화면이 3D로 열리는 그 지점
///   Boss_Dead.Enter()    사망 — 2D로 되돌리는 그 지점
/// </summary>
public class BossHealthPresenter : MonoBehaviour
{
    [Tooltip("이 보스의 체력바가 볼 VM 애셋. 스테이지마다 하나씩 둔다.")]
    [SerializeField] BossHealthVM _vm;

    [Tooltip("읽어올 체력. 같은 프리팹 안이라 그대로 꽂힌다.")]
    [SerializeField] DamagableBase _health;

    [Tooltip("바에 띄울 이름. 비우면 오브젝트 이름을 쓴다.")]
    [SerializeField] string _displayName = "";

    string Label => string.IsNullOrEmpty(_displayName) ? gameObject.name : _displayName;

    float Ratio => _health == null || _health.MaxHealth <= 0f
        ? 0f
        : _health.CurrentHealth / _health.MaxHealth;

    void OnEnable()
    {
        if (_health != null) _health.AddDamageListener(OnDamaged);
    }

    void OnDisable()
    {
        if (_health != null) _health.RemoveDamageListener(OnDamaged);

        // 씬이 내려가며 보스가 사라지는 경우까지 여기서 닫는다.
        // 사망을 거치지 않고 없어지면 바가 빈 화면에 남는다.
        if (_vm != null) _vm.Hide();
    }

    /// <summary>전투가 열릴 때. 보스 상태기계가 부른다.</summary>
    public void Show()
    {
        if (_vm == null) return;

        _vm.Show(Label, Ratio);
    }

    /// <summary>전투가 끝날 때.</summary>
    public void Hide()
    {
        if (_vm == null) return;

        _vm.Hide();
    }

    // 피해량은 쓰지 않는다. 남은 비율은 체력에 직접 묻는 편이 정확하다 —
    // 배율이나 클램프가 끼면 "맞은 만큼 줄었다"가 성립하지 않는다.
    void OnDamaged(float _)
    {
        if (_vm != null) _vm.SetHealth(Ratio);
    }
}
