using UnityEngine;

/// <summary>
/// Time.timeScale의 유일한 주인. 다른 스크립트는 Time.timeScale에 직접 쓰지 않는다.
///
/// 배속과 정지를 <b>따로</b> 들고 있는 게 핵심이다.
/// 여럿이 Time.timeScale 하나를 두고 쓰면 마지막에 쓴 쪽이 이긴다.
/// HPbar는 매 프레임 체력 배속을 쓰고 일시정지는 0을 쓰므로,
/// 같은 칸에 쓰면 정지시켜도 다음 프레임에 체력 배속으로 되돌아온다.
///
/// 여기서는 SetScale이 배속만 갱신하고, 실제 적용은 정지 여부를 본 뒤에 한다.
/// 정지 중에 들어온 배속은 기억만 해두고 Resume에서 살아난다.
///
/// MonoBehaviour가 아니다. 씬에 놓을 오브젝트도, 인스턴스 null 검사도 필요 없다.
/// </summary>
public static class TimeManager
{
    public const float DefaultScale = 1f;

    static float _scale = DefaultScale;
    static bool _stopped;
    static float _ceiling = 1f;

    /// <summary>정지를 걷어냈을 때 돌아갈 배속. 정지 중에도 이 값은 유지된다.</summary>
    public static float Scale => _scale;

    public static bool Stopped => _stopped;

    /// <summary>연출이 눌러둔 상한(0~1). 배속과 곱해져 적용된다.</summary>
    public static float Ceiling => _ceiling;

    /// <summary>
    /// static 필드는 씬을 넘겨도, (도메인 리로드를 끈 경우) 플레이를 다시 눌러도 남는다.
    /// 정지 상태로 끝낸 판이 다음 판까지 따라오지 않도록 여기서 되돌린다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        _scale = DefaultScale;
        _stopped = false;
        _ceiling = 1f;
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
    /// SetScale과 따로 두는 이유는 정지를 따로 둔 이유와 같다. 배속 칸은 HPbar가 매 프레임
    /// 체력으로 덮어쓰므로, 연출이 그 칸에 쓰면 <b>다음 프레임에 사라진다.</b>
    /// 칸을 나눠두면 둘이 곱해질 뿐 서로를 지우지 않는다.
    ///
    /// 정지(Stop)와도 다르다. 그쪽은 껐다 켜는 스위치라 중간값이 없다.
    /// </summary>
    public static void SetCeiling(float ceiling)
    {
        _ceiling = Mathf.Clamp01(ceiling);
        Apply();
    }

    /// <summary>시간을 멈춘다. 배속은 그대로 두므로 Resume이 그 값을 되살린다.</summary>
    public static void Stop()
    {
        _stopped = true;
        Apply();
    }

    /// <summary>정지를 푼다. 멈추기 전 배속이 아니라 <b>가장 최근에 정해진</b> 배속으로 돌아간다.</summary>
    public static void Resume()
    {
        _stopped = false;
        Apply();
    }

    static void Apply()
    {
        Time.timeScale = _stopped ? 0f : _scale * _ceiling;
    }
}
