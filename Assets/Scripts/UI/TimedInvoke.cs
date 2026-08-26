using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 정해둔 시간이 지나면 이벤트를 쏜다. <see cref="KeyInvoke"/>·<see cref="RaiseInvoke"/>와 같은 계열로,
/// 발동 조건만 입력 대신 시간이다.
///
/// <b>기본은 벽시계다.</b> 이 게임은 체력이 <see cref="TimeManager"/>를 통해 시간 배속을 끌어내리므로,
/// 스케일 시계로 재면 빈사 상태에서 3초가 10초가 되고 일시정지 중에는 영영 끝나지 않는다.
/// <see cref="TransitionCurtain"/>이 같은 이유로 벽시계를 쓴다.
/// 게임 안의 지속시간(버프·쿨다운처럼 배속을 따라야 하는 것)을 잴 때만 꺼서 쓴다.
///
/// 코루틴이 아니라 Update로 센다. 껐다 켜는 동안 타이머가 죽지 않고 멈춰 있다가 이어진다.
/// </summary>
public class TimedInvoke : MonoBehaviour
{
    #region Inspector Fields

    [Tooltip("이 시간이 지나면 이벤트가 발동한다. 초 단위.")]
    [SerializeField, Min(0f)] private float _delay = 1f;

    [Tooltip("씬에 올라오자마자 재기 시작한다. 끄면 Begin()을 직접 불러야 한다.")]
    [SerializeField] private bool _beginOnStart = true;

    [Tooltip("시간 배속을 무시하고 실제 흐른 시간으로 잰다. " +
             "연출·안내처럼 체력이나 일시정지에 끌려다니면 안 되는 것은 켜둔다.")]
    [SerializeField] private bool _unscaled = true;

    [Tooltip("시간이 다 됐을 때.")]
    [SerializeField] private UnityEvent _onElapsed;

    #endregion

    #region Internal State

    bool _running;
    float _elapsed;

    /// <summary>지금 재고 있는지.</summary>
    public bool Running => _running;

    /// <summary>남은 시간. 재고 있지 않으면 0이다.</summary>
    public float Remaining => _running ? Mathf.Max(0f, _delay - _elapsed) : 0f;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        if (_beginOnStart) Begin();
    }

    void Update()
    {
        if (!_running) return;

        _elapsed += _unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
        if (_elapsed < _delay) return;

        // 이벤트보다 먼저 멈춘다. 이벤트가 Begin()을 다시 부르는 연출이라면
        // 그 요청이 살아남아야 하는데, 뒤에서 멈추면 방금 건 요청을 도로 끄게 된다.
        _running = false;

        _onElapsed.Invoke();
    }

    #endregion

    #region Public API

    /// <summary>
    /// 처음부터 다시 잰다. 이미 재고 있어도 0으로 되돌린다 —
    /// 트리거를 다시 밟는 것이 "연장"으로 읽히는 편이 자연스럽기 때문이다.
    /// </summary>
    public void Begin()
    {
        _elapsed = 0f;
        _running = true;
    }

    /// <summary>재던 것을 버린다. 이벤트는 발동하지 않는다.</summary>
    public void Cancel()
    {
        _running = false;
    }

    #endregion
}
