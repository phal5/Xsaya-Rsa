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

        Begin(Ledge.CatchClip());
    }

    public override void FixedUpdateState()
    {
        // 문 자리에서 매달릴 자리로 모아 간다. 한 번에 옮기면 그만큼 튄다.
        Settle();
        Transitions();
    }

    /// <summary>손이 모서리에 이만큼 다가오면 놓인 것으로 본다.</summary>
    const float GripReach = 0.12f;

    /// <summary>
    /// 손이 턱에 놓였는지. <b>잡았다는 것은 손이 닿았다는 뜻이지 클립이 끝났다는 뜻이 아니다.</b>
    ///
    /// 클립은 손이 놓인 뒤에도 몸이 내려앉는 동안 계속 돈다 — 내려가기 클립은 그 몫이 절반이다.
    /// 그때까지 조작을 막아두면 잡고도 한참을 못 움직인다.
    /// </summary>
    bool Gripped()
    {
        if (!characterManager.Animation.TryHandCenter(out Vector3 hand)) return false;

        Vector3 grip = Grab.GripOf(Ledge.Anchor, characterManager.FootOffset);

        return (hand - grip).sqrMagnitude <= GripReach * GripReach;
    }

    public override void Transitions()
    {
        // 클립이 끝나는 것은 손이 못 닿았을 때의 최후 보루다.
        if (!Held && !Gripped()) return;

        // 매달린 뒤에야 자세를 바꾼다. 순서를 뒤집으면 아직 손이 닿지도 않았는데 발부터 붙인다.
        if (!_converting && Ledge.BracingPending && Grab.freeToBraced.IsSet)
        {
            _converting = true;

            Ledge.Brace();
            Begin(Grab.freeToBraced);
            return;
        }

        // 전환 클립이 없어도 디딜 곳이 있었다면 자세는 바뀌어야 한다.
        if (Ledge.BracingPending) Ledge.Brace();

        fsm.TransitTo<Ledge_Hang>();
    }
}
