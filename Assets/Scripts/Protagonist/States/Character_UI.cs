using UnityEngine;

/// <summary>
/// UI가 조작을 가져간 상태. 메뉴·대화가 열려 있는 동안 캐릭터는 아무것도 하지 않는다.
///
/// 스스로 빠져나오지 않는다. 열었던 쪽이 CharacterRoot.ToControl()을 불러야 끝난다.
/// FlipBook을 쓰는 경우 onSetBook -> ToUI(), onDialogueNull -> ToControl()로 물리면 된다.
///
/// <b>머신이다.</b> 대사 중에도 맞을 수 있어야 해서다(보스 연출). 맞으면 이 안에서 경직하고,
/// 끝나면 대사를 기다리던 자리로 돌아온다 — 조작 머신으로 끌려 나가지 않는다.
/// 맞았는지는 루트가 판정하고, 이 머신에는 <see cref="Stagger"/>로 알리기만 한다.
///
///   UI_Idle          대사를 기다린다. 그림과 몸을 멈춘다.
///   Character_Stun   경직. Character_Hit 아래의 것과 같은 상태다.
///
/// 멈춤은 이 머신이 아니라 UI_Idle의 것이다. 경직은 멈추지 않은 채 돌아야 하고(넉백이 흘러야 한다),
/// 대기로 돌아오면 그 자리에서 다시 멈춘다.
///
/// <b>들어올 때는 늘 대기에서 시작한다.</b> 부모가 돌아오면 하위를 재개하는 규칙대로 두면,
/// 경직 도중 떠났다가 다음 대사에서 그 경직이 되살아난다.
///
/// 씬 전환은 여기를 거치지 않는다 — <see cref="Character_Transit"/>이 따로 맡는다.
/// 대사 중에는 맞고 죽을 수 있어야 하지만 전환 중에는 아니기 때문이다.
///
/// Initial State는 쓰지 않는다. Enter가 직접 고른다.
/// </summary>
public class Character_UI : FiniteStateMachine, IStunOwner
{
    public override void Enter()
    {
        TransitTo<UI_Idle>();
    }

    /// <summary>대사 중에 맞았다. 루트가 부른다. 이미 경직 중이면 겹쳐 들어가지 않는다.</summary>
    public void Stagger()
    {
        if (_currentStateType == typeof(Character_Stun)) return;

        TransitTo<Character_Stun>();
    }

    /// <summary>경직이 끝났다. 대사를 기다리던 자리로 돌아간다.</summary>
    public void Complete()
    {
        TransitTo<UI_Idle>();
    }
}
