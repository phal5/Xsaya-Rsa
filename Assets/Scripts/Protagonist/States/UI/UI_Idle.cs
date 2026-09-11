using UnityEngine;

/// <summary>
/// 대사를 기다린다. <b>주인공의 그림과 몸을 멈춘다</b> — 시간 배속은 건드리지 않는다. 그러면 세상이 함께 멎는다.
///
/// 되돌리는 것은 Exit이다. 여기서 나가는 길이 무엇이든(경직, 대사 끝, 강제로 닫힘, 사망)
/// Exit은 반드시 불리므로 멈춘 채 남는 길이 없다. 잠금을 거는 쪽(ControlLock)에 두지 않은 이유다 —
/// Release는 전환 중이면 일찍 돌아나가고, 강제로 닫힐 때는 아예 불리지 않는다.
///
/// 몸은 두 몸을 떼어내고 멈춘 순간의 속도를 적어 뒀다가, 나갈 때 다시 싣는다 —
/// 그림과 몸이 같은 순간에 멈추고 같은 순간에 이어진다. 트리거는 그대로 겹치므로 멈춘 동안에도 죽을 수 있다.
/// </summary>
public class UI_Idle : BaseCharacterState
{
    public override void Enter()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
        characterManager.Animation.Freeze(true);
        characterManager.Movement.Freeze(true);
    }

    public override void FixedUpdateState()
    {
        // 대화 중 미끄러지지 않도록 계속 눌러준다.
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void Exit()
    {
        characterManager.Movement.Freeze(false);
        characterManager.Animation.Freeze(false);
    }

    public override void Transitions() { }
}
