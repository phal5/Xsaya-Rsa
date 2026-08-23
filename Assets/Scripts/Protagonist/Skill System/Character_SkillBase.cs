using UnityEngine;

/// <summary>
/// 플레이어 스킬의 공통 골격.
///
/// 콤보가 Request()를 부르면 Character_Execution에 실행을 요청하고,
/// 실제 동작은 이 컴포넌트가 상태로서 수행한다.
/// 덕분에 "스킬 중에는 어떻게 움직일 수 있는가"가 상태 계약으로 표현된다.
///
/// Character_Execution의 Component States에 등록해서 쓴다.
/// </summary>
public abstract class Character_SkillBase : ComponentEntityState<CharacterManager>
{
    [Header("Timing")]
    [Tooltip("스킬이 도는 시간.")]
    [SerializeField, Min(0f)] protected float _duration = 0.4f;

    [Tooltip("스킬을 <b>건 순간부터</b> 다시 쓸 수 있게 되기까지의 시간.")]
    [SerializeField, Min(0f)] protected float _cooldown = 0.2f;

    [Header("Movement")]
    [Tooltip("켜면 스킬이 도는 동안 제자리에 선다. 끄면 지상 속도로 계속 조종할 수 있다.")]
    [SerializeField] bool _lockMovement = true;

    float _readyTime;
    float _endTime;

    /// <summary>
    /// 쿨다운이 지났는지. <b>거는 순간</b>부터 재므로, 끊겨도 남은 쿨다운은 그대로 돈다.
    ///
    /// 끝난 뒤부터 재면 동작이 길수록 다음 공격이 늦어져, 같은 쿨다운이 스킬마다 다른 간격이 된다.
    /// 시작부터 재면 "이 스킬은 0.5초에 한 번"이 동작 길이와 무관하게 그대로 성립한다.
    /// </summary>
    public bool IsReady => Time.time >= _readyTime;

    /// <summary>쿨다운이 풀리는 시각. 다음 입력을 언제부터 받는지가 곧 연격의 기준점이다.</summary>
    protected float ReadyAt => _readyTime;

    /// <summary>지금 이 스킬이 돌고 있는지. 도는 중에 다시 요청받는 스킬이 본다.</summary>
    protected bool Running { get; private set; }

    /// <summary>
    /// 이번 진입이 끝났는지. 시간이 아닌 것으로 재는 스킬이 갈아끼운다.
    ///
    /// 클립을 배속으로 트는 스킬은 벽시계로 잴 수 없다. 같은 동작이라도 배속이 다르면
    /// 걸리는 시간이 달라지므로, 그런 스킬은 <b>클립이 얼마나 지나갔는지</b>로 잰다.
    /// </summary>
    protected virtual bool Elapsed() => Time.time >= _endTime;

    Character_Execution _execution;

    Character_Execution Execution
    {
        get
        {
            if (_execution == null) _execution = GetComponentInParent<Character_Execution>(true);
            return _execution;
        }
    }

    /// <summary>콤보가 부르는 진입점. 쿨다운 중이면 조용히 무시한다.</summary>
    public virtual void Request()
    {
        if (!IsReady) return;

        if (Execution == null)
        {
            Debug.LogError($"[{name}] 상위에 Character_Execution이 없습니다. 스킬을 실행할 수 없습니다.", this);
            return;
        }

        Execution.Play(this);
    }

    public override void Enter()
    {
        Running = true;
        _readyTime = Time.time + _cooldown;
        _endTime = Time.time + _duration;

        // 속도는 상태 밖(Character_Movement)에 남으므로 명시적으로 세워야 한다.
        if (_lockMovement) manager.Steering.Move(Vector3.zero, Vector3.up);

        OnBegin();
    }

    public override void UpdateState()
    {
        if (!_lockMovement) Steer();

        if (Elapsed()) Finish();
    }

    public override void Exit()
    {
        Running = false;
        OnEnd();
    }

    /// <summary>
    /// 스킬 중 이동. 방향은 <b>지금 보는 쪽</b>으로 고정하고 입력은 세기만 정한다.
    ///
    /// 입력 방향을 그대로 넘기면 Steering이 그쪽으로 몸을 돌린다.
    /// 2D에서는 위치만 잠겨 있어 깊이 입력이 이동은 못 만들고 회전만 만들고,
    /// 그래서 공격 도중 캐릭터가 화면을 바라보게 된다.
    /// </summary>
    void Steer()
    {
        float amount = InputManager.CharacterMove.magnitude;

        if (amount < 0.01f) { manager.Steering.Move(Vector3.zero, Vector3.up); return; }

        manager.Steering.Move(manager.FacingAsInput() * (amount * manager.GroundSpeed), Vector3.up);
    }

    void Finish()
    {
        if (Execution != null) Execution.Complete();
    }

    #region Hooks - 파생 스킬이 채운다

    protected virtual void OnBegin() { }

    protected virtual void OnEnd() { }

    #endregion
}
