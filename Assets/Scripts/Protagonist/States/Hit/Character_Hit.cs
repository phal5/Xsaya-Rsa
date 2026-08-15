using UnityEngine;

/// <summary>
/// 피격 서브머신. 진입 판정("피해를 입음")은 CharacterRoot가 하고,
/// 여기서는 경직이냐 다운이냐만 고른다.
/// 보스의 Boss_DamageMachine과 같은 역할 분담이다.
///
/// Initial State는 쓰지 않는다. 진입 시 Enter가 직접 고른다.
/// </summary>
public class Character_Hit : FiniteStateMachine
{
    CharacterManager Character => manager as CharacterManager;

    public CharacterRoot Root => fsm as CharacterRoot;

    public override void Enter()
    {
        CharacterManager character = Character;

        if (character == null || character.Damagable == null)
        {
            Complete();
            return;
        }

        if (character.Damagable.CurrentHealth <= 0f) TransitTo<Character_Down>();
        else TransitTo<Character_Stun>();
    }

    /// <summary>경직이나 부활이 끝났다. 조작으로 되돌린다.</summary>
    public void Complete()
    {
        CharacterRoot root = Root;
        if (root != null) root.ToControl();
    }
}
