using UnityEngine;

/// <summary>
/// 플레이어 스킬 서브머신. Character_Controlled 아래에서 Ground/Airborne과 형제로 놓인다.
///
/// 콤보가 스킬을 요청하면 이 머신이 조작 머신을 자기 쪽으로 끌어온 뒤 해당 스킬 상태로 전이한다.
/// 스킬이 끝나면 접지 여부를 보고 Ground나 Airborne으로 돌려준다.
///
/// ComponentStates: 이 캐릭터가 쓸 Character_SkillBase 컴포넌트들
/// Initial State  : 쓰지 않는다. 요청받은 스킬로 직접 들어간다.
/// </summary>
public class Character_Execution : FiniteStateMachine
{
    [Tooltip("이 서브머신을 담고 있는 조작 머신. 첫 요청 시점에는 아직 Init 전이라 fsm이 비어 있어 직접 참조가 필요하다.")]
    [SerializeField] Character_Controlled _owner;

    Character_SkillBase _pending;
    bool _requested;

    /// <summary>Init 전에는 fsm이 null이므로 직렬화된 소유자로 대신한다.</summary>
    Character_Controlled Owner => (fsm as Character_Controlled) ?? _owner;

    /// <summary>
    /// 스킬이 자기를 실행해 달라고 요청한다.
    /// 콤보 UnityEvent는 입력 콜백에서 불리므로 여기서 바로 전이하지 않고 표시만 남긴다.
    /// 실제 전이는 Character_Controlled.Transitions()가 ConsumeRequest()로 가져가 처리한다.
    /// </summary>
    public void Play(Character_SkillBase skill)
    {
        if (skill == null) return;

        if (Owner == null)
        {
            Debug.LogError($"[{name}] Owner(Character_Controlled)가 연결되어 있지 않아 스킬을 실행할 수 없습니다.", this);
            return;
        }

        _pending = skill;
        _requested = true;
    }

    /// <summary>소유 머신이 갱신 주기에서 요청을 가져간다.</summary>
    public bool ConsumeRequest()
    {
        if (!_requested) return false;

        _requested = false;
        return true;
    }

    public override void Enter()
    {
        if (_pending == null)
        {
            Complete();
            return;
        }

        TransitTo(_pending.GetType());
        _pending = null;
    }

    /// <summary>스킬이 끝났다. 접지 상태에 맞는 이동 상태로 돌려준다.</summary>
    public void Complete()
    {
        Character_Controlled owner = Owner;
        if (owner != null) owner.ToLocomotion();
    }
}
