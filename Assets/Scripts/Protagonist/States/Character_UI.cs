using UnityEngine;

/// <summary>
/// UI가 조작을 가져간 상태. 메뉴·대화가 열려 있는 동안 캐릭터는 아무것도 하지 않는다.
///
/// 스스로 빠져나오지 않는다. 열었던 쪽이 CharacterRoot.ToControl()을 불러야 끝난다.
/// FlipBook을 쓰는 경우 onSetBook -> ToUI(), onDialogueNull -> ToControl()로 물리면 된다.
/// </summary>
public class Character_UI : BaseCharacterState
{
    public override void Enter()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void FixedUpdateState()
    {
        // 대화 중 미끄러지지 않도록 계속 눌러준다.
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void Transitions() { }
}
