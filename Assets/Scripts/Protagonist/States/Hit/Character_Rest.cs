using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 누운 자리에서 기다렸다 일어난다. <b>씬에 처음 설 때와 부활할 때</b> 이 상태로 들어온다.
///
/// CharacterRoot 직속이다. 조작을 가진 상태가 아니라 조작을 <b>돌려주기 전</b>의 상태라,
/// 사망(Character_Down)이나 UI(Character_UI)와 같은 자리에 있는 것이 맞다.
///
/// 세상은 멎어 있고 주인공만 움직인다. 시간을 눌러두는 것은 TimeManager의 상한이고,
/// 애니메이터는 그동안 벽시계로 돈다 — 배속에 끌려가게 두면 배속이 0인 동안 기상 동작도
/// 함께 멎어, 키를 눌러도 아무 일이 일어나지 않는다.
///
/// 네 구간이다.
/// <list type="number">
/// <item>대기 — 그림이 몸에서 RiseOffset만큼 떨어지고 RiseTwist만큼 비틀린 채 시간이 멎어 있다.</item>
/// <item>기상 — 클립이 돌고 시간 배속이 제자리로 차오른다. <b>비틀림은 클립의 마지막 구간에서 풀린다.</b></item>
/// <item>진입 — 선 자세로 섞이며 그림이 몸에 붙는다. 각도는 이미 0이라 위치만 남는다.</item>
/// </list>
///
/// <b>몸의 자리는 처음부터 끝까지 그대로다.</b> 옮기는 것은 그림뿐이다 — 몸이 놓인 자리는
/// 스테이지가 정한 것이고, 그것을 건드리면 Protagonist 대비 오프셋이 뭉개진다.
/// <b>몸의 각도도 건드리지 않는다.</b> 일어설 방향은 SceneDirector가 정한다 —
/// 부활은 체크포인트가 적어둔 값이고 첫 등장은 초기 지점의 값이다.
/// 여기서 다시 돌려세우면 그 둘을 덮어써, 체크포인트마다 방향을 정해둔 것이 무의미해진다.
/// </summary>
public class Character_Rest : BaseCharacterState
{
    bool _rising;
    bool _subscribed;

    /// <summary>일어나기 시작한 시각. <b>벽시계</b>다 — 재는 동안 배속이 0에서 1로 변한다.</summary>
    float _startedAt;

    /// <summary>메시가 원래 놓이는 자리와 각도. 규약값이고 건드리지 않는다.</summary>
    Vector3 _base;
    Quaternion _baseTurn;

    /// <summary>지금 메시에 얹혀 있는 추가 오프셋. 일어서며 위치도 각도도 0이 된다.</summary>
    Vector3 _offset;
    Quaternion _twist;

    /// <summary>선 자세로 섞기 시작했는지. 그 블렌드가 곧 평면으로 들어오는 구간이다.</summary>
    bool _settling;

    bool _turning;
    Quaternion _turnFrom;

    // 입력 콜백이 세우고 갱신 주기가 소비한다.
    bool _raiseRequested;

    /// <summary>
    /// 그림의 제자리를 <b>한 번만</b> 재어 둔다.
    ///
    /// 들어올 때마다 다시 재면, 어긋난 채로 한 번 들어오는 순간 그 어긋난 자리가 새 기준이 되어
    /// 되돌아갈 곳을 잃는다. 그 뒤로는 Exit이 아무리 제대로 돌아도 틀린 곳으로 돌아가고,
    /// 어긋남이 판이 끝날 때까지 쌓인다.
    ///
    /// 한 번이면 족한 것은 이 값이 프리팹이 정한 상수이기 때문이다 —
    /// 그림의 로컬 트랜스폼을 만지는 코드는 이 상태 하나뿐이다.
    /// </summary>
    public override void Bootstrap()
    {
        Transform mesh = characterManager.Animation.Mesh;
        if (mesh == null) return;

        _base = mesh.localPosition;
        _baseTurn = mesh.localRotation;
    }

    public override void Enter()
    {
        base.Enter();

        _rising = false;
        _settling = false;
        _turning = false;
        _raiseRequested = false;

        // <b>자세부터 세우고 나서 시간을 멈춘다.</b> 순서가 뒤바뀌면 자세가 서지 않는다 —
        // 애니메이터에 거는 것은 부탁이고, 그 부탁은 애니메이터가 <b>다음에 돌 때</b> 반영된다.
        // 배속을 먼저 0으로 눌러버리면 그 "다음"이 오지 않아 직전 자세가 그대로 남는다.
        // 부활 직후 주인공이 누워 있지 않고 선 채로 얼어붙던 것이 그것이다.
        characterManager.Animation.SetUnscaled(true);
        // <b>기상 클립의 첫 프레임이 아니라 따로 구운 누운 자세를 세운다.</b>
        // 그래야 누운 모습을 기상 동작과 무관하게 만질 수 있다 —
        // 클립은 character-laid 프리팹에서 굽는다(Tools ▸ Xsaya).
        characterManager.Animation.SetFloat(characterManager.RiseSpeedParameter, 0f);
        characterManager.Animation.Snap(characterManager.LaidState);

        // 세상을 멈춘다. 배속 칸은 HPbar가 매 프레임 쥐고 있으므로 상한만 누른다.
        TimeManager.SetCeiling(0f);

        // 다시 일어나는 자리에서는 회복 횟수도 함께 찬다.
        // 이 상태로 들어오는 길은 최초 로딩과 부활 둘뿐이고, 둘 다 "처음부터 시작하는" 순간이다.
        characterManager.RefillHealCharges();

        // <b>몸의 자리는 건드리지 않는다.</b> 몸이 놓인 자리는 스테이지가 정한 것이고,
        // 그것을 옮기면 Protagonist 대비 오프셋이 뭉개진다. 떼어 놓는 것은 그림뿐이다.
        Transform mesh = characterManager.Animation.Mesh;

        if (mesh != null)
        {
            // 떼어 놓는 거리는 <b>어디서 일어나느냐에 딸린다.</b> 자리마다 바닥과 제단이 다르므로
            // 체크포인트가 적어둔 값을 쓴다. director가 없는 판에서는 매니저에 적힌 값이 그 자리를 대신한다.
            _offset = SceneDirector.instance != null
                ? SceneDirector.instance.RestOffset
                : characterManager.RiseOffset;

            _twist = Quaternion.Euler(characterManager.RiseTwist);

            mesh.localPosition = _base + _offset;
            mesh.localRotation = _baseTurn * _twist;
        }

        Subscribe();
    }

    /// <summary>
    /// 전이도 이동도 <b>여기서</b> 한다. 시간 배속이 0이면 FixedUpdate가 아예 돌지 않아,
    /// 거기 걸어두면 누운 채로 영영 일어나지 못한다.
    /// </summary>
    public override void UpdateState()
    {
        if (!_rising)
        {
            // <b>커튼이 덮여 있는 동안에는 일어나지 않는다.</b> 부활은 이 상태를 커튼 아래서 먼저 들여보내므로,
            // 그 사이 누른 키로 일어서기 시작하면 걷히기 전에 기상이 지나가 버리고,
            // 끝나며 돌려주는 조작이 아직 중력이 꺼진 몸에 닿는다. 누른 것은 버린다 — 걷힌 뒤에 다시 누른다.
            if (Covered())
            {
                _raiseRequested = false;
                return;
            }

            if (Consume(ref _raiseRequested)) Begin();
            return;
        }

        float elapsed = Time.unscaledTime - _startedAt;

        Ramp(elapsed);
        Turn(elapsed);

        // 위치는 진입 구간이 되돌린다. 각도는 이미 클립 안에서 풀렸다.
        if (elapsed < characterManager.RiseTime) return;

        // 클립이 끝났다. 여기서부터가 실제로 일어서는 구간이다.
        if (!_settling) Settle();

        Approach(elapsed - characterManager.RiseTime);

        if (elapsed - characterManager.RiseTime >= characterManager.RiseSettle) Finish();
    }

    public override void Exit()
    {
        Unsubscribe();

        TimeManager.SetCeiling(1f);

        // 어디서 끊겼든 그림은 제자리로 돌려놓는다. 남겨두면 조작 중에 몸과 그림이 어긋난 채로 논다.
        Transform mesh = characterManager.Animation.Mesh;

        if (mesh != null)
        {
            mesh.localPosition = _base;
            mesh.localRotation = _baseTurn;
        }

        characterManager.Animation.SetUnscaled(false);
    }

    void Begin()
    {
        _rising = true;
        _startedAt = Time.unscaledTime;

        characterManager.Animation.SetFloat(characterManager.RiseSpeedParameter, 1f);

        // <b>누운 자세에서 기상 동작으로 건너온다.</b> 예전에는 기상 클립을 배속 0으로 세워 둔 채였으므로
        // 배속만 올리면 그대로 이어졌지만, 이제는 자세가 서로 다른 두 상태라 섞어야 한다.
        //
        // 시작 지점을 0으로 <b>명시해서</b> 건다. 지난번에 일어선 뒤라면 그 상태의 시간이
        // 클립 끝에 남아 있어, 어디서부터 트는지를 맡겨두면 일어선 자세로 섞여 들어간다.
        characterManager.Animation.PlayFrom(characterManager.RiseState, 0f, characterManager.LaidBlend);
    }

    /// <summary>일어나는 동안 시간을 되돌린다. 상한만 올리므로 체력이 정한 배속은 그대로 살아 있다.</summary>
    void Ramp(float elapsed)
    {
        float ramp = characterManager.RiseRamp;

        TimeManager.SetCeiling(ramp <= 0f ? 1f : Mathf.Clamp01(elapsed / ramp));
    }

    /// <summary>
    /// 선 자세로 섞기 시작한다. 섞이는 시간을 <see cref="CharacterManager.RiseSettle"/>로 주는 것이 요점이다 —
    /// 클립의 마지막 프레임은 아직 앉은 자세라, 실제로 일어서는 것은 이 블렌드이기 때문이다.
    /// </summary>
    void Settle()
    {
        _settling = true;

        characterManager.Animation.SetFloat(characterManager.RiseSpeedParameter, 0f);
        characterManager.Animation.Play(standState, characterManager.RiseSettle);
    }

    /// <summary>
    /// 비틀림을 되돌린다. <b>클립이 도는 구간에 겹쳐서</b> 한다.
    ///
    /// 예전에는 클립이 끝난 뒤 선 자세로 섞이는 동안 돌렸다. 그 구간에는 클립 쪽에 회전이 없어
    /// 도는 것이 따로 보였다 — 일어서다 말고 몸이 홱 돌아가는 것처럼 읽힌다.
    /// 클립 스스로 몸을 돌리는 마지막 구간에 맞춰 풀면 둘이 한 동작이 된다.
    ///
    /// 끝나는 지점은 클립의 끝이다. 그래야 선 자세로 섞이기 시작할 때 각도가 이미 0이고,
    /// 진입 구간은 다가오는 일만 남는다.
    /// </summary>
    void Turn(float elapsed)
    {
        Transform mesh = characterManager.Animation.Mesh;
        if (mesh == null) return;

        float from = characterManager.RiseTurnStart;
        float to = characterManager.RiseTime;

        float t = to <= from ? 1f : Mathf.Clamp01((elapsed - from) / (to - from));

        mesh.localRotation = _baseTurn * Quaternion.Slerp(_twist, Quaternion.identity, t);
    }

    /// <summary>
    /// 일어서는 그 시간에 맞춰 그림을 몸에 붙인다. <b>위치만</b> 되돌린다 —
    /// 각도는 <see cref="Turn"/>이 클립 안에서 이미 풀어 두었다.
    /// </summary>
    void Approach(float since)
    {
        Transform mesh = characterManager.Animation.Mesh;
        if (mesh == null) return;

        float settle = characterManager.RiseSettle;
        float t = settle <= 0f ? 1f : Mathf.Clamp01(since / settle);

        mesh.localPosition = _base + Vector3.Lerp(_offset, Vector3.zero, t);
    }

    /// <summary>선 자세의 컨트롤러 상태. 이동 축이 들어오며 같은 이름을 걸므로 두 번 섞이지 않는다.</summary>
    const string standState = "Idle";

    void Finish()
    {
        if (fsm is CharacterRoot root) root.ToControl();
    }

    /// <summary>전환이 아직 도는 중인지. 그동안은 커튼이 덮여 있고 몸의 중력도 꺼져 있다.</summary>
    static bool Covered() => SceneDirector.instance != null && SceneDirector.instance.Busy;

    static bool Consume(ref bool flag)
    {
        if (!flag) return false;

        flag = false;
        return true;
    }

    #region Input

    void Subscribe()
    {
        if (_subscribed || InputManager.instance == null) return;

        if (InputManager.instance.move_raise == null)
        {
            Debug.LogError("[Character_Rest] InputManager.move_raise가 비어 있어 일어날 수 없습니다.");
            return;
        }

        InputManager.instance.move_raise.action.performed += OnRaise;
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed || InputManager.instance == null) return;

        InputManager.instance.move_raise.action.performed -= OnRaise;
        _subscribed = false;
    }

    void OnRaise(InputAction.CallbackContext _)
    {
        _raiseRequested = true;
    }

    #endregion
}
