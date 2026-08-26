using UnityEngine;

/// <summary>
/// 잡히면 재화를 남긴다. Agnostos처럼 잡아서 벌이가 되는 것에 붙인다.
///
/// <b>스스로 붙는다.</b> <see cref="DamagableBase"/>의 onDeath에 인스펙터로 꽂아도 같은 일이 되지만,
/// 그러면 프리팹을 복제하다 한 마리쯤 꽂히지 않은 것이 생기고 — 그건 화면에 아무 표시도 남기지 않는다.
/// 잡았는데 아무 일도 없었다는 것을 플레이어가 알아채기 전까지 아무도 모른다.
///
/// 재화를 몸에 들려 보내지 않고 <see cref="FinanceManager"/>에 바로 넣는다.
/// 떨어뜨린 것을 주우러 가는 규칙이 없으므로, 그 사이를 만들면 주울 수 없는 자리에
/// 떨어진 재화가 생길 뿐이다.
/// </summary>
[RequireComponent(typeof(DamagableBase))]
public class Bounty : MonoBehaviour
{
    [Tooltip("잡았을 때 들어오는 재화의 양.")]
    [SerializeField, Min(0)] int _amount = 1;

    DamagableBase _damagable;

    void Awake()
    {
        _damagable = GetComponent<DamagableBase>();
        _damagable.AddDeathListener(Pay);
    }

    void OnDestroy()
    {
        // 파괴되는 그 순간에도 걷어둔다. 리스너가 파괴된 대상을 붙들고 있으면
        // UnityEvent가 다음 발신에서 조용히 걸러내는데, 조용한 것이 늘 좋지는 않다.
        if (_damagable != null) _damagable.RemoveDeathListener(Pay);
    }

    void Pay()
    {
        FinanceManager.Earn(_amount);
    }
}
