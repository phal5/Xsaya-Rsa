using UnityEngine;

/// <summary>
/// Time.timeScale의 유일한 주인. 다른 스크립트는 Time.timeScale에 직접 쓰지 않는다.
///
/// <b>칸을 나눠 들고 있는 게 핵심이다.</b> 여럿이 Time.timeScale 하나를 두고 쓰면 마지막에 쓴 쪽이 이긴다.
/// HPbar는 매 프레임 체력 배속을 쓰므로, 같은 칸에 쓰면 무엇을 걸어도 다음 프레임에 체력 배속으로 되돌아온다.
/// 칸을 나눠두면 둘이 곱해질 뿐 서로를 지우지 않는다.
///
///   Scale      체력 배속. HPbar가 매 프레임 쓴다.
///   Ceiling    연출이 눌러두는 상한. Character_Rest가 쓴다.
///   Impedance  저항. 0이면 그대로, 1이면 멎는다. 대사와 피격이 쓴다.
///   Paused     일시정지 메뉴. 켜지면 위 셋과 무관하게 0이다.
///
/// 저항은 다시 셋이 겹치고, 가장 센 것이 이긴다.
///   Stop()                  풀어줄 때까지 멈춘다. Play()가 푼다. 대사가 쓴다.
///   Stop(duration)          그 시간 동안 멈춘다. 피격 경직.
///   Slow(amount, duration)  그 시간 동안 amount만큼 저항한다. 피격 감속.
///
/// <b>시간이 붙은 둘은 벽시계로 잰다.</b> 멈춘 동안에는 게임 시간이 흐르지 않으므로,
/// 게임 시간으로 재면 Stop(duration)이 영영 끝나지 않는다.
///
/// MonoBehaviour가 아니다. 씬에 놓을 오브젝트도, 인스턴스 null 검사도 필요 없다.
/// 만료를 볼 시계만은 오브젝트가 있어야 해서, 시간이 붙은 효과를 처음 걸 때 스스로 만든다.
/// </summary>
public static class TimeManager
{
    public const float DefaultScale = 1f;

    static float _scale = DefaultScale;
    static bool _paused;
    static float _ceiling = 1f;

    static bool _held;
    static float _stopUntil;
    static float _slowUntil;
    static float _slowAmount;

    static Clock _clock;

    /// <summary>정지를 걷어냈을 때 돌아갈 배속. 정지 중에도 이 값은 유지된다.</summary>
    public static float Scale => _scale;

    public static bool Paused => _paused;

    /// <summary>연출이 눌러둔 상한(0~1). 배속과 곱해져 적용된다.</summary>
    public static float Ceiling => _ceiling;

    /// <summary>지금의 저항(0~1). 0이면 그대로 흐르고 1이면 멎는다. 겹친 것 중 가장 센 값이다.</summary>
    public static float Impedance
    {
        get
        {
            float now = Time.unscaledTime;

            if (_held || now < _stopUntil) return 1f;

            return now < _slowUntil ? _slowAmount : 0f;
        }
    }

    /// <summary>
    /// static 필드는 씬을 넘겨도, (도메인 리로드를 끈 경우) 플레이를 다시 눌러도 남는다.
    /// 정지 상태로 끝낸 판이 다음 판까지 따라오지 않도록 여기서 되돌린다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        _scale = DefaultScale;
        _paused = false;
        _ceiling = 1f;

        _held = false;
        _stopUntil = 0f;
        _slowUntil = 0f;
        _slowAmount = 0f;

        // 시계는 지난 판과 함께 파괴되었다. 쥐고 있으면 죽은 오브젝트를 켜려 든다.
        _clock = null;

        Apply();
    }

    /// <summary>진행 배속을 정한다. 정지 중이라면 해제될 때까지 반영을 미룬다.</summary>
    public static void SetScale(float scale)
    {
        _scale = Mathf.Max(0f, scale);
        Apply();
    }

    /// <summary>
    /// 연출이 시간을 <b>눌러두는</b> 상한. 0이면 멎고 1이면 배속이 그대로 나간다.
    ///
    /// SetScale과 따로 두는 이유는 위에 적은 대로다. 배속 칸은 HPbar가 매 프레임 체력으로
    /// 덮어쓰므로, 연출이 그 칸에 쓰면 <b>다음 프레임에 사라진다.</b>
    ///
    /// 저항과도 칸을 나눈다. 둘 다 0~1이지만 이쪽은 Character_Rest 하나가 쥐고 기상 동안 매 프레임 쓰고
    /// 나갈 때 1로 되돌린다 — 대사나 피격이 같은 칸에 쓰면 그 되돌림에 지워진다.
    /// </summary>
    public static void SetCeiling(float ceiling)
    {
        _ceiling = Mathf.Clamp01(ceiling);
        Apply();
    }

    #region Pause - 일시정지 메뉴

    /// <summary>일시정지. 배속·상한·저항은 그대로 두므로 Resume이 그 값들을 되살린다.</summary>
    public static void Pause()
    {
        _paused = true;
        Apply();
    }

    /// <summary>일시정지를 푼다. 멈추기 전 배속이 아니라 <b>가장 최근에 정해진</b> 배속으로 돌아간다.</summary>
    public static void Resume()
    {
        _paused = false;
        Apply();
    }

    #endregion

    #region Impedance - 대사와 피격

    /// <summary>
    /// 풀어줄 때까지 멈춘다. <see cref="Play"/>가 푼다.
    ///
    /// <b>스위치 하나다.</b> 여럿이 걸어도 한 번의 Play로 풀린다 — 겹쳐 걸 일이 생기면 세는 방식으로 바꿔야 한다.
    /// 일시정지 메뉴와는 칸이 다르다. 대사 중에 메뉴를 열었다 닫아도 대사의 정지는 그대로 남는다.
    /// </summary>
    public static void Stop()
    {
        _held = true;
        Apply();
    }

    /// <summary>
    /// <paramref name="duration"/>초(벽시계) 동안 멈춘다. 이미 멈춰 있으면 더 늦게 끝나는 쪽을 따른다.
    /// </summary>
    public static void Stop(float duration)
    {
        if (duration <= 0f) return;

        _stopUntil = Mathf.Max(_stopUntil, Time.unscaledTime + duration);
        Wind();
    }

    /// <summary>
    /// <see cref="Stop()"/>이 건 정지를 푼다. <b>시간이 붙은 효과는 걷지 않는다</b> —
    /// 그것들은 제 시간에 스스로 끝난다. 대사가 닫히며 피격 경직을 잘라먹지 않게 하려는 것이다.
    /// </summary>
    public static void Play()
    {
        _held = false;
        Apply();
    }

    /// <summary>
    /// <paramref name="duration"/>초(벽시계) 동안 <paramref name="amount"/>만큼 저항한다.
    /// 0이면 그대로, 1이면 멎는다 — 0.7이면 30% 속도로 흐른다.
    ///
    /// 감속이 도는 중에 또 걸리면 <b>센 쪽과 늦게 끝나는 쪽</b>을 따른다.
    /// 약한 감속이 뒤따라와 센 감속을 덮어쓰면, 두 번째 타격이 첫 타격의 무게를 지운다.
    /// </summary>
    public static void Slow(float amount, float duration)
    {
        if (duration <= 0f) return;

        float now = Time.unscaledTime;
        amount = Mathf.Clamp01(amount);

        _slowAmount = now < _slowUntil ? Mathf.Max(_slowAmount, amount) : amount;
        _slowUntil = Mathf.Max(_slowUntil, now + duration);

        Wind();
    }

    #endregion

    static void Apply()
    {
        Time.timeScale = _paused ? 0f : _scale * _ceiling * (1f - Impedance);
    }

    /// <summary>시간이 붙은 효과를 걸었다. 곧바로 적용하고, 만료를 볼 시계를 돌린다.</summary>
    static void Wind()
    {
        Apply();

        if (_clock == null)
        {
            GameObject clock = new GameObject(nameof(TimeManager) + " Clock");
            clock.hideFlags = HideFlags.HideInHierarchy;
            Object.DontDestroyOnLoad(clock);

            _clock = clock.AddComponent<Clock>();
        }

        _clock.enabled = true;
    }

    /// <summary>
    /// 시간이 붙은 효과가 남아 있는 동안만 돈다. 만료는 누가 알려주지 않으므로 여기서 다시 적용하고,
    /// 다 끝나면 스스로 쉰다.
    ///
    /// 늘 돌지 않는 이유는 Time.timeScale을 매 프레임 덮어쓰지 않기 위해서다 —
    /// 이 클래스 밖에서 직접 쓰는 곳(빌드 만료 화면)이 하나 있어, 늘 돌면 그쪽을 지운다.
    /// </summary>
    sealed class Clock : MonoBehaviour
    {
        void Update()
        {
            Apply();

            float now = Time.unscaledTime;
            if (now >= _stopUntil && now >= _slowUntil) enabled = false;
        }
    }
}
