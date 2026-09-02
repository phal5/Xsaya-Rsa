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

    /// <summary>이 구간에 들어선 자리. 수평을 목표에서 되짚을 때 출발점이 된다.</summary>
    Vector3 _entry;

    /// <summary>걸어둔 클립이 끝났는지.</summary>
    protected bool Held => Time.time - _startedAt >= _holdFor;

    /// <summary>이 구간이 <b>걸어달라고 한</b> 컨트롤러 상태. 애니메이터가 그것을 잡았는지 확인하는 데 쓴다.</summary>
    string _clipState;


    /// <summary>클립을 걸고 그 길이만큼 이 구간에 머문다. 이름이 비어 있으면 걸지 않고 길이만 센다.</summary>
    protected void Begin(LedgeGrab.LedgeClip clip)
    {
        _startedAt = Time.time;
        _holdFor = Grab.Seconds(clip);
        _place = characterManager.Body.position;
        _entry = _place;
        _fitted = false;
        _stretchUp = 1f;
        _clipState = clip.state;

        if (!clip.IsSet) return;

        // 앞부분이 이미 지나간 자세면 거기서부터 튼다.
        if (clip.start > 0f) characterManager.Animation.PlayFrom(clip.state, Grab.Seconds(clip.start));
        else characterManager.Animation.Play(clip.state);
    }

    /// <summary>
    /// 매달릴 자리로 <b>모아 간다.</b> 붙잡는 동안과 매달린 동안이 쓴다.
    ///
    /// 한 번에 옮기지 않는다 — 문 자리는 어디서 뛰었는지에 따라 매번 다르고,
    /// 그 차이를 한 프레임에 지우면 무는 순간 몸이 튄다. 실측으로 0.3~0.84까지 벌어졌다.
    ///
    /// 넘어가지 않으므로 진동하지 않는다. 남은 거리가 0이면 아무 일도 없다.
    /// 쌓인 루트 모션은 버린다 — 여기서 몸을 옮기는 것은 클립이 아니라 이 수렴이다.
    /// </summary>
    protected void Settle()
    {
        characterManager.Animation.ConsumeRootMotion();

        _place = Vector3.MoveTowards(_place, Ledge.Anchor.hang, Grab.settleSpeed * Time.fixedDeltaTime);
        characterManager.Movement.Pin(_place);
    }

    /// <summary>
    /// 위에서 내려와 매달릴 때의 자리. <b>수평을 목표에서 되짚는다.</b>
    ///
    /// <see cref="Settle"/>은 목표를 향해 정해진 속도로 곧장 다가간다. 위에서 내려오는 경우는
    /// 시작 자리가 턱 위이고 매달릴 자리는 벽 바깥이라, 그 등속 이동이 <b>수평과 높이를 따로 끝낸다</b> —
    /// 수평이 먼저 닿으면 남은 높이만 내려가고, 높이가 먼저 닿으면 매달린 채 옆으로 밀려간다.
    ///
    /// 여기서는 수평을 <b>내려온 비율</b>로 정한다. 목표 평면과 들어선 평면을 양 끝으로 잡고
    /// 높이가 간 만큼만 들어가므로, 둘이 언제나 함께 도착한다. 모서리를 돌아 내려오는 모양이 된다.
    ///
    /// 높이는 <see cref="Settle"/>과 같다 — 그쪽은 이미 맞게 돌고 있어 건드릴 이유가 없다.
    /// 옆(턱을 따라가는) 성분도 그대로 둔다. 2D 모드에서 그것이 곧 Z다.
    /// </summary>
    protected void SettleDown()
    {
        characterManager.Animation.ConsumeRootMotion();

        Vector3 target = Ledge.Anchor.hang;

        float y = Mathf.MoveTowards(_place.y, target.y, Grab.settleSpeed * Time.fixedDeltaTime);

        float span = _entry.y - target.y;
        float t = Mathf.Abs(span) < 0.001f ? 1f : Mathf.Clamp01((_entry.y - y) / span);

        Vector3 normal = -(Ledge.Anchor.facing * Vector3.forward);

        float wanted = Mathf.Lerp(Vector3.Dot(_entry, normal), Vector3.Dot(target, normal), t);

        _place.y = y;
        _place += normal * (wanted - Vector3.Dot(_place, normal));

        characterManager.Movement.Pin(_place);
    }

    /// <summary>
    /// 클립이 몸을 옮기는 대로 따라간다. <b>올라서기 하나만</b> 쓴다.
    ///
    /// 루트 모션을 유니티가 스스로 적용하게 두면 Animator가 붙은 자식 오브젝트만 움직여
    /// 메시가 몸에서 떨어져 나간다. 피격 판정은 몸에 있으니 그래서 가로채 여기서 옮겨 싣는다.
    ///
    /// 옆(턱을 따라가는) 성분은 버린다 — 2D 모드에서 그것이 곧 Z이고,
    /// 위치 대입은 FreezePositionZ를 무시하기 때문이다.
    /// </summary>
    /// <summary>
    /// 클립이 몸을 옮기는 대로 따라간다. <b>올라서기 하나만</b> 쓴다.
    ///
    /// 루트 모션을 유니티가 스스로 적용하게 두면 Animator가 붙은 자식 오브젝트만 움직여
    /// 메시가 몸에서 떨어져 나간다. 피격 판정은 몸에 있으니 그래서 가로채 여기서 옮겨 싣는다.
    ///
    /// <b>수평은 손대지 않는다.</b> 클립이 주는 만큼만 앞으로 간다.
    /// 예전에는 도착점의 수평까지 맞추려고 앞으로 가는 몫을 비율로 늘리고, 턱을 넘기 전까지
    /// 미뤘다가 한꺼번에 풀었다. 그렇게 밀어붙인 결과가 절벽을 파고들고 발판 한가운데로
    /// 끌려가는 것이었다 — 어디에 서느냐는 원래 클립과 매달린 자리가 정하는 것이지
    /// 우리가 좌표로 정할 일이 아니다.
    ///
    /// 옆(턱을 따라가는) 성분은 버린다 — 2D 모드에서 그것이 곧 Z이고,
    /// 위치 대입은 FreezePositionZ를 무시하기 때문이다.
    /// </summary>
    protected void Follow(Vector3 destination)
    {
        if (!_fitted) Fit(destination);

        Vector3 delta = characterManager.Animation.ConsumeRootMotion();

        _place += Vector3.up * (delta.y * _stretchUp);

        // <b>수평은 클립에서 받지 않는다.</b> 매달린 평면도 설 평면도 벽면에서 잡히므로
        // 그 사이 거리는 자세당 상수다 — Braced는 wallOffset + standInset = 0.590,
        // Freehang은 freeWallOffset + standInset = 0.352. 클립이 대신 잴 이유가 없다.
        //
        // 예전에는 클립이 주는 앞으로를 턱 넘기 전까지 미뤄 쌓았다가 넘는 스텝에 통째로 풀었다.
        // 총량은 맞았지만 <b>그 한 스텝이 반 미터를 넘는 순간이동</b>이었고, 클립이 주는 양이
        // 매번 달라 서는 깊이도 함께 흔들렸다 — 실측으로 0.17에서 0.59까지 벌어졌다.
        //
        // 파고들지 않는 이유는 그대로다. 턱을 넘기 전에는 진행도가 0이라 매달린 평면에 붙어 있고,
        // 넘어선 뒤 남은 상승에 비례해서만 안으로 들어간다. 벽을 따라 올라가다 허리가 넘고 나서야
        // 넘어오는 실제 동작과 같은 순서다.
        //
        // 더하지 않고 <b>매 스텝 정해 놓는다.</b> 쌓아 가면 그 합이 다시 클립에 딸리고,
        // 오차가 남으면 서는 자리가 또 흔들린다. 옆(턱을 따라가는) 성분은 건드리지 않는다 —
        // 2D 모드에서 그것이 곧 Z이고, 위치 대입은 FreezePositionZ를 무시하기 때문이다.
        Vector3 normal = -(Ledge.Anchor.facing * Vector3.forward);

        float cross = destination.y - characterManager.FootOffset;

        float t = destination.y <= cross
            ? 1f
            : Mathf.Clamp01((_place.y - cross) / (destination.y - cross));

        float wanted = Mathf.Lerp(Vector3.Dot(Ledge.Anchor.hang, normal),
                                  Vector3.Dot(destination, normal), t);

        // <b>모아 가되 한 번에 끌어오지 않는다.</b> 문 자리는 어디서 뛰었는지에 따라 매번 다르고,
        // 오르기 입력을 미리 눌러두면 붙잡기 구간이 통째로 건너뛰어져 Settle()이 그 차이를
        // 좁힐 기회를 못 얻는다 — 실측으로 0.386m가 등반 첫 스텝에 한꺼번에 지워졌다.
        //
        // 속도는 Settle()이 쓰는 것과 같은 값이다. "앵커로 모아 가는 속도"의 주인은 하나여야 한다.
        // 오르는 동안 필요한 몫(약 0.016/스텝)보다 충분히 커서 목표를 따라가는 데는 지장이 없다.
        float current = Vector3.Dot(_place, normal);
        float next = Mathf.MoveTowards(current, wanted, Grab.settleSpeed * Time.fixedDeltaTime);

        _place += normal * (next - current);

        characterManager.Movement.Pin(_place);
    }

    /// <summary>얼마나 늘려 걸지. <b>클립이 주는 총량</b> 대 <b>가야 할 총량</b>의 비다.</summary>
    float _stretchUp = 1f;

    bool _fitted;

    /// <summary>
    /// 클립이 끝나는 높이를 목적지에 맞춘다. <b>목적지로 당기지 않는다</b> — 당기면 경로가 직선이 된다.
    ///
    /// 오르는 도중의 "목적지까지 남은 거리"는 오차가 아니라 아직 가야 할 길이다.
    /// 그것을 오차로 보고 지우면 클립이 그리던 곡선이 통째로 지워지고 몸이 절벽을 뚫고 질러간다.
    /// 어긋난 것은 경로가 아니라 <b>총량</b>이므로, 총량만 비율로 맞추고 모양은 클립에 맡긴다.
    ///
    /// 재는 것은 높이 하나다. 발이 윗면에 정확히 놓이는 것은 지켜야 하지만, 벽에서 얼마나
    /// 안쪽에 서느냐는 지켜야 할 약속이 아니다 — 실측으로 Braced 클립은 앞으로 0.681을
    /// 주는데 매달린 자리가 벽면 바깥 0.19이니 안쪽 0.49에 선다. 그걸로 충분하다.
    ///
    /// 클립을 물을 수 없으면 늘리지 않는다. 섞이기 시작한 직후라 아직 대답이 없는 것이므로
    /// 다음 프레임에 다시 묻는다.
    /// </summary>
    void Fit(Vector3 destination)
    {
        // <b>애니메이터가 우리가 건 클립을 실제로 잡았는지 먼저 본다.</b>
        //
        // Begin()의 Play는 <b>부탁</b>이고, 애니메이터는 렌더 프레임당 한 번만 돈다.
        // 프레임이 낮으면 한 프레임에 물리 스텝이 여럿 도는데, 그 사이 애니메이터는 한 번도 돌지 않아
        // 아직 <b>직전 클립에 서 있다</b> — 전환도 시작 전이라 IsInTransition조차 거짓이다.
        // 그때 travel을 물으면 직전 클립의 값을 <b>자신 있게</b> 돌려준다.
        //
        // 붙잡기 클립들은 travel.y가 모두 음수다(-0.36 ~ -1.91). 올라서기 클립은 양수다(+1.26, +1.92).
        // 그 음수로 배율을 내면 부호가 뒤집혀, 올라서기가 그대로 <b>내려가기</b>가 된다.
        // 두 몸은 그 동안 키네마틱이고 Pin은 위치를 직접 쓰므로 바닥도 막지 못한다 —
        // 실측으로 붙잡은 자리에서 6.78m를 뚫고 내려갔다. 두 번 다 같은 거리였다.
        //
        // 아래 TryClipTravel의 "대답이 없으면 다음 프레임에 다시 묻는다"는 <b>틀린 대답</b>까지는
        // 거르지 못한다. 그래서 대답을 받기 전에, 답할 자격이 있는지를 여기서 묻는다.
        if (string.IsNullOrEmpty(_clipState)) return;
        if (!characterManager.Animation.IsPlaying(_clipState, out _)) return;

        if (!characterManager.Animation.TryClipTravel(out Vector3 travel)) return;

        _fitted = true;

        // 주는 것이 없으면 늘릴 것도 없다. 0으로 나누지 않으려는 것이 아니라, 늘려봐야 0이라서다.
        _stretchUp = Mathf.Abs(travel.y) > 0.001f ? (destination.y - _place.y) / travel.y : 1f;
    }
}
