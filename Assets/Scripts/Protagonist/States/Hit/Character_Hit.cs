using UnityEngine;

/// <summary>
/// 피격 서브머신. 진입 판정("피해를 입음")은 CharacterRoot가 하고, 여기서는 경직만 맡는다.
/// 보스의 Boss_DamageMachine과 같은 역할 분담이다.
///
/// 사망은 여기 소속도, 여기 판단도 아니다. CharacterRoot가 체력을 조건으로 직접 본다.
/// 그래서 이 상태에 들어왔다는 것은 <b>아직 살아 있다</b>는 뜻이다.
///
/// Initial State는 쓰지 않는다. 진입 시 Enter가 직접 고른다.
/// </summary>
public class Character_Hit : FiniteStateMachine, IStunOwner
{
    CharacterManager Character => manager as CharacterManager;

    public CharacterRoot Root => fsm as CharacterRoot;

    public override void Enter()
    {
        if (Character == null || Character.Damagable == null)
        {
            Complete();
            return;
        }

        // 사망 판정을 여기서 하지 않는다. 체력이 0이면 CharacterRoot가 이 상태에 들어오기 전에
        // 이미 사망으로 보내므로, 여기까지 온 것은 아직 살아 있다는 뜻이다.
        TransitTo<Character_Stun>();
    }

    /// <summary>경직이나 부활이 끝났다. 조작으로 되돌린다.</summary>
    public void Complete()
    {
        CharacterRoot root = Root;
        if (root != null) root.ToControl();
    }
}
