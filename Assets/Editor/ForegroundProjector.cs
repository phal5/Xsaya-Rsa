using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 이미 꾸며둔 플랫폼을 통째로 복제해, 카메라를 기준으로 축소하며 앞으로 내보내는 툴.
///
/// 원리는 한 줄이다. 카메라 C를 중심으로 점 P를 k배 축소하면
///
///     P' = C + (P - C) * k
///
/// P'는 카메라에서 P를 향하는 같은 광선 위에 있으므로 <b>화면상 정확히 같은 자리에 찍힌다.</b>
/// 크기도 k배 줄어들어 원근으로 상쇄되므로, 결과는 원본과 완벽히 겹치는 전경 복제본이 된다.
/// 가까운 물체가 화면 바깥으로 밀려 내려가는 "가라앉음"이 자동으로 보정된다는 뜻이다.
///
/// 다른 배치 툴들과 성격이 다르다. 저 셋은 프로파일을 보고 프리팹을 새로 뿌리지만,
/// 이쪽은 씬에 이미 있는 것을 복사한다. 그래서 "플랫폼을 확실히 덮는" 전경을 만들 수 있다.
///
/// 정합은 기준 카메라 위치에서만 정확하다. 카메라가 움직이면 시차로 어긋나는데, 그게 목적이다.
/// </summary>
public class ForegroundProjectorWindow : EditorWindow
{
    const string PREFIX = "_Fore";

    /// <summary>복제 대상에서 빼야 할 컨테이너들. 다른 배치 툴이 만든 것과 이 툴이 만든 것.</summary>
    static readonly string[] DECOR_PREFIXES = { "_Deco", "_Scatter", "_Fill" };

    GameObject _referenceCamera;

    // 기준 카메라를 직접 지정하지 않을 때 리그 값으로 계산한다 (offset2D = (x, 0.75, -10))
    float _rigHeight = 0.75f;
    float _rigDistance = 10f;

    float _delta = 2f;

    float _extraY = 0f;
    float _extraDepth = 0f;

    bool _removeColliders = true;
    bool _createShadowVolume = true;
    float _shadowPadding = 0.2f;

    int _layerIndex = -1;   // -1 = 원본 유지

    [Range(0f, 1f)] float _keepRatio = 1f;
    int _seed = 42;

    Vector2 _scrollPos;

    string _lastReport;

    [MenuItem("Tools/Foreground Projector")]
    public static void ShowWindow()
    {
        ForegroundProjectorWindow window = GetWindow<ForegroundProjectorWindow>("Foreground Projector");
        window.minSize = new Vector2(360, 520);
    }

    void OnSelectionChange()
    {
        Repaint();
    }

    void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        DrawHeader();
        DrawSource();
        DrawCamera();
        DrawProjection();
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
        GUILayout.Label("🎭 Foreground Projector", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "꾸며둔 플랫폼을 복제해 카메라 기준으로 축소하며 앞으로 냅니다.\n" +
            "기준 카메라에서 보면 원본과 정확히 겹칩니다. 높이 보정이 자동으로 들어갑니다.",
            MessageType.Info
        );
        GUILayout.Space(5);
    }

    void DrawSource()
    {
        GUILayout.Label("원본", EditorStyles.miniBoldLabel);

        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            EditorGUILayout.HelpBox("Hierarchy에서 복제할 플랫폼을 선택해주세요.", MessageType.Warning);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("선택됨", selected, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();

            if (TryGetSourceBounds(selected, out Bounds b))
            {
                EditorGUILayout.LabelField($"   경계 = {b.size.x:F2} × {b.size.y:F2} × {b.size.z:F2}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"   장식 컨테이너 {CountDecorContainers(selected)}개 · 기존 전경 {CountForeContainers(selected)}개", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.HelpBox("Renderer나 Collider를 찾지 못했습니다.", MessageType.Warning);
            }
        }

        GUILayout.Space(8);
    }

    void DrawCamera()
    {
        GUILayout.Label("기준 카메라", EditorStyles.miniBoldLabel);

        _referenceCamera = (GameObject)EditorGUILayout.ObjectField(
            new GUIContent("카메라", "비워두면 아래 리그 값으로 위치를 계산합니다. 에디터 카메라 위치는 실제 게임과 다르므로 보통 비워두는 편이 낫습니다."),
            _referenceCamera, typeof(GameObject), true
        );

        if (_referenceCamera == null)
        {
            EditorGUI.indentLevel++;
            _rigHeight = EditorGUILayout.FloatField(
                new GUIContent("리그 높이", "HybridCameraRig의 offset2D.y. 플랫폼 윗면 기준으로 이만큼 위."),
                _rigHeight
            );
            _rigDistance = EditorGUILayout.FloatField(
                new GUIContent("리그 거리", "HybridCameraRig의 offset2D.z 절댓값. 카메라가 뒤로 물러난 거리."),
                _rigDistance
            );
            _rigDistance = Mathf.Max(_rigDistance, 0.1f);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(8);
    }

    void DrawProjection()
    {
        GUILayout.Label("투영", EditorStyles.miniBoldLabel);

        _delta = EditorGUILayout.FloatField(
            new GUIContent("앞으로 (Δ)", "카메라 쪽으로 얼마나 낼지. 클수록 크게 줄어들고 시차가 강해집니다."),
            _delta
        );

        GameObject selected = Selection.activeGameObject;
        if (selected != null && TryGetSourceBounds(selected, out Bounds b))
        {
            Vector3 camPos = ResolveCameraPosition(b);
            Vector3 camFwd = ResolveCameraForward();

            float depth = Vector3.Dot(b.center - camPos, camFwd);
            float foreDepth = depth - _delta;

            if (depth <= 0.01f)
            {
                EditorGUILayout.HelpBox("원본이 카메라 뒤에 있습니다. 리그 거리를 확인하세요.", MessageType.Error);
            }
            else if (foreDepth <= 0.01f)
            {
                EditorGUILayout.HelpBox($"Δ가 너무 큽니다. 카메라까지 거리가 {depth:F2}입니다.", MessageType.Error);
            }
            else
            {
                float k = foreDepth / depth;
                EditorGUILayout.LabelField($"   카메라까지 거리 = {depth:F2}  →  전경 거리 = {foreDepth:F2}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"   축소 배율 k = {k:F3}   (결과 크기 {b.size.x * k:F2} × {b.size.y * k:F2} × {b.size.z * k:F2})", EditorStyles.miniLabel);
            }
        }

        GUILayout.Space(4);
        EditorGUILayout.LabelField("복제 후 미세 조정", EditorStyles.miniLabel);

        EditorGUI.indentLevel++;
        _extraY = EditorGUILayout.FloatField(new GUIContent("추가 Y", "위아래로 더 밀기. 축소로 얻은 높이 보정 위에 얹힙니다."), _extraY);
        _extraDepth = EditorGUILayout.FloatField(new GUIContent("추가 깊이", "카메라 전방으로 더 밀기. 양수면 카메라에서 멀어집니다."), _extraDepth);
        EditorGUI.indentLevel--;

        GUILayout.Space(8);
    }

    void DrawCleanup()
    {
        GUILayout.Label("복제본 처리", EditorStyles.miniBoldLabel);

        _removeColliders = EditorGUILayout.Toggle(
            new GUIContent("콜라이더 제거", "전경 복제본에 콜라이더가 남으면 허공에 밟히는 발판이 생깁니다. 켜두세요."),
            _removeColliders
        );

        _keepRatio = EditorGUILayout.Slider(
            new GUIContent("장식 유지 비율", "복제된 장식 중 남길 비율. 1보다 낮추면 무작위로 솎아 원본과 다르게 보이게 합니다. 장식 컨테이너 안의 것만 솎습니다."),
            _keepRatio, 0f, 1f
        );

        if (_keepRatio < 1f)
        {
            EditorGUI.indentLevel++;
            _seed = EditorGUILayout.IntField("시드", _seed);
            EditorGUI.indentLevel--;
        }

        string[] layerNames = UnityEditorInternal.InternalEditorUtility.layers;
        List<string> options = new List<string> { "원본 유지" };
        options.AddRange(layerNames);

        int current = _layerIndex < 0 ? 0 : _layerIndex + 1;
        int picked = EditorGUILayout.Popup(
            new GUIContent("레이어", "복제본 전체에 적용할 레이어."),
            current, options.ToArray()
        );
        _layerIndex = picked <= 0 ? -1 : picked - 1;

        _createShadowVolume = EditorGUILayout.Toggle(
            new GUIContent("Shadow Volume 생성", "복제본 크기에 맞는 박스를 만들고 lightMultiplier를 0으로 둬 검은 실루엣으로 만듭니다."),
            _createShadowVolume
        );

        if (_createShadowVolume)
        {
            EditorGUI.indentLevel++;
            _shadowPadding = EditorGUILayout.FloatField(
                new GUIContent("박스 여유", "복제본 경계보다 이만큼 크게 잡습니다. 박스 밖으로 삐져나오는 걸 막습니다."),
                _shadowPadding
            );
            EditorGUI.indentLevel--;

            EditorGUILayout.HelpBox(
                "Shadow Volume은 깊이 기반이라 박스 안에 들어온 것은 무엇이든 어두워집니다.\n" +
                "원본 플랫폼까지 먹지 않는지 확인하세요.",
                MessageType.Warning
            );
        }

        GUILayout.Space(8);
    }

    void DrawActions()
    {
        GameObject selected = Selection.activeGameObject;
        bool canProject = selected != null && TryGetSourceBounds(selected, out _);

        GUILayout.Label("실행", EditorStyles.miniBoldLabel);

        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        EditorGUI.BeginDisabledGroup(!canProject);
        if (GUILayout.Button("🎭 Project", GUILayout.Height(32)))
        {
            Project(selected);
        }
        EditorGUI.EndDisabledGroup();

        bool hasFore = selected != null && CountForeContainers(selected) > 0;
        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
        EditorGUI.BeginDisabledGroup(!hasFore);
        if (GUILayout.Button("🗑 Clear (전경 복제본)", GUILayout.Height(26)))
        {
            ClearForeContainers(selected);
        }
        EditorGUI.EndDisabledGroup();

        GUI.backgroundColor = Color.white;
        GUILayout.Space(5);
    }

    #endregion

    #region Core

    void Project(GameObject source)
    {
        if (!TryGetSourceBounds(source, out Bounds bounds)) return;

        Vector3 camPos = ResolveCameraPosition(bounds);
        Vector3 camFwd = ResolveCameraForward();

        float depth = Vector3.Dot(bounds.center - camPos, camFwd);
        float foreDepth = depth - _delta;

        if (depth <= 0.01f || foreDepth <= 0.01f)
        {
            Debug.LogError($"[ForegroundProjector] 거리가 유효하지 않습니다. 원본까지 {depth:F2}, 전경 {foreDepth:F2}.");
            return;
        }

        float k = foreDepth / depth;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Foreground Project");

        // 기존 전경 복제본은 먼저 치운다. 안 그러면 복제본을 또 복제한다.
        ClearForeContainersInternal(source);

        GameObject copy = Object.Instantiate(source);
        copy.name = $"{PREFIX}_{source.name}";
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(copy, source.scene);

        // 카메라 기준 축소. 루트에 있으므로 localScale이 곧 월드 스케일이다.
        //
        // 회전은 건드리지 않는다. 원본과 회전이 같아야 나중에 원본의 자식으로 넣을 때
        // 상대 회전이 0이 되어 비균일 스케일이 전단 없이 상쇄된다.
        copy.transform.rotation = source.transform.rotation;
        copy.transform.localScale = source.transform.lossyScale * k;
        copy.transform.position =
            camPos + (source.transform.position - camPos) * k
            + Vector3.up * _extraY
            + camFwd * _extraDepth;

        Undo.RegisterCreatedObjectUndo(copy, "Foreground Project");

        // 복제본 안에 딸려온 전경 컨테이너 제거
        StripNested(copy);

        int removedColliders = 0;
        if (_removeColliders) removedColliders = RemoveColliders(copy);

        int thinned = 0;
        if (_keepRatio < 1f) thinned = Thin(copy, _keepRatio, new System.Random(_seed));

        if (_layerIndex >= 0) SetLayerRecursive(copy.transform, _layerIndex);

        // 다 만든 뒤 원본의 자식으로 편입. 회전이 같아 정확히 상쇄된다.
        copy.transform.SetParent(source.transform, true);

        GameObject volume = null;
        if (_createShadowVolume) volume = CreateShadowVolume(copy, source);

        Undo.CollapseUndoOperations(group);

        _lastReport =
            $"\"{source.name}\" → \"{copy.name}\"\n" +
            $"거리 {depth:F2} → {foreDepth:F2}   배율 k = {k:F3}\n" +
            $"콜라이더 {removedColliders}개 제거" +
            (thinned > 0 ? $" · 장식 {thinned}개 솎음" : "") +
            (volume != null ? " · Shadow Volume 생성" : "");

        Debug.Log($"[ForegroundProjector] {_lastReport.Replace("\n", "  ")}");
    }

    /// <summary>복제본 크기에 맞춘 Shadow Volume. 원본의 자식으로 붙여 같이 움직이게 한다.</summary>
    GameObject CreateShadowVolume(GameObject copy, GameObject source)
    {
        if (!TryGetWorldBounds(copy.transform, out Bounds b, null)) return null;

        GameObject go = new GameObject($"{PREFIX}_ShadowVolume_{source.name}");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, source.scene);

        // 스케일 1, 회전은 원본과 동일 -> 편입할 때 상대 회전 0
        go.transform.SetPositionAndRotation(b.center, source.transform.rotation);
        go.transform.localScale = Vector3.one;

        ShadowVolume volume = go.AddComponent<ShadowVolume>();
        volume.size = b.size + Vector3.one * (_shadowPadding * 2f);
        volume.lightMultiplier = 0f;
        volume.edgeFade = 0.05f;

        Undo.RegisterCreatedObjectUndo(go, "Foreground Project");

        go.transform.SetParent(source.transform, true);
        return go;
    }

    #endregion

    #region Copy cleanup

    /// <summary>복제본 안에 딸려온 전경 컨테이너를 지운다.</summary>
    static void StripNested(GameObject copy)
    {
        List<Transform> toRemove = new List<Transform>();

        foreach (Transform t in copy.GetComponentsInChildren<Transform>(true))
        {
            if (t == null || t == copy.transform) continue;
            if (t.name.StartsWith(PREFIX)) toRemove.Add(t);
        }

        foreach (Transform t in toRemove)
        {
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }
    }

    static int RemoveColliders(GameObject copy)
    {
        Collider[] colliders = copy.GetComponentsInChildren<Collider>(true);
        int count = 0;

        foreach (Collider c in colliders)
        {
            if (c == null) continue;
            Object.DestroyImmediate(c);
            count++;
        }

        return count;
    }

    /// <summary>장식 컨테이너 안의 인스턴스를 무작위로 솎는다. 플랫폼 본체는 건드리지 않는다.</summary>
    static int Thin(GameObject copy, float keepRatio, System.Random rng)
    {
        List<Transform> instances = new List<Transform>();

        foreach (Transform t in copy.GetComponentsInChildren<Transform>(true))
        {
            if (t == null || t.parent == null) continue;
            if (!IsDecorContainer(t.parent.name)) continue;

            instances.Add(t);
        }

        int removed = 0;
        foreach (Transform t in instances)
        {
            if (t == null) continue;
            if (rng.NextDouble() <= keepRatio) continue;

            Object.DestroyImmediate(t.gameObject);
            removed++;
        }

        return removed;
    }

    static void SetLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t) SetLayerRecursive(child, layer);
    }

    #endregion

    #region Camera

    Vector3 ResolveCameraPosition(Bounds sourceBounds)
    {
        if (_referenceCamera != null) return _referenceCamera.transform.position;

        // 리그 기준: 플랫폼 윗면에 선 캐릭터를 뒤에서 내려다보는 위치
        Vector3 forward = ResolveCameraForward();
        return new Vector3(sourceBounds.center.x, sourceBounds.max.y + _rigHeight, sourceBounds.center.z)
               - forward * _rigDistance;
    }

    Vector3 ResolveCameraForward()
    {
        if (_referenceCamera != null) return _referenceCamera.transform.forward;
        return Vector3.forward;
    }

    #endregion

    #region Bounds & containers

    static bool IsDecorContainer(string name)
    {
        foreach (string p in DECOR_PREFIXES)
        {
            if (name.StartsWith(p)) return true;
        }
        return false;
    }

    static bool IsUnderPrefix(Transform t, Transform root, string prefix)
    {
        Transform current = t;
        while (current != null && current != root)
        {
            if (current.name.StartsWith(prefix)) return true;
            current = current.parent;
        }
        return false;
    }

    /// <summary>원본 경계. 이미 만들어 둔 전경 복제본은 빼야 반복 실행에서 커지지 않는다.</summary>
    static bool TryGetSourceBounds(GameObject source, out Bounds bounds)
    {
        return TryGetWorldBounds(source.transform, out bounds, PREFIX);
    }

    static bool TryGetWorldBounds(Transform root, out Bounds bounds, string excludePrefix)
    {
        bounds = default;
        bool any = false;

        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null) continue;
            if (excludePrefix != null && IsUnderPrefix(r.transform, root, excludePrefix)) continue;

            if (!any) { bounds = r.bounds; any = true; }
            else bounds.Encapsulate(r.bounds);
        }

        if (any) return true;

        foreach (Collider c in root.GetComponentsInChildren<Collider>(true))
        {
            if (c == null) continue;
            if (excludePrefix != null && IsUnderPrefix(c.transform, root, excludePrefix)) continue;

            if (!any) { bounds = c.bounds; any = true; }
            else bounds.Encapsulate(c.bounds);
        }

        return any;
    }

    static int CountDecorContainers(GameObject source)
    {
        int count = 0;
        foreach (Transform child in source.transform)
        {
            if (IsDecorContainer(child.name)) count++;
        }
        return count;
    }

    static int CountForeContainers(GameObject source)
    {
        if (source == null) return 0;

        int count = 0;
        foreach (Transform child in source.transform)
        {
            if (child.name.StartsWith(PREFIX)) count++;
        }
        return count;
    }

    void ClearForeContainers(GameObject source)
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Foreground Clear");

        ClearForeContainersInternal(source);

        Undo.CollapseUndoOperations(group);
        _lastReport = null;
    }

    static void ClearForeContainersInternal(GameObject source)
    {
        if (source == null) return;

        List<Transform> toRemove = new List<Transform>();
        foreach (Transform child in source.transform)
        {
            if (child.name.StartsWith(PREFIX)) toRemove.Add(child);
        }

        foreach (Transform t in toRemove)
        {
            if (t != null) Undo.DestroyObjectImmediate(t.gameObject);
        }
    }

    #endregion
}
