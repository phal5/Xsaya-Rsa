using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 트리거 세 개에서 주 경로와 곁길을 만든다.
///
/// 사람이 정하는 것은 <b>어디서 어디로, 어느 부피 안에서, 몇 번 접어서</b>뿐이다.
/// 걸음의 높이와 거리는 전부 도달 구간에서 역산하고, 놓기 전에 빈자리인지 확인한다 —
/// 그래야 만들어진 자리가 반드시 밟히고, 서로를 막지 않는다.
///
/// 구간은 성격이 다른 여러 갈래를 씨앗으로 섞는다. 지그재그만 반복하면 수직성은 얻어도 지루하다.
/// </summary>
public static class RouteFromBounds
{
    /// <summary>꼭짓점에서 쓰는 높이. 되돌아 점프가 감당하는 최대에 가깝게 잡는다.</summary>
    const float TURN_RISE = 2.4f;


    /// <summary>빈자리를 찾을 때 기본 거리에서 더해 볼 값들.</summary>
    static readonly float[] Nudges = { 0f, 0.9f, 1.8f, 2.7f, -0.9f };

    /// <summary>
    /// 씨앗을 훑는 동안에는 입을 다문다.
    ///
    /// 버려질 후보가 뱉는 경고까지 다 찍으면, 정작 채택된 결과의 경고가 그 속에 묻힌다.
    /// </summary>
    public static bool Quiet { get; set; }

    static void Warn(string message, Object context = null)
    {
        if (!Quiet) Debug.LogWarning(message, context);
    }

    public static void Generate(LevelRoute route, LevelBounds bounds)
    {
        if (route == null || bounds == null || !bounds.Ready)
        {
            Debug.LogError("[RouteFromBounds] 시점 · 종점 · 영역 트리거가 모두 있어야 한다.");
            return;
        }

        // 치수를 먼저 채운다. 동선을 만드는 동안 이미 발판 폭과 아트 두께가 필요하다 —
        // 비어 있으면 모든 발판을 폭 0으로 보고 겹침 검사가 통째로 무력해진다.
        PlatformBuilder.MeasureInto(route);

        Vector2 start = LevelBounds.FootOf(bounds.startTrigger);
        Vector2 end = LevelBounds.FootOf(bounds.endTrigger);
        Bounds region = LevelBounds.WorldBoundsOf(bounds.region);

        // 주인공은 언제나 원점에서 시작한다. 출발 발판의 윗면이 거기가 아니면
        // 레벨을 아무리 잘 지어도 첫 걸음이 허공이거나 발판 속이다.
        if (start.sqrMagnitude > 0.0001f)
            Warn(
                $"[RouteFromBounds] 출발 발판의 바닥이 ({start.x:0.00}, {start.y:0.00})다. " +
                "주인공의 초기 위치는 (0, 0, 0)이므로 Start 트리거의 아랫면을 원점에 맞출 것.",
                bounds.startTrigger);

        Undo.RecordObject(route, "Generate Route");
        route.main.Clear();
        route.branches.Clear();

        // 출발·도착 발판은 트리거의 폭을 그대로 쓴다. 트리거를 옮기거나 키운 것이 레벨에 나타나야 한다.
        route.main.Add(Wide("출발", start, bounds.startModule, LevelBounds.WidthOf(bounds.startTrigger)));

        // 도착을 <b>먼저</b> 놓는다. 그래야 그 뒤로 놓는 모든 발판이 도착 발판을 장애물로 보고 피한다 —
        // 마지막에 얹으면 이미 자리를 차지한 것들과 부딪혀도 물러설 곳이 없다.
        route.main.Add(Wide("도착", end, bounds.endModule, LevelBounds.WidthOf(bounds.endTrigger)));

        Plan plan = Solve(route, bounds, start, end, region);
        if (plan.rise > route.profile.MaxClimb)
        {
            Debug.LogError(
                $"[RouteFromBounds] 영역 안에서 {plan.rise:0.00} m씩 올라야 하는데 한 걸음의 한계는 " +
                $"{route.profile.MaxClimb:0.00} m다. 지그재그를 늘리거나 영역을 넓힐 것.");
            return;
        }

        Emit(route, bounds, plan, start, end, region);
        Approach(route, bounds, plan, end, region);
        EmitBranches(route, bounds, plan, region);
        DropBrokenBranches(route);

        EditorUtility.SetDirty(route);
    }

    #region 노드 다루기

    /// <summary>마지막으로 놓은 발판. 도착 발판은 언제나 목록 끝에 있으므로 그 앞이다.</summary>
    static LevelRoute.Node Last(LevelRoute route) => route.main[route.main.Count - 2];

    /// <summary>도착 발판 앞에 끼워 넣는다.</summary>
    static void Append(LevelRoute route, LevelRoute.Node node)
        => route.main.Insert(route.main.Count - 1, node);

    static LevelRoute.Node Node(string label, Vector2 position, LevelRoute.PadModule module)
        => new LevelRoute.Node
        {
            label = label,
            position = position,
            entry = MoveKind.Auto,
            module = module,
            padWidth = 0f,
        };

    static LevelRoute.Node Wide(string label, Vector2 position, LevelRoute.PadModule module, float width)
    {
        LevelRoute.Node node = Node(label, position, module);
        node.padWidth = width;
        return node;
    }

    #endregion

    #region 배분

    struct Plan
    {
        public float rise;
        public float turnRise;
    }

    /// <summary>
    /// 영역과 층 수에서 걸음의 높이를 역산한다.
    /// 가로는 영역이 정하고, 세로는 남은 높이를 걸음 수로 나눈 것이 된다.
    /// </summary>
    static Plan Solve(LevelRoute route, LevelBounds bounds, Vector2 start, Vector2 end, Bounds region)
    {
        MotionProfile p = route.profile;
        int legs = Mathf.Max(1, bounds.zigzags);

        float padWidth = route.pads[(int)bounds.module].width;
        float span = Mathf.Max(1f, region.size.x - bounds.margin * 2f - padWidth);

        float turnRise = Mathf.Min(TURN_RISE, p.MaxClimb);
        float rise = p.ApexHeight * 0.9f;

        for (int attempt = 0; attempt < 6; attempt++)
        {
            float distance = RouteShapes.StepDistance(p, rise, bounds.spacing);
            if (distance <= 0f) break;

            int steps = Mathf.Max(1, Mathf.FloorToInt(span / distance));
            float next = (end.y - start.y - (legs - 1) * turnRise) / (legs * steps);
            next = Mathf.Clamp(next, 0.4f, p.MaxClimb);

            if (Mathf.Abs(next - rise) < 0.05f) { rise = next; break; }
            rise = next;
        }

        return new Plan { rise = rise, turnRise = turnRise };
    }

    #endregion

    #region 구간 짜기

    /// <summary>구간의 갈래. 같은 것만 이어 붙이면 아무리 잘 재도 지루해진다.</summary>
    enum Kind
    {
        /// <summary>비스듬히 오르는 층. 끝에서 방향을 꺾는다.</summary>
        Leg,

        /// <summary>같은 높이로 건너간다. 홑점프가 가장 멀리 나가는 구간이라 리듬이 달라진다.</summary>
        Terrace,

        /// <summary>제자리에서 곧게 오른다. 되돌아 점프를 연달아 쓴다.</summary>
        Tower,

        /// <summary>한 번 떨어졌다 다시 오른다. 위로만 가던 흐름을 끊는다.</summary>
        Dip,

        /// <summary>2단 점프로도 모자란 틈. 대시를 반드시 쓰게 만든다.</summary>
        Leap,

        /// <summary>발로는 못 딛는 높이. 턱을 물어야만 오른다.</summary>
        Ledge,

        /// <summary>출발 발판만 한 큼지막한 지형. 숨 돌릴 자리이자 눈에 걸리는 이정표가 된다.</summary>
        Plateau,

        /// <summary>두꺼운 발판을 길목에 세운다. 넘어가지 못하니 옆으로 타고 올라야 한다.</summary>
        Wall,
    }

    struct Section
    {
        public Kind kind;
        public int steps;
        public LevelRoute.PadModule module;
    }

    /// <summary>
    /// 구간을 늘어놓는다. 씨앗을 쓰므로 같은 값이면 언제나 같은 레벨이 나온다.
    ///
    /// 발판 모듈도 구간마다 바꾼다 — 슬래브 크기가 일정하면 달리는 거리와 뛰는 거리가 늘 같아서,
    /// 아무리 모양을 섞어도 손끝에 오는 리듬은 하나뿐이다.
    /// </summary>
    static List<Section> Compose(LevelBounds bounds)
    {
        System.Random random = new System.Random(bounds.seed);
        var list = new List<Section>();

        LevelRoute.PadModule[] modules =
        {
            LevelRoute.PadModule.Deck3,
            LevelRoute.PadModule.Deck5,
            LevelRoute.PadModule.Deck3,
            LevelRoute.PadModule.Deck,
        };

        List<Kind> pool = new List<Kind>
        {
            Kind.Terrace, Kind.Tower, Kind.Dip, Kind.Leap, Kind.Ledge, Kind.Plateau,
        };

        if (bounds.walls) pool.Add(Kind.Wall);

        Kind[] interludes = pool.ToArray();
        int legs = Mathf.Max(1, bounds.zigzags);

        for (int i = 0; i < legs; i++)
        {
            list.Add(new Section
            {
                kind = Kind.Leg,
                steps = 2 + random.Next(3),
                module = modules[random.Next(modules.Length)],
            });

            if (i == legs - 1) break;
            if (random.NextDouble() > bounds.variety) continue;

            Kind kind = interludes[random.Next(interludes.Length)];
            list.Add(new Section
            {
                kind = kind,
                steps = kind == Kind.Leap || kind == Kind.Ledge
                    || kind == Kind.Plateau || kind == Kind.Wall
                    ? 1
                    : 2 + random.Next(2),
                module = modules[random.Next(modules.Length)],
            });
        }

        return list;
    }

    #endregion

    #region 놓기

    static void Emit(LevelRoute route, LevelBounds bounds, Plan plan,
        Vector2 start, Vector2 end, Bounds region)
    {
        // 시작점이 어디에 놓였는지가 첫 방향을 정한다 — 벽을 등지고 넓은 쪽으로 나아간다.
        float direction = start.x <= region.center.x ? 1f : -1f;

        foreach (Section section in Compose(bounds))
        {
            switch (section.kind)
            {
                case Kind.Leg:
                    Run(route, bounds, plan, section, ref direction, plan.rise, region);
                    direction = -direction;
                    Step(route, bounds, section.module, ref direction, plan.turnRise, region, "꺾임");
                    break;

                case Kind.Terrace:
                    Run(route, bounds, plan, section, ref direction, 0f, region);
                    break;

                case Kind.Tower:
                    for (int i = 0; i < section.steps; i++)
                        Step(route, bounds, section.module, ref direction, plan.turnRise, region, "탑");
                    break;

                case Kind.Dip:
                    Step(route, bounds, section.module, ref direction, -plan.turnRise, region, "낙하");
                    Run(route, bounds, plan, section, ref direction, plan.rise, region);
                    break;

                case Kind.Leap:
                    Leap(route, bounds, section, ref direction, region);
                    break;

                case Kind.Ledge:
                    Ledge(route, bounds, section, ref direction, region);
                    break;

                case Kind.Plateau:
                    Plateau(route, bounds, plan, ref direction, region);
                    break;

                case Kind.Wall:
                    Wall(route, bounds, plan, ref direction, region);
                    break;
            }
        }
    }

    static void Run(LevelRoute route, LevelBounds bounds, Plan plan, Section section,
        ref float direction, float rise, Bounds region)
    {
        int bounces = 0;

        for (int i = 0; i < section.steps; i++)
        {
            if (Step(route, bounds, section.module, ref direction, rise, region, "층")) continue;

            // 벽에 닿았거나 자리가 없다. 꼭짓점을 하나 세우고 반대쪽으로 남은 걸음을 잇는다.
            if (++bounces > 1) return;

            direction = -direction;
            if (!Step(route, bounds, section.module, ref direction, plan.turnRise, region, "꺾임")) return;

            i--;
        }
    }

    /// <summary>
    /// 2단 점프로도 모자란 틈. 대시를 쓰지 않고는 건널 수 없는 거리를 골라 놓는다.
    /// </summary>
    static void Leap(LevelRoute route, LevelBounds bounds, Section section, ref float direction, Bounds region)
    {
        MotionProfile p = route.profile;

        float withoutDash = MotionEnvelope.MaxRange(p, Ability.GroundJump | Ability.AirJump, 0f);
        float withDash = MotionEnvelope.MaxRange(p, Ability.GroundJump | Ability.AirJump | Ability.Dash, 0f);

        // 둘 사이에 놓아야 대시가 강제된다. 한계에 너무 붙이면 착지창이 사라지므로 가운데를 쓴다.
        //
        // Place는 목표 발판의 <b>한가운데</b>까지를 재고, 도달 판정은 <b>가장자리</b>부터 잰다.
        // 그 차이를 더해 주지 않으면 실제 틈이 반 폭만큼 좁아져 2단 점프로 넘어가 버린다.
        float gap = Mathf.Lerp(withoutDash, withDash, 0.45f)
            + route.pads[(int)section.module].width * 0.5f;

        if (!Place(route, bounds, section.module, ref direction, 0f, gap, region, "대시"))
            Place(route, bounds, section.module, ref direction, 0f, gap * 0.8f, region, "대시");
    }

    /// <summary>
    /// 발로는 못 딛는 높이. 2단 점프의 한계 위, 턱 잡기의 한계 아래에 놓아 손으로만 오르게 한다.
    /// </summary>
    static void Ledge(LevelRoute route, LevelBounds bounds, Section section, ref float direction, Bounds region)
    {
        MotionProfile p = route.profile;
        float rise = Mathf.Lerp(p.MaxClimb, p.LedgeCeiling, 0.4f);

        float distance = RouteShapes.StepDistance(p, p.MaxClimb * 0.9f, bounds.spacing);
        Place(route, bounds, section.module, ref direction, rise, distance, region, "턱");
    }

    /// <summary>
    /// 출발 발판만 한 큼지막한 지형.
    ///
    /// 열주가 매달린 두꺼운 것부터 시도한다 — 아래가 6.5 m 넘게 비어 있으면 기둥째 서서
    /// 멀리서도 보이는 이정표가 되고, 자리가 없으면 얇은 것으로 물러난다.
    /// </summary>
    static void Plateau(LevelRoute route, LevelBounds bounds, Plan plan, ref float direction, Bounds region)
    {
        LevelRoute.PadModule[] preference =
        {
            LevelRoute.PadModule.ThickerDeck,
            LevelRoute.PadModule.Deck,
            LevelRoute.PadModule.Deck5,
        };

        float width = LevelBounds.WidthOf(bounds.startTrigger);
        float rise = plan.rise;
        float distance = RouteShapes.StepDistance(route.profile, rise, bounds.spacing);

        foreach (LevelRoute.PadModule module in preference)
        {
            // 넓은 발판은 목표 지점이 한가운데다. 반 폭만큼 더 나아가야 가장자리가 닿는다.
            if (Place(route, bounds, module, ref direction, rise, distance + width * 0.5f, region, "고원", width))
                return;
        }
    }

    /// <summary>
    /// 길목에 두꺼운 발판을 세우고, 그 옆을 타고 오르게 한다.
    ///
    /// 세운 것은 경로가 아니라 <b>장애물</b>이다 — 밟고 지나가는 자리가 아니라 부피로 길을 막는다.
    /// 이 애셋의 두꺼운 모듈은 딛는 면 아래로 7 m 넘게 열주가 매달려 있어서, 그대로 벽이 된다.
    /// 넘어가려면 그 꼭대기 위로 올라야 하는데 한 번에 오를 수 있는 높이는 4.4 m가 한계이므로,
    /// 옆에서 되돌아 점프를 연달아 걸어 좁은 폭으로 기어오르는 수밖에 없다.
    /// </summary>
    static void Wall(LevelRoute route, LevelBounds bounds, Plan plan, ref float direction, Bounds region)
    {
        MotionProfile p = route.profile;
        LevelRoute.Node anchor = Last(route);

        float width = LevelBounds.WidthOf(bounds.startTrigger);
        float half = width * 0.5f;

        float left = region.min.x + bounds.margin;
        float right = region.max.x - bounds.margin;

        // 앞을 막도록 나아가던 쪽에 세운다. 오르는 발판이 들어갈 틈은 남긴다.
        float lane = RouteShapes.StepDistance(p, plan.turnRise, bounds.spacing);
        float x = anchor.position.x + direction * (route.HalfWidthOf(anchor) + lane + half);

        if (x - half < left || x + half > right) return;

        // 한 번에 오를 수 있는 한계(4.4 m)보다 높아야 옆으로 도는 수밖에 없어진다.
        // 다만 천장이 낮으면 그만큼 낮춰서라도 세운다 — 못 세우느니 낮은 벽이 낫다.
        float ceiling = region.max.y - p.characterHeight;

        foreach (float scale in new[] { 1.25f, 1.05f })
        {
            float top = anchor.position.y + p.LedgeCeiling * scale;
            if (top > ceiling) continue;

            LevelRoute.Node candidate = Wide("벽", new Vector2(x, top), LevelRoute.PadModule.ThickerDeck, width);
            if (!route.Fits(candidate)) continue;

            Raise(route, bounds, plan, ref direction, region, candidate, top);
            return;
        }
    }

    /// <summary>벽을 세우고 그 옆을 기어올라 넘어간다.</summary>
    static void Raise(LevelRoute route, LevelBounds bounds, Plan plan, ref float direction,
        Bounds region, LevelRoute.Node wall, float top)
    {
        MotionProfile p = route.profile;
        float width = wall.padWidth;
        float lane = RouteShapes.StepDistance(p, plan.turnRise, bounds.spacing);

        route.obstacles.Add(wall);

        // 벽 옆을 되돌아 점프로 기어오른다. 꼭대기를 사람 키만큼 넘길 때까지.
        int guard = 0;
        while (Last(route).position.y < top + p.characterHeight && guard++ < 8)
        {
            float climb = direction;
            if (Step(route, bounds, LevelRoute.PadModule.Deck3, ref climb, plan.turnRise, region, "벽타기")) continue;

            climb = -direction;
            if (!Step(route, bounds, LevelRoute.PadModule.Deck3, ref climb, plan.turnRise, region, "벽타기")) break;
        }

        // 다 올랐으면 벽 너머로 건너간다.
        Place(route, bounds, LevelRoute.PadModule.Deck3, ref direction, 0f,
            width + lane, region, "벽넘기");
    }

    /// <summary>기본 걸음. 이 높이에 맞는 거리를 도달 구간에서 뽑아 놓는다.</summary>
    static bool Step(LevelRoute route, LevelBounds bounds, LevelRoute.PadModule module,
        ref float direction, float rise, Bounds region, string label)
    {
        float distance = RouteShapes.StepDistance(route.profile, rise, bounds.spacing);
        if (distance <= 0f) distance = route.pads[(int)module].width;

        return Place(route, bounds, module, ref direction, rise, distance, region, label);
    }

    /// <summary>
    /// 한 발판을 놓는다.
    ///
    /// 기본 거리에서 조금씩 옮겨 가며 <b>빈자리이면서 닿는</b> 첫 자리를 고른다.
    /// 곁길만 이렇게 하고 주 경로는 그냥 놓았더니, 되돌아오는 걸음들이 서로 위를 덮었다.
    /// </summary>
    static bool Place(LevelRoute route, LevelBounds bounds, LevelRoute.PadModule module,
        ref float direction, float rise, float distance, Bounds region, string label, float width = 0f)
    {
        LevelRoute.Node previous = Last(route);
        float half = route.HalfWidthOf(previous);

        float left = region.min.x + bounds.margin;
        float right = region.max.x - bounds.margin;

        foreach (float nudge in Nudges)
        {
            float x = previous.position.x + direction * (half + distance + nudge);
            LevelRoute.Node candidate = width > 0f
                ? Wide(label, new Vector2(x, previous.position.y + rise), module, width)
                : Node(label, new Vector2(x, previous.position.y + rise), module);

            float own = route.HalfWidthOf(candidate);
            if (x - own < left || x + own > right) continue;
            if (candidate.position.y > region.max.y || candidate.position.y < region.min.y) continue;

            if (!route.Fits(candidate)) continue;
            if (!route.HoldReach(previous, candidate, out _, out _, out _, out _, out _)) continue;

            Append(route, candidate);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 종점까지 남은 만큼을 마저 잇는다. 도달 판정이 이미 있으니 그것을 종료 조건으로 쓴다.
    /// </summary>
    static void Approach(LevelRoute route, LevelBounds bounds, Plan plan, Vector2 end, Bounds region)
    {
        LevelRoute.Node target = route.main[route.main.Count - 1];

        for (int guard = 0; guard < 48; guard++)
        {
            LevelRoute.Node previous = Last(route);
            if (route.HoldReach(previous, target, out _, out _, out _, out _, out _)) return;

            float remaining = end.y - previous.position.y;
            float toward = end.x >= previous.position.x ? 1f : -1f;

            // 종점으로 다가가는 방법을 여러 가지로 시도한다. 하나가 막혔다고 포기하면
            // 마지막 한 걸음을 못 놓아 레벨 전체가 끊긴다.
            float[] rises =
            {
                Mathf.Clamp(remaining, -plan.turnRise, Mathf.Min(plan.rise, plan.turnRise)),
                plan.turnRise,
                plan.rise,
                0f,
                -plan.turnRise,
            };

            bool placed = false;
            foreach (float rise in rises)
            {
                float direction = toward;
                if (Step(route, bounds, bounds.module, ref direction, rise, region, "이음")) { placed = true; break; }

                direction = -toward;
                if (Step(route, bounds, bounds.module, ref direction, rise, region, "꺾임")) { placed = true; break; }
            }

            if (!placed) break;
        }

        Warn("[RouteFromBounds] 종점까지 잇지 못했다. 영역이나 종점 높이를 확인할 것.");
    }

    #endregion

    #region 곁길

    /// <summary>곁길이 이어도 될 만큼 세로로 겹쳐 보이는 거리.</summary>
    const float BRANCH_DRIFT = 5f;

    /// <summary>
    /// 층을 걸어 도는 대신 곧게 올라 건너뛰는 지름길을 붙인다.
    /// 짧은 대신 되돌아 점프를 연달아 써야 하는, 곁길다운 곁길이 된다.
    /// </summary>
    static void EmitBranches(LevelRoute route, LevelBounds bounds, Plan plan, Bounds region)
    {
        if (bounds.branches <= 0) return;

        float maxRise = Mathf.Min(plan.turnRise, route.profile.MaxClimb);

        // 건너뛰는 양에 위쪽 뚜껑을 씌운다. 가장 많이 건너뛰는 것을 고르면 곁길이 아니라
        // 레벨을 통째로 우회하는 두 번째 본선이 되어, 주 경로가 아무도 안 가는 길이 된다.
        int least = Mathf.Max(2, bounds.branchSkip);
        int most = least * 2;

        int made = 0;
        int cursor = 0;

        while (made < bounds.branches && cursor < route.main.Count - 1)
        {
            int chosen = -1;
            int bestDistance = int.MaxValue;

            for (int j = cursor + least; j <= cursor + most && j < route.main.Count - 1; j++)
            {
                LevelRoute.Node from = route.main[cursor];
                LevelRoute.Node to = route.main[j];

                if (to.position.y - from.position.y <= maxRise) continue;
                if (Mathf.Abs(to.position.x - from.position.x) > BRANCH_DRIFT) continue;

                int distance = Mathf.Abs((j - cursor) - least);
                if (distance >= bestDistance) continue;

                chosen = j;
                bestDistance = distance;
            }

            if (chosen >= 0 && Shaft(route, bounds, region, cursor, chosen, maxRise))
            {
                made++;
                cursor = chosen;
                continue;
            }

            cursor++;
        }
    }

    static bool Shaft(LevelRoute route, LevelBounds bounds, Bounds region, int fromIndex, int toIndex, float maxRise)
    {
        LevelRoute.Node from = route.main[fromIndex];
        LevelRoute.Node to = route.main[toIndex];

        float dy = to.position.y - from.position.y;
        float left = region.min.x + bounds.margin;
        float right = region.max.x - bounds.margin;

        float[] offsets = { 0f, -1.6f, 1.6f, -3.2f, 3.2f, -4.8f, 4.8f };

        for (int extra = 0; extra < 4; extra++)
        {
            int steps = Mathf.Max(2, Mathf.CeilToInt(dy / maxRise) + extra);

            LevelRoute.Branch branch = new LevelRoute.Branch
            {
                name = $"지름길 {fromIndex}→{toIndex}",
                fromIndex = fromIndex,
                rejoinIndex = toIndex,
            };

            route.branches.Add(branch);

            bool laid = true;
            for (int k = 1; k < steps && laid; k++)
            {
                float t = (float)k / steps;
                float y = Mathf.Lerp(from.position.y, to.position.y, t);
                float baseX = Mathf.Lerp(from.position.x, to.position.x, t);

                LevelRoute.Node previous = k == 1 ? from : branch.nodes[k - 2];
                laid = false;

                foreach (float offset in offsets)
                {
                    float half = route.HalfWidthOf(previous);
                    LevelRoute.Node candidate = Node($"지름길 {k}",
                        new Vector2(Mathf.Clamp(baseX + offset, left + half, right - half), y), bounds.module);

                    if (!route.Fits(candidate)) continue;
                    if (!route.HoldReach(previous, candidate, out _, out _, out _, out _, out _)) continue;

                    branch.nodes.Add(candidate);
                    laid = true;
                    break;
                }
            }

            if (laid && Walkable(route, branch)) return true;

            route.branches.Remove(branch);
        }

        return false;
    }

    static bool Walkable(LevelRoute route, LevelRoute.Branch branch)
    {
        foreach (LevelRoute.Segment segment in route.Segments())
        {
            if (segment.branch != branch) continue;
            if (!route.HoldReach(segment.from, segment.to, out _, out _, out _, out _, out _)) return false;
        }

        return true;
    }

    /// <summary>반쯤 밟히는 길을 남기느니 없는 편이 낫다 — 플레이어는 그것이 길인지 알 수 없다.</summary>
    static void DropBrokenBranches(LevelRoute route)
    {
        for (int i = route.branches.Count - 1; i >= 0; i--)
        {
            LevelRoute.Branch branch = route.branches[i];
            if (Walkable(route, branch)) continue;

            Warn($"[RouteFromBounds] '{branch.name}'을 걷어냈다. 자리를 낼 수 없다.");
            route.branches.RemoveAt(i);
        }
    }

    #endregion
}
