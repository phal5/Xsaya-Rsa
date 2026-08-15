using UnityEngine;

/// <summary>
/// 스킬 선택기. 슬롯 세 개를 알고 있고(여기까지는 결합을 수용한다),
/// 각 슬롯이 실제로 무슨 스킬을 수행하는지는 모른다.
///
/// 근접      → 속공 슬롯
/// 원거리    → 강공 슬롯
/// 특수 조건 → 특수기 슬롯
///
/// ComponentStates: Boss_QuickSlot, Boss_HeavySlot, Boss_SpecialSlot
/// </summary>
public class Boss_SkillMachine : FiniteStateMachine
{
    BossManager _boss;

    BossManager Boss
    {
        get
        {
            if (_boss == null) _boss = manager as BossManager;
            return _boss;
        }
    }

    public override void Enter()
    {
        Select();
    }

    void Select()
    {
        if (Boss == null) { Complete(); return; }

        // TODO: 특수기 발동 조건은 BossManager.SpecialCondition에 채워 넣으면 여기서 바로 잡힌다.
        if (Boss.SpecialCondition && Ready<Boss_SpecialSlot>())
        {
            TransitTo<Boss_SpecialSlot>();
            return;
        }

        bool melee = Boss.DistanceToPlayer() <= Boss.meleeRadius;

        // 거리에 맞는 슬롯을 먼저 보고, 쿨다운이면 남은 쪽으로 넘긴다.
        if (melee)
        {
            if (Use<Boss_QuickSlot>()) return;
            if (Use<Boss_HeavySlot>()) return;
        }
        else
        {
            if (Use<Boss_HeavySlot>()) return;
            if (Use<Boss_QuickSlot>()) return;
        }

        // 전부 쿨다운이면 이동으로 되돌려 거리를 다시 잡게 한다.
        Complete();
    }

    bool Use<TSlot>() where TSlot : Boss_SkillSlot
    {
        if (!Ready<TSlot>()) return false;

        TransitTo<TSlot>();
        return true;
    }

    /// <summary>
    /// 슬롯을 _componentStates에서 직접 찾는다.
    /// _statePool은 이 머신이 처음 진입될 때 만들어지므로, 진입 전 조회에는 쓸 수 없다.
    /// </summary>
    TSlot Slot<TSlot>() where TSlot : Boss_SkillSlot
    {
        if (_componentStates == null) return null;

        foreach (MonoBehaviour component in _componentStates)
        {
            if (component is TSlot slot) return slot;
        }
        return null;
    }

    bool Ready<TSlot>() where TSlot : Boss_SkillSlot
    {
        TSlot slot = Slot<TSlot>();
        if (slot == null || Boss == null) return false;

        // 쿨다운뿐 아니라 사거리도 본다. 이게 없으면 먼 거리에서 허공에 휘두르며 접근하지 않는다.
        return slot.Profile.IsReady && Boss.DistanceToPlayer() <= slot.Profile.range;
    }

    /// <summary>
    /// 이동 머신이 스킬로 넘어갈지 판단할 때 묻는다.
    /// Select와 같은 조건을 써야 한다. 어긋나면 넘어왔다가 되돌아가는 왕복이 생긴다.
    /// </summary>
    public bool AnySlotReady()
    {
        if (Boss == null) return false;

        if (Boss.SpecialCondition && Ready<Boss_SpecialSlot>()) return true;

        return Ready<Boss_QuickSlot>() || Ready<Boss_HeavySlot>();
    }

    /// <summary>슬롯이 끝났거나 쓸 수 있는 슬롯이 없을 때 이동으로 되돌린다.</summary>
    public void Complete()
    {
        if (fsm is Boss_Alert alert) alert.ToMovement();
    }
}
