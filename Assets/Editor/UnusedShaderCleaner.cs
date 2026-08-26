using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 빌드 세팅에 등록된 씬을 기준으로 "쓰이지 않는 셰이더"를 찾아 목록으로 보여주고,
/// 확인한 것만 골라 한 번에 지우는 툴.
///
/// [판정 방식] 빌드 씬들을 뿌리로 삼아 AssetDatabase 의존성을 훑고, 거기 걸리지 않은
///   Assets/ 아래 셰이더만 후보로 남긴다. 씬 의존성만으로는 놓치는 경로가 있어서
///   아래 보호 규칙을 겹쳐 둔다. 각 규칙은 옵션에서 끌 수 있고, 규칙에 걸려 살아남은
///   셰이더는 "보호됨" 목록에서 이유와 함께 확인할 수 있다.
///
///   - Resources / StreamingAssets : 런타임에 경로로 불러오는 것들
///   - 렌더 파이프라인 설정        : URP 에셋, 렌더러 데이터, Always Included Shaders
///   - Editor 폴더                 : 빌드에는 안 들어가지만 지우면 에디터 툴이 깨진다
///   - 스크립트 문자열             : Shader.Find("...") 처럼 이름으로 찾는 경우
///   - 셰이더 → 셰이더 참조        : 서브그래프, Fallback, UsePass
///
/// [삭제] 기본값은 휴지통으로 보내기(MoveAssetsToTrash)라 되돌릴 수 있다. 그래도
///   지우기 전에 커밋해 두는 편이 안전하다. 플레이 모드에서는 동작하지 않는다.
/// </summary>
public class UnusedShaderCleaner : EditorWindow
{
    const string PrefPrefix = "UnusedShaderCleaner.";

    /// <summary>삭제 후보 한 줄.</summary>
    class Entry
    {
        public string path;
        public string shaderName;
        public Object asset;
        public int materialCount;   // 이 셰이더를 참조하는 머티리얼 수. 그 머티리얼들도 미사용일 가능성이 높다.
        public long bytes;
        public bool delete = true;
    }

    /// <summary>보호 규칙에 걸려 후보에서 빠진 셰이더.</summary>
    class Kept
    {
        public string path;
        public string reason;
    }

    // --- 옵션 ---
    bool _includeDisabledScenes = true;
    bool _keepResources = true;
    bool _keepRenderPipeline = true;
    bool _keepEditorFolder = true;
    bool _keepScriptStrings = true;
    bool _includeSubGraphs = false;
    string _ignorePrefixes = "Assets/TextMesh Pro/";
    bool _useTrash = true;

    // --- 결과 ---
    readonly List<Entry> _unused = new List<Entry>();
    readonly List<Kept> _kept = new List<Kept>();
    bool _scanned;
    int _totalShaders;
    int _sceneRootCount;
    int _dynamicFindCount;      // 이름이 변수로 들어가는 Shader.Find 호출 수. 자동 판정이 안 된다.

    bool _showOptions = true;
    bool _showKept;
    Vector2 _scroll;

    [MenuItem("Tools/Unused Shader Cleaner")]
    public static void ShowWindow()
    {
        UnusedShaderCleaner window = GetWindow<UnusedShaderCleaner>("Unused Shaders");
        window.minSize = new Vector2(460, 520);
    }

    void OnEnable()
    {
        _includeDisabledScenes = EditorPrefs.GetBool(PrefPrefix + "includeDisabled", true);
        _keepResources = EditorPrefs.GetBool(PrefPrefix + "keepResources", true);
        _keepRenderPipeline = EditorPrefs.GetBool(PrefPrefix + "keepRP", true);
        _keepEditorFolder = EditorPrefs.GetBool(PrefPrefix + "keepEditor", true);
        _keepScriptStrings = EditorPrefs.GetBool(PrefPrefix + "keepStrings", true);
        _includeSubGraphs = EditorPrefs.GetBool(PrefPrefix + "subGraphs", false);
        _useTrash = EditorPrefs.GetBool(PrefPrefix + "useTrash", true);
        _ignorePrefixes = EditorPrefs.GetString(PrefPrefix + "ignore", "Assets/TextMesh Pro/");
    }

    void OnDisable()
    {
        EditorPrefs.SetBool(PrefPrefix + "includeDisabled", _includeDisabledScenes);
        EditorPrefs.SetBool(PrefPrefix + "keepResources", _keepResources);
        EditorPrefs.SetBool(PrefPrefix + "keepRP", _keepRenderPipeline);
        EditorPrefs.SetBool(PrefPrefix + "keepEditor", _keepEditorFolder);
        EditorPrefs.SetBool(PrefPrefix + "keepStrings", _keepScriptStrings);
        EditorPrefs.SetBool(PrefPrefix + "subGraphs", _includeSubGraphs);
        EditorPrefs.SetBool(PrefPrefix + "useTrash", _useTrash);
        EditorPrefs.SetString(PrefPrefix + "ignore", _ignorePrefixes);
    }

    #region GUI

    void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        GUILayout.Space(5);
        GUILayout.Label("미사용 셰이더 정리", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "빌드 세팅에 등록된 씬에서 출발해 의존성을 훑고, 거기 걸리지 않은 Assets/ 아래 셰이더를 찾습니다.\n" +
            "삭제는 목록을 확인한 뒤 고른 것만 지웁니다.",
            MessageType.Info
        );

        DrawOptions();

        GUILayout.Space(8);

        GUI.backgroundColor = new Color(0.4f, 0.7f, 0.9f);
        if (GUILayout.Button("🔍 셰이더 스캔", GUILayout.Height(28)))
        {
            Scan();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(8);

        DrawResults();

        EditorGUILayout.EndScrollView();
    }

    void DrawOptions()
    {
        GUILayout.Space(5);
        _showOptions = EditorGUILayout.Foldout(_showOptions, "옵션", true);
        if (!_showOptions) return;

        EditorGUI.indentLevel++;

        _includeDisabledScenes = EditorGUILayout.Toggle(
            new GUIContent("체크 해제된 씬도 포함", "빌드 세팅 목록에 있지만 체크가 꺼진 씬도 뿌리로 쓴다. 나중에 다시 켤 씬을 지키려면 켜둔다."),
            _includeDisabledScenes
        );

        _keepResources = EditorGUILayout.Toggle(
            new GUIContent("Resources / StreamingAssets 보호", "런타임에 경로로 불러오는 에셋들. 씬 의존성에는 안 잡힌다."),
            _keepResources
        );

        _keepRenderPipeline = EditorGUILayout.Toggle(
            new GUIContent("렌더 파이프라인 설정 보호", "URP 에셋, 렌더러 데이터, Always Included Shaders가 참조하는 셰이더."),
            _keepRenderPipeline
        );

        _keepEditorFolder = EditorGUILayout.Toggle(
            new GUIContent("Editor 폴더 셰이더 보호", "빌드에는 안 들어가지만 지우면 에디터 툴이 깨지는 셰이더."),
            _keepEditorFolder
        );

        _keepScriptStrings = EditorGUILayout.Toggle(
            new GUIContent("스크립트 문자열 보호", "Shader.Find 처럼 코드에서 이름으로 찾는 셰이더. .cs 안의 문자열과 셰이더 이름을 대조한다."),
            _keepScriptStrings
        );

        _includeSubGraphs = EditorGUILayout.Toggle(
            new GUIContent("서브그래프도 후보에 포함", "쓰이는 그래프가 참조하는 .shadersubgraph는 자동으로 보호된다."),
            _includeSubGraphs
        );

        EditorGUILayout.LabelField(new GUIContent("제외 경로 (한 줄에 하나)", "이 경로로 시작하는 셰이더는 후보에서 뺀다."));
        _ignorePrefixes = EditorGUILayout.TextArea(_ignorePrefixes, GUILayout.Height(38));

        EditorGUI.indentLevel--;
    }

    void DrawResults()
    {
        if (!_scanned) return;

        EditorGUILayout.LabelField(
            $"뿌리 씬 {_sceneRootCount}개 · 셰이더 전체 {_totalShaders}개 · 사용 중 {_totalShaders - _unused.Count}개 · 미사용 {_unused.Count}개",
            EditorStyles.miniLabel
        );

        if (_dynamicFindCount > 0)
        {
            EditorGUILayout.HelpBox(
                $"이름이 변수로 들어가는 Shader.Find 호출이 {_dynamicFindCount}곳 있습니다. 이런 건 자동 판정이 안 되니 목록을 한 번 눈으로 확인해 주세요.",
                MessageType.Warning
            );
        }

        DrawKeptList();

        GUILayout.Space(6);

        if (_unused.Count == 0)
        {
            EditorGUILayout.HelpBox("미사용 셰이더가 없습니다.", MessageType.None);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"미사용 {_unused.Count}개 · {FormatSize(_unused.Sum(e => e.bytes))}", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("전체 선택", EditorStyles.miniButton)) SetAll(true);
        if (GUILayout.Button("전체 해제", EditorStyles.miniButton)) SetAll(false);
        if (GUILayout.Button("목록 복사", EditorStyles.miniButton)) CopyList();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        foreach (Entry entry in _unused)
        {
            EditorGUILayout.BeginHorizontal();

            entry.delete = EditorGUILayout.Toggle(entry.delete, GUILayout.Width(16));

            if (GUILayout.Button(new GUIContent(entry.path, entry.shaderName), EditorStyles.label))
            {
                EditorGUIUtility.PingObject(entry.asset);
            }

            GUILayout.FlexibleSpace();

            if (entry.materialCount > 0)
            {
                GUILayout.Label(
                    new GUIContent($"mat ×{entry.materialCount}", "이 셰이더를 참조하는 머티리얼 수. 그 머티리얼들도 미사용이라 같이 정리 대상일 가능성이 높다."),
                    EditorStyles.miniLabel, GUILayout.Width(52)
                );
            }

            GUILayout.Label(FormatSize(entry.bytes), EditorStyles.miniLabel, GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        _useTrash = EditorGUILayout.Toggle(
            new GUIContent("휴지통으로 이동 (복구 가능)", "끄면 프로젝트에서 완전히 지운다."),
            _useTrash
        );

        int selected = _unused.Count(e => e.delete);

        EditorGUI.BeginDisabledGroup(selected == 0 || EditorApplication.isPlaying);
        GUI.backgroundColor = new Color(0.85f, 0.35f, 0.35f);
        if (GUILayout.Button($"🗑 선택한 {selected}개 삭제", GUILayout.Height(30)))
        {
            DeleteSelected();
        }
        GUI.backgroundColor = Color.white;
        EditorGUI.EndDisabledGroup();

        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 모드에서는 삭제하지 않습니다.", MessageType.Warning);
        }
    }

    void DrawKeptList()
    {
        if (_kept.Count == 0) return;

        _showKept = EditorGUILayout.Foldout(_showKept, $"보호됨 {_kept.Count}개 (씬 의존성 외의 이유로 남은 셰이더)", true);
        if (!_showKept) return;

        EditorGUILayout.BeginVertical("box");
        foreach (Kept kept in _kept)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(kept.path, EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label(kept.reason, EditorStyles.miniLabel, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    void SetAll(bool value)
    {
        foreach (Entry entry in _unused) entry.delete = value;
    }

    void CopyList()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"미사용 셰이더 {_unused.Count}개 ({FormatSize(_unused.Sum(e => e.bytes))})");
        foreach (Entry entry in _unused) sb.AppendLine(entry.path);

        EditorGUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log($"[UnusedShaderCleaner] 목록 {_unused.Count}개를 클립보드에 복사했습니다.");
    }

    static string FormatSize(long bytes)
    {
        if (bytes >= 1024 * 1024) return $"{bytes / (1024f * 1024f):0.0} MB";
        if (bytes >= 1024) return $"{bytes / 1024f:0} KB";
        return $"{bytes} B";
    }

    #endregion

    #region Scan

    static bool IsShaderPath(string path, bool includeSubGraphs)
    {
        if (path.EndsWith(".shader") || path.EndsWith(".shadergraph")) return true;
        return includeSubGraphs && path.EndsWith(".shadersubgraph");
    }

    /// <summary>서브그래프 참조를 따라가야 하므로, 후보 여부와 무관하게 그래프 파일 전부를 훑는다.</summary>
    static bool IsShaderLikePath(string path)
    {
        return path.EndsWith(".shader") || path.EndsWith(".shadergraph") || path.EndsWith(".shadersubgraph");
    }

    void Scan()
    {
        _unused.Clear();
        _kept.Clear();
        _dynamicFindCount = 0;

        try
        {
            EditorUtility.DisplayProgressBar("미사용 셰이더 스캔", "후보 수집", 0f);

            string[] ignore = _ignorePrefixes
                .Split('\n')
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToArray();

            // 1. Assets/ 아래 셰이더 전부를 후보로 깐다. Packages는 어차피 지울 수 없다.
            List<string> candidates = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith("Assets/") && IsShaderPath(p, _includeSubGraphs))
                .OrderBy(p => p)
                .ToList();

            _totalShaders = candidates.Count;

            // 보호 이유가 붙은 경로 → 이유. 먼저 붙은 이유를 유지한다.
            Dictionary<string, string> keepReason = new Dictionary<string, string>();

            void Keep(string path, string reason)
            {
                if (!string.IsNullOrEmpty(path) && !keepReason.ContainsKey(path)) keepReason[path] = reason;
            }

            foreach (string path in candidates)
            {
                if (ignore.Any(prefix => path.StartsWith(prefix))) Keep(path, "제외 경로");
                else if (_keepEditorFolder && IsUnderEditorFolder(path)) Keep(path, "Editor 폴더");
            }

            // 2. 빌드 세팅 씬을 뿌리로 의존성 수집
            EditorUtility.DisplayProgressBar("미사용 셰이더 스캔", "빌드 씬 의존성", 0.15f);

            string[] sceneRoots = EditorBuildSettings.scenes
                .Where(s => (_includeDisabledScenes || s.enabled) && File.Exists(s.path))
                .Select(s => s.path)
                .ToArray();

            _sceneRootCount = sceneRoots.Length;

            HashSet<string> used = new HashSet<string>();

            if (sceneRoots.Length > 0)
            {
                foreach (string dependency in AssetDatabase.GetDependencies(sceneRoots, true)) used.Add(dependency);
            }

            // 3. 씬 밖에서 들어오는 경로들
            if (_keepResources)
            {
                EditorUtility.DisplayProgressBar("미사용 셰이더 스캔", "Resources / StreamingAssets", 0.3f);

                string[] roots = AssetDatabase.GetAllAssetPaths()
                    .Where(p => p.StartsWith("Assets/")
                                && !AssetDatabase.IsValidFolder(p)
                                && (p.Contains("/Resources/") || p.Contains("/StreamingAssets/")))
                    .ToArray();

                MarkRoots(roots, candidates, keepReason, "Resources 계열", Keep);
            }

            if (_keepRenderPipeline)
            {
                EditorUtility.DisplayProgressBar("미사용 셰이더 스캔", "렌더 파이프라인 설정", 0.45f);
                MarkRoots(CollectRenderPipelineRoots(), candidates, keepReason, "렌더 파이프라인", Keep);
            }

            // 4. 그래프 → 서브그래프, 셰이더 → Fallback/UsePass 참조를 닫는다.
            EditorUtility.DisplayProgressBar("미사용 셰이더 스캔", "셰이더 간 참조 추적", 0.6f);
            CloseShaderReferences(candidates, used, keepReason, Keep);

            // 5. 코드에서 이름으로 찾는 셰이더 보호
            Dictionary<string, string> shaderNames = new Dictionary<string, string>();
            foreach (string path in candidates)
            {
                string name = GetShaderName(path);
                if (!string.IsNullOrEmpty(name)) shaderNames[path] = name;
            }

            if (_keepScriptStrings)
            {
                EditorUtility.DisplayProgressBar("미사용 셰이더 스캔", "스크립트 문자열 대조", 0.75f);

                HashSet<string> literals = CollectScriptLiterals();
                foreach (string path in candidates)
                {
                    if (used.Contains(path) || keepReason.ContainsKey(path)) continue;
                    if (shaderNames.TryGetValue(path, out string name) && literals.Contains(name)) Keep(path, "스크립트 문자열");
                }
            }

            // 6. 남은 것이 미사용
            EditorUtility.DisplayProgressBar("미사용 셰이더 스캔", "머티리얼 참조 집계", 0.85f);

            List<string> leftovers = candidates
                .Where(p => !used.Contains(p) && !keepReason.ContainsKey(p))
                .ToList();

            Dictionary<string, int> materialUsage = CountMaterialUsage(leftovers);

            foreach (string path in leftovers)
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                materialUsage.TryGetValue(guid, out int matCount);

                _unused.Add(new Entry
                {
                    path = path,
                    shaderName = shaderNames.TryGetValue(path, out string name) ? name : Path.GetFileNameWithoutExtension(path),
                    asset = AssetDatabase.LoadMainAssetAtPath(path),
                    materialCount = matCount,
                    bytes = FileSize(path),
                    delete = true,
                });
            }

            foreach (KeyValuePair<string, string> pair in keepReason.OrderBy(p => p.Key))
            {
                _kept.Add(new Kept { path = pair.Key, reason = pair.Value });
            }

            _scanned = true;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log($"[UnusedShaderCleaner] 셰이더 {_totalShaders}개 중 미사용 {_unused.Count}개 (빌드 씬 {_sceneRootCount}개 기준).");
    }

    static bool IsUnderEditorFolder(string path)
    {
        return path.Contains("/Editor/") || path.Contains("/Editor Resources/");
    }

    /// <summary>뿌리 에셋들의 의존성에 걸리는 후보에 보호 이유를 붙인다.</summary>
    void MarkRoots(string[] roots, List<string> candidates, Dictionary<string, string> keepReason, string reason, System.Action<string, string> keep)
    {
        if (roots == null || roots.Length == 0) return;

        HashSet<string> set = new HashSet<string>(AssetDatabase.GetDependencies(roots, true));
        foreach (string path in candidates)
        {
            if (keepReason.ContainsKey(path)) continue;
            if (set.Contains(path)) keep(path, reason);
        }
    }

    /// <summary>URP 에셋 · 렌더러 데이터 · Always Included Shaders 등 설정 쪽 뿌리.</summary>
    static string[] CollectRenderPipelineRoots()
    {
        HashSet<string> roots = new HashSet<string>();

        void AddByType(string typeFilter)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeFilter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Assets/")) roots.Add(path);
            }
        }

        AddByType("RenderPipelineAsset");
        AddByType("ScriptableRendererData");
        AddByType("UniversalRendererData");
        AddByType("RenderPipelineGlobalSettings");
        AddByType("ShaderVariantCollection");

        string defaultRp = AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline);
        if (!string.IsNullOrEmpty(defaultRp) && defaultRp.StartsWith("Assets/")) roots.Add(defaultRp);

        SerializedObject graphics = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
        SerializedProperty alwaysIncluded = graphics.FindProperty("m_AlwaysIncludedShaders");
        if (alwaysIncluded != null && alwaysIncluded.isArray)
        {
            for (int i = 0; i < alwaysIncluded.arraySize; i++)
            {
                Object shader = alwaysIncluded.GetArrayElementAtIndex(i).objectReferenceValue;
                string path = AssetDatabase.GetAssetPath(shader);
                if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets/")) roots.Add(path);
            }
        }

        return roots.ToArray();
    }

    /// <summary>
    /// 쓰이는 셰이더/그래프가 텍스트로 참조하는 다른 셰이더를 따라간다.
    /// 서브그래프는 GUID로, Fallback/UsePass는 셰이더 이름으로 적혀 있다.
    /// </summary>
    void CloseShaderReferences(List<string> candidates, HashSet<string> used, Dictionary<string, string> keepReason, System.Action<string, string> keep)
    {
        // 후보 GUID / 이름 → 경로 역인덱스
        Dictionary<string, string> byGuid = new Dictionary<string, string>();
        Dictionary<string, string> byName = new Dictionary<string, string>();

        foreach (string path in candidates)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid)) byGuid[guid] = path;

            string name = GetShaderName(path);
            if (!string.IsNullOrEmpty(name)) byName[name] = path;
        }

        // 살아 있다고 확정된 셰이더 파일들에서 출발한다.
        Queue<string> frontier = new Queue<string>();
        HashSet<string> visited = new HashSet<string>();

        foreach (string path in AssetDatabase.GetAllAssetPaths())
        {
            if (!path.StartsWith("Assets/") || !IsShaderLikePath(path)) continue;
            if (used.Contains(path) || keepReason.ContainsKey(path)) frontier.Enqueue(path);
        }

        Regex passRefs = new Regex("(?:Fallback|UsePass)\\s+\"([^\"]+)\"", RegexOptions.IgnoreCase);

        while (frontier.Count > 0)
        {
            string path = frontier.Dequeue();
            if (!visited.Add(path)) continue;
            if (!File.Exists(path)) continue;

            string text;
            try { text = File.ReadAllText(path); }
            catch { continue; }

            // 서브그래프 등 GUID 참조
            foreach (Match match in Regex.Matches(text, "[0-9a-f]{32}"))
            {
                if (!byGuid.TryGetValue(match.Value, out string referenced)) continue;
                if (used.Contains(referenced) || keepReason.ContainsKey(referenced)) continue;

                keep(referenced, "다른 셰이더가 참조");
                frontier.Enqueue(referenced);
            }

            // Fallback / UsePass 는 셰이더 이름으로 적힌다. UsePass는 "Name/PASSNAME" 꼴.
            foreach (Match match in passRefs.Matches(text))
            {
                string reference = match.Groups[1].Value;

                foreach (string name in new[] { reference, TrimLastSegment(reference) })
                {
                    if (string.IsNullOrEmpty(name)) continue;
                    if (!byName.TryGetValue(name, out string referenced)) continue;
                    if (used.Contains(referenced) || keepReason.ContainsKey(referenced)) continue;

                    keep(referenced, "다른 셰이더가 참조");
                    frontier.Enqueue(referenced);
                }
            }
        }
    }

    static string TrimLastSegment(string value)
    {
        int index = value.LastIndexOf('/');
        return index > 0 ? value.Substring(0, index) : null;
    }

    /// <summary>.cs 안의 문자열 리터럴을 모은다. 셰이더 이름이 코드 어딘가에 적혀 있으면 보호한다.</summary>
    HashSet<string> CollectScriptLiterals()
    {
        HashSet<string> literals = new HashSet<string>();
        Regex literal = new Regex("\"([^\"\\\\\\r\\n]{3,})\"");
        Regex dynamicFind = new Regex("Shader\\.Find\\s*\\(\\s*[^\"\\)]");

        foreach (string path in AssetDatabase.GetAllAssetPaths())
        {
            if (!path.StartsWith("Assets/") || !path.EndsWith(".cs")) continue;
            if (!File.Exists(path)) continue;

            string text;
            try { text = File.ReadAllText(path); }
            catch { continue; }

            foreach (Match match in literal.Matches(text)) literals.Add(match.Groups[1].Value);
            _dynamicFindCount += dynamicFind.Matches(text).Count;
        }

        return literals;
    }

    /// <summary>후보 셰이더를 참조하는 머티리얼 수. .mat은 텍스트 직렬화라 파일에서 바로 읽는다.</summary>
    static Dictionary<string, int> CountMaterialUsage(List<string> candidatePaths)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        if (candidatePaths.Count == 0) return counts;

        HashSet<string> wanted = new HashSet<string>(
            candidatePaths.Select(AssetDatabase.AssetPathToGUID).Where(g => !string.IsNullOrEmpty(g))
        );

        Regex shaderRef = new Regex("m_Shader:\\s*\\{fileID:\\s*-?\\d+,\\s*guid:\\s*([0-9a-f]{32})");

        foreach (string guid in AssetDatabase.FindAssets("t:Material"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/") || !File.Exists(path)) continue;

            string text;
            try { text = File.ReadAllText(path); }
            catch { continue; }

            Match match = shaderRef.Match(text);
            if (!match.Success) continue;

            string shaderGuid = match.Groups[1].Value;
            if (!wanted.Contains(shaderGuid)) continue;

            counts.TryGetValue(shaderGuid, out int count);
            counts[shaderGuid] = count + 1;
        }

        return counts;
    }

    static string GetShaderName(string path)
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
        if (shader != null) return shader.name;

        if (!path.EndsWith(".shader") || !File.Exists(path)) return null;

        try
        {
            Match match = Regex.Match(File.ReadAllText(path), "^\\s*Shader\\s+\"([^\"]+)\"", RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value : null;
        }
        catch
        {
            return null;
        }
    }

    static long FileSize(string path)
    {
        try { return new FileInfo(path).Length; }
        catch { return 0; }
    }

    #endregion

    #region Delete

    void DeleteSelected()
    {
        if (EditorApplication.isPlaying) return;

        List<string> targets = _unused.Where(e => e.delete).Select(e => e.path).ToList();
        if (targets.Count == 0) return;

        string preview = string.Join("\n", targets.Take(15));
        if (targets.Count > 15) preview += $"\n... 외 {targets.Count - 15}개";

        string action = _useTrash ? "휴지통으로 이동" : "완전 삭제";

        bool confirmed = EditorUtility.DisplayDialog(
            $"셰이더 {targets.Count}개 {action}",
            $"{preview}\n\n{action}합니다. 진행할까요?",
            action,
            "취소"
        );

        if (!confirmed) return;

        List<string> failed = new List<string>();

        if (_useTrash) AssetDatabase.MoveAssetsToTrash(targets.ToArray(), failed);
        else AssetDatabase.DeleteAssets(targets.ToArray(), failed);

        AssetDatabase.Refresh();

        int removed = targets.Count - failed.Count;
        Debug.Log($"[UnusedShaderCleaner] 셰이더 {removed}개를 {action}했습니다." +
                  (failed.Count > 0 ? $" 실패 {failed.Count}개: {string.Join(", ", failed)}" : ""));

        if (failed.Count > 0)
        {
            EditorUtility.DisplayDialog("일부 실패", $"{failed.Count}개를 지우지 못했습니다.\n\n{string.Join("\n", failed.Take(10))}", "확인");
        }

        Scan();
    }

    #endregion
}
