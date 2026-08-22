using UnityEditor;
using UnityEngine;

/// <summary>
/// <see cref="LevelRoute"/>의 인스펙터와 씬 핸들.
///
/// 하는 일은 세 가지다 — 캐릭터에서 실제 수치를 긁어오고, 구간마다 판정을 보여주고,
/// 그 동선으로 발판을 짓는다. 배치를 손으로 고칠 일이 생기면 그건 동선이 틀린 것이다.
/// </summary>
[CustomEditor(typeof(LevelRoute))]
public class LevelRouteEditor : Editor
{
    /// <summary>씬에 캐릭터가 없을 때 수치를 읽어올 프리팹.</summary>
    const string PROTAGONIST_GUID = "8544061f06e2dc448a94c2209b9dd634";

    LevelRoute Route => (LevelRoute)target;

    public override void OnInspectorGUI()
    {
        DrawProfileBox();
        EditorGUILayout.Space();

        DrawDefaultInspector();

        EditorGUILayout.Space();
        DrawShapes();

        EditorGUILayout.Space();
        DrawVerdicts();

        EditorGUILayout.Space();
        DrawActions();
    }

    #region 이동 성능

    void DrawProfileBox()
    {
        MotionProfile p = Route.profile;

        EditorGUILayout.LabelField("이동 성능", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("캐릭터에서 읽기"))
            {
                PullProfile();
                GUIUtility.ExitGUI();
            }
        }

        EditorGUILayout.HelpBox(
            $"점프 정점 {p.ApexHeight:0.00} m · 2단까지 오르는 높이 {p.MaxClimb:0.00} m · 턱까지 {p.LedgeCeiling:0.00} m\n" +
            $"평지 도달 — 점프만 {TraversalSolver.JumpRange(p, 0f):0.00} m · 전부 쓰면 {TraversalSolver.FullRange(p, 0f):0.00} m",
            MessageType.None);
    }

    void PullProfile()
    {
        CharacterManager character = Route.sampleCharacter;

        if (character == null)
            character = Object.FindFirstObjectByType<CharacterManager>(FindObjectsInactive.Include);

        if (character == null)
        {
            string path = AssetDatabase.GUIDToAssetPath(PROTAGONIST_GUID);
            GameObject prefab = string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null) character = prefab.GetComponent<CharacterManager>();
        }

        if (character == null)
        {
            Debug.LogWarning("[LevelRoute] CharacterManager를 찾지 못했다. 씬에 캐릭터를 올리거나 직접 지정할 것.");
            return;
        }

        Undo.RecordObject(Route, "Pull Motion Profile");
        Route.profile.ReadFrom(character);
        EditorUtility.SetDirty(Route);

        Debug.Log($"[LevelRoute] '{character.name}'에서 이동 성능을 읽었다.");
    }

    #endregion

    #region 구간 판정

    void DrawVerdicts()
    {
        EditorGUILayout.LabelField("구간 판정", EditorStyles.boldLabel);

        int blocked = 0;
        int tight = 0;

        foreach (LevelRoute.Segment segment in Route.Segments())
        {
            Route.EdgePoints(segment.from, segment.to, out Vector3 from, out Vector3 to);
            Traversal verdict = TraversalSolver.Solve(Route.profile, from, to, segment.to.entry);

            // 도달만 보는 판정은 발판 폭을 모른다. 방향을 쥔 채 실제로 무엇을 써야 하는지는
            // 발판까지 함께 보는 쪽이 안다 — 그쪽이 알아낸 수단으로 이름을 고쳐 단다.
            if (segment.to.entry != MoveKind.Ground
                && Route.HoldReach(segment.from, segment.to, out Ability actual, out _, out _, out _, out _)
                && actual != Ability.None)
                verdict.abilities = actual;

            if (!verdict.Possible) blocked++;
            else if (verdict.load > TraversalSolver.ComfortableLoad) tight++;

            string where = segment.branch == null
                ? $"메인 {segment.index - 1}→{segment.index}"
                : $"{segment.branch.name} {segment.index}";

            Color previous = GUI.color;
            GUI.color = verdict.Color;
            EditorGUILayout.LabelField($"  {where} · {verdict.AbilityLabel} — {verdict.note}", EditorStyles.miniLabel);
            GUI.color = previous;
        }

        int held = 0;
        foreach ((LevelRoute.Segment segment, string reason) in Route.HoldProblems())
        {
            held++;
            Color previous = GUI.color;
            GUI.color = new Color(1f, 0.45f, 0.15f);
            EditorGUILayout.LabelField($"  {segment.from.label} → {segment.to.label}: {reason}", EditorStyles.miniLabel);
            GUI.color = previous;
        }

        if (held > 0)
            EditorGUILayout.HelpBox($"방향 키를 놓아야 하는 구간 {held}곳.", MessageType.Warning);

        int cramped = 0;
        foreach ((LevelRoute.Node below, LevelRoute.Node above, float clearance) in Route.ClearanceProblems())
        {
            cramped++;
            Color previous = GUI.color;
            GUI.color = new Color(1f, 0.25f, 0.25f);
            EditorGUILayout.LabelField(
                clearance < 0f
                    ? $"  관통 {-clearance:0.00} m — '{above.label}'의 아트가 '{below.label}'를 파고든다"
                    : $"  빈 공간 {clearance:0.00} m — '{below.label}' 위를 '{above.label}'가 덮는다 (키 {Route.profile.characterHeight:0.00} m)",
                EditorStyles.miniLabel);
            GUI.color = previous;
        }

        if (cramped > 0)
            EditorGUILayout.HelpBox(
                $"겹치는 발판 {cramped}곳. 이 애셋은 딛는 면 아래로 열주가 "
                + $"{(Route.pads != null && Route.pads.Length > 0 ? Route.pads[0].drop : 0f):0.0} m 매달려 있어, "
                + "X로 비켜 놓거나 그만큼 띄우지 않으면 아트가 관통한다.",
                MessageType.Error);

        if (blocked > 0)
            EditorGUILayout.HelpBox($"닿지 않는 구간 {blocked}개. 발판을 짓기 전에 고칠 것.", MessageType.Error);
        else if (tight > 0)
            EditorGUILayout.HelpBox($"여유가 빠듯한 구간 {tight}개. 의도한 것이면 그대로 둔다.", MessageType.Warning);
        else
            EditorGUILayout.HelpBox("모든 구간이 넉넉하게 닿는다.", MessageType.Info);
    }

    #endregion

    #region 모양 붙이기

    int _shapeSteps = 4;
    int _shapeLegs = 3;
    float _shapeRise = RouteShapes.DEFAULT_RISE;
    float _shapeTightness = RouteShapes.DEFAULT_TIGHTNESS;
    LevelRoute.PadModule _shapeModule = LevelRoute.PadModule.Deck3;

    void DrawShapes()
    {
        EditorGUILayout.LabelField("모양 붙이기", EditorStyles.boldLabel);

        _shapeSteps = EditorGUILayout.IntSlider("걸음 수", _shapeSteps, 1, 12);
        _shapeLegs = EditorGUILayout.IntSlider("지그재그 층 수", _shapeLegs, 2, 10);
        _shapeRise = EditorGUILayout.Slider("걸음당 높이", _shapeRise, 0.2f, Route.profile.MaxClimb);
        _shapeTightness = EditorGUILayout.Slider("여유", _shapeTightness, 0f, 1f);
        _shapeModule = (LevelRoute.PadModule)EditorGUILayout.EnumPopup("발판", _shapeModule);

        float step = RouteShapes.StepDistance(Route.profile, _shapeRise, _shapeTightness);
        EditorGUILayout.LabelField(
            step < 0f
                ? "   이 높이는 한 번에 오르지 못한다"
                : $"   한 걸음 {step:0.00} m · 최소 {MotionEnvelope.MinRange(Route.profile, _shapeRise):0.00} m",
            EditorStyles.miniLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            Shape("^", RouteShapes.Shape.Peak);
            Shape("v", RouteShapes.Shape.Valley);
            Shape("<", RouteShapes.Shape.TurnLeft);
            Shape(">", RouteShapes.Shape.TurnRight);
            Shape("지그재그", RouteShapes.Shape.Zigzag);
        }
    }

    void Shape(string label, RouteShapes.Shape shape)
    {
        if (!GUILayout.Button(label, GUILayout.Height(22))) return;

        RouteShapes.Append(Route, shape, _shapeSteps, _shapeModule, _shapeRise, _shapeTightness, _shapeLegs);
        GUIUtility.ExitGUI();
    }

    #endregion

    #region 실행

    void DrawActions()
    {
        LevelBounds bounds = Object.FindFirstObjectByType<LevelBounds>(FindObjectsInactive.Include);

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(bounds == null || !bounds.Ready))
            {
                if (GUILayout.Button(bounds == null ? "트리거에서 생성 (LevelBounds 없음)" : "트리거에서 생성",
                    GUILayout.Height(26)))
                {
                    RouteFromBounds.Generate(Route, bounds);
                    GUIUtility.ExitGUI();
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("발판 짓기", GUILayout.Height(26)))
            {
                PlatformBuilder.Build(Route);
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("지우기", GUILayout.Height(26), GUILayout.Width(80)))
            {
                PlatformBuilder.Clear(Route);
                GUIUtility.ExitGUI();
            }
        }
    }

    #endregion

    #region 씬 핸들

    void OnSceneGUI()
    {
        DrawHandles(Route.main, "M");

        for (int b = 0; b < Route.branches.Count; b++)
            DrawHandles(Route.branches[b].nodes, Route.branches[b].name);
    }

    void DrawHandles(System.Collections.Generic.List<LevelRoute.Node> nodes, string prefix)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            LevelRoute.Node node = nodes[i];
            Vector3 world = Route.WorldOf(node);

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.PositionHandle(world, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Route, "Move Route Node");

                // Z는 손대지 못하게 한다. 주인공이 갈 수 없는 곳에 발판이 생기지 않도록.
                Vector3 local = Route.transform.InverseTransformPoint(new Vector3(moved.x, moved.y, 0f));
                node.position = new Vector2(local.x, local.y);

                EditorUtility.SetDirty(Route);
            }

            Handles.Label(world + Vector3.up * 0.35f,
                string.IsNullOrEmpty(node.label) ? $"{prefix}{i}" : $"{prefix}{i} {node.label}");
        }
    }

    #endregion
}
