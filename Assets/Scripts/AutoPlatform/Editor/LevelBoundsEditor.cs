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
        DrawDefaultInspector();

        EditorGUILayout.Space();
        DrawStartCheck();

        EditorGUILayout.Space();
        DrawBuild();

        if (_report.ran) DrawReport();
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
