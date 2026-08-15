using UnityEngine;

/// <summary>
/// 플레이어의 최상위 상태기계. 피격은 어느 상태에서든 끊고 들어와야 하므로
/// 구독을 여기 한 곳에 두고, 하위는 각자의 관심사에만 집중한다.
/// 보스의 BossRoot와 같은 역할이다.
///
/// Initial State  : Character_Controlled
/// ComponentStates: Character_Controlled, Character_Hit
/// </summary>
public class CharacterRoot : FiniteStateMachine
{
    CharacterManager _character;

    CharacterManager Character
    {
        get
        {
            if (_character == null) _character = manager as CharacterManager;
            return _character;
        }
    }

    public override void Bootstrap()
    {
        if (Character == null)
        {
            Debug.LogError($"[{name}] manager에 CharacterManager가 연결되어 있지 않습니다.", this);
            return;
        }

        if (Character.Damagable == null)
        {
            Debug.LogError($"[{name}] CharacterManager.Damagable이 비어 있어 피격을 감지할 수 없습니다.", this);
            return;
        }

        Character.Damagable.AddDamageListener(OnDamaged);
    }

    // 피격은 OnTriggerEnter에서 올라오므로 갱신 주기 밖이다.
    // 콜백은 표시만 남기고 전이는 Transitions()에서 한다.
    bool _hitRequested;

    void OnDamaged(float _)
    {
        _hitRequested = true;
    }

    public override void Transitions()
    {
        if (!_hitRequested) return;
        _hitRequested = false;

        // 이미 피격 계열 안이면 겹쳐 들어가지 않는다.
        // 사망 판정은 Character_Hit이 진입 시점에 다시 본다.
        if (_currentStateType == typeof(Character_Hit)) return;

        TransitTo<Character_Hit>();
    }

    public void ToControl()
    {
        if (_currentStateType == typeof(Character_Controlled)) return;
        TransitTo<Character_Controlled>();
    }

    public void ToHit() { TransitTo<Character_Hit>(); }

    /// <summary>메뉴·대화가 열릴 때 부른다. 닫을 때 ToControl()을 불러야 풀린다.</summary>
    public void ToUI()
    {
        if (_currentStateType == typeof(Character_UI)) return;
        TransitTo<Character_UI>();
    }
}
