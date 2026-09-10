using UnityEngine;

/// <summary>
/// UI가 조작을 가져간 상태. 메뉴·대화가 열려 있는 동안 캐릭터는 아무것도 하지 않는다.
///
/// 스스로 빠져나오지 않는다. 열었던 쪽이 CharacterRoot.ToControl()을 불러야 끝난다.
/// FlipBook을 쓰는 경우 onSetBook -> ToUI(), onDialogueNull -> ToControl()로 물리면 된다.
///
/// <b>주인공의 그림을 멈춘다.</b> 시간 배속은 건드리지 않는다 — 그러면 세상이 함께 멎는다.
/// 되돌리는 것은 Exit이다. 이 상태를 떠나는 길이 무엇이든(대사 끝, 강제로 닫힘, 씬 전환의 끝)
/// Exit은 반드시 불리므로 멈춘 채 남는 길이 없다. 잠금을 거는 쪽(ControlLock)에 두지 않은 이유다 —
/// Release는 전환 중이면 일찍 돌아나가고, 강제로 닫힐 때는 아예 불리지 않는다.
///
/// 씬 전환(SceneDirector)도 이 상태를 거친다. 그동안에도 그림이 멈추므로, 커튼이 덮이는 동안 멈춘 자세로 보인다.
/// </summary>
public class Character_UI : BaseCharacterState
{
    public override void Enter()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
        characterManager.Animation.Freeze(true);
    }

    public override void FixedUpdateState()
    {
        // 대화 중 미끄러지지 않도록 계속 눌러준다.
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void Exit()
    {
        characterManager.Animation.Freeze(false);
    }

    public override void Transitions() { }
}
