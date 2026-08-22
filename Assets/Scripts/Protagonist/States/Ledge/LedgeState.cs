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
        _place = characterManager.Body.position;
        _fitted = false;
        _stretchUp = 1f;
        _withheld = 0f;

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
        Vector3 wall = Ledge.Anchor.facing * Vector3.forward;

        if (!_fitted) Fit(destination);

        Vector3 delta = characterManager.Animation.ConsumeRootMotion();

        _place += Vector3.up * (delta.y * _stretchUp);

        // 턱을 넘기 전에는 앞으로 가지 않는다.
        //
        // 오르는 동안 두 몸 다 물리에서 떼어 두므로, 파고들어도 밀어내 줄 것이 없다.
        // 그런데 몸이 아직 턱보다 아래인 동안 앞으로 가는 것은 곧 절벽 면을 파고드는 것이다.
        // 실제로 오르는 동작도 그렇지 않다 — 벽을 따라 올라가다 허리가 턱을 넘고 나서야 넘어온다.
        //
        // <b>총량은 여전히 클립이 정한다.</b> 미룬 몫은 버리지 않고 넘어선 뒤에 함께 실리므로,
        // 어디에 서는지는 그대로고 가는 순서만 바뀐다.
        float forward = Vector3.Dot(delta, wall);

        if (_place.y < destination.y - characterManager.FootOffset)
        {
            _withheld += forward;
        }
        else
        {
            _place += wall * (forward + _withheld);
            _withheld = 0f;
        }

        characterManager.Movement.Pin(_place);
    }

    /// <summary>턱을 넘기 전까지 미뤄둔 앞으로 가는 몫.</summary>
    float _withheld;

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
        if (!characterManager.Animation.TryClipTravel(out Vector3 travel)) return;

        _fitted = true;

        // 주는 것이 없으면 늘릴 것도 없다. 0으로 나누지 않으려는 것이 아니라, 늘려봐야 0이라서다.
        _stretchUp = Mathf.Abs(travel.y) > 0.001f ? (destination.y - _place.y) / travel.y : 1f;

        // 수평은 <b>물는 순간 기록해 둔 턱</b>에서 시작한다.
        //
        // 클립이 앞으로 주는 양은 상수다. 그러니 서는 깊이는 오직 어디서 시작했느냐가 정하는데,
        // 몸이 있던 자리에서 시작하면 그 자리가 매번 다르다 — 모아 가기가 끝났으면 기록된 자리이고
        // 진입 중에 눌렀으면 아직 문 자리다. 같은 조작이 어떤 때는 깊이 들어가고 어떤 때는
        // 가장자리에 걸치던 것이 이것이다.
        //
        // 높이와 옆은 건드리지 않는다. 높이는 늘리기가 맞추고, 옆은 2D 평면이라 그대로 두어야 한다.
        Vector3 normal = -(Ledge.Anchor.facing * Vector3.forward);

        _place += normal * Vector3.Dot(Ledge.Anchor.hang - _place, normal);
    }
}
