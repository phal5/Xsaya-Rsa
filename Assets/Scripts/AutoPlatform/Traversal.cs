using System.Collections.Generic;
using UnityEngine;

/// <summary>한 번의 공중 이동에서 쓰는 자원. 착지하면 전부 되돌아온다.</summary>
[System.Flags]
public enum Ability
{
    None = 0,

    /// <summary>지상 점프. 티켓을 쓰지 않는다 — Character_Ground가 속도만 싣는다.</summary>
    GroundJump = 1 << 0,

    /// <summary>공중 점프. Character_Airborne이 뜰 때마다 한 장 준다.</summary>
    AirJump = 1 << 1,

    /// <summary>공중 대시. 역시 한 장. 중력을 끄고 세로 속도를 지운다.</summary>
    Dash = 1 << 2,

    /// <summary>
    /// 공중 점프를 <b>반대 방향</b>으로 건다.
    ///
    /// 수평 속도는 공중에서도 즉시 뒤집힌다(가속 100). 그래서 나갔다가 되돌아올 수 있고,
    /// 그 덕에 <b>제자리 바로 위</b>나 <b>뒤쪽 위</b>의 발판에 오를 수 있다 —
    /// 방향을 고정한 채로는 절대 닿지 못하는 자리다.
    /// 나를 덮고 있는 더 큰 발판 위로 오르는 것도 이것이다: 밖으로 나갔다가 위로 돌아 들어온다.
    /// </summary>
    Reverse = 1 << 3,
}

/// <summary>구간을 넘는 방식의 큰 갈래.</summary>
public enum MoveKind
{
    /// <summary>구간을 풀어서 가장 싼 수단을 찾는다.</summary>
    Auto,

    /// <summary>바닥이 이어져 있다. 빌더는 두 발판을 하나로 붙인다.</summary>
    Ground,

    /// <summary>걸어 나가 떨어지기만 하면 된다.</summary>
    Drop,

    /// <summary>공중 이동. 무엇을 쓰는지는 <see cref="Traversal.abilities"/>에 있다.</summary>
    Air,

    /// <summary>발로는 못 딛고 턱을 물어야 오른다.</summary>
    Ledge,

    /// <summary>어떤 수단으로도 닿지 않는다.</summary>
    Unreachable,
}

/// <summary>한 구간을 어떻게 넘는지에 대한 판정 결과.</summary>
public struct Traversal
{
    public MoveKind kind;
    public Ability abilities;

    /// <summary>요구 / 한계. 1을 넘으면 못 넘는다. 0.75 아래면 넉넉하고, 그 위는 타이트하다.</summary>
    public float load;

    /// <summary>도달 한계선까지 남은 거리(m). 음수면 모자란 양이다.</summary>
    public float margin;

    /// <summary>이 수단으로 갈 수 있는 최대 수평 거리.</summary>
    public float limit;

    public string note;

    public bool Possible => kind != MoveKind.Unreachable;

    public Color Color
    {
        get
        {
            if (!Possible) return new Color(1f, 0.25f, 0.25f);
            if (load > 0.95f) return new Color(1f, 0.45f, 0.15f);
            if (load > TraversalSolver.ComfortableLoad) return new Color(1f, 0.85f, 0.2f);
            return new Color(0.35f, 0.95f, 0.5f);
        }
    }

    public string AbilityLabel
    {
        get
        {
            switch (kind)
            {
                case MoveKind.Ground: return "지면";
                case MoveKind.Drop: return "낙하";
                case MoveKind.Unreachable: return "불가";
            }

            string name = (abilities & Ability.Reverse) != 0
                ? "되돌아 점프"
                : (abilities & Ability.AirJump) != 0 ? "2단 점프" : "점프";

            if ((abilities & Ability.Dash) != 0) name += "+대시";
            if (kind == MoveKind.Ledge) name += " · 턱 잡기";

            return name;
        }
    }
}

/// <summary>
/// 공중에서 실제로 어디까지 갈 수 있는지를 <b>적분해서</b> 재는 계산부.
///
/// 닫힌 식을 쓰지 않는다. 이 캐릭터의 능력은 서로 간섭하고, 최적 타이밍이 직관과 어긋난다.
///   · 점프는 세로 속도를 +v로 <b>덮어쓴다</b>. 그래서 높은 데서 쓰면 남은 낙하 시간을 버리는 셈이 되고,
///     가장 낮은 지점에서 써야 체공이 최대가 된다.
///   · 대시는 중력을 끄고 세로 속도를 <b>0으로 지운다</b>. 그래서 반대로 한창 떨어지는 중이 아니라
///     중간 높이에서 걸어야 지워지는 속도와 벌어지는 시간의 합이 최대가 된다.
/// 두 최적점이 서로 다른 곳에 있어서, 손으로 유도한 식은 번번이 실제보다 짧게 잡는다.
///
/// 그래서 타이밍을 격자로 훑고 국소 세밀화로 다듬는다. 결과는 프로파일이 바뀔 때까지 캐시한다.
/// </summary>
public static class MotionEnvelope
{
    /// <summary>타이밍 격자의 첫 간격(초). 이후 두 번 세밀화한다.</summary>
    const float COARSE_STEP = 0.04f;

    const int REFINE_PASSES = 3;

    /// <summary>세밀화할 때 이전 격자 간격의 몇 배까지 되돌아볼지. 1.0이면 바로 옆 골짜기를 놓친다.</summary>
    const float REFINE_WINDOW = 1.5f;

    /// <summary>착지로 볼 최소 경과 시간. 발을 떼는 순간을 착지로 읽지 않기 위한 것.</summary>
    const float LIFTOFF_EPSILON = 1e-4f;

    /// <summary>
    /// 착지하지 못했다는 표시. 0이나 음수를 쓸 수 없다 —
    /// 되돌아 점프가 들어오면서 착지 변위 자체가 음수일 수 있게 됐다.
    /// </summary>
    public const float NO_LANDING = float.NegativeInfinity;

    /// <summary>되돌아 점프까지 포함해 싼 것부터 늘어놓은 수단들.</summary>
    public static readonly Ability[] Plans =
    {
        Ability.GroundJump,
        Ability.GroundJump | Ability.AirJump,
        Ability.GroundJump | Ability.Dash,
        Ability.GroundJump | Ability.AirJump | Ability.Dash,
        Ability.GroundJump | Ability.AirJump | Ability.Reverse,
        Ability.GroundJump | Ability.AirJump | Ability.Dash | Ability.Reverse,
    };

    static readonly Dictionary<long, float> _cache = new Dictionary<long, float>();

    /// <summary>
    /// <paramref name="dy"/>만큼의 높이차를 두고 착지할 때 갈 수 있는 최대 수평 거리.
    /// 닿지 못하면 음수.
    /// </summary>
    public static float MaxRange(MotionProfile p, Ability abilities, float dy)
    {
        long key = Key(p, abilities, dy);
        if (_cache.TryGetValue(key, out float cached)) return cached;

        float best = Optimize(p, abilities, dy, false, out _, out _);
        if (best == NO_LANDING) best = -1f;

        _cache[key] = best;
        return best;
    }

    /// <summary>최적 타이밍까지 함께 돌려준다. 기즈모가 실제로 그 궤적을 그리기 위한 것.</summary>
    public static float MaxRange(MotionProfile p, Ability abilities, float dy,
        out float jumpAt, out float dashAt)
        => Optimize(p, abilities, dy, false, out jumpAt, out dashAt);

    /// <summary>
    /// 같은 높이차에 <b>가장 짧게</b> 닿는 거리.
    ///
    /// 방향 키를 놓지 않는 한 수평 속도는 언제나 최대다. 그래서 점프를 짧게 만들 방법이 없고,
    /// 목표가 이 거리보다 가까우면 밟지 못하고 지나쳐 버린다. 도달 여부가 상한만의 문제가 아닌 이유다.
    ///
    /// 대시와 공중 점프는 둘 다 체공을 늘리기만 하므로, 줄이는 쪽으로는 쓸모가 없다 —
    /// 높이가 모자라 어쩔 수 없이 써야 할 때만 센다.
    /// </summary>
    public static float MinRange(MotionProfile p, float dy)
    {
        long key = Key(p, (Ability)(-1), dy);
        if (_cache.TryGetValue(key, out float cached)) return cached;

        float best = float.MaxValue;

        // 아래로 가는 것이면 뛰지 않고 걸어 나가는 것이 가장 짧다.
        if (dy < 0f)
        {
            float drift = Optimize(p, Ability.None, dy, true, out _, out _);
            if (drift != NO_LANDING) best = drift;
        }

        // 되돌아 점프는 세지 않는다 — 그것은 방향을 놓는 기술이고, 여기서 재는 것은
        // 방향을 <b>쥔 채로</b> 낼 수 있는 가장 짧은 거리다.
        foreach (Ability plan in new[] { Ability.GroundJump, Ability.GroundJump | Ability.AirJump })
        {
            float range = Optimize(p, plan, dy, true, out _, out _);
            if (range != NO_LANDING && range < best) best = range;
        }

        float result = best == float.MaxValue ? -1f : best;
        _cache[key] = result;
        return result;
    }

    public static void ClearCache() => _cache.Clear();

    #region 탐색

    /// <summary>
    /// 이 수단으로 닿을 수 있는 착지 변위의 <b>구간</b>.
    ///
    /// 상한 하나로는 부족하다. 방향 키를 놓지 않으면 짧게 뛸 수 없어 하한이 생기고,
    /// 되돌아 점프를 쓰면 그 하한이 음수까지 내려간다 — 뒤쪽 위에 있는 발판이 그때 열린다.
    /// </summary>
    /// <returns>닿을 수 있는 자리가 아예 없으면 거짓.</returns>
    public static bool Reach(MotionProfile p, Ability abilities, float dy, out float lo, out float hi)
    {
        lo = Optimize(p, abilities, dy, true, out _, out _);
        hi = Optimize(p, abilities, dy, false, out _, out _);

        return lo != NO_LANDING && hi != NO_LANDING;
    }

    static float Optimize(MotionProfile p, Ability abilities, float dy, bool minimize,
        out float jumpAt, out float dashAt)
    {
        bool usesJump = (abilities & Ability.AirJump) != 0;
        bool usesDash = (abilities & Ability.Dash) != 0;

        float horizon = Horizon(p, dy);

        jumpAt = 0f;
        dashAt = 0f;

        float best = Evaluate(p, abilities, dy, 0f, 0f);

        float step = COARSE_STEP;
        float jumpLow = 0f, jumpHigh = usesJump ? horizon : 0f;
        float dashLow = 0f, dashHigh = usesDash ? horizon : 0f;

        for (int pass = 0; pass <= REFINE_PASSES; pass++)
        {
            for (float tj = jumpLow; tj <= jumpHigh + 1e-4f; tj += step)
            {
                for (float td = dashLow; td <= dashHigh + 1e-4f; td += step)
                {
                    float range = Evaluate(p, abilities, dy, tj, td);
                    if (range == NO_LANDING) continue;

                    bool better = best == NO_LANDING || (minimize ? range < best : range > best);
                    if (!better) continue;

                    best = range;
                    jumpAt = tj;
                    dashAt = td;
                }
            }

            // 찾은 자리 둘레만 좁혀 다시 훑는다.
            float window = step * REFINE_WINDOW;
            jumpLow = Mathf.Max(0f, jumpAt - window);
            jumpHigh = usesJump ? jumpAt + window : 0f;
            dashLow = Mathf.Max(0f, dashAt - window);
            dashHigh = usesDash ? dashAt + window : 0f;
            step *= 0.3f;
        }

        return best;
    }

    /// <summary>타이밍을 훑을 시간 범위. 두 번 뛰고 한 번 대시해도 이 안에서 끝난다.</summary>
    static float Horizon(MotionProfile p, float dy)
    {
        float drop = Mathf.Max(0f, p.ApexHeight * 2f - dy);
        return 4f * p.jumpSpeed / p.gravity + p.dashTime + Mathf.Sqrt(2f * drop / p.gravity) + 0.5f;
    }

    #endregion

    #region 적분

    /// <summary>
    /// 한 번의 공중 이동을 끝까지 적분한다. 구간마다 닫힌 식으로 건너뛰되, 사건 사이에서만 그렇게 한다.
    /// </summary>
    /// <returns>
    /// 착지 지점의 <b>부호 있는</b> 수평 변위. 처음 뛴 방향이 양이고, 되돌아 착지하면 음수가 된다.
    /// 착지하지 못하면 <see cref="NO_LANDING"/>.
    /// </returns>
    static float Evaluate(MotionProfile p, Ability abilities, float dy, float jumpAt, float dashAt)
    {
        bool usesJump = (abilities & Ability.AirJump) != 0;
        bool usesDash = (abilities & Ability.Dash) != 0;

        // 대시 중에 점프를 걸면 대시가 세로 속도를 계속 눌러 버린다. 겹치는 안은 세지 않는다.
        if (usesJump && usesDash && jumpAt > dashAt && jumpAt < dashAt + p.dashTime) return NO_LANDING;

        float t = 0f;
        float x = 0f;
        float y = 0f;
        float vy = (abilities & Ability.GroundJump) != 0 ? p.jumpSpeed : 0f;

        // 수평 방향. 되돌아 점프는 공중 점프를 거는 순간 이것을 뒤집는다.
        float sign = 1f;

        bool jumpDone = !usesJump;
        bool dashDone = !usesDash;

        for (int guard = 0; guard < 8; guard++)
        {
            float next = float.MaxValue;
            if (!jumpDone) next = Mathf.Min(next, jumpAt);
            if (!dashDone) next = Mathf.Min(next, dashAt);

            float span = next == float.MaxValue ? Horizon(p, dy) * 2f : Mathf.Max(0f, next - t);

            if (Fall(p, span, dy, sign, ref t, ref x, ref y, ref vy, out float landed)) return landed;

            if (next == float.MaxValue) return NO_LANDING;

            bool jumpNow = !jumpDone && Mathf.Approximately(next, jumpAt);
            bool dashNow = !dashDone && Mathf.Approximately(next, dashAt);

            // 같은 순간이면 대시가 나중이다 — 점프로 실은 속도를 대시가 지워버리므로 그 편이 손해가 없다.
            if (jumpNow)
            {
                vy = p.jumpSpeed;
                if ((abilities & Ability.Reverse) != 0) sign = -sign;
                jumpDone = true;
                continue;
            }

            if (dashNow)
            {
                // 중력이 꺼지고 세로 속도가 지워진다. 그동안 수평은 지금 향한 쪽으로 대시 속도로 나간다.
                x += sign * p.dashSpeed * p.dashTime;
                t += p.dashTime;
                vy = 0f;
                dashDone = true;

                // 여기서 "이미 목표 높이 아래인가"를 보고 착지로 치면 안 된다.
                // 애초에 닿지 못하는 높이를 요구받았을 때도 그 조건이 참이라, 없는 착지를 만들어낸다.
                // 착지 판정은 Fall 하나에만 맡긴다.
            }
        }

        return NO_LANDING;
    }

    /// <summary>자유 낙하 구간을 <paramref name="span"/>초만큼 진행한다. 도중에 착지하면 참.</summary>
    static bool Fall(MotionProfile p, float span, float dy, float sign,
        ref float t, ref float x, ref float y, ref float vy, out float landed)
    {
        landed = 0f;
        if (span <= 0f) return false;

        float g = p.gravity;

        // ½gτ² − vy·τ + (dy − y) = 0 의 하강 쪽 근.
        float discriminant = vy * vy + 2f * g * (y - dy);

        if (discriminant >= 0f)
        {
            float touch = (vy + Mathf.Sqrt(discriminant)) / g;

            if (touch >= 0f && touch <= span && t + touch > LIFTOFF_EPSILON)
            {
                landed = x + sign * p.airborneSpeed * touch;
                return true;
            }
        }

        y += vy * span - 0.5f * g * span * span;
        vy -= g * span;
        x += sign * p.airborneSpeed * span;
        t += span;

        return false;
    }

    #endregion

    #region 캐시 키

    static long Key(MotionProfile p, Ability abilities, float dy)
    {
        unchecked
        {
            long hash = p.Signature;
            hash = hash * 397 + (int)abilities;
            hash = hash * 397 + Mathf.RoundToInt(dy * 100f);
            return hash;
        }
    }

    #endregion
}

/// <summary>
/// 발 위치 두 개를 받아 그 사이를 무엇으로 넘는지 푸는 판정부.
///
/// 도달 여부는 전부 <see cref="MotionEnvelope"/>가 잰다. 여기서는 가장 싼 수단부터 차례로 물어볼 뿐이다.
/// </summary>
public static class TraversalSolver
{
    /// <summary>수평 도달 한계에서 이만큼은 남겨두고 "넉넉하다"고 부른다.</summary>
    public const float ComfortableLoad = 0.75f;

    /// <summary>같은 높이로 볼 오차. 발판 두께보다 작게 잡는다.</summary>
    const float LEVEL_EPSILON = 0.05f;

    /// <summary>싼 것부터. 앞에 있을수록 플레이어에게 요구하는 것이 적다.</summary>
    static readonly Ability[] Plans =
    {
        Ability.GroundJump,
        Ability.GroundJump | Ability.AirJump,
        Ability.GroundJump | Ability.Dash,
        Ability.GroundJump | Ability.AirJump | Ability.Dash,
    };

    public static float JumpRange(MotionProfile p, float dy)
        => MotionEnvelope.MaxRange(p, Ability.GroundJump, dy);

    public static float FullRange(MotionProfile p, float dy)
        => MotionEnvelope.MaxRange(p, Ability.GroundJump | Ability.AirJump | Ability.Dash, dy);

    /// <summary>
    /// 발 위치 <paramref name="from"/>에서 <paramref name="to"/>로 가는 최소 수단을 찾는다.
    /// XY 평면만 본다 — 주인공은 언제나 Z=0에 있다.
    /// </summary>
    public static Traversal Solve(MotionProfile p, Vector3 from, Vector3 to, MoveKind forced = MoveKind.Auto)
    {
        float dx = Mathf.Abs(to.x - from.x);
        float dy = to.y - from.y;

        if (forced == MoveKind.Ground)
        {
            return new Traversal
            {
                kind = MoveKind.Ground,
                note = Mathf.Abs(dy) > LEVEL_EPSILON
                    ? string.Format("바닥으로 이었는데 {0:0.00} m 단차가 있다", dy)
                    : "이어진 바닥",
            };
        }

        // 걸어 나가 떨어지기만 해도 닿는가. 능력을 하나도 쓰지 않는 경우다.
        if (dy < -LEVEL_EPSILON)
        {
            float drift = MotionEnvelope.MaxRange(p, Ability.None, dy);
            if (drift > 0f && dx <= drift)
                return Make(MoveKind.Drop, Ability.None, dx, drift, "걸어 나가 떨어지면 닿는다");
        }

        // 발로 딛는 경우.
        foreach (Ability plan in Plans)
        {
            float limit = MotionEnvelope.MaxRange(p, plan, dy);
            if (limit > 0f && dx <= limit)
                return Make(MoveKind.Air, plan, dx, limit, null);
        }

        // 턱 잡기. 발이 목표보다 손 길이만큼 아래에 있을 때 물린다.
        // 가장 후한 조건인 reachHigh로 본다 — 실제 판정은 reachLow~reachHigh 사이에서 걸린다.
        float footTarget = dy - p.ledgeReachHigh;

        foreach (Ability plan in Plans)
        {
            float span = MotionEnvelope.MaxRange(p, plan, footTarget);
            if (span <= 0f) continue;

            float limit = span + p.ledgeReach;
            if (dx <= limit) return Make(MoveKind.Ledge, plan, dx, limit, null);
        }

        // 못 닿는다. 무엇이 모자란지 구분해서 알려준다.
        Ability everything = Ability.GroundJump | Ability.AirJump | Ability.Dash;
        float footBest = MotionEnvelope.MaxRange(p, everything, footTarget);
        float best = Mathf.Max(
            MotionEnvelope.MaxRange(p, everything, dy),
            footBest > 0f ? footBest + p.ledgeReach : -1f);

        string reason = best <= 0f
            ? string.Format("높이가 모자란다 — 이 높이차({0:0.00} m)에는 어떤 궤적도 닿지 않는다", dy)
            : string.Format("거리가 {0:0.00} m 모자란다", dx - best);

        return new Traversal
        {
            kind = MoveKind.Unreachable,
            abilities = everything,
            load = best > 0f ? dx / best : 999f,
            margin = best - dx,
            limit = best,
            note = reason,
        };
    }

    static Traversal Make(MoveKind kind, Ability abilities, float dx, float limit, string note)
    {
        Traversal traversal = new Traversal
        {
            kind = kind,
            abilities = abilities,
            load = limit <= 0f ? 0f : dx / limit,
            margin = limit - dx,
            limit = limit,
        };

        traversal.note = note ?? string.Format("{0} (한계 {1:0.00} m)", traversal.AbilityLabel, limit);
        return traversal;
    }

    /// <summary>
    /// 판정에 쓴 <b>한계 궤적</b>을 채운다. 목표로 가는 선이 아니라 도달 가능 영역의 경계선이다 —
    /// 목표점이 이 선 안쪽에 있으면 닿는다.
    /// </summary>
    public static void SampleEnvelope(MotionProfile p, Vector3 from, float direction, Ability abilities,
        float endHeight, List<Vector3> into)
    {
        into.Clear();
        if (p.gravity <= 0f) return;

        MotionEnvelope.MaxRange(p, abilities, endHeight, out float jumpAt, out float dashAt);

        float sign = direction < 0f ? -1f : 1f;
        float t = 0f, x = 0f, y = 0f;
        float vy = (abilities & Ability.GroundJump) != 0 ? p.jumpSpeed : 0f;

        bool jumpPending = (abilities & Ability.AirJump) != 0;
        bool dashPending = (abilities & Ability.Dash) != 0;

        into.Add(from);

        const float STEP = 0.02f;
        for (int i = 0; i < 512; i++)
        {
            if (jumpPending && t >= jumpAt) { vy = p.jumpSpeed; jumpPending = false; }

            if (dashPending && t >= dashAt)
            {
                x += sign * p.dashSpeed * p.dashTime;
                t += p.dashTime;
                vy = 0f;
                dashPending = false;
                into.Add(from + new Vector3(x, y, 0f));
                continue;
            }

            y += vy * STEP - 0.5f * p.gravity * STEP * STEP;
            vy -= p.gravity * STEP;
            x += sign * p.airborneSpeed * STEP;
            t += STEP;

            into.Add(from + new Vector3(x, y, 0f));

            if (t > LEVEL_EPSILON && y <= endHeight) break;
        }
    }
}
