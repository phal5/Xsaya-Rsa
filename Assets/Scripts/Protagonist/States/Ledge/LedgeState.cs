using UnityEngine;

/// <summary>
/// 턱 축의 상태들이 공유하는 뼈대.
///
/// 각 상태가 하는 일은 "클립 하나를 걸고, 그 길이만큼 머물고, 끝나면 넘긴다"뿐이다.
/// 그 셋을 여기 모아두면 각 상태에는 <b>어떤 클립을 고르고 어디로 넘기는지</b>만 남는다.
///
/// 자세(Braced/Freehang)로 갈리는 것도 상태가 아니라 클립 선택이다.
/// 상태를 둘로 쪼갰다면 올라가기·놓기·벽점프가 저마다 두 벌이 되어 전이가 여덟 갈래로 늘었다.
///
/// <b>매달린 동안 위치를 건드리지 않는다.</b> 자리는 무는 순간 축이 한 번 정하고,
/// 중력이 꺼져 있으니 그대로 있는다. 매 프레임 다시 놓으면 물리가 밀어낸 것을 도로 끌어와
/// 눈에 보이는 떨림이 된다 — 자세를 만드는 것은 클립의 일이지 우리 일이 아니다.
/// </summary>
public abstract class LedgeState : BaseCharacterState
{
    protected Character_Ledge Ledge => fsm as Character_Ledge;

    protected LedgeGrab Grab => characterManager.Ledge;

    float _startedAt;
    float _holdFor;

    /// <summary>
    /// 클립이 옮겨 놓을 자리. <b>몸에서 다시 읽지 않는다.</b>
    ///
    /// 오르는 동안은 몸이 아직 턱보다 아래라, 앞으로 가는 것이 곧 절벽 면을 파고드는 것이다.
    /// 물리는 매 프레임 그것을 밀어내는데 몸에서 읽어 더하면 밀려난 자리에서 다시 시작해
    /// 앞으로 가는 몫이 그만큼 지워진다 — 실측으로 클립이 준 0.643 중 0.137만 남았다.
    /// 우리가 셈한 자리를 우리가 들고 있으면 그 창이 없다.
    /// </summary>
    Vector3 _place;

    /// <summary>걸어둔 클립이 끝났는지.</summary>
    protected bool Held => Time.time - _startedAt >= _holdFor;

    /// <summary>클립을 걸고 그 길이만큼 이 구간에 머문다. 이름이 비어 있으면 걸지 않고 길이만 센다.</summary>
    protected void Begin(LedgeGrab.LedgeClip clip)
    {
        _startedAt = Time.time;
        _holdFor = Grab.Seconds(clip);
        _place = characterManager.Rigidbody.position;

        if (!clip.IsSet) return;

        // 앞부분이 이미 지나간 자세면 거기서부터 튼다.
        if (clip.start > 0f) characterManager.Animation.PlayFrom(clip.state, Grab.Seconds(clip.start));
        else characterManager.Animation.Play(clip.state);
    }

    /// <summary>
    /// 클립이 <paramref name="target"/>에서 끝나도록 시작 자리를 <b>뒤로 물린 뒤</b> 클립을 건다.
    ///
    /// 붙잡는 동작은 손이 턱에 놓인 채 몸이 내려앉는 것이다. 몸을 매달릴 자리에 붙들어 두면
    /// 그 내려앉음이 갈 곳이 팔밖에 없어, 몸은 멎어 있고 손만 올라와 자리를 잡는 그림이 된다.
    /// 클립이 옮기는 양만큼 물려두면 몸이 내려앉고 손은 처음부터 턱에 놓인다.
    /// </summary>
    protected void BeginAt(LedgeGrab.LedgeClip clip, Vector3 target)
    {
        Begin(clip);

        Vector3 wall = Ledge.Anchor.facing * Vector3.forward;
        _place = target - (wall * clip.travel.x + Vector3.up * clip.travel.y);

        characterManager.Movement.Pin(_place);
    }

    /// <summary>
    /// 클립이 몸을 옮기는 대로 따라간다. <b>올라서기 하나만</b> 쓴다.
    ///
    /// 루트 모션을 유니티가 스스로 적용하게 두면 Animator가 붙은 자식 오브젝트만 움직여
    /// 메시가 몸에서 떨어져 나간다. 피격 판정은 몸에 있으니 그래서 가로채 여기서 옮겨 싣는다.
    ///
    /// 가로(턱을 따라가는) 성분은 버린다 — 2D 모드에서 그것이 곧 Z이고,
    /// 위치 대입은 FreezePositionZ를 무시하기 때문이다.
    /// </summary>
    protected void Follow()
    {
        Vector3 delta = characterManager.Animation.ConsumeRootMotion();
        Vector3 wall = Ledge.Anchor.facing * Vector3.forward;
        Vector3 planar = Vector3.Dot(delta, wall) * wall + Vector3.up * delta.y;

        _place += planar;

        characterManager.Movement.Pin(_place);
    }
}
