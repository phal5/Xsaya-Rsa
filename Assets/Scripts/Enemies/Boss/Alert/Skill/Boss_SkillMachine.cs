using UnityEngine;

/// <summary>
/// 스킬 선택기. 행동 트리의 셀렉터와 같은 일을 한다.
///
/// Component States에 넣은 <b>순서가 곧 우선순위이자 조건 검사 순서</b>다.
/// 위에서부터 훑어 쓸 수 있는 첫 스킬을 고르고, 전부 안 되면 이동으로 되돌린다.
///
/// 이 머신은 스킬의 종류를 모른다. 개수도 모른다.
/// 쿨다운·사거리·조건은 전부 스킬이 스스로 답하므로(IsReady), 스킬을 늘리려면
/// 컴포넌트를 붙이고 목록에 넣기만 하면 된다.
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

    /// <summary>
    /// 스킬은 타입이 아니라 인스턴스로 가린다.
    /// 그래야 같은 부품(Boss_Skill)을 값만 달리해 여러 개 달 수 있다 — 스킬마다 클래스를 쓰지 않아도 된다.
    /// </summary>
    protected override bool ComponentStatesAreInstances => true;

    public override void Enter()
    {
        Boss_SkillBase skill = Pick();

        if (skill == null) { Complete(); return; }   // 쓸 게 없으면 이동으로 되돌린다

        IState state = ResolveComponent(skill);
        if (state == null) { Complete(); return; }

        Transit(state);
    }

    /// <summary>
    /// 목록을 위에서부터 훑어 쓸 수 있는 첫 스킬을 고른다. 없으면 null.
    /// <b>고르기만 하고 전이하지 않는다</b> — 그래야 이동 머신이 같은 판단을 부작용 없이 물어볼 수 있다.
    /// 조건이 한 곳에만 있으므로 두 판단이 어긋날 여지가 없다.
    /// </summary>
    Boss_SkillBase Pick()
    {
        if (Boss == null || _componentStates == null) return null;

        foreach (MonoBehaviour component in _componentStates)
        {
            if (component is not Boss_SkillBase skill) continue;
            if (skill.IsReady(Boss)) return skill;
        }

        return null;
    }

    /// <summary>이동 머신이 스킬로 넘어갈지 판단할 때 묻는다. 고르기와 같은 함수를 쓴다.</summary>
    public bool AnySkillReady() => Pick() != null;

    /// <summary>스킬이 끝났거나 쓸 수 있는 스킬이 없을 때 이동으로 되돌린다.</summary>
    public void Complete()
    {
        if (fsm is Boss_Alert alert) alert.ToMovement();
    }
}
