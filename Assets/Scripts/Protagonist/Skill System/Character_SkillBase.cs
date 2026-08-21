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

    [Tooltip("끝난 뒤 다시 쓸 수 있게 되기까지의 시간.")]
    [SerializeField, Min(0f)] protected float _cooldown = 0.2f;

    [Header("Movement")]
    [Tooltip("켜면 스킬이 도는 동안 제자리에 선다. 끄면 지상 속도로 계속 조종할 수 있다.")]
    [SerializeField] bool _lockMovement = true;

    float _readyTime;
    float _endTime;

    public bool IsReady => Time.time >= _readyTime;

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
    public void Request()
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
        _endTime = Time.time + _duration;

        // 속도는 상태 밖(Character_Movement)에 남으므로 명시적으로 세워야 한다.
        if (_lockMovement) manager.Steering.Move(Vector3.zero, Vector3.up);

        OnBegin();
    }

    public override void UpdateState()
    {
        if (!_lockMovement) Steer();

        if (Time.time >= _endTime) Finish();
    }

    public override void Exit()
    {
        // 중간에 끊겼든 끝났든 쿨다운은 돈다.
        _readyTime = Time.time + _cooldown;
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
