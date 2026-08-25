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

    public override void Enter()
    {
        base.Enter();

        _rising = false;
        _settling = false;
        _turning = false;
        _raiseRequested = false;

        // 세상을 멈춘다. 배속 칸은 HPbar가 매 프레임 쥐고 있으므로 상한만 누른다.
        TimeManager.SetCeiling(0f);

        // <b>몸의 자리는 건드리지 않는다.</b> 몸이 놓인 자리는 스테이지가 정한 것이고,
        // 그것을 옮기면 Protagonist 대비 오프셋이 뭉개진다. 떼어 놓는 것은 그림뿐이다.
        Transform mesh = characterManager.Animation.Mesh;

        if (mesh != null)
        {
            _base = mesh.localPosition;
            _baseTurn = mesh.localRotation;

            // 떼어 놓는 거리는 <b>어디서 일어나느냐에 딸린다.</b> 자리마다 바닥과 제단이 다르므로
            // 체크포인트가 적어둔 값을 쓴다. director가 없는 판에서는 매니저에 적힌 값이 그 자리를 대신한다.
            _offset = SceneDirector.instance != null
                ? SceneDirector.instance.RestOffset
                : characterManager.RiseOffset;

            _twist = Quaternion.Euler(characterManager.RiseTwist);

            mesh.localPosition = _base + _offset;
            mesh.localRotation = _baseTurn * _twist;
        }

        characterManager.Animation.SetUnscaled(true);

        // 배속 0으로 눌러 첫 프레임 — 누운 자세 — 에 세워 둔다.
        //
        // <b>Play가 아니라 PlayFrom이다.</b> Play는 이름만 보고 같으면 돌아나가는데,
        // 이미 한 번 일어선 뒤라면 애니메이터는 이름은 그대로 Rise인 채로 클립의 끝 —
        // 일어선 자세 — 에 서 있다. 그 위에 배속 0을 얹으면 선 자세로 얼어붙는다.
        // 되감는 일을 아무도 하지 않는 것이 문제였으므로, 시작 지점을 직접 준다.
        characterManager.Animation.SetFloat(characterManager.RiseSpeedParameter, 0f);
        characterManager.Animation.PlayFrom(characterManager.RiseState, 0f);

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
