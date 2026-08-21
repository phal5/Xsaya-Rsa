using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 열려 있는 씬의 배칭 상태를 훑어 일괄 조정하는 툴. 두 개의 탭으로 나뉜다.
///
/// [GPU 인스턴싱] 인스턴싱이 이득인 머티리얼을 찾아 켜준다.
///   인스턴싱은 "같은 메시 + 같은 머티리얼"이 여럿일 때만 이득이므로 렌더러 하나만 쓰는
///   머티리얼은 후보에서 뺀다. 정적 배칭이 걸린 렌더러도 뺀다 - 정적 배칭이 우선하기 때문에
///   거기에 인스턴싱을 켜봐야 무시된다.
///   켜는 대상은 씬 오브젝트가 아니라 머티리얼 에셋이다. 프로젝트 전역에 남는 변경이다.
///
/// [정적 플래그] 씬의 렌더러를 출처 프리팹별로 묶어, 프리팹 종류 단위로 정적 플래그를 건다.
///   "이 덤불 프리팹 313개를 전부 정적으로" 같은 작업을 한 번에 하기 위한 것이다.
///   손대는 것은 씬 인스턴스뿐이고 프리팹 에셋 자체는 건드리지 않는다.
///
/// 두 작업 모두 적용 전에 목록을 먼저 보여주고, Undo 한 덩어리로 묶어 되돌릴 수 있게 한다.
/// </summary>
public class GpuInstancingSweeper : EditorWindow
{
    enum Tab
    {
        Instancing,
        StaticFlags,
    }

    class Candidate
    {
        public Material material;
        public int rendererCount;
        public bool apply = true;
    }

    /// <summary>같은 프리팹에서 나온 씬 인스턴스들의 묶음.</summary>
    class PrefabGroup
    {
        public string assetPath;
        public GameObject prefabAsset;
        public readonly List<GameObject> roots = new List<GameObject>();
        public int rendererCount;
        public int staticRendererCount;
        public bool apply = true;

        public bool FullyStatic => rendererCount > 0 && staticRendererCount == rendererCount;
    }

    Tab _tab = Tab.Instancing;
    Vector2 _scrollPos;

    // 두 탭이 공유하는 스캔 조건
    bool _includeInactive = true;

    // --- 인스턴싱 탭 ---
    readonly List<Candidate> _candidates = new List<Candidate>();
    bool _scanned;
    int _minRenderers = 2;
    bool _includeStaticBatched = false;
    int _renderersSeen;
    int _skippedSkinned;
    int _skippedStaticBatched;
    int _alreadyEnabled;

    // --- 정적 플래그 탭 ---
    readonly List<PrefabGroup> _groups = new List<PrefabGroup>();
    bool _prefabScanned;
    int _looseRenderers;
    bool _additiveFlags = true;
    StaticEditorFlags _targetStaticFlags =
        StaticEditorFlags.BatchingStatic |
        StaticEditorFlags.OccluderStatic |
        StaticEditorFlags.OccludeeStatic |
        StaticEditorFlags.ContributeGI;

    [MenuItem("Tools/Batching Sweeper")]
    public static void ShowWindow()
    {
        GpuInstancingSweeper window = GetWindow<GpuInstancingSweeper>("Batching");
        window.minSize = new Vector2(380, 460);
    }

    void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        DrawTabs();

        if (_tab == Tab.Instancing) DrawInstancingTab();
        else DrawStaticFlagsTab();

        EditorGUILayout.EndScrollView();
    }

    void DrawTabs()
    {
        GUILayout.Space(5);
        _tab = (Tab)GUILayout.Toolbar((int)_tab, new string[] { "GPU 인스턴싱", "정적 플래그" }, GUILayout.Height(24));
        GUILayout.Space(8);
    }

    #region Instancing Tab

    void DrawInstancingTab()
    {
        GUILayout.Label("머티리얼 GPU 인스턴싱", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "열려 있는 씬에서 인스턴싱이 이득인 머티리얼을 찾습니다.\n" +
            "적용은 머티리얼 에셋을 수정하므로 다른 씬에도 영향이 갑니다.",
            MessageType.Info
        );
        GUILayout.Space(5);

        GUILayout.Label("스캔 조건", EditorStyles.miniBoldLabel);

        _minRenderers = EditorGUILayout.IntSlider(
            new GUIContent("최소 사용 렌더러 수", "이만큼 이상의 렌더러가 함께 쓰는 머티리얼만 후보로 잡는다. 1개짜리는 인스턴싱해도 이득이 없다."),
            _minRenderers, 1, 20
        );

        _includeInactive = EditorGUILayout.Toggle(
            new GUIContent("비활성 오브젝트 포함", "꺼진 채로 배치된 오브젝트도 센다."),
            _includeInactive
        );

        _includeStaticBatched = EditorGUILayout.Toggle(
            new GUIContent("Batching Static 포함", "정적 배칭이 걸린 렌더러도 후보에 넣는다. 정적 배칭이 우선하므로 보통은 꺼둔다."),
            _includeStaticBatched
        );

        GUILayout.Space(8);

        GUI.backgroundColor = new Color(0.4f, 0.7f, 0.9f);
        if (GUILayout.Button("🔍 씬 스캔", GUILayout.Height(28)))
        {
            ScanMaterials();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(8);

        DrawInstancingResults();
    }

    void DrawInstancingResults()
    {
        if (!_scanned) return;

        EditorGUILayout.LabelField(
            $"렌더러 {_renderersSeen}개 확인 · 스킨드 {_skippedSkinned}개 · 정적 배칭 {_skippedStaticBatched}개 제외 · 이미 켜짐 {_alreadyEnabled}개",
            EditorStyles.miniLabel
        );

        if (_candidates.Count == 0)
        {
            EditorGUILayout.HelpBox("조건에 맞는 머티리얼이 없습니다.", MessageType.None);
            return;
        }

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"후보 {_candidates.Count}개", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("전체 선택", EditorStyles.miniButton)) SetAllCandidates(true);
        if (GUILayout.Button("전체 해제", EditorStyles.miniButton)) SetAllCandidates(false);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");

        foreach (Candidate candidate in _candidates)
        {
            if (candidate.material == null) continue;

            EditorGUILayout.BeginHorizontal();

            candidate.apply = EditorGUILayout.Toggle(candidate.apply, GUILayout.Width(16));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(candidate.material, typeof(Material), false);
            EditorGUI.EndDisabledGroup();

            GUILayout.Label($"×{candidate.rendererCount}", EditorStyles.miniLabel, GUILayout.Width(44));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        int selected = SelectedCandidateCount();

        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        EditorGUI.BeginDisabledGroup(selected == 0);
        if (GUILayout.Button($"⚡ 선택한 {selected}개에 GPU 인스턴싱 적용", GUILayout.Height(30)))
        {
            ApplyInstancing();
        }
        EditorGUI.EndDisabledGroup();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox(
            "인스턴싱을 지원하지 않는 셰이더에서는 이 설정이 그냥 무시됩니다(에러 없음).\n" +
            "적용 후에는 Ctrl+Z로 되돌릴 수 있습니다.",
            MessageType.Warning
        );
    }

    void ScanMaterials()
    {
        _candidates.Clear();
        _renderersSeen = _skippedSkinned = _skippedStaticBatched = _alreadyEnabled = 0;
        _scanned = true;

        Renderer[] renderers = FindRenderers();
        Dictionary<Material, int> usage = new Dictionary<Material, int>();

        try
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if ((i & 0x7F) == 0 &&
                    EditorUtility.DisplayCancelableProgressBar("Batching Sweeper", $"렌더러 확인 중 {i}/{renderers.Length}", (float)i / renderers.Length))
                {
                    _scanned = false;
                    return;
                }

                Renderer renderer = renderers[i];
                if (renderer == null) continue;

                _renderersSeen++;

                // 스킨드 메시는 인스턴싱 대상이 아니다. 본 스키닝은 인스턴스별로 다르다.
                if (renderer is SkinnedMeshRenderer)
                {
                    _skippedSkinned++;
                    continue;
                }

                // 파티클/라인/트레일 등도 제외한다. 일반 메시 렌더러만 본다.
                if (renderer is not MeshRenderer) continue;

                if (!_includeStaticBatched && IsBatchingStatic(renderer.gameObject))
                {
                    _skippedStaticBatched++;
                    continue;
                }

                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null) continue;

                    usage.TryGetValue(material, out int count);
                    usage[material] = count + 1;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        foreach (var pair in usage)
        {
            if (pair.Value < _minRenderers) continue;

            if (pair.Key.enableInstancing)
            {
                _alreadyEnabled++;
                continue;
            }

            _candidates.Add(new Candidate { material = pair.Key, rendererCount = pair.Value });
        }

        // 많이 쓰이는 것부터 보여준다. 이득이 큰 순서다.
        _candidates.Sort((a, b) => b.rendererCount.CompareTo(a.rendererCount));

        Debug.Log($"[BatchingSweeper] 렌더러 {_renderersSeen}개 확인, 후보 머티리얼 {_candidates.Count}개.");
    }

    void ApplyInstancing()
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Enable GPU Instancing");

        int applied = 0;

        foreach (Candidate candidate in _candidates)
        {
            if (!candidate.apply || candidate.material == null) continue;
            if (candidate.material.enableInstancing) continue;

            Undo.RecordObject(candidate.material, "Enable GPU Instancing");
            candidate.material.enableInstancing = true;
            EditorUtility.SetDirty(candidate.material);
            applied++;
        }

        Undo.CollapseUndoOperations(group);
        AssetDatabase.SaveAssets();

        Debug.Log($"[BatchingSweeper] 머티리얼 {applied}개에 GPU 인스턴싱을 켰습니다.");

        ScanMaterials();
    }

    void SetAllCandidates(bool value)
    {
        foreach (Candidate candidate in _candidates) candidate.apply = value;
    }

    int SelectedCandidateCount()
    {
        int count = 0;
        foreach (Candidate candidate in _candidates)
        {
            if (candidate.apply) count++;
        }
        return count;
    }

    #endregion

    #region Static Flags Tab

    void DrawStaticFlagsTab()
    {
        GUILayout.Label("프리팹별 정적 플래그", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "씬의 메시 렌더러를 출처 프리팹별로 묶습니다.\n" +
            "프리팹 종류를 골라 그 인스턴스 전체에 정적 플래그를 한 번에 겁니다.",
            MessageType.Info
        );
        GUILayout.Space(5);

        GUILayout.Label("적용할 플래그", EditorStyles.miniBoldLabel);

        _targetStaticFlags = (StaticEditorFlags)EditorGUILayout.EnumFlagsField("플래그", _targetStaticFlags);

        _additiveFlags = EditorGUILayout.Toggle(
            new GUIContent("기존 플래그에 더하기", "켜면 이미 걸린 플래그를 유지한 채 추가한다. 끄면 지정한 플래그로 덮어쓴다."),
            _additiveFlags
        );

        _includeInactive = EditorGUILayout.Toggle(
            new GUIContent("비활성 오브젝트 포함", "꺼진 채로 배치된 오브젝트도 센다."),
            _includeInactive
        );

        GUILayout.Space(8);

        GUI.backgroundColor = new Color(0.4f, 0.7f, 0.9f);
        if (GUILayout.Button("🔍 프리팹별 스캔", GUILayout.Height(28)))
        {
            ScanPrefabs();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(8);

        DrawPrefabResults();
    }

    void DrawPrefabResults()
    {
        if (!_prefabScanned) return;

        EditorGUILayout.LabelField(
            $"프리팹 {_groups.Count}종 · 프리팹에 속하지 않은 렌더러 {_looseRenderers}개",
            EditorStyles.miniLabel
        );

        if (_groups.Count == 0)
        {
            EditorGUILayout.HelpBox("프리팹 인스턴스를 찾지 못했습니다.", MessageType.None);
            return;
        }

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("프리팹 종류", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("전체 선택", EditorStyles.miniButton)) SetAllGroups(true);
        if (GUILayout.Button("전체 해제", EditorStyles.miniButton)) SetAllGroups(false);
        if (GUILayout.Button("미완료만", EditorStyles.miniButton)) SelectIncompleteGroups();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");

        foreach (PrefabGroup group in _groups)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(group.FullyStatic);
            group.apply = EditorGUILayout.Toggle(group.apply, GUILayout.Width(16));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(group.prefabAsset, typeof(GameObject), false);
            EditorGUI.EndDisabledGroup();

            GUILayout.Label($"{group.roots.Count}개", EditorStyles.miniLabel, GUILayout.Width(46));

            string state = group.FullyStatic
                ? "완료"
                : $"{group.staticRendererCount}/{group.rendererCount}";
            GUILayout.Label(state, EditorStyles.miniLabel, GUILayout.Width(58));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.LabelField(
            "개수 = 씬 인스턴스 수, 오른쪽 = 플래그가 걸린 렌더러 / 전체 렌더러",
            EditorStyles.miniLabel
        );

        GUILayout.Space(8);

        int selected = SelectedGroupCount();

        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        EditorGUI.BeginDisabledGroup(selected == 0);
        if (GUILayout.Button($"🧱 선택한 프리팹 {selected}종에 정적 플래그 적용", GUILayout.Height(30)))
        {
            ApplyStaticFlags();
        }
        EditorGUI.EndDisabledGroup();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox(
            "씬 인스턴스에만 적용합니다. 프리팹 에셋 자체는 건드리지 않습니다.\n" +
            "Batching Static을 걸면 그 렌더러는 GPU 인스턴싱을 타지 않습니다.\n" +
            "적용 후에는 Ctrl+Z로 되돌릴 수 있습니다.",
            MessageType.Warning
        );
    }

    void ScanPrefabs()
    {
        _groups.Clear();
        _looseRenderers = 0;
        _prefabScanned = true;

        Renderer[] renderers = FindRenderers();

        Dictionary<string, PrefabGroup> map = new Dictionary<string, PrefabGroup>();
        HashSet<GameObject> seenRoots = new HashSet<GameObject>();

        try
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if ((i & 0x7F) == 0 &&
                    EditorUtility.DisplayCancelableProgressBar("Batching Sweeper", $"프리팹 확인 중 {i}/{renderers.Length}", (float)i / renderers.Length))
                {
                    _prefabScanned = false;
                    return;
                }

                Renderer renderer = renderers[i];
                if (renderer == null) continue;

                // 정적 배칭은 메시 렌더러 이야기다. 스킨드/파티클은 대상이 아니다.
                if (renderer is not MeshRenderer) continue;

                GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(renderer.gameObject);
                if (root == null)
                {
                    _looseRenderers++;
                    continue;
                }

                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
                if (string.IsNullOrEmpty(path))
                {
                    _looseRenderers++;
                    continue;
                }

                if (!map.TryGetValue(path, out PrefabGroup group))
                {
                    group = new PrefabGroup
                    {
                        assetPath = path,
                        prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path)
                    };
                    map[path] = group;
                }

                // 한 프리팹 인스턴스 안에 렌더러가 여럿일 수 있다. 루트는 한 번만 센다.
                if (seenRoots.Add(root)) group.roots.Add(root);

                group.rendererCount++;

                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                if ((flags & _targetStaticFlags) == _targetStaticFlags) group.staticRendererCount++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        _groups.AddRange(map.Values);

        // 인스턴스가 많은 프리팹부터. 정적화 이득이 큰 순서다.
        _groups.Sort((a, b) => b.rendererCount.CompareTo(a.rendererCount));

        SelectIncompleteGroups();

        Debug.Log($"[BatchingSweeper] 프리팹 {_groups.Count}종 발견. 프리팹 밖 렌더러 {_looseRenderers}개.");
    }

    void ApplyStaticFlags()
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Set Static Flags By Prefab");

        int changed = 0;
        int touchedRoots = 0;

        try
        {
            for (int g = 0; g < _groups.Count; g++)
            {
                PrefabGroup prefabGroup = _groups[g];
                if (!prefabGroup.apply || prefabGroup.FullyStatic) continue;

                if (EditorUtility.DisplayCancelableProgressBar(
                        "Batching Sweeper",
                        $"플래그 적용 중 {g + 1}/{_groups.Count}",
                        (float)g / _groups.Count))
                {
                    Undo.RevertAllDownToGroup(group);
                    Debug.Log("[BatchingSweeper] 취소되었습니다. 작업 전 상태로 되돌렸습니다.");
                    return;
                }

                foreach (GameObject root in prefabGroup.roots)
                {
                    if (root == null) continue;

                    touchedRoots++;

                    // 배칭은 렌더러 단위로 걸린다. 루트만이 아니라 자식까지 훑는다.
                    foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    {
                        GameObject go = t.gameObject;

                        StaticEditorFlags current = GameObjectUtility.GetStaticEditorFlags(go);
                        StaticEditorFlags next = _additiveFlags ? (current | _targetStaticFlags) : _targetStaticFlags;

                        if (current == next) continue;

                        Undo.RecordObject(go, "Set Static Flags");
                        GameObjectUtility.SetStaticEditorFlags(go, next);
                        changed++;
                    }
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Undo.CollapseUndoOperations(group);

        Debug.Log($"[BatchingSweeper] 인스턴스 {touchedRoots}개 아래 오브젝트 {changed}개의 정적 플래그를 갱신했습니다.");

        ScanPrefabs();
    }

    void SetAllGroups(bool value)
    {
        foreach (PrefabGroup group in _groups)
        {
            group.apply = value && !group.FullyStatic;
        }
    }

    void SelectIncompleteGroups()
    {
        foreach (PrefabGroup group in _groups)
        {
            group.apply = !group.FullyStatic;
        }
    }

    int SelectedGroupCount()
    {
        int count = 0;
        foreach (PrefabGroup group in _groups)
        {
            if (group.apply && !group.FullyStatic) count++;
        }
        return count;
    }

    #endregion

    #region Shared

    Renderer[] FindRenderers()
    {
        FindObjectsInactive inactive = _includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
        return Object.FindObjectsByType<Renderer>(inactive, FindObjectsSortMode.None);
    }

    static bool IsBatchingStatic(GameObject go)
    {
        return (GameObjectUtility.GetStaticEditorFlags(go) & StaticEditorFlags.BatchingStatic) != 0;
    }

    #endregion
}
