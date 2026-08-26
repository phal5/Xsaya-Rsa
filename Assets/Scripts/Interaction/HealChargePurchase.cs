using UnityEngine;

/// <summary>
/// 재화를 치르고 회복 횟수를 하나 산다. 상점의 판매대 하나에 해당한다.
///
/// 상호작용 자체는 맡지 않는다. <see cref="InteractableEvent"/>의 onInteract에 <see cref="Buy"/>를
/// 꽂거나, UI 버튼에서 부르거나, 코드에서 <see cref="TryBuy"/>를 부른다 —
/// 무엇이 부르느냐를 여기서 정해두면 판매대를 놓는 방법이 하나로 굳는다.
///
/// <b>대사도 직접 띄우지 않는다.</b> 결과마다 <see cref="BookHolder"/>를 하나씩 가리키고 그것을 열 뿐이다.
/// 대사를 화면에 올리는 길은 <see cref="FlipBook"/> 하나여야 하고, 그 문을 여는 입구는
/// 이미 BookHolder다 — 여기서 MessageUI를 따로 잡으면 중앙 문구의 주인이 둘이 된다.
///
/// <b>순서가 규칙이다.</b> 자리를 먼저 보고, 그 다음에 값을 치른다.
/// 뒤집으면 꽉 찬 상태에서도 재화가 깎이고, 플레이어는 무엇을 샀는지 모른 채 잔액만 잃는다.
/// </summary>
public class HealChargePurchase : MonoBehaviour
{
    [Tooltip("회복 횟수 하나를 사는 데 드는 재화.")]
    [SerializeField, Min(0)] int _cost = 3;

    [Header("Dialogue")]
    [Tooltip("샀을 때 열 대사. 비워두면 아무 말도 하지 않는다.")]
    [SerializeField] BookHolder _bought;

    [Tooltip("재화가 모자랄 때 열 대사.")]
    [SerializeField] BookHolder _tooPoor;

    [Tooltip("회복 횟수가 이미 꽉 찼을 때 열 대사.")]
    [SerializeField] BookHolder _alreadyFull;

    CharacterManager _character;

    /// <summary>드는 값. 표시하는 쪽이 읽는다.</summary>
    public int Cost => _cost;

    /// <summary>
    /// 지금 살 수 있는지. <b>묻기만 한다</b> — 안내 문구를 바꾸거나 버튼을 흐리는 쪽이 쓴다.
    /// 이걸 보고 나서 <see cref="TryBuy"/>를 부를 필요는 없다. 그쪽이 같은 것을 다시 본다.
    /// </summary>
    public bool CanBuy
    {
        get
        {
            CharacterManager character = Character;

            return character != null
                && character.HealCharges < character.HealChargeMax
                && FinanceManager.Balance >= _cost;
        }
    }

    /// <summary>
    /// UnityEvent에 꽂는 자리. 인스펙터의 이벤트 목록에는 <b>void를 돌려주는 것만 뜨므로</b>
    /// <see cref="TryBuy"/>를 그대로 꽂을 수 없어 여기를 둔다.
    /// </summary>
    public void Buy()
    {
        TryBuy();
    }

    /// <summary>
    /// 살 수 있으면 사고, 아니면 아무것도 건드리지 않는다. 어느 쪽이든 그에 맞는 대사를 연다.
    /// </summary>
    /// <returns>실제로 샀는지.</returns>
    public bool TryBuy()
    {
        CharacterManager character = Character;
        if (character == null) return false;

        // 1. 꽉 차 있으면 살 것이 없다. 값을 치르기 전에 본다.
        if (character.HealCharges >= character.HealChargeMax)
        {
            Say(_alreadyFull);
            return false;
        }

        // 2. 검사와 차감이 한 연산이다. 모자라면 잔액은 그대로다.
        if (!FinanceManager.Spend(_cost))
        {
            Say(_tooPoor);
            return false;
        }

        // 3. 자리는 위에서 이미 확인했으므로 여기서 실패할 길이 없다.
        character.GrantHealCharge();
        Say(_bought);
        return true;
    }

    /// <summary>대사를 연다. 꽂아두지 않았으면 조용히 지나간다 — 말이 없는 상인도 있을 수 있다.</summary>
    static void Say(BookHolder book)
    {
        if (book != null) book.Open();
    }

    /// <summary>
    /// 주인공의 매니저. <b>런타임에 찾는다</b> — 상점은 배경 씬에 놓이고 캐릭터는 제 씬에 있어
    /// 인스펙터로 꽂을 수 없다. 씬을 넘는 참조는 유니티가 저장하지 못한다.
    /// 캐릭터 씬은 내려가지 않으므로 한 번 찾으면 들고 있는다.
    /// </summary>
    CharacterManager Character
    {
        get
        {
            if (_character != null) return _character;

            Transform player = PlayerManager.instance != null ? PlayerManager.instance.player : null;
            if (player == null) return null;

            // player가 몸일 수도, 그 위의 루트일 수도 있다. 루트에서 훑으면 어느 쪽이든 걸린다.
            _character = player.root.GetComponentInChildren<CharacterManager>(true);

            return _character;
        }
    }
}
