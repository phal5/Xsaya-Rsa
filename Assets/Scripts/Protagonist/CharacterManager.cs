using UnityEngine;

public class CharacterManager : EntityManager
{
    [Tooltip("캡슐과 피격 판정이 있는 몸. 바깥에서 '캐릭터가 어디 있나'를 묻는 곳은 여기다.")]
    [field: SerializeField] public Rigidbody Body { get; private set; }
    [field:SerializeField] public Character_Movement Movement {  get; private set; }
    [field: SerializeField] public Character_Steering Steering { get; private set; }
    [Tooltip("상태들이 Enter에서 재생할 애니메이션 이름을 넘기는 곳.")]
    [field: SerializeField] public Character_Animation Animation { get; private set; }
    [field:SerializeField] public CapsuleCollider CapsuleCollider { get; private set; }
    [Tooltip("회피 무적과 피격 상태가 참조한다.")]
    [field: SerializeField] public DamagableBase Damagable { get; private set; }
    [Tooltip("상호작용 대상 탐지기. 비어 있으면 Interact 상태로 들어가지 않는다.")]
    [field: SerializeField] public InteractionDetector Interactor { get; private set; }
    [Header("Gameplay")]
    [field: SerializeField] public float CoyoteTime { get; private set; } = 0.2f;
    [field: SerializeField] public float JumpBufferTime { get; private set; } = 0.2f;
    [Header("Horizontal Speed")]
    [field: SerializeField] public float GroundSpeed { get; private set; }
    [field: SerializeField] public float AirborneSpeed { get; private set; }
    [field: SerializeField] public float DashSpeed { get; private set; }
    [Tooltip("회피(Dodge)가 지속되는 시간.")]
    [field: SerializeField] public float DashTime { get; private set; } = 0.2f;
    [Header("Vertical Speed")]
    [field:SerializeField] public float JumpSpeed { get; private set; }
    [field: SerializeField] public JumpAnimation Jump { get; private set; } = new JumpAnimation();
    [Header("Ledge Grab")]
    [field: SerializeField] public LedgeGrab Ledge { get; private set; } = new LedgeGrab();
    [Header("Hit / Down")]
    [Tooltip("피격 경직이 지속되는 시간.")]
    [field: SerializeField] public float StunTime { get; private set; } = 0.4f;
    [Tooltip("다운 후 부활까지 걸리는 시간.")]
    [field: SerializeField] public float RespawnDelay { get; private set; } = 2f;
    [Tooltip("벽을 찬 뒤 벽 반대쪽을 볼 때까지 도는 속도(초당 각도).")]
    [field: SerializeField] public float WallTurnSpeed { get; private set; } = 360f;

    [Header("Heal - 자원 소모형")]
    [Tooltip("최대 회복 횟수. 시작 시 이만큼 채워진다.")]
    [field: SerializeField] public int HealChargeMax { get; private set; } = 3;
    [field: SerializeField] public float HealAmount { get; private set; } = 40f;
    [Tooltip("회복 동작이 걸리는 시간. 이 동안 조작을 잃는다.")]
    [field: SerializeField] public float HealTime { get; private set; } = 1.2f;
    [Tooltip("회복 동작 중의 이동 속도. 지상 속도보다 느리게 둔다.")]
    [field: SerializeField] public float HealSpeed { get; private set; } = 1.5f;

    [Header("Swap")]
    [field: SerializeField] public float SwapTime { get; private set; } = 0.5f;

    [Header("Interact")]
    [Tooltip("상호작용 동작이 붙드는 시간. 대화가 열리면 UI 상태가 따로 조작을 잠근다.")]
    [field: SerializeField] public float InteractTime { get; private set; } = 0.3f;

    [Header("Spherecaster")]
    [field:SerializeField] public SphereCaster GroundCaster { get; private set; }

    [Header("Ledge Gizmo")]
    [Tooltip("턱 탐지를 씬 뷰에 그린다. 그리기만 하고 아무것도 바꾸지 않는다.")]
    [SerializeField] bool _drawLedgeProbe = true;
    [Tooltip("가장자리에서 내려가는 탐지도 함께 그린다.")]
    [SerializeField] bool _drawLedgeGroundProbe = true;

    /// <summary>
    /// 몸의 원점에서 발끝까지의 거리. 원점이 캡슐 <b>중앙</b>에 있어 0이 아니다.
    ///
    /// 턱 탐지의 높이 수치는 모두 발밑 기준으로 적는다 — 캐릭터 키로 가늠하는 값들이라
    /// 그래야 인스펙터에서 읽힌다. 원점과의 차이는 여기서 한 번만 환산해 넘긴다.
    /// 캡슐에서 재므로 몸 크기를 바꿔도 따로 맞출 것이 없다.
    /// </summary>
    public float FootOffset
    {
        get
        {
            if (CapsuleCollider == null || Body == null) return 0f;

            Transform capsule = CapsuleCollider.transform;
            float half = CapsuleCollider.height * 0.5f * capsule.lossyScale.y;

            return Body.transform.position.y - (capsule.TransformPoint(CapsuleCollider.center).y - half);
        }
    }

    /// <summary>캡슐에서 잰 키. 손이 닿는 높이를 이것으로 가늠한다.</summary>
    public float CharacterHeight =>
        CapsuleCollider == null ? 0f : CapsuleCollider.height * CapsuleCollider.transform.lossyScale.y;

    /// <summary>
    /// 지금 바라보는 방향을 Steering이 받는 좌표계로 바꿔 돌려준다.
    ///
    /// Steering.Move는 인자를 <b>카메라 공간</b>으로 보고 TransformVector를 태운다.
    /// 월드 방향인 transform.forward를 그냥 넘기면 카메라 각도만큼 한 번 더 돌아가,
    /// 2D에서는 방향이 굳고 3D에서는 시점에 따라 달라진다.
    /// </summary>
    public Vector3 FacingAsInput()
    {
        Vector3 forward = CustomMath.RemoveY(Body.transform.forward);
        if (forward.sqrMagnitude < 0.0001f) return Vector3.forward;

        forward.Normalize();

        Camera camera = Camera.main;
        if (camera == null) return forward;

        return camera.transform.InverseTransformVector(forward);
    }

    #region Checkpoint

    /// <summary>
    /// 마지막으로 쉬어간 자리. <b>체크포인트 오브젝트가 아니라 그때 몸이 서 있던 자리</b>다.
    ///
    /// 이 기록이 매니저에 있는 이유는 캐릭터 씬이 내려가지 않기 때문이다 —
    /// Background만 갈아끼우는 구성이라 여기 적어둔 것이 씬을 넘어 살아남는다.
    /// 직렬화하지 않는다. 앱을 껐다 켜도 남아야 하는 것은 체크포인트가 아니라 세이브의 몫이다.
    /// </summary>
    string _restScene;
    Vector3 _restPlace;
    Quaternion _restFacing;

    /// <param name="scene">그 자리가 속한 Background 씬의 이름.</param>
    public void SetCheckpoint(string scene)
    {
        if (Body == null) return;

        _restScene = scene;
        _restPlace = Body.position;
        _restFacing = Body.rotation;
    }

    /// <summary>
    /// 쉬어간 자리를 <b>씬 이름까지</b> 묻는다.
    ///
    /// 그 씬이 지금 올라와 있는지는 따지지 않는다. 안 올라와 있으면 불러오면 되는 일이고,
    /// 불러올지 자리만 옮길지를 정하는 것은 <see cref="SceneDirector"/>의 몫이다.
    /// 여기서 미리 걸러내면 다른 스테이지의 기록이 없는 것처럼 보여 되돌아갈 길이 막힌다.
    /// </summary>
    /// <returns>한 번이라도 쉬어간 적이 있는지.</returns>
    public bool TryCheckpoint(out string scene, out Vector3 place, out Quaternion facing)
    {
        scene = _restScene;
        place = _restPlace;
        facing = _restFacing;

        return !string.IsNullOrEmpty(_restScene);
    }

    #endregion

    #region Heal Charges

    /// <summary>남은 회복 횟수. 직렬화하지 않고 시작 시 최대치로 채운다.</summary>
    public int HealCharges { get; private set; }

    void Awake()
    {
        HealCharges = HealChargeMax;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        Jump?.Validate(this);
        Ledge?.Validate(this, CharacterHeight);
    }
#endif

    /// <summary>회복을 시도한다. 남은 횟수가 없으면 실패한다.</summary>
    public bool ConsumeHealCharge()
    {
        if (HealCharges <= 0) return false;

        HealCharges--;
        return true;
    }

    /// <summary>휴식 지점 등에서 회복 횟수를 되돌린다.</summary>
    public void RefillHealCharges()
    {
        HealCharges = HealChargeMax;
    }

    #endregion

    #region Ledge Gizmo

    // 그림은 실제로 도는 코드가 남긴 자취(LedgeGrab.Trace)로만 그린다.
    // 여기서 탐지를 다시 짜 넣으면 둘이 갈라져, 그림은 멀쩡한데 게임은 틀린 상태를
    // 눈으로 못 알아보게 된다 — 그건 시각화가 없느니만 못하다.

    readonly LedgeGrab.Trace _airTrace = new LedgeGrab.Trace();
    readonly LedgeGrab.Trace _groundTrace = new LedgeGrab.Trace();

    static readonly Color GizmoNothing = new Color(0.35f, 0.35f, 0.35f);
    static readonly Color GizmoRejected = new Color(0.9f, 0.25f, 0.2f);
    static readonly Color GizmoAccepted = new Color(0.3f, 0.9f, 0.4f);
    static readonly Color GizmoWall = new Color(0.3f, 0.8f, 0.95f);
    static readonly Color GizmoNormal = new Color(0.95f, 0.35f, 0.9f);
    static readonly Color GizmoBraced = new Color(1f, 0.6f, 0.15f);
    static readonly Color GizmoFree = new Color(0.35f, 0.55f, 1f);
    static readonly Color GizmoStand = new Color(0.95f, 0.95f, 0.95f);
    static readonly Color GizmoBand = new Color(1f, 0.9f, 0.3f, 0.5f);

    void OnDrawGizmos()
    {
        if (!_drawLedgeProbe || Ledge == null || Body == null) return;

        // 에디터에서 몸을 끌어 옮기면 물리 쪽 좌표가 아직 따라오지 않았을 수 있다.
        if (!Application.isPlaying) Physics.SyncTransforms();

        Transform body = Body.transform;
        float foot = FootOffset;

        DrawScanBand(body, foot);

        Ledge.Probe(body, foot, out _, _airTrace);
        DrawLedgeTrace(_airTrace, foot);

        if (!_drawLedgeGroundProbe) return;

        Ledge.ProbeFromTop(body, foot, out _, _groundTrace);
        DrawLedgeTrace(_groundTrace, foot);
    }

    /// <summary>어디서 얼마나 쏘는지. 탐지 결과와 무관하게 언제나 보인다.</summary>
    void DrawScanBand(Transform body, float foot)
    {
        Vector3 forward = CustomMath.RemoveY(body.forward);
        if (forward.sqrMagnitude < 0.0001f) return;
        forward.Normalize();

        Vector3 feet = body.position;
        feet.y -= foot;

        Gizmos.color = GizmoBand;

        Vector3 origin = Ledge.ScanOrigin(feet, forward);
        Vector3 down = Vector3.down * Ledge.ScanLength;

        // 짚는 자리와 그 깊이
        Gizmos.DrawLine(origin, origin + down);

        // 발끝에서 어디까지가 손이 닿는 높이인지, 그리고 그 아래 끝이 어디인지
        Gizmos.DrawLine(feet, feet + Vector3.up * Ledge.reachHigh);
        Gizmos.DrawLine(feet + Vector3.up * Ledge.reachLow, origin + down);
    }

    void DrawLedgeTrace(LedgeGrab.Trace trace, float foot)
    {
        if (trace.hasEdgeCheck)
        {
            // 이 광선은 맞으면 실패다 — 앞에 바닥이 이어진다는 뜻이라 가장자리가 아니다.
            Gizmos.color = trace.edgeBlocked ? GizmoRejected : GizmoAccepted;
            Gizmos.DrawLine(trace.edgeFrom, trace.edgeTo);
            Gizmos.DrawWireCube(trace.edgeTo, Vector3.one * 0.06f);
        }

        foreach (LedgeGrab.Trace.Step step in trace.scan)
        {
            if (step.verdict == LedgeGrab.Trace.Verdict.NoHit)
            {
                Gizmos.color = GizmoNothing;
                Gizmos.DrawLine(step.from, step.to);
                continue;
            }

            bool accepted = step.verdict == LedgeGrab.Trace.Verdict.Accepted;

            Gizmos.color = accepted ? GizmoAccepted : GizmoRejected;
            Gizmos.DrawLine(step.from, step.point);

            if (accepted) Gizmos.DrawSphere(step.point, 0.05f);
            else Gizmos.DrawWireCube(step.point, Vector3.one * 0.08f);

            // 위에 설 자리가 있는지 본 구. 막혀서 거부된 경우에도 그려 이유를 보여준다.
            if (accepted || step.verdict == LedgeGrab.Trace.Verdict.NoClearance)
                Gizmos.DrawWireSphere(Ledge.ClearanceCenter(step.point), Ledge.probeRadius);
        }

        if (trace.hasWall)
        {
            Gizmos.color = trace.wallRejected ? GizmoRejected : GizmoWall;
            Gizmos.DrawLine(trace.wallFrom, trace.wallTo);

            if (trace.wallNormal != Vector3.zero)
            {
                Gizmos.DrawSphere(trace.wallPoint, 0.04f);
                Gizmos.color = GizmoNormal;
                Gizmos.DrawRay(trace.wallPoint, trace.wallNormal * 0.4f);
            }
        }

        if (!trace.found) return;

        bool braced = Ledge.IsBraced(trace.anchor);

        DrawLedgeBody(trace.anchor.hang, trace.anchor.facing, foot, braced ? GizmoBraced : GizmoFree);
        DrawLedgeBody(trace.anchor.stand, trace.anchor.facing, foot, GizmoStand);

        // 발을 디딜 벽면을 찾는 광선. 이것이 Braced와 Freehang을 가른다.
        Vector3 brace = trace.anchor.hang;
        brace.y += Ledge.braceOffset;

        Gizmos.color = braced ? GizmoBraced : GizmoNothing;
        Gizmos.DrawRay(brace, trace.anchor.facing * Vector3.forward * (Ledge.wallOffset + Ledge.braceReach));
    }


    /// <summary>몸 크기의 캡슐. 자리가 맞는지는 점이 아니라 몸으로 봐야 알 수 있다.</summary>
    void DrawLedgeBody(Vector3 origin, Quaternion facing, float foot, Color color)
    {
        float radius = CapsuleCollider != null ? CapsuleCollider.radius * CapsuleCollider.transform.lossyScale.x : 0.25f;
        float height = CharacterHeight > 0f ? CharacterHeight : 1.5f;

        Vector3 bottom = origin + Vector3.up * (radius - foot);
        Vector3 top = origin + Vector3.up * (height - radius - foot);

        Gizmos.color = color;
        Gizmos.DrawWireSphere(bottom, radius);
        Gizmos.DrawWireSphere(top, radius);

        Vector3 ahead = facing * Vector3.forward * radius;
        Vector3 side = facing * Vector3.right * radius;

        Gizmos.DrawLine(bottom + ahead, top + ahead);
        Gizmos.DrawLine(bottom - ahead, top - ahead);
        Gizmos.DrawLine(bottom + side, top + side);
        Gizmos.DrawLine(bottom - side, top - side);

        Gizmos.DrawRay(origin, facing * Vector3.forward * (radius * 1.8f));
    }

    #endregion
}

/// <summary>
/// 점프 클립 하나를 네 구간으로 끊어 쓰기 위한 눈금.
///
/// 클립은 도약 · 상승 · 하강 · 착지를 한 줄에 담고 있는데, 실제 체공 시간은 클립 길이와 무관하다.
/// 그래서 통째로 틀지 않고 <b>구간마다 세워두고 기다린다</b>.
///
///   도약   현재 자세에서 launchFrame으로 섞어 들어가 riseHoldFrame까지 재생
///   상승   riseHoldFrame에 세워둔 채 떠오르는 동안 기다린다
///   하강   내려가기 시작하면 풀어 fallHoldFrame까지 재생
///   착지   fallHoldFrame에 세워두었다가, 착지하고 입력이 없으면 끝까지 재생
///
/// 세우는 것은 배속 파라미터로 한다. AnimatorState의 Speed Multiplier에 걸어두면
/// 0으로 내렸을 때 그 프레임에 멈춘다. normalizedTime은 배속과 무관하게 읽히므로
/// 멈춘 동안에도 진행도를 그대로 볼 수 있다.
/// </summary>
[System.Serializable]
public class JumpAnimation
{
    [Tooltip("컨트롤러의 상태 이름.")]
    public string stateName = "Jump";

    [Tooltip("그 상태의 Speed Multiplier에 걸어둔 Float 파라미터 이름.")]
    public string speedParameter = "JumpSpeed";

    [Tooltip("아래 프레임 값들이 기준으로 삼는 프레임레이트.")]
    [Min(1f)] public float frameRate = 30f;

    [Tooltip("클립의 총 프레임 수.")]
    [Min(1f)] public float totalFrames = 50f;

    [Tooltip("도약 시 섞여 들어갈 프레임. 첫 프레임부터 틀지 않는다.")]
    [Min(0f)] public float launchFrame = 18f;

    [Tooltip("떠오르는 동안 세워둘 프레임.")]
    [Min(0f)] public float riseHoldFrame = 24f;

    [Tooltip("내려가는 동안 세워둘 프레임. 착지 마무리는 여기서 이어진다.")]
    [Min(0f)] public float fallHoldFrame = 31f;

    [Header("완급 - 세우고 푸는 순간을 부드럽게")]
    [Tooltip("세울 프레임까지 이만큼 남았을 때부터 감속을 시작한다. 0이면 감속 없이 그 프레임에서 딱 멈춘다.")]
    [Min(0f)] public float easeFrames = 2f;

    [Tooltip("멈춘 상태에서 제 속도까지 끌어올리는 데 걸리는 시간(초). 0이면 즉시.")]
    [Min(0f)] public float easeInTime = 0.15f;

    [Tooltip("착지 마무리 구간의 배속.")]
    [Min(0.1f)] public float landingSpeed = 2f;

    /// <summary>감속 끝에서 목표 프레임에 닿았다고 볼 여유. 눈에 띄지 않는 거리다.</summary>
    const float SettleEpsilon = 0.05f;

    /// <summary>
    /// 감속 곡선의 지수. 남은 거리 x에 대해 속도가 x^n 꼴로 줄어든다.
    ///
    /// 이 값은 0.5와 1 사이여야 한다. 두 성질을 동시에 만족하는 구간이 거기뿐이다.
    ///   n ≤ 0.5 이면 감속도가 일정하거나 커져, 속도가 기울기를 유지한 채 0에 부딪힌다 — 끝이 각진다.
    ///   n ≥ 1   이면 지수적으로 줄어 목표에 영영 닿지 못한다.
    /// 0.75는 감속도가 끝에서 0으로 잦아들면서도 유한 시간에 내려앉는 지점이다.
    /// </summary>
    const float DecelerationCurve = 0.75f;

    public float LaunchOffset => launchFrame / frameRate;

    /// <summary>뛰지 않고 떨어질 때(발판에서 걸어 나감) 들어갈 지점. 도약 구간을 건너뛴다.</summary>
    public float DropOffset => riseHoldFrame / frameRate;

    /// <summary>
    /// 목표 프레임으로 다가가는 배속.
    ///
    /// 감속은 <b>남은 거리</b>로 건다. 시간으로 걸면 목표를 지나치거나 못 미치는데,
    /// 거리로 걸면 어디서 시작하든 목표에 정확히 내려앉는다.
    /// 곡선의 모양은 <see cref="DecelerationCurve"/>가 정한다.
    ///
    /// 가속은 <b>시간</b>으로 건다. 멈춰 있던 참이라 기준으로 삼을 거리가 없다.
    ///
    /// 둘 중 작은 쪽을 쓴다. 풀자마자 다시 세워야 하는 짧은 구간에서도 튀지 않는다.
    /// </summary>
    public float ApproachSpeed(float frame, float targetFrame, float cruise, float sinceResume)
    {
        float slowing = easeFrames <= 0f
            ? 1f
            : Mathf.Pow(Mathf.Clamp01((targetFrame - frame) / easeFrames), DecelerationCurve);

        return cruise * Mathf.Min(slowing, RiseFactor(sinceResume));
    }

    /// <summary>세울 곳 없이 그냥 풀 때. 가속만 건다.</summary>
    public float ResumeSpeed(float cruise, float sinceResume) => cruise * RiseFactor(sinceResume);

    float RiseFactor(float sinceResume)
    {
        if (easeInTime <= 0f) return 1f;
        return Mathf.SmoothStep(0f, 1f, sinceResume / easeInTime);
    }

    /// <summary>감속이 목표에 내려앉았는지. 여기서 배속을 0으로 못 박는다.</summary>
    public bool Reached(float frame, float targetFrame) => targetFrame - frame <= SettleEpsilon;

    /// <summary>지금 점프 클립이 재생 중이면 그 진행 프레임. 아니면 거짓.</summary>
    public bool TryGetFrame(Character_Animation animation, out float frame)
    {
        frame = 0f;
        if (animation == null || !animation.IsPlaying(stateName, out float normalized)) return false;

        frame = normalized * totalFrames;
        return true;
    }

    /// <summary>
    /// 점프 클립이 착지 직전 자세로 멈춰 있는지.
    /// 이게 참이라는 것은 방금 그 점프로 착지했다는 뜻이라, 착지 신호를 따로 주고받을 필요가 없다.
    /// 공중에서 회피나 스킬이 끼어들어 클립이 갈렸으면 저절로 거짓이 된다.
    /// </summary>
    public bool TailPending(Character_Animation animation)
    {
        return TryGetFrame(animation, out float frame) && frame >= fallHoldFrame;
    }

#if UNITY_EDITOR
    public void Validate(Object owner)
    {
        if (launchFrame < riseHoldFrame && riseHoldFrame < fallHoldFrame && fallHoldFrame < totalFrames) return;

        Debug.LogWarning(
            $"[{owner.name}] 점프 구간의 프레임 순서가 어긋났습니다: " +
            $"도약 {launchFrame} < 상승 {riseHoldFrame} < 하강 {fallHoldFrame} < 전체 {totalFrames} 여야 합니다.", owner);
    }
#endif
}

/// <summary>
/// 턱을 잡고 매달렸다 올라서기 위한 눈금과 탐지.
///
/// 탐지는 캐스터 컴포넌트를 따로 달지 않는다. 전방 탐지는 몸이 도는 대로 방향이 바뀌어야 하는데
/// <see cref="SphereCaster"/>는 방향을 월드 고정으로 들고 있어 회전을 따라오지 못한다.
/// 그래서 원점도 방향도 여기서 몸 기준으로 매번 만든다 — 자식 오브젝트를 배치할 일이 없고,
/// 인스펙터 수치만으로 손이 닿는 범위가 정해진다.
/// </summary>
[System.Serializable]
public class LedgeGrab
{
    [Header("감지")]
    [Tooltip("턱으로 인정할 레이어.")]
    public LayerMask mask = ~0;

    [Tooltip("발끝에서 이 높이까지의 턱을 잡는다. 팔을 뻗은 손끝이라 키보다 조금 높다.")]
    [Min(0f)] public float reachHigh = 1.85f;

    [Tooltip("발끝에서 이 높이까지 내려간 턱도 잡는다. 키보다 낮게 두면 매달리는 순간 몸이 아래로 끌려간다.")]
    [Min(0f)] public float reachLow = 1.5f;

    [Tooltip("몸 앞 이 거리에서 위에서 아래로 짚는다. 크게 잡으면 절벽에서 그만큼 떨어진 채 매달리게 된다.")]
    [Min(0f)] public float reach = 0.4f;

    [Tooltip("윗면 위에 이만큼의 여유가 있어야 올라설 수 있다고 본다.")]
    [Min(0.01f)] public float probeRadius = 0.2f;

    [Tooltip("윗면이 발 디딜 만하다고 볼 최대 경사각.")]
    [Range(0f, 60f)] public float maxSlope = 40f;

    [Header("자세 - 무는 순간 몸을 둘 자리")]
    [Tooltip("벽면에서 몸까지의 거리. 발을 디딘 자세 기준이며, 그 자세의 손이 몸보다 앞에 나온 만큼이다.")]
    public float wallOffset = 0.19f;

    [Tooltip("팔만으로 매달릴 때의 벽면 거리. 팔을 곧게 뻗어 손이 몸보다 뒤에 오므로 음수다.")]
    public float freeWallOffset = -0.048f;

    [Tooltip("턱 윗면에서 몸까지의 낙차.")]
    [Min(0f)] public float hangDrop = 1.55f;

    [Tooltip("매달릴 자리로 모이는 속도(초당 거리). 진입을 손이 닿는 순간에 끊으므로 남는 몫을 이 속도로 좁힌다.")]
    [Min(0.01f)] public float settleSpeed = 2f;

    [Tooltip("올라선 뒤 턱 안쪽으로 들어갈 거리.")]
    [Min(0f)] public float standInset = 0.4f;

    [Header("발 디딤 - Braced인지 Freehang인지")]
    [Tooltip("매달린 몸의 몸통 높이에서 이만큼 위아래로 옮겨 벽을 찾는다. 0이면 몸통 한가운데.")]
    public float braceOffset = 0f;

    [Tooltip("벽면에서 이 거리 안에 면이 있으면 발을 디딜 수 있다고 본다.")]
    [Min(0.01f)] public float braceReach = 0.15f;

    [Header("지상 진입 - 가장자리에서 내려가기")]
    [Tooltip("발 앞 이만큼에 바닥이 없으면 가장자리로 본다.")]
    [Min(0f)] public float edgeAhead = 0.5f;

    [Tooltip("그 앞이 이 깊이까지 비어 있어야 가장자리다. 낮은 턱에서 헛되이 매달리지 않게 한다.")]
    [Min(0f)] public float edgeDepth = 1.5f;

    [Header("벽 점프")]
    [Tooltip("벽을 차고 오르는 수직 속도.")]
    [Min(0f)] public float wallJumpUp = 6f;

    [Tooltip("벽에서 밀려나는 수평 속도.")]
    [Min(0f)] public float wallJumpOut = 4f;

    [Header("동작")]
    [Tooltip("방향키가 이만큼 이상 기울어야 반응한다. 벽 쪽이면 오르고 반대쪽이면 놓는다.")]
    [Range(0f, 1f)] public float pushMargin = 0.5f;

    [Header("애니메이션")]
    [Tooltip("아래 프레임 값들이 기준으로 삼는 프레임레이트.")]
    [Min(1f)] public float frameRate = 30f;

    [Tooltip("발을 디딘 채 매달린 자세. 길이는 쓰지 않는다.")]
    public LedgeClip bracedHang = new LedgeClip("Braced Hanging Idle", 0f);

    [Tooltip("철봉처럼 매달린 자세. 길이는 쓰지 않는다.")]
    public LedgeClip freeHang = new LedgeClip("Free Hanging Idle", 0f);

    [Tooltip("공중에서 발 디딜 턱을 문 순간. 이 동안에도 조작은 받는다 — 길이는 가만히 뒀을 때 보이는 시간일 뿐이다.")]
    public LedgeClip catchBraced = new LedgeClip("Jumping To Braced Hanging", 24f);

    [Tooltip("공중에서 디딜 것 없는 턱을 문 순간.")]
    public LedgeClip catchFree = new LedgeClip("Jump To Free Hang", 24f);

    [Tooltip("가장자리에서 내려가 매달리는 동작. 지상 진입은 언제나 여기부터 시작한다.")]
    public LedgeClip dropToFree = new LedgeClip("Drop To Freehang", 24f);

    [Tooltip("매달린 뒤 발 디딜 것이 있으면 이어서 트는 동작.")]
    public LedgeClip freeToBraced = new LedgeClip("Free Hang To Braced", 16f);

    [Tooltip("발을 디딘 채 올라서기.")]
    public LedgeClip climbBraced = new LedgeClip("Braced Hang To Crouch", 55f);

    [Tooltip("철봉처럼 매달린 채 올라서기.")]
    public LedgeClip climbFree = new LedgeClip("Freehang Climb", 70f);

    [Tooltip("다 올라선 뒤 넘어갈 자세와, 거기까지 섞이는 시간(프레임). 오르기 자세와 크게 다르므로 기본값보다 길게 준다.")]
    public LedgeClip mount = new LedgeClip("Idle", 12f);

    [Tooltip("벽을 차고 뛰기. Braced일 때만 쓴다. 길이는 0 — 붙들지 않고 클립만 걸고 곧바로 공중 축에 넘긴다.")]
    public LedgeClip wallJump = new LedgeClip("Jump From Wall", 0f, 13f);

    // 놓기·벽점프 클립은 여기 없다. 그 둘은 붙들지 않고 곧바로 공중 축에 넘기므로,
    // 재생을 걸어도 공중 축이 제 점프 자세로 곧장 덮는다. 걸 자리가 있다면 이쪽이 아니라 저쪽이다.

    /// <summary>클립 하나와 그 동작을 붙들 길이. 길이는 프레임으로 적고 <see cref="frameRate"/>가 초로 바꾼다.</summary>
    [System.Serializable]
    public struct LedgeClip
    {
        [Tooltip("컨트롤러의 상태 이름.")]
        public string state;

        [Tooltip("이 동작을 붙들 프레임 수.")]
        [Min(0f)] public float frames;

        [Tooltip("클립의 이 프레임부터 튼다. 앞부분이 이미 지나간 자세일 때 건너뛴다.")]
        [Min(0f)] public float start;

        public LedgeClip(string state, float frames, float start = 0f)
        {
            this.state = state;
            this.frames = frames;
            this.start = start;
        }

        public bool IsSet => !string.IsNullOrEmpty(state);
    }

    public float Seconds(float frames) => frameRate <= 0f ? 0f : frames / frameRate;

    public float Seconds(LedgeClip clip) => Seconds(clip.frames);

    public LedgeClip Hang(bool braced) => braced ? bracedHang : freeHang;

    public LedgeClip Climb(bool braced) => braced ? climbBraced : climbFree;


    /// <summary>잡을 자리. 잡는 순간 한 번 계산해 두고 매달린 동안 다시 재지 않는다.</summary>
    /// <summary>
    /// 매달릴 자리에서 되짚은 턱 윗면. <b>손이 여기 닿으면 잡은 것이다.</b>
    ///
    /// 클립마다 손이 놓이는 프레임이 다르다. 프레임 수로 적어두면 클립이 바뀔 때마다 다시 재야 하고,
    /// 재려면 루트 모션을 켜야 한다. 손 위치를 직접 물어 이 높이와 견주면 어느 클립이든 저절로 맞는다.
    /// </summary>
    public float TopOf(Anchor anchor, float footOffset) => anchor.hang.y + hangDrop - footOffset;

    /// <summary>
    /// 붙잡는 지점. 벽면 위 턱 모서리다.
    ///
    /// 높이만으로 판정하면 안 된다 — 진입 클립은 손을 턱 <b>위로 뻗었다가</b> 내려놓아서,
    /// 뻗는 도중에 이미 그 높이를 지난다. 모서리까지의 거리로 봐야 내려놓은 순간이 잡힌다.
    /// </summary>
    public Vector3 GripOf(Anchor anchor, float footOffset)
    {
        Vector3 toWall = anchor.facing * Vector3.forward;

        return anchor.hang + toWall * wallOffset + Vector3.up * (hangDrop - footOffset);
    }

    public struct Anchor
    {
        public Vector3 hang;
        public Vector3 stand;
        public Quaternion facing;
    }

    /// <summary>
    /// 탐지가 지나간 자리. 기즈모가 <b>실제로 도는 코드</b>를 그대로 그리기 위한 것이다.
    ///
    /// 기즈모에 탐지를 다시 짜 넣으면 둘이 갈라진다 — 그림은 멀쩡한데 게임은 틀린 상태를
    /// 눈으로 못 알아보게 되고, 그건 시각화가 없느니만 못하다.
    /// 그리는 쪽은 아무것도 계산하지 않고 여기 담긴 것만 옮긴다.
    /// </summary>
    public sealed class Trace
    {
        public enum Verdict
        {
            NoHit,        // 아무것도 없다
            Inside,       // 출발점이 이미 콜라이더 속이었다
            TooSteep,     // 너무 가파르다
            NoClearance,  // 위에 설 자리가 없다
            Accepted,     // 이것을 윗면으로 삼았다
        }

        public struct Step
        {
            public Vector3 from;
            public Vector3 to;
            public Verdict verdict;
            public Vector3 point;
            public Vector3 normal;
        }

        /// <summary>위에서 아래로 훑은 광선들. 채택된 것이 나오면 거기서 멈추므로 뒤는 비어 있다.</summary>
        public readonly System.Collections.Generic.List<Step> scan = new System.Collections.Generic.List<Step>();

        /// <summary>가장자리인지 보는 광선(지상 진입). 맞으면 바닥이 이어진다는 뜻이라 실패다.</summary>
        public bool hasEdgeCheck;
        public Vector3 edgeFrom, edgeTo;
        public bool edgeBlocked;

        /// <summary>벽면을 찾는 광선.</summary>
        public bool hasWall;
        public Vector3 wallFrom, wallTo, wallPoint, wallNormal;
        public bool wallRejected;

        public bool found;
        public Anchor anchor;

        public void Clear()
        {
            scan.Clear();
            hasEdgeCheck = edgeBlocked = false;
            hasWall = wallRejected = false;
            found = false;
        }
    }

    /// <summary>
    /// 앞에 잡을 턱이 있는지 본다. 두 번 쏘되 <b>위에서 아래로 먼저</b> 쏜다.
    ///
    /// 순서가 이 방향이어야 한다. 벽을 먼저 찾고 그 지점에서 아래로 훑으면
    /// 출발점이 벽 속이 되는데, 그때 유니티는 거리 0과 <b>광선의 반대</b>를 법선으로 답한다.
    /// 아래로 쏜 광선이니 법선은 위 — 경사 검사를 그냥 통과해 벽 한복판이 윗면으로 읽힌다.
    /// 거리를 함께 읽어 걸러낼 수는 있지만, 그건 만들지 않아도 될 모호함을 뒤늦게 가려내는 일이다.
    ///
    /// 위에서 내려오는 광선은 열린 공간에서 시작하므로 그 모호함이 <b>생기지 않는다</b>.
    /// 윗면을 먼저 확정하고, 그 높이에서 앞을 봐 벽면과 법선을 얻는다.
    /// </summary>
    /// <param name="footOffset">몸의 원점에서 발끝까지의 거리. <see cref="CharacterManager.FootOffset"/>.</param>
    public bool Probe(Transform body, float footOffset, out Anchor anchor, Trace trace = null)
    {
        anchor = default;
        trace?.Clear();

        Vector3 forward = CustomMath.RemoveY(body.forward);
        if (forward.sqrMagnitude < 0.0001f) return false;
        forward.Normalize();

        Vector3 feet = body.position;
        feet.y -= footOffset;

        if (!FindTop(feet, forward, out RaycastHit top, trace)) return false;

        // 윗면 바로 아래를 앞으로 훑는다. 몸에서 출발하므로 이 광선도 열린 공간에서 시작한다.
        Vector3 origin = body.position;
        origin.y = top.point.y - EdgeFaceDepth;

        if (trace != null)
        {
            trace.hasWall = true;
            trace.wallFrom = origin;
            trace.wallTo = origin + forward * (reach + probeRadius);
            trace.wallRejected = true;
        }

        if (!Physics.Raycast(origin, forward, out RaycastHit wall, reach + probeRadius, mask, QueryTriggerInteraction.Ignore))
            return false;

        if (trace != null) { trace.wallPoint = wall.point; trace.wallNormal = wall.normal; }

        if (wall.distance <= InsideEpsilon) return false;

        // 수평 성분이 없으면 벽이 아니라 바닥이나 천장을 문 것이다.
        Vector3 normal = CustomMath.RemoveY(wall.normal);
        if (normal.sqrMagnitude < 0.0001f) return false;
        normal.Normalize();

        // 마주보고 있는 면이어야 한다. 비스듬히 스친 면을 잡으면 몸이 엉뚱한 쪽으로 돌아간다.
        if (Vector3.Dot(normal, forward) > -FacingTolerance) return false;

        // 두 자리 모두 수평은 벽면(wall.point)을 기준으로 잡는다.
        // 윗면(top.point)은 모서리 안쪽에서 재어진 것이라, 그쪽을 기준으로 삼으면
        // standInset이 훑는 간격에 딸려 움직인다.
        anchor = Compose(body.position, normal, wall.point, top.point.y, footOffset);

        if (trace != null) { trace.wallRejected = false; trace.found = true; trace.anchor = anchor; }
        return true;
    }

    /// <summary>
    /// 몸 앞을 위에서 아래로 훑어 올라설 윗면을 찾는다.
    /// 가까운 쪽부터 보고 처음 걸리는 것을 쓴다 — 눈앞의 모서리를 두고 그 너머를 잡을 이유가 없다.
    /// </summary>
    /// <summary>아래로 훑는 광선의 길이.</summary>
    public float ScanLength => reachHigh - reachLow;

    /// <summary>
    /// 아래로 훑는 광선의 출발점. 몇 번째 지점인지로 고른다.
    ///
    /// 탐지와 기즈모가 <b>같은 자리</b>를 봐야 하므로 여기 둔다.
    /// 그리는 쪽이 자기 식으로 다시 계산하면 그림과 실제가 조용히 갈라진다.
    /// </summary>
    public Vector3 ScanOrigin(Vector3 feet, Vector3 forward)
    {
        Vector3 origin = feet + forward * reach;
        origin.y = feet.y + reachHigh;
        return origin;
    }

    bool FindTop(Vector3 feet, Vector3 forward, out RaycastHit top, Trace trace = null)
    {
        top = default;

        float span = ScanLength;
        if (span <= 0f) return false;

        Vector3 origin = ScanOrigin(feet, forward);

        Trace.Step step = new Trace.Step
        {
            from = origin,
            to = origin + Vector3.down * span,
            verdict = Trace.Verdict.NoHit,
        };

        bool hitSomething = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, span, mask, QueryTriggerInteraction.Ignore);

        if (hitSomething)
        {
            step.point = hit.point;
            step.normal = hit.normal;

            // 이 높이에서 이미 무언가의 속이라는 뜻이다. 벽이 손 닿는 높이보다 높으면 그렇게 된다.
            if (hit.distance <= InsideEpsilon)
                step.verdict = Trace.Verdict.Inside;

            else if (Vector3.Dot(hit.normal, Vector3.up) < Mathf.Cos(maxSlope * Mathf.Deg2Rad))
                step.verdict = Trace.Verdict.TooSteep;

            // 올라갈 수 없는 곳은 턱이 아니다. 처마 밑이나 벽에 붙은 장식 턱이 여기서 걸린다.
            else if (Physics.CheckSphere(ClearanceCenter(hit.point), probeRadius, mask, QueryTriggerInteraction.Ignore))
                step.verdict = Trace.Verdict.NoClearance;

            else
                step.verdict = Trace.Verdict.Accepted;
        }

        trace?.scan.Add(step);

        if (step.verdict != Trace.Verdict.Accepted) return false;

        top = hit;
        return true;
    }

    /// <summary>
    /// 가장자리 위에 서 있는 몸에게, 앞으로 내려가 매달릴 곳이 있는지 본다.
    ///
    /// 공중 탐지와 반대 방향으로 판단한다. 저쪽은 "앞에 벽이 있고 그 위에 윗면이 있나"이고
    /// 이쪽은 "앞에 바닥이 <b>없고</b>, 그 빈 곳에서 뒤를 돌아보면 벽면이 있나"다.
    /// 딛고 선 면이 곧 턱의 윗면이므로 높이는 다시 잴 것이 없다.
    /// </summary>
    public bool ProbeFromTop(Transform body, float footOffset, out Anchor anchor, Trace trace = null)
    {
        anchor = default;
        trace?.Clear();

        Vector3 forward = CustomMath.RemoveY(body.forward);
        if (forward.sqrMagnitude < 0.0001f) return false;
        forward.Normalize();

        // 딛고 선 면은 몸의 원점이 아니라 발끝 높이다.
        float ground = body.position.y - footOffset;

        Vector3 ahead = body.position + forward * edgeAhead;
        ahead.y = ground + 0.1f;

        if (trace != null)
        {
            trace.hasEdgeCheck = true;
            trace.edgeFrom = ahead;
            trace.edgeTo = ahead + Vector3.down * (edgeDepth + 0.1f);
        }

        // 앞이 비어 있어야 가장자리다. 바닥이 이어지면 그냥 걸어갈 곳이다.
        if (Physics.Raycast(ahead, Vector3.down, edgeDepth + 0.1f, mask, QueryTriggerInteraction.Ignore))
        {
            if (trace != null) trace.edgeBlocked = true;
            return false;
        }

        // 빈 곳에서 턱 바로 아래 높이로 되돌아보면 내가 선 단의 벽면이 잡힌다.
        // 깊이는 얕게 잡는다 — 깊이 내려갈수록 아래가 파여 있는 지형에서 벽면을 놓친다.
        Vector3 back = ahead - Vector3.up * (0.1f + EdgeFaceDepth);

        if (trace != null)
        {
            trace.hasWall = true;
            trace.wallFrom = back;
            trace.wallTo = back - forward * (edgeAhead + probeRadius);
            trace.wallRejected = true;
        }

        if (!Physics.Raycast(back, -forward, out RaycastHit wall, edgeAhead + probeRadius, mask, QueryTriggerInteraction.Ignore))
            return false;

        if (trace != null) { trace.wallPoint = wall.point; trace.wallNormal = wall.normal; }

        // 공중 탐지와 같은 이유로 거리를 함께 읽는다. 되돌아보는 지점이 벽 속이었다면
        // 유니티는 거리 0에 광선의 반대를 법선으로 답해, 있지도 않은 벽면을 지어낸다.
        if (wall.distance <= InsideEpsilon) return false;

        Vector3 normal = CustomMath.RemoveY(wall.normal);
        if (normal.sqrMagnitude < 0.0001f) return false;
        normal.Normalize();

        // 몸을 돌려 벽을 마주보게 된다. 딛고 선 면이 그대로 턱의 윗면이다.
        anchor = Compose(body.position, normal, wall.point, ground, footOffset);

        if (trace != null) { trace.wallRejected = false; trace.found = true; trace.anchor = anchor; }
        return true;
    }

    /// <summary>
    /// 윗면 위에 설 자리가 있는지 볼 구의 중심.
    ///
    /// 면에서 <b>살짝 띄운다.</b> 정확히 반경만큼만 올리면 구가 그 면에 접하는데,
    /// 접한 상태는 부동소수점이 어느 쪽으로 떨어지느냐에 달린 값이다 —
    /// 어떤 좌표에서는 통과하고 어떤 좌표에서는 <b>자기가 딛고 선 면에 막혔다</b>고 답한다.
    /// 같은 모양의 턱인데 자리에 따라 잡히고 안 잡히던 것이 이것이었다.
    /// </summary>
    public Vector3 ClearanceCenter(Vector3 top) => top + Vector3.up * (SurfaceSkin + probeRadius);

    /// <summary>면에 접하지 않을 만큼만 띄우는 여유. 눈에 띄지 않는 거리다.</summary>
    const float SurfaceSkin = 0.02f;

    /// <summary>훑기 시작한 지점이 이미 콜라이더 속이었다고 볼 거리.</summary>
    const float InsideEpsilon = 0.001f;

    /// <summary>마주보고 있다고 볼 최소 정렬도. 1이면 정면만, 0이면 스치는 면까지 받는다.</summary>
    const float FacingTolerance = 0.5f;

    /// <summary>가장자리 아래 벽면을 찾을 깊이. 얕게 둔다.</summary>
    const float EdgeFaceDepth = 0.25f;

    /// <summary>
    /// 발을 디딜 벽면이 있는지. Braced와 Freehang을 가르는 유일한 판정이다.
    ///
    /// 재는 높이는 <b>매달린 발끝이 아니라 몸통</b>이다. Braced Hang은 무릎을 접어
    /// 발바닥을 벽에 붙이는 자세라, 발이 닿는 자리가 평상시 몸통쯤에 온다.
    /// 늘어뜨린 발 높이에서 재면 실제로 디딜 곳보다 한참 아래를 보게 된다.
    ///
    /// 몸통은 곧 <see cref="Anchor.hang"/>(몸의 원점)이라 따로 환산할 것이 없다 —
    /// 캐릭터 키가 바뀌어도 원점이 캡슐 중앙에 있는 한 저절로 따라온다.
    /// </summary>
    public bool IsBraced(Anchor anchor)
    {
        // facing이 벽을 보고 있으므로 그 앞이 곧 벽 쪽이다.
        Vector3 toWall = anchor.facing * Vector3.forward;

        Vector3 origin = anchor.hang;
        origin.y += braceOffset;

        return Physics.Raycast(origin, toWall, wallOffset + braceReach, mask, QueryTriggerInteraction.Ignore);
    }

    /// <param name="topHeight">턱 윗면의 높이. <b>발끝이 닿을 자리</b>이지 몸의 원점이 올 자리가 아니다.</param>
    /// <param name="footOffset">원점에서 발끝까지의 거리. 두 자리 모두 여기서 원점 높이로 환산된다.</param>
    Anchor Compose(Vector3 current, Vector3 normal, Vector3 wallPoint, float topHeight, float footOffset)
    {
        // 두 자리 모두 수평은 벽면을 기준으로 잡는다.
        // 윗면을 기준으로 삼으면 그것을 어디서 쟀는지(탐지 구 크기, 가장자리 여유)에 딸려 움직인다.
        float wallPlane = Vector3.Dot(wallPoint, normal);

        return new Anchor
        {
            facing = Quaternion.LookRotation(-normal, Vector3.up),
            hang = Place(current, normal, wallPlane + wallOffset, topHeight - hangDrop + footOffset),
            stand = Place(current, normal, wallPlane - standInset, topHeight + footOffset),
        };
    }

    /// <summary>
    /// 자세가 정해진 뒤 매달릴 자리를 그 자세의 손 위치로 옮긴다.
    ///
    /// 두 자세는 손이 몸에 대해 놓이는 곳이 다르다 — 발을 디딘 쪽은 손이 0.19 앞,
    /// 팔만으로 매달린 쪽은 0.05 뒤다. 한 값으로 두면 한쪽은 반드시 손이 벽에서 뜬다.
    ///
    /// <b>Compose가 준 것을 그대로 넘겨야 한다.</b> 이미 옮긴 것을 또 넘기면 두 번 옮겨진다.
    /// </summary>
    public Anchor Pose(Anchor anchor, bool braced)
    {
        if (braced) return anchor;

        Vector3 normal = -(anchor.facing * Vector3.forward);
        anchor.hang += normal * (freeWallOffset - wallOffset);

        return anchor;
    }

    /// <summary>
    /// 법선 방향 성분과 높이만 갈아끼우고, <b>턱을 따라가는 가로 성분은 지금 자리를 그대로 둔다.</b>
    ///
    /// 2D 모드에서는 벽 법선이 평면 안에 있어 가로 성분이 곧 Z다. 그러니 이렇게 두면
    /// 잡는 순간에도 오르는 동안에도 평면을 벗어나지 않는다 — Dimensional에 물을 일이 없다.
    /// 위치 대입은 FreezePositionZ를 무시하고 순간이동시키므로, 물어봤어야 할 곳이다.
    /// </summary>
    static Vector3 Place(Vector3 current, Vector3 normal, float alongNormal, float height)
    {
        Vector3 placed = current + normal * (alongNormal - Vector3.Dot(current, normal));
        placed.y = height;
        return placed;
    }

#if UNITY_EDITOR
    /// <param name="height">캐릭터의 키. 잡을 높이대가 그보다 위에 있어야 한다.</param>
    public void Validate(Object owner, float height)
    {
        if (reachLow >= reachHigh)
            Debug.LogWarning(
                $"[{owner.name}] 잡을 수 있는 높이대가 비어 있습니다: " +
                $"reachLow {reachLow} < reachHigh {reachHigh} 여야 합니다.", owner);

        // 매달리면 발이 턱에서 낙차만큼 아래로 간다. 잡은 턱이 그보다 낮으면
        // 매달리는 것이 곧 내려가는 것이 된다 — 떨어지던 몸을 붙잡아 더 아래로 끌어내린다.
        // 손이 닿는다고 매달릴 이유가 있는 건 아니다. 그 높이는 아예 훑지 않는 것이 옳다.
        if (height > 0f && reachLow < height)
            Debug.LogWarning(
                $"[{owner.name}] 매달릴 때 몸이 아래로 끌려갑니다: " +
                $"reachLow {reachLow} ≥ 키 {height} 여야 합니다. " +
                $"그보다 낮은 턱은 매달릴 곳이 아니라 그냥 올라설 곳입니다.", owner);
    }
#endif
}
