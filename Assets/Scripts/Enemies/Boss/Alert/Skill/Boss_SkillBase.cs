using System.Collections.Generic;
using UnityEngine;

/// <summary>스킬 시작 기준의 히트박스 구간. 프레임으로 적는다. 연타는 이걸 여러 개 늘어놓는다.</summary>
[System.Serializable]
public struct HitWindow
{
    [Tooltip("스킬 시작(Enter) 기준 프레임. 이 프레임에 판정이 켜진다.")]
    [Min(0)] public int start;

    [Tooltip("이 프레임에 판정이 꺼진다.")]
    [Min(0)] public int end;

    public bool Contains(int frame) => frame >= start && frame < end;
}

/// <summary>
/// 스킬 시작 시점 기준의 애니메이션 전환. 히트박스 구간과 같은 방식으로 늘어놓는다.
/// 한 스킬이 여러 동작으로 쪼개질 때 쓴다 — 도약과 내리꽂기처럼.
/// </summary>
[System.Serializable]
public struct AnimationCue
{
    [Tooltip("스킬 시작(Enter) 기준 프레임. 0이면 진입과 동시에 건다.")]
    [Min(0)] public int frame;

    [Tooltip("애니메이터 트리거 이름.")]
    public string trigger;
}

/// <summary>
/// 스킬 하나. 타이밍·위력·사거리·발동 조건을 스스로 들고 있어, 선택기의 목록에 넣기만 하면 된다.
///
/// 히트박스는 두 가지 방식 중 하나로 열린다.
///  - Hit Windows가 비어 있으면: 발동 구간 전체가 하나의 판정 (단타)
///  - Hit Windows가 있으면: 그 프레임 구간들이 판정을 지배 (연타)
/// 구간이 열릴 때마다 StartAttack이 다시 불려 피격 목록이 비워지므로, 타마다 새로 맞는다.
///
/// 애니메이션도 같은 방식이다.
///  - Animation Cues가 비어 있으면: 프로파일의 Animator Trigger 하나를 진입 시 재생
///  - Animation Cues가 있으면: 그 프레임들이 재생을 지배 (복합 동작)
/// 시간으로 잡히지 않는 순간은 파생 스킬이 PlayAnimation을 직접 부른다.
/// </summary>
public abstract class Boss_SkillBase : ComponentEntityState<BossManager>
{
    protected enum Phase { Windup, Active, Recovery }

    /// <summary>
    /// 이 스킬이 끊기지 않기 시작하는 지점. 곧 캔슬 정책이다.
    /// 피격 경직과 회피·방어 반응을 <b>함께</b> 막는다 — 둘 중 하나만 막으면 슈퍼아머가 반쪽이 된다.
    /// </summary>
    public enum Poise
    {
        None,        // 언제든 끊긴다
        FromWindup,  // 시전을 시작하면 끊을 수 없다
        FromActive,  // 선딜은 끊을 수 있고, 휘두르기 시작하면 못 끊는다
    }

    [Header("Profile - 이 스킬의 구간 길이와 위력")]
    [SerializeField] BossSkillProfile _profile = new BossSkillProfile();

    [Tooltip("이 스킬이 재생할 클립. 넣어두면 BossManager의 '스킬 애니메이션 동기화'가 " +
             "Animator Trigger와 같은 이름의 상태를 만들어 이 클립을 물려준다.\n" +
             "비워두면 아무것도 만들지 않는다 — 손으로 짠 상태를 트리거 이름으로 부르는 스킬용.")]
    [SerializeField] AnimationClip _clip;

    [Header("Condition - 쿨다운·사거리 외에 더 걸 조건. 비워두면 조건 없음.")]
#if UNITY_EDITOR
    [SerializeField] UnityEditor.MonoScript _conditionScript;
#endif
    // 스크립트 참조는 에디터 전용이라 빌드에 남지 않는다. 클래스 이름을 문자열로 들고 간다.
    [SerializeField, HideInInspector] string _conditionTypeName;

    [Header("Movement Source - 둘 다 켜면 이동이 겹쳐 발이 미끄러진다")]
    [Tooltip("애니메이션의 이동량을 몸체에 반영한다. 전진하며 베는 공격 등.")]
    [SerializeField] bool _useRootMotion;

    [Tooltip("스크립트 속도를 유지한다. 끄면 진입 시 수평 속도를 버리고 그 동안 계속 0으로 눌러둔다.")]
    [SerializeField] bool _useVelocity;

    [Header("Poise - 이 스킬이 도는 동안 경직될지")]
    [SerializeField] Poise _superArmor = Poise.None;

    [Header("Animation Window - 클립의 어느 구간을 쓸지 (클립 자체의 프레임 기준)")]
    [Tooltip("클립의 이 프레임부터 재생한다. 0이면 처음부터.")]
    [SerializeField, Min(0f)] float _clipStartFrame;

    [Tooltip("클립의 이 프레임에서 구간이 끝난다. 0이면 클립 끝까지.")]
    [SerializeField, Min(0f)] float _clipEndFrame;

    [Tooltip("직전 동작에서 섞여 들어오는 시간(초).")]
    [SerializeField, Min(0f)] float _blendTime = 0.12f;

    [Tooltip("켜면 구간을 스킬 길이에 맞춰 늘리거나 줄인다 — 끝 프레임에 스킬 종료와 함께 닿는다.\n" +
             "끄면 원래 속도로 재생하고 끝 프레임에서 멈춰 선 채 스킬이 끝나기를 기다린다.")]
    [SerializeField] bool _fitToDuration = true;

    [Header("Timeline - 스킬 시작 기준 프레임")]
    [Tooltip("아래 두 목록이 기준으로 삼는 프레임레이트. 클립의 프레임레이트와는 별개다 — " +
             "이쪽은 스킬이 시작한 뒤 흐른 시간의 눈금이다.")]
    [SerializeField, Min(1f)] float _frameRate = 30f;

    [Tooltip("스킬 시작 기준 히트박스 구간들. 비워두면 발동 구간 전체가 한 번의 판정이 된다.")]
    [SerializeField] List<HitWindow> _hitWindows = new List<HitWindow>();

    [Tooltip("스킬 시작 기준 애니메이션 전환 시점들. 프레임 순서대로 넣는다.")]
    [SerializeField] List<AnimationCue> _animationCues = new List<AnimationCue>();

    public BossSkillProfile Profile => _profile;

    /// <summary>동기화 도구가 컨트롤러에 물려줄 클립. 비어 있으면 손 배선이라는 뜻이다.</summary>
    public AnimationClip Clip => _clip;

    protected Phase phase { get; private set; }

    protected Boss_SkillMachine Machine => fsm as Boss_SkillMachine;

    bool UsesHitWindows => _hitWindows != null && _hitWindows.Count > 0;

    bool UsesAnimationCues => _animationCues != null && _animationCues.Count > 0;

    /// <summary>스킬 시작 기준 현재 프레임.</summary>
    protected int Frame => Mathf.FloorToInt(_elapsed * _frameRate);

    /// <summary>지금 구간이 끝나기까지 남은 시간. 구간 안에서 진행도를 재는 스킬이 쓴다.</summary>
    protected float PhaseRemaining => _timer;

    float _timer;
    float _elapsed;
    bool _windowHeld;
    int _openWindow = -1;
    int _nextCue;

    ISkillCondition _condition;
    bool _conditionResolved;

    #region Selection - 선택기가 묻는 것

    /// <summary>
    /// 지금 이 스킬을 쓸 수 있는지. 쿨다운, 사거리, 그리고 조건.
    /// 스킬이 자기만 아는 준비 조건을 더 걸 수 있게 열어둔다 — 던질 물건이 남아 있는지 같은 것.
    /// </summary>
    public virtual bool IsReady(BossManager boss)
    {
        if (boss == null) return false;

        return _profile.IsReady
            && boss.DistanceToPlayer() <= _profile.range
            && boss.DistanceToPlayer() >= _profile.minRange
            && ConditionMet(boss);
    }

    bool ConditionMet(BossManager boss)
    {
        ResolveCondition();
        return _condition == null || _condition.Met(boss);
    }

    /// <summary>이름으로 조건을 한 번만 만든다. 조건은 상태가 없으므로 재사용해도 된다.</summary>
    void ResolveCondition()
    {
        if (_conditionResolved) return;
        _conditionResolved = true;

        // 저장된 값은 AssemblyQualifiedName이라 네임스페이스가 바뀌면 그대로는 못 찾는다.
        System.Type type = SerializedType.Resolve(_conditionTypeName, typeof(ISkillCondition), this);
        if (type == null) return;

        _condition = System.Activator.CreateInstance(type) as ISkillCondition;
    }

    #endregion

    public override void Enter()
    {
        phase = Phase.Windup;
        _timer = _profile.windupTime;
        _elapsed = 0f;
        _windowHeld = false;
        _openWindow = -1;
        _nextCue = 0;

        if (!_useVelocity) manager.Stop();

        // 큐가 있으면 재생은 전적으로 큐가 지배한다. 0프레임 큐가 여기서 걸린다.
        if (UsesAnimationCues) UpdateAnimationCues();
        else PlayWindow();

        if (manager.rootMotion != null) manager.rootMotion.Active = _useRootMotion;

        manager.casting = true;

        ApplyPoise();
        OnWindup();
    }

    public override void UpdateState()
    {
        _elapsed += Time.deltaTime;
        _timer -= Time.deltaTime;

        if (_timer <= 0f) Advance();

        // 속도를 안 쓰는 스킬은 잔여 속도를 계속 눌러둔다.
        // Stop()만으로는 maxAccel로 서서히 줄어들어 그 동안 발이 미끄러진다.
        if (!_useVelocity && manager.steering != null) manager.steering.KillHorizontalVelocity();

        UpdateHitWindows();
        UpdateAnimationCues();
        HoldAtWindowEnd();
        Transitions();
    }

    public override void Exit()
    {
        // 끊겼든 끝났든 루트 모션은 반드시 꺼야 한다. 켜둔 채 나가면 이동 상태와 싸운다.
        if (manager.rootMotion != null) manager.rootMotion.Active = false;

        // 반응이나 피격에 중간에 끊겼을 수 있다. 히트박스를 반드시 내린다.
        if (manager.weapon != null) manager.weapon.EndAttack();
        _openWindow = -1;

        // 끊겼든 끝났든 쿨다운은 돌린다. 끊긴 직후 같은 스킬이 즉시 다시 나가는 걸 막는다.
        _profile.StartCooldown();

        // 경직 면역은 반드시 되돌린다. 남기면 보스가 영영 안 끊긴다.
        manager.staggerImmune = false;

        // 시전 표시도 같은 이유로 반드시 내린다. 끊겨 나가는 길도 여기를 지난다.
        manager.casting = false;

        // 배속도 되돌린다. 남기면 다음 동작이 이 스킬의 배속으로 돈다.
        manager.SetSkillSpeed(1f);
    }

    public override void Transitions() { }

    /// <summary>지금 구간이 면역 구간에 들어왔는지 매기고 매니저에 알린다.</summary>
    void ApplyPoise()
    {
        manager.staggerImmune = _superArmor switch
        {
            Poise.FromWindup => true,
            Poise.FromActive => phase != Phase.Windup,
            _ => false,
        };
    }

    #region Phases

    void Advance()
    {
        switch (phase)
        {
            case Phase.Windup:
                phase = Phase.Active;
                _timer = _profile.activeTime;
                Activate();
                OnActivate();
                break;

            case Phase.Active:
                phase = Phase.Recovery;
                _timer = _profile.recoveryTime;
                if (!UsesHitWindows && manager.weapon != null) manager.weapon.EndAttack();
                OnRecover();
                break;

            case Phase.Recovery:
                Complete();
                return;   // 이미 나갔다. 면역을 다시 세우지 않는다.
        }

        ApplyPoise();
    }

    void Activate()
    {
        // 구간 목록이 있으면 판정은 그쪽이 지배한다.
        if (UsesHitWindows || manager.weapon == null) return;

        manager.weapon.SetDamage(_profile.damage);
        manager.weapon.StartAttack();
    }

    #endregion

    #region Hit Windows

    /// <summary>
    /// 지금 프레임이 어느 구간에 들었는지 보고, 바뀌었을 때만 무기를 껐다 켠다.
    /// 켤 때마다 피격 목록이 비워지므로 한 스킬 안에서 여러 번 맞힐 수 있다.
    /// </summary>
    void UpdateHitWindows()
    {
        if (!UsesHitWindows) return;

        int frame = Frame;
        int current = -1;

        for (int i = 0; i < _hitWindows.Count; i++)
        {
            if (_hitWindows[i].Contains(frame)) { current = i; break; }
        }

        if (current == _openWindow) return;

        if (_openWindow >= 0 && manager.weapon != null) manager.weapon.EndAttack();

        _openWindow = current;

        if (current >= 0 && manager.weapon != null)
        {
            manager.weapon.SetDamage(_profile.damage);
            manager.weapon.StartAttack();   // 피격 목록이 비워져 이번 타는 새로 맞는다
        }
    }

    #endregion

    #region Animation

    /// <summary>스킬 전체 길이. 애니메이션 구간을 여기에 맞춘다.</summary>
    public float TotalTime => _profile.windupTime + _profile.activeTime + _profile.recoveryTime;

    /// <summary>쓰기로 한 클립 구간의 끝 프레임. 0으로 두면 클립 끝을 뜻한다.</summary>
    float EndFrame => _clipEndFrame > _clipStartFrame ? _clipEndFrame : ClipFrames;

    /// <summary>
    /// 클립 좌표계의 프레임레이트. <see cref="_frameRate"/>와 <b>다른 눈금</b>이다 —
    /// 저쪽은 스킬이 시작한 뒤 흐른 시간을, 이쪽은 클립 안의 위치를 잰다.
    /// 클립이 자기 값을 들고 있으므로 물어보면 된다. 클립이 없으면 잴 방법이 없어 구간 기능도 쉰다.
    /// </summary>
    float ClipFrameRate => _clip != null ? _clip.frameRate : 0f;

    float ClipFrames => _clip != null ? _clip.length * _clip.frameRate : 0f;

    bool HasWindow => _clip != null && EndFrame > _clipStartFrame;

    /// <summary>
    /// 클립의 지정 구간으로 섞어 들어간다.
    ///
    /// 배속은 컨트롤러가 아니라 여기서 정한다 — 구간 길이와 스킬 길이를 둘 다 아는 건 스킬뿐이다.
    /// 모든 스킬 상태가 같은 Float 하나를 Speed Multiplier로 물고 있어도 되는 이유는,
    /// 스킬이 한 번에 하나만 돌기 때문이다.
    /// </summary>
    void PlayWindow()
    {
        string state = _profile.animatorTrigger;
        if (manager.animator == null || string.IsNullOrEmpty(state)) return;

        float offset = ClipFrameRate > 0f ? _clipStartFrame / ClipFrameRate : 0f;

        manager.SetSkillSpeed(WindowSpeed());
        manager.animator.CrossFadeInFixedTime(state, _blendTime, 0, offset);
    }

    /// <summary>구간을 스킬 길이에 맞추는 배속. 맞추지 않기로 했거나 잴 수 없으면 1.</summary>
    float WindowSpeed()
    {
        if (!_fitToDuration || !HasWindow || TotalTime <= 0f) return 1f;

        return (EndFrame - _clipStartFrame) / ClipFrameRate / TotalTime;
    }

    /// <summary>
    /// 끝 프레임에 닿으면 거기서 멈춰 선다. 스킬이 끝날 때까지 그 자세를 들고 있는다.
    ///
    /// 길이에 맞추기로 했으면 할 일이 없다 — 그쪽은 스킬 종료와 동시에 끝 프레임에 닿도록
    /// 배속을 정해두었으므로, 멈춰 세울 남는 시간 자체가 없다.
    /// </summary>
    void HoldAtWindowEnd()
    {
        if (_fitToDuration || _windowHeld || !HasWindow) return;
        if (manager.animator == null || string.IsNullOrEmpty(_profile.animatorTrigger)) return;

        AnimatorStateInfo info = manager.animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(_profile.animatorTrigger)) return;

        // info.length는 배속으로 나뉜 값이라 쓰면 안 된다. normalizedTime은 배속과 무관하다.
        if (info.normalizedTime * ClipFrames < EndFrame) return;

        _windowHeld = true;
        manager.SetSkillSpeed(0f);
    }

    void UpdateAnimationCues()
    {
        if (!UsesAnimationCues) return;

        int frame = Frame;

        while (_nextCue < _animationCues.Count && _animationCues[_nextCue].frame <= frame)
        {
            manager.PlayAnimation(_animationCues[_nextCue].trigger);
            _nextCue++;
        }
    }

    /// <summary>시간으로 잡히지 않는 순간에 파생 스킬이 직접 부른다.</summary>
    protected void PlayAnimation(string trigger)
    {
        manager.PlayAnimation(trigger);
    }

    #endregion

    /// <summary>스킬이 정상적으로 끝났다. 선택기로 돌아간다.</summary>
    protected void Complete()
    {
        if (Machine != null) Machine.Complete();
    }

    #region Hooks - 파생 스킬이 필요할 때만 채운다

    protected virtual void OnWindup() { }

    protected virtual void OnActivate() { }

    protected virtual void OnRecover() { }

    #endregion

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        ValidateCondition();
        ValidateClipWindow();
        ValidateWindows();

        // 사거리가 보스의 이동 구간과 어긋나는지는 보스만 안다. 판정은 그쪽 한 곳에 둔다.
        BossManager boss = GetComponentInParent<BossManager>(true);
        if (boss != null) boss.WarnIfSkillRangeCollides(this);
    }

    void ValidateCondition()
    {
        if (_conditionScript == null) { _conditionTypeName = string.Empty; return; }

        System.Type type = _conditionScript.GetClass();

        if (type == null || !typeof(ISkillCondition).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
        {
            Debug.LogWarning($"[{name}] {_conditionScript.name}은(는) ISkillCondition을 구현한 구체 클래스가 아닙니다.", this);
            _conditionScript = null;
            _conditionTypeName = string.Empty;
            return;
        }

        _conditionTypeName = type.AssemblyQualifiedName;
    }

    void ValidateClipWindow()
    {
        if (_clip == null)
        {
            // 클립이 없으면 프레임을 잴 눈금이 없다. 값을 적어두면 안 듣는 채로 남는다.
            if (_clipStartFrame > 0f || _clipEndFrame > 0f)
                Debug.LogWarning($"[{name}] Clip이 비어 있어 시작/종료 프레임이 무시됩니다. " +
                                 $"클립을 넣거나 프레임 값을 0으로 두세요.", this);
            return;
        }

        if (_clipStartFrame >= ClipFrames)
            Debug.LogWarning($"[{name}] 시작 프레임({_clipStartFrame})이 클립 길이 {ClipFrames:0}프레임을 벗어납니다.", this);

        if (_clipEndFrame > 0f && _clipEndFrame <= _clipStartFrame)
            Debug.LogWarning($"[{name}] 종료 프레임({_clipEndFrame})이 시작({_clipStartFrame}) 이하라 구간이 비었습니다. " +
                             $"클립 끝까지 쓸 것이면 0으로 두세요.", this);

        if (_clipEndFrame > ClipFrames)
            Debug.LogWarning($"[{name}] 종료 프레임({_clipEndFrame})이 클립 길이 {ClipFrames:0}프레임을 넘어 " +
                             $"영영 닿지 않습니다.", this);
    }

    void ValidateWindows()
    {
        int total = Mathf.RoundToInt((_profile.windupTime + _profile.activeTime + _profile.recoveryTime) * _frameRate);

        for (int i = 0; UsesHitWindows && i < _hitWindows.Count; i++)
        {
            HitWindow w = _hitWindows[i];

            if (w.end <= w.start)
                Debug.LogWarning($"[{name}] Hit Window {i}: end({w.end})가 start({w.start}) 이하라 판정이 열리지 않습니다.", this);
            else if (w.end > total)
                Debug.LogWarning($"[{name}] Hit Window {i}: end({w.end})가 스킬 전체 {total}프레임을 넘어 잘립니다.", this);
        }

        for (int i = 1; UsesAnimationCues && i < _animationCues.Count; i++)
        {
            if (_animationCues[i].frame < _animationCues[i - 1].frame)
                Debug.LogWarning($"[{name}] Animation Cue {i}: 프레임이 앞 큐보다 빠릅니다. 순서대로 넣어야 합니다.", this);
        }
    }
#endif
}
