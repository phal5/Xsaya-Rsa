using UnityEngine;

/// <summary>
/// 기본 근접 공격. 연달아 베면 <b>이어지는 한 동작</b>으로 보이게 한다.
///
/// 휘두름마다 상태를 만들지 않았다. 그랬다면 전이가 동작 수만큼 늘어나는데
/// 그 전이들이 할 일은 전부 "다음 클립을 걸고 시간을 다시 센다"뿐이라, 상태로 나눌 값이 없다.
/// 여기서는 <see cref="Swing"/> 배열의 색인 하나가 그 자리를 대신한다.
///
/// 한 번의 베기는 <b>시작 자세 세우기 → 휘두름 → 끝 자세로 쿨다운 채우기 → 이동 자세로 복귀</b>다.
/// 쿨다운은 <b>거는 순간부터</b> 재므로 "이 스킬은 0.5초에 한 번"이 동작 길이와 무관하게 성립하고,
/// 남는 시간을 끝 자세로 채우니 무엇을 했는지가 화면에 남는다.
///
/// <b>휘두르는 동안에는 공격 키를 받지 않는다.</b> 한 동작이 끝나고 <see cref="_chainWindow"/>
/// 안에 다시 누르면 그때 다음 동작으로 이어진다. 마지막 동작 뒤에는 잇지 않고 1타로 돌아간다.
/// 도중에 눌린 것을 쌓아두었다가 풀면 손이 아니라 버퍼가 콤보를 치게 되고,
/// 그러면 한 번 눌러 시작한 연격이 어디서 끝나는지 손으로 가늠할 수 없다.
///
/// 끊기는 것은 <b>다른 스킬과 대시</b>뿐이다. 이동과 점프로는 끊기지 않는데,
/// 그 둘은 조작 축(Ground/Airborne)의 입력이고 스킬이 도는 동안 그 축은 돌지 않기 때문이다.
/// 막으려고 따로 해둔 것이 아니라 축이 갈려 있어 저절로 그렇다.
///
/// 콤보가 부르던 Attack() 진입점은 그대로 유지했다. UnityEvent 배선을 건드릴 필요가 없다.
/// </summary>
public class BasicAttack : Character_SkillBase
{
    [Header("Weapon")]
    [SerializeField] MeleeWeapon _weapon;

    [Tooltip("공격하지 않는 동안 무기 오브젝트를 통째로 꺼둔다.")]
    [SerializeField] bool _disableWeaponObject;

    [Header("Chain")]
    [Tooltip("적은 순서대로 이어진다. 비워두면 동작 없이 예전처럼 판정만 낸다.")]
    [SerializeField] Swing[] _chain;

    [Tooltip("Swing에 적은 프레임을 초로 바꿀 때 쓰는 기준. 클립의 프레임 레이트와 맞춘다.")]
    [SerializeField, Min(1f)] float _frameRate = 30f;

    [Tooltip("한 동작이 끝나고 이 시간 안에 다시 누르면 이어서 다음 동작으로 간다.")]
    [SerializeField, Min(0f)] float _chainWindow = 0.4f;

    [Tooltip("동작으로 들어갈 때 섞이는 시간. 짧을수록 타격이 또렷하다.")]
    [SerializeField, Min(0f)] float _fade = 0.03f;

    [Tooltip("쿨다운이 끝나고 이동 자세로 돌아가는 데 걸리는 시간.")]
    [SerializeField, Min(0f)] float _exitFade = 0.5f;

    [Header("Tempo")]
    [Tooltip("동작의 진행도(0~1)에 대한 재생 배속. 가운데가 빠른 산 모양이 휘두름을 또렷하게 만든다.")]
    [SerializeField] AnimationCurve _tempo = new AnimationCurve(
        new Keyframe(0f, 2f), new Keyframe(0.5f, 3f), new Keyframe(1f, 2f));

    [Tooltip("컨트롤러에서 이 동작들의 Speed에 물린 파라미터 이름.")]
    [SerializeField] string _tempoParameter = "SlashSpeed";

    int _index;

    /// <summary>지금 동작이 <b>클립 기준으로</b> 얼마나 지나갔는지. 배속을 적분한 값이다.</summary>
    float _frame;

    float _endedAt = float.NegativeInfinity;
    bool _opened;

    /// <summary>시작 자세를 세워 둘 남은 시간(초). 0보다 크면 클립도 우리 시계도 서 있다.</summary>
    float _hold;

    /// <summary>끝 프레임에 닿았는지. 닿은 뒤에는 쿨다운이 끝나기를 기다린다.</summary>
    bool _arrived;

    bool Usable => _chain != null && _chain.Length > 0;

    Swing Current => _chain[_index];

    bool Last => _index + 1 >= _chain.Length;

    void Awake()
    {
        if (_disableWeaponObject && _weapon != null) _weapon.gameObject.SetActive(false);
    }

    /// <summary>콤보 UnityEvent가 부르는 이름. 유지 목적으로 남겨둔다.</summary>
    public void Attack()
    {
        Request();
    }

    /// <summary>
    /// 휘두르는 동안 들어온 공격 입력은 <b>여기서 끝난다.</b>
    ///
    /// 예전에는 그 요청이 조작 머신까지 올라갔다가 버려졌다. 이미 실행 축이라 전이가 멱등하게
    /// 아무것도 하지 않았기 때문인데, 그 사이 Character_Execution에는 요청이 남아
    /// 한참 뒤 다른 스킬이 들어갈 때 대신 터졌다. 받는 곳을 여기로 옮겨 그 길을 없앤다.
    /// </summary>
    public override void Request()
    {
        if (Running) return;

        base.Request();
    }

    public override void Enter()
    {
        _index = Resume() ? _index + 1 : 0;

        base.Enter();
    }

    protected override void OnBegin()
    {
        if (_disableWeaponObject && _weapon != null) _weapon.gameObject.SetActive(true);

        // 동작이 안 적혀 있으면 예전처럼 스킬이 도는 내내 판정을 연다.
        if (!Usable)
        {
            Open();
            return;
        }

        _frame = Current.start;
        _hold = Current.holdStart / _frameRate;
        _arrived = false;
        _opened = false;

        float speed = Speed();

        // 첫 프레임부터 제 배속으로 돌게 값을 먼저 얹는다. 시계는 아직 밀지 않는다.
        manager.Animation.SetFloat(_tempoParameter, speed);

        // 우리 시계와 클립을 같은 지점에서 출발시킨다.
        //
        // 오프셋을 배속으로 나누는 이유. 유니티는 이 값을 클립 시간이 아니라
        // <b>배속이 적용된 상태 시간</b>으로 읽는다 — 정규화할 때 클립 길이가 아니라
        // (클립 길이 / 배속)으로 나눈다. 배속 5에 0.333초를 그대로 넘겼더니 정규화 시각이
        // 1.389가 되어 클립 끝을 지났고, 루프가 아니라 마지막 자세에 붙박였다.
        // 세 동작이 모두 "칼을 든 채 가만히"로 보이던 이유가 이것이다.
        if (Current.IsSet)
            manager.Animation.PlayFrom(Current.state, Current.start / _frameRate / speed, _fade);
    }

    public override void UpdateState()
    {
        if (Usable)
        {
            Tempo();
            Windows();
        }

        base.UpdateState();
    }

    /// <summary>휘두름이 끝났고 쿨다운도 지났을 때. 그 사이는 끝 자세로 서 있다.</summary>
    protected override bool Elapsed() => Usable ? (_arrived && IsReady) : base.Elapsed();

    protected override void OnEnd()
    {
        Close();

        // 끝까지 마친 동작만 이어칠 수 있다. 대시로 끊거나 얻어맞고 끊긴 연격은 1타부터 다시 간다.
        _endedAt = Elapsed() ? Time.time : float.NegativeInfinity;

        // 나가는 블렌드를 여기서 정한다.
        // 다음 자세를 거는 쪽(Ground_Idle 등)은 자기가 무엇을 밀어내는지 모르고 제 기본값을 쓴다.
        if (Usable) manager.Animation.FadeOnce(_exitFade);

        if (_disableWeaponObject && _weapon != null) _weapon.gameObject.SetActive(false);
    }

    /// <summary>직전 동작에 이어서 들어가는지. 마지막 동작 뒤에는 잇지 않는다.</summary>
    bool Resume()
    {
        if (!Usable || Last) return false;

        return Time.time - _endedAt <= _chainWindow;
    }

    #region Tempo

    /// <summary>
    /// 배속을 애니메이터에 넘기고, <b>같은 값으로</b> 우리 시계도 돌린다.
    ///
    /// 클립을 배속으로 틀면 벽시계와 클립 시간이 어긋난다. 히트 창과 끝 지점은 클립을 보고
    /// 적은 것이라 클립 시간으로 재야 맞다. 그래서 여기서 한 번 정한 배속으로 둘을 함께 민다 —
    /// 어느 한쪽만 빨라지면 판정이 동작보다 앞서거나 뒤처진다.
    /// </summary>
    void Tempo()
    {
        // 세워 두는 동안은 배속 0이다. 클립이 그 프레임에 멈춰 자세가 눈에 남는다.
        if (_hold > 0f)
        {
            manager.Animation.SetFloat(_tempoParameter, 0f);
            _hold -= Time.deltaTime;
            return;
        }

        if (_arrived) return;

        float speed = Speed();

        manager.Animation.SetFloat(_tempoParameter, speed);

        _frame += speed * _frameRate * Time.deltaTime;

        if (_frame < Current.end) return;

        // 끝 자세를 지나치지 않는다. 넘어간 몫을 남겨두면 세워 둔 자세가 칠 때마다 달라진다.
        // 여기서부터는 쿨다운이 끝날 때까지 이 자세로 서 있는다.
        _frame = Current.end;
        _arrived = true;

        manager.Animation.SetFloat(_tempoParameter, 0f);
    }

    /// <summary>진행도는 <b>남긴 구간</b> 안에서 잰다. 잘라낸 앞부분은 배속 곡선에 들어가지 않는다.</summary>
    float Speed()
    {
        float span = Current.end - Current.start;
        float progress = span > 0f ? Mathf.Clamp01((_frame - Current.start) / span) : 1f;

        // 0 이하는 클립을 세우는 것이 아니라 시계를 멈춰 동작이 끝나지 않게 만든다.
        // 세우는 일은 holdStart/holdEnd가 따로 한다.
        return Mathf.Max(_tempo.Evaluate(progress), 0.01f);
    }

    #endregion

    #region Hit Window

    void Windows()
    {
        if (!_opened)
        {
            if (_frame >= Current.hitOpen && _frame < Current.hitClose) Open();
        }
        else if (_frame >= Current.hitClose) Close();
    }

    /// <summary>StartAttack이 피격 목록을 비우므로 다음 타가 같은 적을 다시 맞힌다.</summary>
    void Open()
    {
        if (_weapon == null) return;

        _opened = true;
        _weapon.StartAttack();
    }

    void Close()
    {
        if (!_opened) return;

        _opened = false;
        if (_weapon != null) _weapon.EndAttack();
    }

    #endregion
}
