using UnityEngine;

/// <summary>
/// 플레이어의 최상위 상태기계. 피격은 어느 상태에서든 끊고 들어와야 하므로
/// 구독을 여기 한 곳에 두고, 하위는 각자의 관심사에만 집중한다.
/// 보스의 BossRoot와 같은 역할이다.
///
/// 사망(Character_Down)도 여기 직속이다. 피격에서 이어지지만 피격에 속하지는 않는다 —
/// 때린 주체가 없는 죽음(낙사·함정)도 같은 자리로 들어와야 하기 때문이다.
///
/// Initial State  : Character_Controlled
/// ComponentStates: Character_Controlled, Character_Hit, Character_UI
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
        // 사망은 부르는 것이 아니라 <b>조건</b>이다. 매 갱신마다 이 머신이 직접 확인한다.
        //
        // 바깥에 "죽여라"는 문을 내면 그 문을 지나지 않는 죽음(낙사·함정)마다 문을 하나씩 더 내야 하고,
        // 반대로 죽은 뒤에도 그 문들이 열려 있어 아무나 사망을 취소할 수 있게 된다.
        // 조건으로 두면 문이 없어지고, 누가 무엇을 하든 다음 갱신에 규칙이 스스로 회복된다.
        if (Dead)
        {
            // 죽인 그 일격이 세운 표시는 여기서 버린다.
            // 남겨두면 사망 구간을 건너뛰고 살아남았다가, 부활해 조건을 빠져나가는 순간
            // 경직으로 터진다 — 일어나자마자 한 번 더 맞은 것처럼 보인다.
            _hitRequested = false;

            if (_currentStateType != typeof(Character_Down)) TransitTo<Character_Down>();
            return;
        }

        if (!_hitRequested) return;
        _hitRequested = false;

        // 대사 중이면 그 안에서 경직한다. 끝나면 대사로 돌아가야 하므로 조작 머신으로 끌어내지 않는다 —
        // 예전에는 여기서 Character_Hit으로 보내, 경직이 끝나는 순간 대사가 떠 있는 채로 조작이 돌아왔다.
        if (_state is Character_UI ui)
        {
            ui.Stagger();
            return;
        }

        // 이미 피격 계열 안이면 겹쳐 들어가지 않는다.
        if (_currentStateType == typeof(Character_Hit)) return;

        TransitTo<Character_Hit>();
    }

    /// <summary>
    /// 죽었는지. 체력 하나만 본다 — 무엇이 깎았는지는 묻지 않는다.
    /// 낙사든 함정이든 체력을 0으로 만들면 그것이 곧 사망이다.
    /// </summary>
    bool Dead => Character != null && Character.Damagable != null && Character.Damagable.CurrentHealth <= 0f;

    /// <summary>
    /// 하위가 제 할 일을 마치고 조작을 돌려줄 때 쓴다.
    /// 바깥에서 부르는 문이 아니라 자식이 부모에게 넘기는 통로다 — 보스의 Complete()들과 같은 자리다.
    /// </summary>
    public void ToControl()
    {
        if (_currentStateType == typeof(Character_Controlled)) return;
        TransitTo<Character_Controlled>();
    }

    /// <summary>
    /// 누운 자리에서 시작한다. 씬에 처음 설 때와 부활할 때 이리로 온다.
    /// 조작은 <see cref="Character_Rest"/>가 일어선 뒤 스스로 돌려준다.
    /// </summary>
    public void ToRest()
    {
        if (_currentStateType == typeof(Character_Rest)) return;
        TransitTo<Character_Rest>();
    }

    /// <summary>
    /// 가사 상태로 쓰러뜨린다. 연출이 <see cref="CharacterCue"/>를 거쳐 부른다.
    ///
    /// <b>죽음과 갈리는 지점은 체력이다.</b> 이쪽은 체력을 건드리지 않으므로 아래 Dead 조건에
    /// 걸리지 않고, 그래서 부활 시계도 돌지 않는다. 실제로 죽여야 하면 체력을 비우면 된다 -
    /// 그러면 다음 갱신에 이 머신이 스스로 Character_Down으로 끌고 간다.
    /// </summary>
    public void ToFaint()
    {
        if (_currentStateType == typeof(Character_Faint)) return;
        TransitTo<Character_Faint>();
    }

    /// <summary>메뉴·대화가 열릴 때 부른다. 닫을 때 ToControl()을 불러야 풀린다.</summary>
    public void ToUI()
    {
        if (_currentStateType == typeof(Character_UI)) return;

        // 씬 전환 중에는 대사가 조작을 가져가지 않는다. 전환이 끝나며 스스로 정할 최종 상태에 맡긴다 —
        // 여기서 넘기면 전환 상태가 끊겨, 무적과 멈춤이 전환 도중에 풀린다.
        if (_currentStateType == typeof(Character_Transit)) return;

        TransitTo<Character_UI>();
    }

    /// <summary>
    /// 씬 전환에 들어간다. <see cref="SceneDirector"/>가 부른다. 이 동안에는 맞지도 죽지도 않는다.
    ///
    /// 들어가기 직전에 세워진 피격 표시는 버린다. 전환 중에는 새로 세워질 일이 없지만,
    /// 남아 있던 것이 전환이 끝난 뒤 엉뚱한 때 경직으로 터진다.
    /// </summary>
    public void ToTransit()
    {
        _hitRequested = false;

        if (_currentStateType == typeof(Character_Transit)) return;
        TransitTo<Character_Transit>();
    }
}
