using UnityEngine;

/// <summary>
/// 스킬 한 개를 담당하는 슬롯. 역할(속공/강공/특수기)과 타이밍·위력은 슬롯이 갖고,
/// 실제로 무엇을 하는지는 안에 꽂힌 상태가 정한다.
///
/// Initial State에 다른 Boss_SkillBase를 꽂으면 그 역할의 스킬이 통째로 바뀐다.
/// 선택기는 슬롯 타입만 알면 되므로 스킬 구현이 바뀌어도 건드릴 필요가 없다.
///
/// Component States에 이 슬롯이 수행할 Boss_SkillBase 컴포넌트 하나를 넣는다.
/// 다른 스킬 컴포넌트로 교체하면 그 역할의 스킬이 통째로 바뀐다.
/// </summary>
public abstract class Boss_SkillSlot : FiniteStateMachine
{
    [Space(10f)]
    [Header("Skill Profile - 이 슬롯의 구간 길이와 위력")]
    [SerializeField] BossSkillProfile _profile = new BossSkillProfile();

    public BossSkillProfile Profile => _profile;

    Boss_SkillBase _skill;

    /// <summary>이 슬롯에 등록된 스킬 컴포넌트.</summary>
    public Boss_SkillBase Skill
    {
        get
        {
            if (_skill == null && _componentStates != null)
            {
                foreach (MonoBehaviour component in _componentStates)
                {
                    if (component is Boss_SkillBase skill) { _skill = skill; break; }
                }
            }
            return _skill;
        }
    }

    public override void Enter()
    {
        // 슬롯은 진입할 때마다 담당 스킬을 처음부터 다시 돌린다.
        if (Skill == null)
        {
            Debug.LogError($"[{name}] 슬롯에 스킬 컴포넌트가 없습니다. Component States를 확인하세요.", this);
            Complete();
            return;
        }

        TransitTo(Skill.GetType());
    }

    /// <summary>담당 스킬이 끝났거나 꽂힌 상태가 없을 때 선택기로 돌려보낸다.</summary>
    public void Complete()
    {
        if (fsm is Boss_SkillMachine machine) machine.Complete();
    }
}
