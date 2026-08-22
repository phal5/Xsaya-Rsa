using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 카메라 앞을 스쳐 지나가는 전경 띠를 만든다. 2D 플랫포머에서 화면 아래를 훑고 지나가는 풀 같은 것.
///
/// 다른 배치 툴들과 붙는 자리가 다르다. 저쪽은 "이 플랫폼을 꾸민다"지만 이쪽은 어떤 오브젝트에도
/// 붙지 않는다. 레벨의 X 구간을 가로지르는 <b>독립된 띠</b>이고, 카메라가 그 앞을 지나갈 뿐이다.
///
/// 플레이 평면보다 앞에 있으므로 카메라가 움직일 때 더 빨리 흘러간다. 그 속도 차이가 깊이감을 만든다.
/// 가릴 목적이 아니라 스쳐 지나갈 목적이므로, 밑동은 화면 아래로 묻고 끝만 프레임에 걸치게 둔다.
///
/// 프리팹 목록은 EnvironmentPlacerProfile을 그대로 쓴다. 배치는 (X, Z) 평면 Poisson이라
/// 깊이로도 흩어져 서로 겹치며 지나간다.
/// </summary>
public class ForegroundBandWindow : EditorWindow
{
    const string CONTAINER_PREFIX = "_FgBand";
    const int HARD_LIMIT = 20000;
    const int WARN_LIMIT = 2000;

    EnvironmentPlacerProfile _profile;

    // 띠가 걸치는 X 구간
    float _xMin = 0f;
    float _xMax = 40f;

    // 깊이. Z가 작을수록 카메라에 가깝다.
    float _zCenter = -4f;
    float _zThickness = 2f;

    // 높이
    float _baselineY = -1f;
    float _yJitter = 0.4f;

    float _density = 1f;
    float _globalScale = 1.5f;

    // 프레임 하단 계산에 쓰는 리그 값
    float _fieldOfView = 60f;
    float _playPlaneZ = 0f;
    float _characterY = 0f;
    float _rigHeight = 0.75f;
    float _rigDistance = 10f;

    bool _removeColliders = true;
    bool _createShadowVolume = true;
    float _shadowPadding = 0.3f;

    Transform _parent;

    Vector2 _scrollPos;
    bool _canceled;

    string _lastReport;

    [MenuItem("Tools/Foreground Band")]
    public static void ShowWindow()
    {
        ForegroundBandWindow window = GetWindow<ForegroundBandWindow>("Foreground Band");
        window.minSize = new Vector2(380, 560);
    }

    void OnSelectionChange()
    {
        Repaint();
    }

    void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        DrawHeader();
        DrawProfile();
        DrawSpan();
        DrawDepth();
        DrawHeight();
        DrawCleanup();
        DrawActions();

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
        GUILayout.Label("🌾 Foreground Band", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "카메라 앞을 스쳐 지나가는 전경 띠를 만듭니다.\n" +
            "어떤 오브젝트에도 붙지 않고 레벨 X 구간을 가로지릅니다.",
            MessageType.Info
        );
        GUILayout.Space(5);
    }

    void DrawProfile()
    {
        GUILayout.Label("프로파일", EditorStyles.miniBoldLabel);

        _profile = (EnvironmentPlacerProfile)EditorGUILayout.ObjectField("프로파일", _profile, typeof(EnvironmentPlacerProfile), false);

        if (_profile != null)
        {
            int entries = _profile.Entries != null ? _profile.Entries.Length : 0;
            EditorGUILayout.LabelField($"   프리팹 {entries}종 · 최소 간격 {_profile.MinimumSpacing:F2}", EditorStyles.miniLabel);
        }

        _density = EditorGUILayout.Slider(
            new GUIContent("밀도", "프로파일의 최소 간격을 이 값으로 나눕니다. 높을수록 촘촘합니다."),
            _density, 0.1f, 3f
        );

        _globalScale = EditorGUILayout.Slider(
            new GUIContent("전체 스케일", "전경은 카메라에 가까워 원근으로 커 보입니다. 그래도 실루엣이 성기면 여기서 키웁니다."),
            _globalScale, 0.1f, 5f
        );

        GUILayout.Space(8);
    }

    void DrawSpan()
    {
        GUILayout.Label("가로 구간", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        _xMin = EditorGUILayout.FloatField("X 시작", _xMin);
        _xMax = EditorGUILayout.FloatField("X 끝", _xMax);
        EditorGUILayout.EndHorizontal();

        if (_xMax < _xMin)
        {
            float t = _xMin; _xMin = _xMax; _xMax = t;
        }

        EditorGUI.BeginDisabledGroup(Selection.gameObjects == null || Selection.gameObjects.Length == 0);
        if (GUILayout.Button("선택한 오브젝트들의 X 범위로 채우기", EditorStyles.miniButton))
        {
            FillSpanFromSelection();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField($"   길이 {(_xMax - _xMin):F2}", EditorStyles.miniLabel);

        GUILayout.Space(8);
    }

    void DrawDepth()
    {
        GUILayout.Label("깊이", EditorStyles.miniBoldLabel);

        _zCenter = EditorGUILayout.FloatField(
            new GUIContent("Z 중심", "작을수록 카메라에 가깝습니다. 플레이 평면보다 앞이어야 스쳐 지나갑니다."),
            _zCenter
        );

        _zThickness = EditorGUILayout.FloatField(
            new GUIContent("Z 두께", "이 범위 안에서 깊이가 흩어집니다. 0이면 한 평면에 납작하게 서고, 크면 서로 겹치며 지나갑니다."),
            _zThickness
        );
        _zThickness = Mathf.Max(_zThickness, 0f);

        float camZ = _playPlaneZ - _rigDistance;
        float d = _zCenter - camZ;

        if (d <= 0.01f)
        {
            EditorGUILayout.HelpBox("띠가 카메라 뒤에 있습니다.", MessageType.Error);
        }
        else
        {
            float playDepth = _playPlaneZ - camZ;
            EditorGUILayout.LabelField(
                $"   카메라까지 {d:F2}   플레이 평면은 {playDepth:F2}   →   시차 {(playDepth / d):F2}배 빠르게 흐름",
                EditorStyles.miniLabel
            );
        }

        GUILayout.Space(8);
    }

    void DrawHeight()
    {
        GUILayout.Label("높이", EditorStyles.miniBoldLabel);

        _baselineY = EditorGUILayout.FloatField(
            new GUIContent("기준선 Y", "프리팹 밑동이 놓이는 높이. 프레임 하단보다 아래여야 밑동이 안 보입니다."),
            _baselineY
        );

        _yJitter = EditorGUILayout.FloatField(
            new GUIContent("Y 흔들기", "기준선에서 위아래로 흩뜨리는 폭. 일직선으로 늘어선 티를 없앱니다."),
            _yJitter
        );
        _yJitter = Mathf.Max(_yJitter, 0f);

        float frameBottom = FrameBottomY();
        EditorGUILayout.LabelField($"   이 깊이에서 프레임 하단 = Y {frameBottom:F2}", EditorStyles.miniLabel);

        if (_baselineY > frameBottom)
        {
            EditorGUILayout.HelpBox(
                $"기준선이 프레임 하단보다 {(_baselineY - frameBottom):F2} 위에 있습니다.\n" +
                "밑동이 화면에 보여 풀이 공중에 뜬 것처럼 읽힙니다.",
                MessageType.Warning
            );
        }

        if (GUILayout.Button("기준선을 프레임 하단으로 내리기", EditorStyles.miniButton))
        {
            _baselineY = frameBottom;
        }

        EditorGUILayout.LabelField("프레임 하단 계산에 쓰는 값", EditorStyles.miniLabel);
        EditorGUI.indentLevel++;
        _fieldOfView = EditorGUILayout.FloatField("FOV", _fieldOfView);
        _characterY = EditorGUILayout.FloatField(new GUIContent("캐릭터 발 높이", "이 구간에서 캐릭터가 서는 Y."), _characterY);
        _playPlaneZ = EditorGUILayout.FloatField("플레이 평면 Z", _playPlaneZ);
        _rigHeight = EditorGUILayout.FloatField(new GUIContent("리그 높이", "offset2D.y"), _rigHeight);
        _rigDistance = EditorGUILayout.FloatField(new GUIContent("리그 거리", "offset2D.z 절댓값"), _rigDistance);
        _rigDistance = Mathf.Max(_rigDistance, 0.1f);
        EditorGUI.indentLevel--;

        GUILayout.Space(8);
    }

    void DrawCleanup()
    {
        GUILayout.Label("결과 처리", EditorStyles.miniBoldLabel);

        _parent = (Transform)EditorGUILayout.ObjectField(
            new GUIContent("부모", "비워두면 씬 루트에 만듭니다. 전경 띠는 특정 플랫폼에 딸린 게 아니므로 보통 루트가 맞습니다."),
            _parent, typeof(Transform), true
        );

        _removeColliders = EditorGUILayout.Toggle(
            new GUIContent("콜라이더 제거", "전경에 콜라이더가 남으면 보이지 않는 벽이 됩니다. 켜두세요."),
            _removeColliders
        );

        _createShadowVolume = EditorGUILayout.Toggle(
            new GUIContent("Shadow Volume 생성", "띠 크기에 맞는 박스를 만들고 lightMultiplier를 0으로 둬 검은 실루엣으로 만듭니다."),
            _createShadowVolume
        );

        if (_createShadowVolume)
        {
            EditorGUI.indentLevel++;
            _shadowPadding = EditorGUILayout.FloatField("박스 여유", _shadowPadding);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(8);
    }

    void DrawActions()
    {
        GUILayout.Label("실행", EditorStyles.miniBoldLabel);

        bool hasProfile = _profile != null && _profile.Entries != null && _profile.Entries.Length > 0;
        bool hasSpan = _xMax - _xMin > 0.01f;

        int estimate = EstimateCount();
        if (hasProfile && hasSpan)
        {
            EditorGUILayout.LabelField($"   생성 예상: 약 {estimate}개", EditorStyles.miniLabel);
        }

        if (estimate > HARD_LIMIT)
        {
            EditorGUILayout.HelpBox($"{estimate}개는 상한({HARD_LIMIT})을 넘습니다. 밀도를 낮추거나 구간을 줄이세요.", MessageType.Error);
        }
        else if (estimate > WARN_LIMIT)
        {
            EditorGUILayout.HelpBox($"{estimate}개는 시간이 걸립니다. 진행 중 취소할 수 있습니다.", MessageType.Warning);
        }

        bool canBuild = hasProfile && hasSpan && estimate <= HARD_LIMIT;

        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        EditorGUI.BeginDisabledGroup(!canBuild);
        if (GUILayout.Button("🌾 Build Band", GUILayout.Height(32)))
        {
            RunCancelable("Foreground Band Build", Build);
        }
        EditorGUI.EndDisabledGroup();

        int existing = CountBands();
        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
        EditorGUI.BeginDisabledGroup(existing == 0);
        if (GUILayout.Button($"🗑 Clear (전경 띠 {existing}개)", GUILayout.Height(26)))
        {
            RunCancelable("Foreground Band Clear", ClearBands);
        }
        EditorGUI.EndDisabledGroup();

        GUI.backgroundColor = Color.white;
        GUILayout.Space(5);
    }

    #endregion

    #region Core

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
            Debug.Log("[ForegroundBand] 취소되었습니다. 작업 전 상태로 되돌렸습니다.");
            return;
        }

        Undo.CollapseUndoOperations(group);
    }

    bool PollCancel(string info, float progress)
    {
        if (_canceled) return true;

        if (EditorUtility.DisplayCancelableProgressBar("Foreground Band", info, progress))
        {
            _canceled = true;
        }

        return _canceled;
    }

    void Build()
    {
        float spacing = _profile.MinimumSpacing / Mathf.Max(_density, 0.01f);

        int seed = _profile.UseRandomSeed ? System.Environment.TickCount : _profile.Seed;
        System.Random rng = new System.Random(seed);

        // (X, Z) 평면에 뿌린다. Z로도 흩어져야 서로 겹치며 지나간다.
        float halfZ = Mathf.Max(_zThickness, 0.01f) * 0.5f;

        List<Vector2> points = PoissonDisk.Sample(
            new Vector2(_xMin, _zCenter - halfZ),
            new Vector2(_xMax, _zCenter + halfZ),
            spacing,
            _profile.SamplesBeforeRejection,
            rng,
            found => PollCancel($"배치 지점 계산 중… {found}개", -1f)
        );

        if (_canceled) return;

        if (points.Count == 0)
        {
            Debug.LogWarning("[ForegroundBand] 배치 지점이 없습니다. 간격이 구간보다 큰지 확인하세요.");
            return;
        }

        // 컨테이너는 루트에서 만들어 다 채운 뒤 마지막에 편입한다.
        GameObject container = new GameObject($"{CONTAINER_PREFIX}_{_profile.name}");
        if (_parent != null) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(container, _parent.gameObject.scene);
        container.transform.position = Vector3.zero;
        container.transform.rotation = _parent != null ? _parent.rotation : Quaternion.identity;
        container.transform.localScale = Vector3.one;
        Undo.RegisterCreatedObjectUndo(container, "Foreground Band Build");

        int count = 0;

        for (int i = 0; i < points.Count; i++)
        {
            if ((i & 0x3F) == 0 && PollCancel($"배치 중 {i}/{points.Count}", (float)i / points.Count)) return;

            GameObject prefab = _profile.PickRandomPrefab(rng);
            if (prefab == null) continue;

            EnvironmentEntry entry = _profile.GetEntryForPrefab(prefab);

            float y = _baselineY + ((float)rng.NextDouble() * 2f - 1f) * _yJitter;

            // 회전 -> 스케일 -> 위치 순.
            // 전경은 세워둔다. 눕히면 풀이 아니라 잔해로 읽힌다.
            Quaternion rotation = Quaternion.identity;
            if (entry.RandomYRotation)
            {
                rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            }

            float scale = Mathf.Lerp(
                Mathf.Max(entry.MinScale, 0.01f),
                Mathf.Max(entry.MaxScale, 0.01f),
                (float)rng.NextDouble()
            ) * _globalScale;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, container.scene);
            if (instance == null) continue;

            instance.transform.rotation = rotation;
            instance.transform.localScale = Vector3.one * scale;
            instance.transform.position = new Vector3(points[i].x, y, points[i].y);

            instance.transform.SetParent(container.transform, true);

            Undo.RegisterCreatedObjectUndo(instance, "Foreground Band Build");

            ApplyInstancing(instance, _profile.UsesGpuInstancing(entry));
            count++;
        }

        int removedColliders = 0;
        if (_removeColliders) removedColliders = RemoveColliders(container);

        GameObject volume = null;
        if (_createShadowVolume) volume = CreateShadowVolume(container);

        if (_parent != null) container.transform.SetParent(_parent, true);

        _lastReport =
            $"{count}개 배치 · X {_xMin:F1}~{_xMax:F1} · Z {_zCenter:F1}\n" +
            $"콜라이더 {removedColliders}개 제거" +
            (volume != null ? " · Shadow Volume 생성" : "") +
            $"   (Seed: {seed})";

        Debug.Log($"[ForegroundBand] {_lastReport.Replace("\n", "  ")}");
    }

    GameObject CreateShadowVolume(GameObject container)
    {
        if (!TryGetBounds(container.transform, out Bounds b)) return null;

        GameObject go = new GameObject($"{CONTAINER_PREFIX}_ShadowVolume");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, container.scene);

        go.transform.SetPositionAndRotation(b.center, Quaternion.identity);
        go.transform.localScale = Vector3.one;

        ShadowVolume volume = go.AddComponent<ShadowVolume>();
        volume.size = b.size + Vector3.one * (_shadowPadding * 2f);
        volume.lightMultiplier = 0f;
        volume.edgeFade = 0.05f;

        Undo.RegisterCreatedObjectUndo(go, "Foreground Band Build");

        go.transform.SetParent(container.transform, true);
        return go;
    }

    static void ApplyInstancing(GameObject instance, bool gpuInstancing)
    {
        if (!gpuInstancing) return;

        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null || material.enableInstancing) continue;

                Undo.RecordObject(material, "Enable GPU Instancing");
                material.enableInstancing = true;
                EditorUtility.SetDirty(material);
            }
        }
    }

    static int RemoveColliders(GameObject root)
    {
        int count = 0;
        foreach (Collider c in root.GetComponentsInChildren<Collider>(true))
        {
            if (c == null) continue;
            Object.DestroyImmediate(c);
            count++;
        }
        return count;
    }

    #endregion

    #region Math

    /// <summary>이 띠 깊이에서 화면 아래 끝이 걸리는 월드 Y.</summary>
    float FrameBottomY()
    {
        float camY = _characterY + _rigHeight;
        float camZ = _playPlaneZ - _rigDistance;

        float d = Mathf.Max(_zCenter - camZ, 0.01f);
        float visibleHeight = 2f * d * Mathf.Tan(_fieldOfView * 0.5f * Mathf.Deg2Rad);

        return camY - visibleHeight * 0.5f;
    }

    int EstimateCount()
    {
        if (_profile == null) return 0;

        float spacing = _profile.MinimumSpacing / Mathf.Max(_density, 0.01f);
        if (spacing <= 0f) return 0;

        float area = (_xMax - _xMin) * Mathf.Max(_zThickness, 0.01f);

        // Poisson 채움률은 대략 0.7 정도다. 셀 하나가 spacing^2 을 차지한다고 보고 어림한다.
        return Mathf.RoundToInt(area / (spacing * spacing) * 0.7f);
    }

    void FillSpanFromSelection()
    {
        bool any = false;
        float min = 0f, max = 0f;

        foreach (GameObject go in Selection.gameObjects)
        {
            if (go == null) continue;

            foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null) continue;

                if (!any) { min = r.bounds.min.x; max = r.bounds.max.x; any = true; }
                else { min = Mathf.Min(min, r.bounds.min.x); max = Mathf.Max(max, r.bounds.max.x); }
            }
        }

        if (!any) return;

        _xMin = min;
        _xMax = max;
    }

    static bool TryGetBounds(Transform root, out Bounds bounds)
    {
        bounds = default;
        bool any = false;

        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null) continue;
            if (!any) { bounds = r.bounds; any = true; }
            else bounds.Encapsulate(r.bounds);
        }

        return any;
    }

    #endregion

    #region Containers

    static List<GameObject> FindBands()
    {
        List<GameObject> found = new List<GameObject>();

        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t == null) continue;
            if (!t.name.StartsWith(CONTAINER_PREFIX)) continue;
            if (t.parent != null && t.parent.name.StartsWith(CONTAINER_PREFIX)) continue;   // 안쪽 볼륨은 뺀다

            found.Add(t.gameObject);
        }

        return found;
    }

    static int CountBands()
    {
        return FindBands().Count;
    }

    void ClearBands()
    {
        List<GameObject> bands = FindBands();

        foreach (GameObject go in bands)
        {
            if (go != null) Undo.DestroyObjectImmediate(go);
        }

        _lastReport = null;
        Debug.Log($"[ForegroundBand] 전경 띠 {bands.Count}개를 지웠습니다.");
    }

    #endregion
}
