using UnityEditor;
using UnityEngine;

/// <summary>
/// 동선을 손으로 찍지 않고 <b>모양</b>으로 붙인다.
///
/// 한 걸음의 수평 거리를 사람이 정하지 않는다 — 높이차를 주면 도달 구간에서 역산한다.
/// 그래야 방향 키를 놓지 않고도 밟히는 자리에만 발판이 선다.
///
/// 방향 전환은 모양의 꼭짓점에서만 일어난다. 꼭짓점에서도 <b>정지가 아니라 반대 방향 입력</b>으로
/// 넘어가도록 다음 발판을 뒤쪽에 놓는다. 위로 곧게 뛰게 만들면 손을 떼야 하기 때문이다.
/// </summary>
public static class RouteShapes
{
    public enum Shape
    {
        /// <summary>^ — 올랐다가 내려온다. 방향은 그대로.</summary>
        Peak,

        /// <summary>v — 내려갔다가 올라온다. 방향은 그대로.</summary>
        Valley,

        /// <summary>&lt; — 왼쪽 꼭짓점. 되돌아갔다가 다시 나온다.</summary>
        TurnLeft,

        /// <summary>&gt; — 오른쪽 꼭짓점.</summary>
        TurnRight,

        /// <summary>지그재그 — 꼭짓점을 번갈아 쌓아 좁은 폭에서 높이를 번다.</summary>
        Zigzag,
    }

    /// <summary>
    /// 걸음마다 벌 높이. 점프 정점(1.27 m)보다 낮게 두면 한 걸음이 홑점프로 끝나,
    /// 손을 떼지 않고 리듬만으로 오를 수 있다.
    /// </summary>
    public const float DEFAULT_RISE = 1.1f;

    /// <summary>
    /// 꼭짓점에서 한 번에 오르는 높이. 여기서만 2단 점프를 요구한다.
    ///
    /// 2.1 m가 이 캐릭터에서 가장 가파른 걸음이다 — 최소 걸음이 3.66 m라 기울기 0.573 m/m로,
    /// 홑점프로 낼 수 있는 최대(0.379 m/m)를 크게 넘는다. 어차피 손이 방향 키를 옮기는 자리라
    /// 여기에 2단 점프를 몰아주는 편이 리듬을 덜 끊는다.
    /// </summary>
    public const float DEFAULT_TURN_RISE = 2.1f;

    /// <summary>도달 구간 안에서 어디를 고를지. 0이면 가장 짧게, 1이면 한계까지.</summary>
    public const float DEFAULT_TIGHTNESS = 0.12f;

    public static void Append(LevelRoute route, Shape shape, int steps,
        LevelRoute.PadModule module = LevelRoute.PadModule.Deck3,
        float rise = DEFAULT_RISE, float tightness = DEFAULT_TIGHTNESS, int legs = 3)
    {
        if (route == null || route.main.Count == 0)
        {
            Debug.LogError("[RouteShapes] 붙일 곳이 없다. 출발 발판이 될 노드를 먼저 하나 두어야 한다.");
            return;
        }

        Undo.RecordObject(route, "Append Route Shape");

        float direction = LastDirection(route);

        switch (shape)
        {
            case Shape.Peak:
                Run(route, module, steps, direction, rise, tightness, "^ 오름");
                Run(route, module, steps, direction, -rise, tightness, "^ 내림");
                break;

            case Shape.Valley:
                Run(route, module, steps, direction, -rise, tightness, "v 내림");
                Run(route, module, steps, direction, rise, tightness, "v 오름");
                break;

            case Shape.TurnLeft:
                Run(route, module, steps, -1f, rise, tightness, "< 되돌기");
                Turn(route, module, 1f, tightness, "< 꼭짓점");
                Run(route, module, steps, 1f, rise, tightness, "< 나오기");
                break;

            case Shape.TurnRight:
                Run(route, module, steps, 1f, rise, tightness, "> 나가기");
                Turn(route, module, -1f, tightness, "> 꼭짓점");
                Run(route, module, steps, -1f, rise, tightness, "> 되돌기");
                break;

            case Shape.Zigzag:
                Zigzag(route, module, legs, steps, direction, rise, tightness);
                break;
        }

        EditorUtility.SetDirty(route);
    }

    /// <summary>
    /// 지그재그. 층마다 방향을 뒤집어 같은 X 폭 안에서 높이만 쌓는다.
    ///
    /// 수직성은 여기서 나온다 — 가로로 쓴 거리를 되돌아오며 <b>다시 쓰기</b> 때문에,
    /// 층을 더 올려도 레벨의 가로 폭은 거의 늘지 않는다.
    /// </summary>
    static void Zigzag(LevelRoute route, LevelRoute.PadModule module, int legs, int perLeg,
        float direction, float rise, float tightness)
    {
        for (int leg = 0; leg < Mathf.Max(1, legs); leg++)
        {
            Run(route, module, perLeg, direction, rise, tightness, $"지그 {leg + 1}");

            // 마지막 층에서는 꺾지 않는다. 다음 모양이 그대로 이어받도록.
            if (leg == legs - 1) break;

            direction = -direction;
            Turn(route, module, direction, tightness, $"꺾임 {leg + 1}");
        }
    }

    #region 걸음

    /// <summary>한 방향으로 <paramref name="steps"/>걸음. 걸음마다 높이가 <paramref name="rise"/>씩 변한다.</summary>
    static void Run(LevelRoute route, LevelRoute.PadModule module, int steps,
        float direction, float rise, float tightness, string label)
    {
        for (int i = 0; i < steps; i++)
        {
            if (!Step(route, module, direction, rise, tightness, $"{label} {i + 1}")) return;
        }
    }

    /// <summary>
    /// 꼭짓점 한 걸음. 방향을 뒤집으면서 크게 오른다 —
    /// 여기서만 2단 점프를 쓰고, 여기서만 손이 방향 키를 옮긴다.
    /// </summary>
    static void Turn(LevelRoute route, LevelRoute.PadModule module,
        float direction, float tightness, string label)
        => Step(route, module, direction, DEFAULT_TURN_RISE, tightness, label);

    static bool Step(LevelRoute route, LevelRoute.PadModule module,
        float direction, float rise, float tightness, string label)
    {
        LevelRoute.Node previous = route.main[route.main.Count - 1];

        float reach = StepDistance(route.profile, rise, tightness);
        if (reach < 0f)
        {
            Debug.LogError($"[RouteShapes] '{label}': {rise:0.00} m는 한 번에 오르지 못한다. rise를 낮출 것.");
            return false;
        }

        LevelRoute.Node node = new LevelRoute.Node
        {
            label = label,
            module = module,
            entry = MoveKind.Auto,
            padWidth = 0f,
        };

        // 도약은 출발 발판의 끝에서 일어나고, 착지는 다음 발판 한가운데에 떨어뜨린다.
        // 한가운데로 떨어뜨려야 도약을 앞당기거나 능력을 더 써도 발판 위에 남는다.
        float half = route.HalfWidthOf(previous);
        node.position = new Vector2(
            previous.position.x + direction * (half + reach),
            previous.position.y + rise);

        route.main.Add(node);
        return true;
    }

    /// <summary>
    /// 이 높이차에 쓸 한 걸음의 수평 거리.
    ///
    /// 최소와 최대 사이에서 고른다. 최소 쪽에 붙일수록 발판이 촘촘해지고 홑점프로 끝나며,
    /// 최대 쪽으로 갈수록 능력을 더 요구한다.
    /// </summary>
    public static float StepDistance(MotionProfile p, float rise, float tightness)
    {
        float shortest = MotionEnvelope.MinRange(p, rise);
        float longest = MotionEnvelope.MaxRange(p, Ability.GroundJump | Ability.AirJump | Ability.Dash, rise);

        if (shortest < 0f || longest < 0f) return -1f;

        return Mathf.Lerp(shortest, longest, Mathf.Clamp01(tightness));
    }

    #endregion

    /// <summary>지금까지 나아가던 방향. 노드가 하나뿐이면 오른쪽으로 본다.</summary>
    static float LastDirection(LevelRoute route)
    {
        if (route.main.Count < 2) return 1f;

        float dx = route.main[route.main.Count - 1].position.x - route.main[route.main.Count - 2].position.x;
        return dx >= 0f ? 1f : -1f;
    }
}
