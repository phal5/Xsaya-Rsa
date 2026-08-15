using UnityEngine;

/// <summary>
/// 전투 상태. 이동 / 스킬 / 반응 세 서브머신을 갈아끼운다.
///
/// 회피와 방어는 진입 조건이 "플레이어 공격 감지"로 같으므로 판정을 여기서 한 번만 하고,
/// 둘 중 무엇을 할지는 Boss_ReactionMachine이 정한다.
///
/// Initial State  : Boss_MoveMachine
/// ComponentStates: Boss_MoveMachine, Boss_SkillMachine, Boss_ReactionMachine
/// </summary>
public class Boss_Alert : FiniteStateMachine
{
    BossManager _boss;
    MeleeWeapon _subscribedWeapon;

    BossManager Boss
    {
        get
        {
            if (_boss == null) _boss = manager as BossManager;
            return _boss;
        }
    }

    public override void Bootstrap()
    {
        if (Boss == null)
            Debug.LogError($"[{name}] Boss_Alert의 manager에 BossManager가 연결되어 있지 않습니다.", this);
    }

    public override void Enter()
    {
        Subscribe();
        base.Enter();
    }

    public override void Exit()
    {
        Unsubscribe();
        base.Exit();
    }

    #region Player Attack Detection

    void Subscribe()
    {
        if (_subscribedWeapon != null) return;

        MeleeWeapon weapon = PlayerManager.instance != null ? PlayerManager.instance.playerWeapon : null;
        if (weapon == null)
        {
            Debug.LogWarning($"[{name}] PlayerManager.playerWeapon이 비어 있어 회피/방어가 발동하지 않습니다.", this);
            return;
        }

        weapon.onAttackStart += OnPlayerAttack;
        _subscribedWeapon = weapon;
    }

    void Unsubscribe()
    {
        if (_subscribedWeapon == null) return;

        _subscribedWeapon.onAttackStart -= OnPlayerAttack;
        _subscribedWeapon = null;
    }

    // 플레이어 무기 이벤트는 플레이어 쪽 갱신 중에 올라온다.
    // 콜백은 표시만 남기고 전이는 Transitions()에서 한다.
    bool _reactionRequested;

    void OnPlayerAttack()
    {
        _reactionRequested = true;
    }

    void ConsumeReaction()
    {
        if (!_reactionRequested) return;
        _reactionRequested = false;

        if (Boss == null) return;

        // 이미 반응 중이면 겹쳐 들어가지 않는다.
        if (_currentStateType == typeof(Boss_ReactionMachine)) return;

        if (!Boss.ReactionReady) return;
        if (Boss.DistanceToPlayer() > Boss.reactionRadius) return;
        if (Random.value > Boss.reactionChance) return;

        Boss.StartReactionCooldown();
        TransitTo<Boss_ReactionMachine>();
    }

    #endregion

    #region Leash - 이탈

    public override void Transitions()
    {
        ConsumeReaction();
        ToIdle();
    }

    void ToIdle()
    {
        // 아레나 보스는 useLeash를 꺼둔다. 한 번 깨어나면 다시 잠들지 않는다.
        if (Boss == null || !Boss.useLeash) return;
        if (!Boss.HasPlayer) return;
        if (Boss.DistanceToPlayer() <= Boss.LeashRadius) return;

        if (fsm is BossRoot root) root.ToIdle();
    }

    #endregion

    #region Sub-machine Handoff

    public void ToMovement() { TransitTo<Boss_MoveMachine>(); }

    public void ToSkill() { TransitTo<Boss_SkillMachine>(); }

    /// <summary>이동 머신이 스킬로 넘어갈지 물어볼 때 스킬 머신에 위임한다.</summary>
    public bool AnySkillReady()
    {
        Boss_SkillMachine machine = SkillMachine;
        return machine != null && machine.AnySlotReady();
    }

    Boss_SkillMachine _skillMachine;

    Boss_SkillMachine SkillMachine
    {
        get
        {
            if (_skillMachine == null && _componentStates != null)
            {
                foreach (MonoBehaviour component in _componentStates)
                {
                    if (component is Boss_SkillMachine machine) { _skillMachine = machine; break; }
                }
            }
            return _skillMachine;
        }
    }

    #endregion
}
