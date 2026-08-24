using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 같은 메시·같은 머티리얼로 수백 개씩 깔린 정적 오브젝트를 하나로 합쳐 개수를 줄이는 툴.
///
/// 합치는 단위는 (메시 + 머티리얼 + 레이어 + 정적 플래그) 조합이고, 그 안에서 다시 <b>공간 격자</b>로
/// 쪼갠다. 전부 하나로 합치면 화면 구석에 한 조각만 걸쳐도 통째로 그려지므로 컬링이 죽는다.
/// 격자로 나눠야 멀리 있는 덩어리를 버릴 수 있다.
///
/// <b>콜라이더가 있는 것은 건드리지 않는다.</b> 합치면 콜라이더와 메시의 대응이 끊어지기 때문이다.
/// 자기 자신이든 자식이든 콜라이더가 하나라도 있으면 후보에서 뺀다.
///
/// 대가가 있다:
///   - 라이트맵을 다시 구워야 한다. 합친 메시는 원본의 라이트맵 UV·인덱스를 잇지 못한다.
///   - 컬링이 격자 단위로 거칠어진다.
///   - 합친 결과물은 프리팹 연결이 없는 평범한 오브젝트다.
/// 그래서 원본은 기본적으로 지우지 않고 비활성으로만 둔다.
/// </summary>
public class StaticMeshMergerWindow : EditorWindow
{
    const string CONTAINER_NAME = "_Merged";
    const int VERTEX_BUDGET = 500000;

    /// <summary>합칠 수 있는 한 묶음. 같은 메시·머티리얼·레이어·정적 플래그.</summary>
    class MergeGroup
    {
        public Mesh mesh;
        public Material[] materials;
        public int layer;
        public StaticEditorFlags staticFlags;
        public readonly List<MeshRenderer> renderers = new List<MeshRenderer>();

        public string Label => $"{(mesh != null ? mesh.name : "<null>")} × {renderers.Count}";
        public int VertexCount => mesh != null ? mesh.vertexCount * renderers.Count : 0;
    }

    bool _selectionOnly = false;
    bool _includeInactive = false;
    bool _requireStatic = true;
    bool _skipLodGroups = true;

    float _cellSize = 25f;
    int _minInstances = 8;

    bool _deleteOriginals = false;

    string _outputFolder = "Assets/Art/Merged";

    readonly List<MergeGroup> _groups = new List<MergeGroup>();
    bool _scanned;
    bool _canceled;

    int _seenRenderers;
    int _skippedCollider;
    int _skippedLod;
    int _skippedNotStatic;

    Vector2 _scrollPos;
    string _lastReport;

    [MenuItem("Tools/Static Mesh Merger")]
    public static void ShowWindow()
    {
        StaticMeshMergerWindow window = GetWindow<StaticMeshMergerWindow>("Mesh Merger");
        window.minSize = new Vector2(400, 560);
    }

    void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        DrawHeader();
        DrawScanOptions();
        DrawChunkOptions();
        DrawOutputOptions();
        DrawScanButton();
        DrawResults();

        if (!string.IsNullOrEmpty(_lastReport))
        {
            EditorGUILayout.HelpBox(_lastReport, MessageType.None);
        }

        EditorGUILayout.EndScrollView();
    }

    #region UI

    void DrawHeader()
    {
        GUILayout.Space(5);
        GUILayout.Label("🧱 Static Mesh Merger", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "같은 메시·머티리얼로 깔린 정적 오브젝트를 공간 격자 단위로 합칩니다.\n" +
            "콜라이더가 있는 오브젝트는 건드리지 않습니다.",
            MessageType.Info
        );
        GUILayout.Space(5);
    }

    void DrawScanOptions()
    {
        GUILayout.Label("스캔 조건", EditorStyles.miniBoldLabel);

        _selectionOnly = EditorGUILayout.Toggle(
            new GUIContent("선택 항목 안에서만", "끄면 열려 있는 씬 전체를 훑습니다."),
            _selectionOnly
        );

        _requireStatic = EditorGUILayout.Toggle(
            new GUIContent("Batching Static 만", "정적으로 표시된 것만 합칩니다. 움직이는 오브젝트를 합치면 못 움직이게 됩니다."),
            _requireStatic
        );

        _skipLodGroups = EditorGUILayout.Toggle(
            new GUIContent("LOD 그룹 제외", "LODGroup에 속한 렌더러는 건너뜁니다. 레벨을 섞어 합치면 LOD가 깨집니다."),
            _skipLodGroups
        );

        _includeInactive = EditorGUILayout.Toggle(
            new GUIContent("비활성 포함", "꺼져 있는 오브젝트도 합칩니다. 보통 꺼둡니다."),
            _includeInactive
        );

        _minInstances = EditorGUILayout.IntSlider(
            new GUIContent("최소 개수", "이만큼 이상 모인 묶음만 합칩니다. 두세 개짜리는 합쳐도 이득이 없습니다."),
            _minInstances, 2, 100
        );

        GUILayout.Space(8);
    }

    void DrawChunkOptions()
    {
        GUILayout.Label("격자", EditorStyles.miniBoldLabel);

        _cellSize = EditorGUILayout.FloatField(
            new GUIContent("셀 크기", "이 크기의 격자 안에서만 합칩니다. 작을수록 컬링이 잘 듣고, 클수록 개수가 더 줄어듭니다."),
            _cellSize
        );
        _cellSize = Mathf.Max(_cellSize, 1f);

        EditorGUILayout.LabelField(
            "   전부 하나로 합치면 화면에 한 조각만 걸쳐도 통째로 그려집니다.",
            EditorStyles.miniLabel
        );

        GUILayout.Space(8);
    }

    void DrawOutputOptions()
    {
        GUILayout.Label("결과", EditorStyles.miniBoldLabel);

        _outputFolder = EditorGUILayout.TextField(
            new GUIContent("메시 저장 폴더", "합친 메시는 에셋으로 저장해야 씬에 묻히지 않습니다."),
            _outputFolder
        );

        _deleteOriginals = EditorGUILayout.Toggle(
            new GUIContent("원본 삭제", "끄면 비활성으로만 둡니다. 켜면 지웁니다 — 되돌리려면 Ctrl+Z 뿐입니다."),
            _deleteOriginals
        );

        EditorGUILayout.HelpBox(
            "합친 메시는 원본의 라이트맵 UV·인덱스를 잇지 못합니다. 라이트맵을 구워 두셨다면 다시 구워야 합니다.\n" +
            "합친 결과물은 프리팹 연결이 없는 평범한 오브젝트가 됩니다.",
            MessageType.Warning
        );

        GUILayout.Space(8);
    }

    void DrawScanButton()
    {
        GUI.backgroundColor = new Color(0.4f, 0.7f, 0.9f);
        if (GUILayout.Button("🔍 스캔", GUILayout.Height(28)))
        {
            Scan();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(8);
    }

    void DrawResults()
    {
        if (!_scanned) return;

        EditorGUILayout.LabelField(
            $"렌더러 {_seenRenderers}개 확인 · 콜라이더 {_skippedCollider} · LOD {_skippedLod} · 비정적 {_skippedNotStatic} 제외",
            EditorStyles.miniLabel
        );

        if (_groups.Count == 0)
        {
            EditorGUILayout.HelpBox("합칠 묶음이 없습니다.", MessageType.None);
            return;
        }

        int totalInstances = 0;
        int totalCells = 0;
        foreach (MergeGroup g in _groups)
        {
            totalInstances += g.renderers.Count;
            totalCells += CountCells(g);
        }

        GUILayout.Space(5);
        EditorGUILayout.LabelField($"묶음 {_groups.Count}종 · 대상 {totalInstances}개  →  합친 뒤 {totalCells}개", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"   오브젝트 {totalInstances - totalCells}개 감소", EditorStyles.miniLabel);

        EditorGUILayout.BeginVertical("box");

        int shown = _groups.Count < 15 ? _groups.Count : 15;
        for (int i = 0; i < shown; i++)
        {
            MergeGroup g = _groups[i];
            int cells = CountCells(g);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(g.mesh, typeof(Mesh), false);
            EditorGUI.EndDisabledGroup();
            GUILayout.Label($"×{g.renderers.Count} → {cells}", EditorStyles.miniLabel, GUILayout.Width(90));
            EditorGUILayout.EndHorizontal();
        }

        if (_groups.Count > shown)
        {
            EditorGUILayout.LabelField($"   … 외 {_groups.Count - shown}종", EditorStyles.miniLabel);
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        if (GUILayout.Button($"🧱 Merge ({totalInstances}개 → {totalCells}개)", GUILayout.Height(32)))
        {
            RunCancelable("Static Mesh Merge", Merge);
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);
    }

    #endregion

    #region Scan

    void Scan()
    {
        _groups.Clear();
        _seenRenderers = _skippedCollider = _skippedLod = _skippedNotStatic = 0;
        _scanned = true;
        _lastReport = null;

        MeshRenderer[] renderers = CollectRenderers();
        Dictionary<string, MergeGroup> map = new Dictionary<string, MergeGroup>();

        try
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if ((i & 0x7F) == 0 &&
                    EditorUtility.DisplayCancelableProgressBar("Mesh Merger", $"렌더러 확인 중 {i}/{renderers.Length}", (float)i / renderers.Length))
                {
                    _scanned = false;
                    return;
                }

                MeshRenderer renderer = renderers[i];
                if (renderer == null) continue;

                _seenRenderers++;

                if (!IsEligible(renderer, out Mesh mesh)) continue;

                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);

                string key = BuildKey(mesh, renderer.sharedMaterials, renderer.gameObject.layer, flags);

                if (!map.TryGetValue(key, out MergeGroup group))
                {
                    group = new MergeGroup
                    {
                        mesh = mesh,
                        materials = renderer.sharedMaterials,
                        layer = renderer.gameObject.layer,
                        staticFlags = flags
                    };
                    map[key] = group;
                }

                group.renderers.Add(renderer);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        foreach (MergeGroup g in map.Values)
        {
            if (g.renderers.Count < _minInstances) continue;
            _groups.Add(g);
        }

        // 개수가 많은 묶음부터. 이득이 큰 순서다.
        _groups.Sort((a, b) => b.renderers.Count.CompareTo(a.renderers.Count));

        Debug.Log($"[MeshMerger] 렌더러 {_seenRenderers}개 확인, 합칠 묶음 {_groups.Count}종.");
    }

    MeshRenderer[] CollectRenderers()
    {
        if (!_selectionOnly)
        {
            FindObjectsInactive inactive = _includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
            return Object.FindObjectsByType<MeshRenderer>(inactive, FindObjectsSortMode.None);
        }

        List<MeshRenderer> found = new List<MeshRenderer>();
        foreach (GameObject go in Selection.gameObjects)
        {
            if (go == null) continue;
            found.AddRange(go.GetComponentsInChildren<MeshRenderer>(_includeInactive));
        }
        return found.ToArray();
    }

    /// <summary>합쳐도 되는 렌더러인지.</summary>
    bool IsEligible(MeshRenderer renderer, out Mesh mesh)
    {
        mesh = null;

        if (!_includeInactive && !renderer.gameObject.activeInHierarchy) return false;

        // 콜라이더가 있으면 손대지 않는다. 자기 자신이든 자식이든.
        if (renderer.GetComponentInChildren<Collider>(true) != null)
        {
            _skippedCollider++;
            return false;
        }

        if (_skipLodGroups && renderer.GetComponentInParent<LODGroup>() != null)
        {
            _skippedLod++;
            return false;
        }

        if (_requireStatic)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
            if ((flags & StaticEditorFlags.BatchingStatic) == 0)
            {
                _skippedNotStatic++;
                return false;
            }
        }

        if (!renderer.TryGetComponent(out MeshFilter filter)) return false;
        if (filter.sharedMesh == null) return false;
        if (!filter.sharedMesh.isReadable) return false;

        if (renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0) return false;

        mesh = filter.sharedMesh;
        return true;
    }

    static string BuildKey(Mesh mesh, Material[] materials, int layer, StaticEditorFlags flags)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(mesh.GetInstanceID()).Append('|');

        foreach (Material m in materials)
        {
            sb.Append(m != null ? m.GetInstanceID() : 0).Append(',');
        }

        sb.Append('|').Append(layer).Append('|').Append((int)flags);
        return sb.ToString();
    }

    #endregion

    #region Cells

    /// <summary>격자 좌표별로 렌더러를 모은다. 정점 예산을 넘으면 한 칸을 더 쪼갠다.</summary>
    Dictionary<Vector3Int, List<MeshRenderer>> BuildCells(MergeGroup group)
    {
        Dictionary<Vector3Int, List<MeshRenderer>> cells = new Dictionary<Vector3Int, List<MeshRenderer>>();

        foreach (MeshRenderer r in group.renderers)
        {
            if (r == null) continue;

            Vector3 p = r.transform.position;
            Vector3Int cell = new Vector3Int(
                Mathf.FloorToInt(p.x / _cellSize),
                Mathf.FloorToInt(p.y / _cellSize),
                Mathf.FloorToInt(p.z / _cellSize)
            );

            if (!cells.TryGetValue(cell, out List<MeshRenderer> list))
            {
                list = new List<MeshRenderer>();
                cells[cell] = list;
            }

            list.Add(r);
        }

        return cells;
    }

    int CountCells(MergeGroup group)
    {
        int cells = 0;
        int perCellCap = group.mesh != null && group.mesh.vertexCount > 0
            ? Mathf.Max(VERTEX_BUDGET / group.mesh.vertexCount, 1)
            : int.MaxValue;

        foreach (var pair in BuildCells(group))
        {
            cells += Mathf.CeilToInt((float)pair.Value.Count / perCellCap);
        }

        return cells;
    }

    #endregion

    #region Merge

    void RunCancelable(string groupName, System.Action work)
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(groupName);

        _canceled = false;

        try
        {
            work();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (_canceled)
        {
            Undo.RevertAllDownToGroup(group);
            Debug.Log("[MeshMerger] 취소되었습니다. 작업 전 상태로 되돌렸습니다.");
            return;
        }

        Undo.CollapseUndoOperations(group);
    }

    void Merge()
    {
        if (!EnsureFolder(_outputFolder))
        {
            Debug.LogError($"[MeshMerger] 저장 폴더를 만들 수 없습니다: {_outputFolder}");
            return;
        }

        GameObject container = GameObject.Find(CONTAINER_NAME);
        if (container == null)
        {
            container = new GameObject(CONTAINER_NAME);
            Undo.RegisterCreatedObjectUndo(container, "Static Mesh Merge");
        }

        int madeObjects = 0;
        int consumed = 0;
        int savedMeshes = 0;

        for (int gi = 0; gi < _groups.Count; gi++)
        {
            MergeGroup group = _groups[gi];
            if (group.mesh == null) continue;

            if (PollCancel($"묶음 {gi + 1}/{_groups.Count}: {group.mesh.name}", (float)gi / _groups.Count)) return;

            int perCellCap = Mathf.Max(VERTEX_BUDGET / Mathf.Max(group.mesh.vertexCount, 1), 1);
            Dictionary<Vector3Int, List<MeshRenderer>> cells = BuildCells(group);

            int chunkIndex = 0;

            foreach (var pair in cells)
            {
                List<MeshRenderer> list = pair.Value;

                for (int start = 0; start < list.Count; start += perCellCap)
                {
                    int count = Mathf.Min(perCellCap, list.Count - start);
                    List<MeshRenderer> slice = list.GetRange(start, count);

                    GameObject made = MergeSlice(group, slice, container.transform, chunkIndex++, ref savedMeshes);
                    if (made == null) continue;

                    madeObjects++;
                    consumed += slice.Count;

                    RetireOriginals(slice);
                }
            }
        }

        AssetDatabase.SaveAssets();

        _lastReport =
            $"오브젝트 {consumed}개 → {madeObjects}개  ({consumed - madeObjects}개 감소)\n" +
            $"메시 에셋 {savedMeshes}개 저장 · {_outputFolder}\n" +
            (_deleteOriginals ? "원본은 삭제했습니다." : "원본은 비활성으로 남겨뒀습니다.");

        Debug.Log($"[MeshMerger] {_lastReport.Replace("\n", "  ")}");

        Scan();
    }

    /// <summary>한 격자 칸의 인스턴스들을 메시 하나로 합쳐 오브젝트를 만든다.</summary>
    GameObject MergeSlice(MergeGroup group, List<MeshRenderer> slice, Transform parent, int chunkIndex, ref int savedMeshes)
    {
        if (slice.Count == 0) return null;

        // 합친 메시의 원점. 정점 좌표가 커지지 않게 칸의 중심을 잡는다.
        Vector3 origin = Vector3.zero;
        foreach (MeshRenderer r in slice) origin += r.transform.position;
        origin /= slice.Count;

        Matrix4x4 worldToLocal = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one).inverse;

        int subMeshCount = group.mesh.subMeshCount;

        // 서브메시별로 먼저 합치고, 그 결과들을 다시 합쳐 서브메시 구조를 유지한다.
        CombineInstance[] parts = new CombineInstance[subMeshCount];

        for (int sub = 0; sub < subMeshCount; sub++)
        {
            CombineInstance[] instances = new CombineInstance[slice.Count];

            for (int i = 0; i < slice.Count; i++)
            {
                instances[i] = new CombineInstance
                {
                    mesh = group.mesh,
                    subMeshIndex = sub,
                    transform = worldToLocal * slice[i].transform.localToWorldMatrix
                };
            }

            Mesh part = new Mesh();
            part.indexFormat = IndexFormat.UInt32;
            part.CombineMeshes(instances, true, true, false);

            parts[sub] = new CombineInstance { mesh = part, subMeshIndex = 0, transform = Matrix4x4.identity };
        }

        Mesh combined = new Mesh();
        combined.indexFormat = IndexFormat.UInt32;
        combined.name = $"{group.mesh.name}_merged_{chunkIndex}";
        combined.CombineMeshes(parts, false, false, false);
        combined.RecalculateBounds();

        foreach (CombineInstance part in parts)
        {
            if (part.mesh != null) Object.DestroyImmediate(part.mesh);
        }

        string path = AssetDatabase.GenerateUniqueAssetPath($"{_outputFolder}/{combined.name}.asset");
        AssetDatabase.CreateAsset(combined, path);
        savedMeshes++;

        GameObject go = new GameObject(combined.name);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, slice[0].gameObject.scene);
        go.transform.position = origin;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.layer = group.layer;

        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = combined;

        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = group.materials;

        // 원본의 렌더링 설정을 그대로 옮긴다.
        MeshRenderer source = slice[0];
        renderer.shadowCastingMode = source.shadowCastingMode;
        renderer.receiveShadows = source.receiveShadows;
        renderer.lightProbeUsage = source.lightProbeUsage;
        renderer.reflectionProbeUsage = source.reflectionProbeUsage;
        renderer.motionVectorGenerationMode = source.motionVectorGenerationMode;

        GameObjectUtility.SetStaticEditorFlags(go, group.staticFlags);

        Undo.RegisterCreatedObjectUndo(go, "Static Mesh Merge");
        go.transform.SetParent(parent, true);

        return go;
    }

    void RetireOriginals(List<MeshRenderer> slice)
    {
        foreach (MeshRenderer r in slice)
        {
            if (r == null) continue;

            if (_deleteOriginals)
            {
                Undo.DestroyObjectImmediate(r.gameObject);
            }
            else
            {
                Undo.RecordObject(r.gameObject, "Static Mesh Merge");
                r.gameObject.SetActive(false);
            }
        }
    }

    bool PollCancel(string info, float progress)
    {
        if (_canceled) return true;

        if (EditorUtility.DisplayCancelableProgressBar("Mesh Merger", info, progress))
        {
            _canceled = true;
        }

        return _canceled;
    }

    static bool EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return true;

        string[] parts = folder.Split('/');
        if (parts.Length < 2 || parts[0] != "Assets") return false;

        string current = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }

        return AssetDatabase.IsValidFolder(folder);
    }

    #endregion
}
