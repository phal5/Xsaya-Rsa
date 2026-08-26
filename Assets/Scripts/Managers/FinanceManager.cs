using UnityEngine;

/// <summary>
/// 가진 재화의 유일한 주인. 다른 스크립트는 잔액을 제 안에 따로 들고 있지 않는다.
///
/// <see cref="TimeManager"/>와 같은 꼴이다 — MonoBehaviour가 아니고, 씬에 놓을 오브젝트도
/// 인스턴스 null 검사도 없다. 재화는 스테이지를 넘어 이어져야 하는데, 씬에 놓인 오브젝트로 두면
/// 그 씬이 내려가는 순간 함께 사라지거나 다음 씬에서 하나가 더 생긴다.
///
/// <b>버는 것과 쓰는 것을 한 연산으로 둔다.</b> <see cref="Spend"/>는 검사와 차감을 함께 하고
/// 모자라면 아무것도 건드리지 않는다. 바깥에서 "볼 수 있나"와 "깎는다"를 나눠 부르게 두면
/// 그 사이에 다른 것이 끼어들 수 있고, 무엇보다 둘 중 하나를 잊은 자리가 조용히 생긴다.
/// </summary>
public static class FinanceManager
{
    static int _balance;

    /// <summary>지금 가진 재화. 읽기만 된다 — 늘리고 줄이는 길은 아래 둘뿐이다.</summary>
    public static int Balance => _balance;

    /// <summary>
    /// 재화를 얻는다. 0 이하는 조용히 지나간다 —
    /// 음수로 버는 것은 쓰는 것이고, 그건 <see cref="Spend"/>가 할 일이다.
    /// </summary>
    public static void Earn(int amount)
    {
        if (amount <= 0) return;

        _balance += amount;
    }

    /// <summary>
    /// 정해진 만큼 쓴다. <b>모자라면 아무것도 깎지 않고 거짓을 돌려준다.</b>
    /// </summary>
    /// <returns>실제로 깎였는지.</returns>
    public static bool Spend(int cost)
    {
        if (cost < 0) return false;
        if (_balance < cost) return false;

        _balance -= cost;
        return true;
    }

    /// <summary>
    /// 판을 처음부터 시작할 때 잔액을 되돌린다.
    ///
    /// static 필드는 도메인 리로드에서만 저절로 비워진다. 에디터에서 Reload Domain을 꺼두면
    /// 지난 판의 잔액이 그대로 남아, 아무것도 잡지 않았는데 살 수 있는 판이 된다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset()
    {
        _balance = 0;
    }
}
