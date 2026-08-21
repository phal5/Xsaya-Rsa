using UnityEngine;

/// <summary>
/// 매달려 기다린다.
///
/// 중력을 끄고 속도를 턴 것은 축(<see cref="Character_Ledge"/>)이 이미 했고,
/// 오르기·놓기·벽점프도 방향이든 버튼이든 전부 축이 받는다.
/// 여기 남은 것은 대기 자세를 걸고 자리를 지키는 일뿐이다.
/// </summary>
public class Ledge_Hang : LedgeState
{
    public override void Enter()
    {
        base.Enter();

        Begin(Grab.Hang(Ledge.Braced));
    }

    public override void FixedUpdateState()
    {
        // 손이 턱에 박혀 있고 몸이 흔들리는 클립이다. 몸을 클립에 맡겨야 그 관계가 지켜진다 —
        // 붙들어 두면 흔들림이 갈 곳이 손밖에 없어 손이 턱을 떠난다.
        // 대기 클립에는 루트 모션이 없다. 실어봐야 앞 클립이 섞이며 남긴 몫이 몸을 밀어낼 뿐이다.
        Settle();
    }
}
