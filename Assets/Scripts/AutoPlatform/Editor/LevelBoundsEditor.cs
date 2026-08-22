using UnityEditor;
using UnityEngine;

/// <summary>
/// 레벨을 만드는 자리. 조건이 여기 있으니 버튼도 여기 있어야 한다.
/// </summary>
[CustomEditor(typeof(LevelBounds))]
public class LevelBoundsEditor : Editor
{
    LevelBuilder.Report _report;

    LevelBounds Bounds => (LevelBounds)target;

    public override void OnInspectorGUI()
    {
        DrawFolder();

        EditorGUILayout.Space();
        DrawDefaultInspector();

        EditorGUILayout.Space();
        DrawStartCheck();

        EditorGUILayout.Space();
        DrawBuild();

        if (_report.ran) DrawReport();
    }

    /// <summary>
    /// 발판 폴더를 끌어다 놓게 한다.
    ///
    /// 경로를 문자열로 두면 오타 한 글자에 아무것도 안 나오고 이유도 안 보인다.
    /// 폴더 애셋을 직접 받아 경로로 바꿔 넣으면 그럴 수 없다.
    /// </summary>
    void DrawFolder()
    {
        EditorGUILayout.LabelField("발판 프리팹 폴더", EditorStyles.boldLabel);

        DefaultAsset current = AssetDatabase.LoadAssetAtPath<DefaultAsset>(Bounds.moduleFolder);
        DefaultAsset picked = (DefaultAsset)EditorGUILayout.ObjectField(
            "폴더", current, typeof(DefaultAsset), false);

        if (picked != current)
        {
            string path = AssetDatabase.GetAssetPath(picked);

            if (picked != null && !AssetDatabase.IsValidFolder(path))
                Debug.LogWarning("[LevelBounds] 폴더만 놓을 수 있다.", Bounds);
            else
            {
                Undo.RecordObject(Bounds, "Set Module Folder");
                Bounds.moduleFolder = path;
                EditorUtility.SetDirty(Bounds);
            }
        }

        if (!AssetDatabase.IsValidFolder(Bounds.moduleFolder))
        {
            EditorGUILayout.HelpBox("폴더를 지정해야 한다. 발판으로 쓸 프리팹만 모아 둘 것.", MessageType.Warning);
            return;
        }

        LevelRoute route = Object.FindFirstObjectByType<LevelRoute>(FindObjectsInactive.Include);
        if (route == null || route.pads == null || route.pads.Length == 0)
        {
            EditorGUILayout.HelpBox("아직 재지 않았다. [레벨 만들기]를 누르면 폴더를 훑어 실측한다.", MessageType.None);
            return;
        }

        // 무엇을 쓰게 되는지 보여준다. 이름이 아니라 실측으로 역할이 정해지므로,
        // 어느 것이 디딤돌이고 어느 것이 벽이 될지는 눈으로 확인할 수 있어야 한다.
        for (int i = 0; i < route.pads.Length; i++)
        {
            LevelRoute.PadSpec pad = route.pads[i];

            string role = i == route.NarrowPad ? " ← 디딤돌"
                : i == route.ThickPad ? " ← 벽 · 고원"
                : i == route.WidePad ? " ← 가장 넓음"
                : "";

            EditorGUILayout.LabelField(
                $"   {pad.name} — 폭 {pad.width:0.00} m · 아래로 {pad.drop:0.00} m{role}",
                EditorStyles.miniLabel);
        }
    }

    /// <summary>
    /// 주인공은 언제나 원점에서 시작한다. 어긋난 채로 지으면 첫 걸음이 허공이거나 발판 속이라,
    /// 짓기 전에 눈에 띄어야 한다.
    /// </summary>
    void DrawStartCheck()
    {
        if (Bounds.startTrigger == null) return;

        Vector2 foot = LevelBounds.FootOf(Bounds.startTrigger);
        if (foot.sqrMagnitude <= 0.0001f)
        {
            EditorGUILayout.HelpBox("출발 발판의 바닥이 원점에 있다.", MessageType.None);
            return;
        }

        EditorGUILayout.HelpBox(
            $"출발 발판의 바닥이 ({foot.x:0.00}, {foot.y:0.00})다. 주인공의 초기 위치는 (0, 0, 0)이다.",
            MessageType.Warning);

        if (!GUILayout.Button("Start 트리거를 원점에 맞추기")) return;

        Undo.RecordObject(Bounds.startTrigger.transform, "Snap Start To Origin");

        Bounds bounds = Bounds.startTrigger.bounds;
        Bounds.startTrigger.transform.position += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);

        EditorUtility.SetDirty(Bounds.startTrigger.transform);
    }

    void DrawBuild()
    {
        using (new EditorGUI.DisabledScope(!Bounds.Ready || EditorApplication.isPlaying))
        {
            GUI.backgroundColor = new Color(0.6f, 0.9f, 0.7f);

            if (GUILayout.Button("레벨 만들기", GUILayout.Height(36)))
            {
                _report = LevelBuilder.Run(Bounds);
                GUIUtility.ExitGUI();
            }

            GUI.backgroundColor = Color.white;
        }

        if (!Bounds.Ready)
            EditorGUILayout.HelpBox("시점 · 종점 · 영역 트리거를 모두 지정해야 한다.", MessageType.Warning);
        else if (EditorApplication.isPlaying)
            EditorGUILayout.HelpBox("플레이 모드에서는 짓지 않는다 — 종료하면 전부 사라진다.", MessageType.Warning);
    }

    void DrawReport()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("지은 결과", EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"   씨앗 {_report.seed} · 구간: {_report.mix}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(
            $"   노드 {_report.nodes} · 곁길 {_report.branches} · 타일 {_report.tiles}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(
            $"   {_report.width:0.0} × {_report.height:0.0} m · 기울기 {_report.Slope:0.00} m/m", EditorStyles.miniLabel);

        if (_report.Clean)
        {
            EditorGUILayout.HelpBox("검사를 전부 통과했다.", MessageType.Info);
            return;
        }

        EditorGUILayout.HelpBox(
            $"불가 {_report.unreachable} · 키를 놓아야 함 {_report.mustRelease} · 겹침 {_report.cramped} · "
            + $"관통 {_report.intersecting} · 영역 이탈 {_report.outside}\n"
            + "자세한 것은 Level Route 인스펙터의 구간 판정과 콘솔에 있다.",
            MessageType.Error);
    }
}
