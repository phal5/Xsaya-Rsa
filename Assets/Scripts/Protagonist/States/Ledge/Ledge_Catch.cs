using UnityEngine;

/// <summary>
/// 무는 순간의 진입 동작. <b>클립이 몸을 옮기고 손은 턱에 남는다.</b>
///
/// 두 구간이 이어질 수 있다. 가장자리에서 내려가는 경우가 그렇다 —
/// 언제나 철봉처럼 매달린 채 시작해서(Drop To Freehang), 발 디딜 곳이 있으면
/// 이어서 자세를 바꾼다(Free Hang To Braced).
///
/// 공중에서 발을 디딘 채 문 경우에는 진입 동작이 없어 이 상태를 아예 거치지 않는다.
/// </summary>
public class Ledge_Catch : LedgeState
{
    bool _converting;

    public override void Enter()
    {
        base.Enter();

        _converting = false;

        BeginAt(Ledge.CatchClip(), Ledge.Anchor.hang);
    }

    public override void FixedUpdateState()
    {
        Follow();
        Transitions();
    }

    public override void Transitions()
    {
        if (!Held) return;

        // 매달린 뒤에야 자세를 바꾼다. 순서를 뒤집으면 아직 손이 닿지도 않았는데 발부터 붙인다.
        if (!_converting && Ledge.BracingPending && Grab.freeToBraced.IsSet)
        {
            _converting = true;

            Ledge.Brace();
            BeginAt(Grab.freeToBraced, Ledge.Anchor.hang);
            return;
        }

        // 전환 클립이 없어도 디딜 곳이 있었다면 자세는 바뀌어야 한다.
        if (Ledge.BracingPending) Ledge.Brace();

        fsm.TransitTo<Ledge_Hang>();
    }
}
