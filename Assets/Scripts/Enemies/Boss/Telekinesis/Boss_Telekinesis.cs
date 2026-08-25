using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 바닥의 물건을 끌어올려 들고 있는 스킬들의 공통 골격.
///
/// 두 파생이 공유하는 것은 "무엇을 몇 개 집어 어디에 띄워 두는가"까지다.
/// 그 뒤에 던지느냐 돌리느냐는 파생이 정한다.
///
/// 집는 규칙은 하나뿐이다 — <see cref="Boss_Throwable.Available"/>한 것 중 가까운 순서.
/// 하나도 못 집으면 스킬이 성립하지 않으므로 <see cref="IsReady"/>가 거짓이 된다.
/// 사거리와 쿨다운만 보고 골랐다가 빈손으로 시전하는 일을 여기서 막는다.
///
/// <b>거리는 보지 않는다.</b> 예전에는 보스 반경 안의 것만 집었는데, 회수는 창이 박힌 그 자리에서
/// 이뤄지므로(<see cref="Boss_Throwable"/>) 멀리 빗나가 박힌 창은 다시 쓸 수 있게 된 뒤에도
/// 영영 집히지 않았다. 그런 것이 몇 개 쌓이면 최소 개수 아래로 떨어져 스킬이 통째로 사라진다.
/// 염력에 사거리를 두는 대신 그 경로를 없앴다 — 창이 놓인 곳은 보스 아레나뿐이고
/// 배경은 한 번에 하나만 올라오므로, 남의 물건을 집을 일이 없다.
/// </summary>
public abstract class Boss_Telekinesis : Boss_SkillBase
{
    [Header("Telekinesis - 무엇을 몇 개 집을지")]
    [Tooltip("한 번에 끌어올릴 개수. 주변에 이보다 적으면 있는 만큼만 쓴다.")]
    [SerializeField, Min(1)] int _count = 5;

    [Tooltip("이보다 적게 모이면 이 스킬을 쓰지 않는다.")]
    [SerializeField, Min(1)] int _minimumCount = 2;

    [Header("Telekinesis - 어디에 띄울지")]
    [Tooltip("보스를 기준으로 물건들이 도는 고리의 반지름.")]
    [SerializeField, Min(0.5f)] float _ringRadius = 2.5f;

    [Tooltip("보스 발치에서 고리까지의 높이.")]
    [SerializeField] float _ringHeight = 1.5f;

    [Tooltip("자리로 따라붙는 빠르기. 클수록 딱딱 붙는다.")]
    [SerializeField, Min(0.1f)] float _followSpeed = 8f;

    protected readonly List<Boss_Throwable> held = new List<Boss_Throwable>();

    /// <summary>
    /// 지금 물건을 들고 있는 스킬. 되날아온 창이 합류할 고리를 여기서 찾는다.
    ///
    /// 하나뿐이어도 되는 것은 보스가 한 번에 한 스킬만 시전하기 때문이다.
    /// 창이 스킬을 거슬러 올라가 찾게 두면 보스 - 상태기계 - 스킬을 훑어야 하는데,
    /// 그 길은 창이 알아야 할 것이 아니다.
    /// </summary>
    public static Boss_Telekinesis Holding { get; private set; }

    /// <summary>고리가 지금 몇 도 돌아가 있는지. 파생이 굴린다.</summary>
    protected float ringAngle;

    protected float RingRadius => _ringRadius;
    protected float RingHeight => _ringHeight;

    /// <summary>한 번에 집기로 한 최대 개수. 실제로 몇 개가 모일지는 씬에 달렸다.</summary>
    protected int MaxCount => _count;

    /// <summary>던지는 쪽의 뿌리. 날아가는 물건이 이 아래 있는 것은 때리지 않는다.</summary>
    Transform Thrower => manager != null ? manager.character : null;

    #region Selection

    /// <summary>
    /// 쿨다운·사거리·조건에 더해, 실제로 집을 것이 있는지까지 본다.
    /// 이게 없으면 물건이 다 소진된 뒤에도 계속 골라져 빈손으로 시전한다.
    /// </summary>
    public override bool IsReady(BossManager boss)
    {
        if (!base.IsReady(boss)) return false;

        return CountAvailable(boss) >= _minimumCount;
    }

    int CountAvailable(BossManager boss)
    {
        if (boss == null) return 0;

        int n = 0;

        foreach (Boss_Throwable t in Boss_Throwable.All)
        {
            if (t == null || !t.Available) continue;
            n++;
        }

        return n;
    }

    #endregion

    public override void Enter()
    {
        held.Clear();
        ringAngle = 0f;

        Holding = this;

        Gather();
        base.Enter();
    }

    public override void Exit()
    {
        // 끊겼든 끝났든 들고 있던 것을 남기지 않는다. 남기면 영영 집을 수 없는 물건이 된다.
        foreach (Boss_Throwable t in held) if (t != null) t.Cancel();
        held.Clear();

        // 다음 스킬이 이미 들어와 있으면 그쪽을 지우지 않는다.
        if (Holding == this) Holding = null;

        base.Exit();
    }

    /// <summary>
    /// 되날아온 창을 고리에 받아들인다.
    ///
    /// 자리는 따로 만들지 않는다 — <see cref="SlotOf"/>가 개수로 나누므로,
    /// 목록에 들어가는 순간 고리가 한 칸씩 벌어지며 제 자리가 생긴다.
    /// 던질 때도 목록을 그대로 훑으므로 같이 나간다.
    /// </summary>
    /// <returns>실제로 받아들였는지.</returns>
    public virtual bool Absorb(Boss_Throwable throwable)
    {
        if (throwable == null || held.Contains(throwable)) return false;

        throwable.Grab(Thrower);

        // 집히지 않았다면 이미 다른 국면에 있다는 뜻이다. 목록에 넣으면 자리 지정이 어긋난다.
        if (throwable.phase != Boss_Throwable.Phase.Held) return false;

        held.Add(throwable);
        return true;
    }

    /// <summary>가까운 순서로 집는다.</summary>
    void Gather()
    {
        if (manager == null || manager.character == null) return;

        Vector3 origin = manager.character.position;

        List<Boss_Throwable> candidates = new List<Boss_Throwable>();

        foreach (Boss_Throwable t in Boss_Throwable.All)
        {
            if (t == null || !t.Available) continue;
            candidates.Add(t);
        }

        candidates.Sort((a, b) =>
            (a.transform.position - origin).sqrMagnitude.CompareTo(
            (b.transform.position - origin).sqrMagnitude));

        int take = Mathf.Min(_count, candidates.Count);

        for (int i = 0; i < take; i++)
        {
            candidates[i].Grab(Thrower);
            held.Add(candidates[i]);
        }
    }

    #region Ring

    /// <summary>
    /// 고리에서 i번째 물건이 설 자리. 개수에 맞춰 균등하게 벌린다.
    /// 파생이 모양을 바꿀 수 있게 열어둔다 — HoldRing이 이걸 통해 자리를 묻는다.
    /// </summary>
    protected virtual Vector3 SlotOf(int index, int total)
    {
        if (manager == null || manager.character == null) return Vector3.zero;
        if (total <= 0) total = 1;

        float step = 360f / total;
        float angle = (ringAngle + step * index) * Mathf.Deg2Rad;

        Vector3 centre = manager.character.position + Vector3.up * _ringHeight;

        return centre + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _ringRadius;
    }

    /// <summary>
    /// 들고 있는 것들을 고리 자리로 보낸다.
    /// </summary>
    /// <param name="exact">
    /// 참이면 계산한 자리에 <b>정확히</b> 둔다. 거짓이면 그 자리로 부드럽게 따라붙는다.
    ///
    /// 도는 동안에는 참이어야 한다. 보간으로만 따라가면 궤도가 회전 속도만큼 안쪽으로 처져,
    /// 인스펙터에 적은 반지름과 실제로 도는 반지름이 달라진다 —
    /// 반지름 6m를 180도/초로 돌리면 접선 속도가 초당 19m라, 따라붙기로는 2m 넘게 뒤진다.
    /// 바닥에서 끌어올리는 동안에만 거짓으로 두어 딸려 오는 모습을 보인다.
    /// </param>
    protected void HoldRing(bool exact)
    {
        for (int i = 0; i < held.Count; i++)
        {
            // 지금 들고 있는 것만 배치한다.
            // 겨누러 나간 창과 이미 날아간 창은 제 자리를 스스로 정하므로, 여기서 끌어오면 서로를 덮어쓴다.
            if (held[i] == null || held[i].phase != Boss_Throwable.Phase.Held) continue;

            Vector3 slot = SlotOf(i, held.Count);

            if (exact) held[i].SnapTo(slot);
            else held[i].SetSlot(slot, _followSpeed);
        }
    }

    protected void HoldRing() => HoldRing(false);

    /// <summary>고리를 즉시 짠다. 진입 순간 미끄러져 모이지 않고 제자리에서 시작할 때.</summary>
    protected void SnapRing()
    {
        for (int i = 0; i < held.Count; i++)
        {
            if (held[i] == null || held[i].phase != Boss_Throwable.Phase.Held) continue;
            held[i].SnapTo(SlotOf(i, held.Count));
        }
    }

    #endregion
}
