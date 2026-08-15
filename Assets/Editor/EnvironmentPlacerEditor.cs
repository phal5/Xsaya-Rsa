using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tools 메뉴에서 접근 가능한 환경 오브젝트 절차적 배치 유틸리티.
/// 여러 프로파일을 레이어로 등록하고, 각 레이어를 독립적으로 또는 일괄 적용할 수 있습니다.
/// </summary>
public class EnvironmentPlacerWindow : EditorWindow
{
    const string CONTAINER_PREFIX = "_Deco";

    public enum PlacementSide
    {
        Top,    // (+Y)
        Bottom, // (-Y)
        Left,   // (-X)
        Right,  // (+X)
        Front,  // (+Z)
        Back    // (-Z)
    }

    /// <summary>
    /// 개별 배치 레이어. 프로파일 + 레이어별 설정을 담습니다.
    /// </summary>
    [System.Serializable]
    class PlacementLayer
    {
        public string name = "Layer";
        public EnvironmentPlacerProfile profile;
        public float density = 1f;
        public float globalScale = 1f;
        public float zOffset = 0f;
        public PlacementSide side = PlacementSide.Top;
        public bool enabled = true;
        public bool foldout = true;
    }

    List<PlacementLayer> _layers = new List<PlacementLayer>();
    Vector2 _scrollPos;
    bool _fitBlockZScale = false;

    // 6개 방향별 그룹 폴드아웃 상태
    bool[] _sideGroupFoldouts = new bool[6] { true, true, true, true, true, true };

    // 마지막 작업 결과
    int _lastGeneratedCount;
    string _lastTargetName;

    [MenuItem("Tools/Environment Placer")]
    public static void ShowWindow()
    {
        EnvironmentPlacerWindow window = GetWindow<EnvironmentPlacerWindow>("Environment Placer");
        window.minSize = new Vector2(340, 450);
    }

    void OnEnable()
    {
        if (_layers.Count == 0)
        {
            _layers.Add(new PlacementLayer());
        }
    }

    void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        DrawHeader();
        DrawSelectionInfo();
        DrawLayers();
        DrawActions();
        DrawLastResult();

        EditorGUILayout.EndScrollView();
    }

    void OnSelectionChange()
    {
        Repaint();
    }

    #region UI Sections

    void DrawHeader()
    {
        GUILayout.Space(5);
        GUILayout.Label("🌿 Environment Placer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "레이어별로 프로파일을 등록하고 Generate를 눌러주세요.\n" +
            "각 레이어는 독립된 컨테이너로 관리됩니다.",
            MessageType.Info
        );
        GUILayout.Space(5);
    }

    void DrawSelectionInfo()
    {
        GUILayout.Label("선택된 오브젝트", EditorStyles.miniBoldLabel);

        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            EditorGUILayout.HelpBox("Hierarchy에서 블록 오브젝트를 선택해주세요.", MessageType.Warning);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("대상", selected, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();

            // 기존 레이어 컨테이너 정보
            int existingCount = CountContainersOnTarget(selected);
            if (existingCount > 0)
            {
                EditorGUILayout.LabelField($"   기존 레이어 컨테이너: {existingCount}개", EditorStyles.miniLabel);
            }
        }

        GUILayout.Space(10);
    }

    static string GetSideDisplayName(PlacementSide side)
    {
        switch (side)
        {
            case PlacementSide.Top: return "⬆ (Top)";
            case PlacementSide.Bottom: return "⬇ (Bottom)";
            case PlacementSide.Left: return "⬅ (Left)";
            case PlacementSide.Right: return "➡ (Right)";
            case PlacementSide.Front: return "⏹ (Front)";
            case PlacementSide.Back: return "⏺ (Back)";
            default: return side.ToString();
        }
    }

    void DrawLayers()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("방향별 레이어 그룹", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+ 레이어 전체 펼치기", EditorStyles.miniButton))
        {
            for (int k = 0; k < _sideGroupFoldouts.Length; k++) _sideGroupFoldouts[k] = true;
        }
        if (GUILayout.Button("- 레이어 전체 접기", EditorStyles.miniButton))
        {
            for (int k = 0; k < _sideGroupFoldouts.Length; k++) _sideGroupFoldouts[k] = false;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        PlacementSide[] sides = (PlacementSide[])System.Enum.GetValues(typeof(PlacementSide));
        int removeIndex = -1;

        foreach (PlacementSide side in sides)
        {
            int sideIndex = (int)side;
            List<PlacementLayer> sideLayers = _layers.FindAll(l => l.side == side);

            // 방향 그룹 박스
            Color groupBg = sideLayers.Count > 0 ? new Color(0.22f, 0.25f, 0.28f) : new Color(0.18f, 0.18f, 0.18f);
            GUI.backgroundColor = groupBg;
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();

            string headerTitle = $"{GetSideDisplayName(side)} ({sideLayers.Count}개 레이어)";
            _sideGroupFoldouts[sideIndex] = EditorGUILayout.Foldout(_sideGroupFoldouts[sideIndex], headerTitle, true, EditorStyles.foldoutHeader);

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(0.4f, 0.7f, 0.9f);
            if (GUILayout.Button("+ 레이어 추가", EditorStyles.miniButton, GUILayout.Width(90)))
            {
                _sideGroupFoldouts[sideIndex] = true;
                _layers.Add(new PlacementLayer
                {
                    name = $"{side} Layer {sideLayers.Count + 1}",
                    side = side
                });
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            if (_sideGroupFoldouts[sideIndex])
            {
                GUILayout.Space(3);

                if (sideLayers.Count == 0)
                {
                    EditorGUILayout.LabelField("   등록된 레이어가 없습니다.", EditorStyles.miniLabel);
                }
                else
                {
                    for (int i = 0; i < sideLayers.Count; i++)
                    {
                        PlacementLayer layer = sideLayers[i];
                        int globalIndex = _layers.IndexOf(layer);

                        // 레이어 개별 박스
                        Color bgColor = layer.enabled ? (layer.profile != null ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.3f, 0.28f, 0.2f)) : new Color(0.2f, 0.2f, 0.2f);
                        GUI.backgroundColor = bgColor;
                        EditorGUILayout.BeginVertical("box");
                        GUI.backgroundColor = Color.white;

                        // 헤더: 활성화 + 이름 + 삭제
                        EditorGUILayout.BeginHorizontal();

                        layer.enabled = EditorGUILayout.Toggle(layer.enabled, GUILayout.Width(15));

                        string displayName = $"[{i}] {layer.name}";
                        if (layer.profile != null)
                            displayName += $" ({layer.profile.name})";

                        layer.foldout = EditorGUILayout.Foldout(layer.foldout, displayName, true);

                        // 삭제 버튼
                        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
                        if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(22)))
                        {
                            removeIndex = globalIndex;
                        }
                        GUI.backgroundColor = Color.white;

                        EditorGUILayout.EndHorizontal();

                        // 상세 설정
                        if (layer.foldout)
                        {
                            EditorGUI.indentLevel++;

                            layer.name = EditorGUILayout.TextField("이름", layer.name);

                            layer.profile = (EnvironmentPlacerProfile)EditorGUILayout.ObjectField(
                                "프로파일", layer.profile, typeof(EnvironmentPlacerProfile), false
                            );

                            if (layer.profile != null)
                            {
                                int entryCount = layer.profile.Entries != null ? layer.profile.Entries.Length : 0;
                                string placementModeStr = layer.profile.IsRandomPlacement ? "랜덤" : "고정 간격";
                                EditorGUILayout.LabelField($"   프리팹 종류: {entryCount}개 | 배치: {placementModeStr}", EditorStyles.miniLabel);
                            }

                            layer.density = EditorGUILayout.Slider("밀도", layer.density, 0.1f, 3f);
                            layer.globalScale = EditorGUILayout.Slider("전체 스케일", layer.globalScale, 0.1f, 5f);
                            layer.zOffset = EditorGUILayout.FloatField("깊이 오프셋 (Z)", layer.zOffset);

                            // 개별 레이어 Generate/Clear
                            EditorGUILayout.BeginHorizontal();

                            GameObject sel = Selection.activeGameObject;
                            bool canGen = layer.enabled && layer.profile != null && sel != null;

                            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                            EditorGUI.BeginDisabledGroup(!canGen);
                            if (GUILayout.Button("Generate", EditorStyles.miniButton, GUILayout.Height(18)))
                            {
                                GenerateLayer(sel, layer, globalIndex);
                            }
                            EditorGUI.EndDisabledGroup();

                            bool hasCont = sel != null && sel.transform.Find(GetContainerName(globalIndex)) != null;
                            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
                            EditorGUI.BeginDisabledGroup(!hasCont);
                            if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Height(18)))
                            {
                                ClearLayer(sel, globalIndex);
                            }
                            EditorGUI.EndDisabledGroup();

                            GUI.backgroundColor = Color.white;
                            EditorGUILayout.EndHorizontal();

                            EditorGUI.indentLevel--;
                        }

                        EditorGUILayout.EndVertical();
                        GUILayout.Space(2);
                    }
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(4);
        }

        if (removeIndex >= 0 && removeIndex < _layers.Count)
        {
            _layers.RemoveAt(removeIndex);
        }

        GUILayout.Space(5);
    }

    void DrawActions()
    {
        GameObject selected = Selection.activeGameObject;
        bool hasSelection = selected != null;
        bool hasAnyProfile = false;
        foreach (var layer in _layers)
        {
            if (layer.enabled && layer.profile != null)
            {
                hasAnyProfile = true;
                break;
            }
        }
        bool canGenerate = hasAnyProfile && hasSelection;

        GUILayout.Label("옵션 및 실행", EditorStyles.boldLabel);
        _fitBlockZScale = EditorGUILayout.Toggle("Z 스케일 자동 맞춤", _fitBlockZScale);
        GUILayout.Space(5);

        // Generate All 버튼
        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        EditorGUI.BeginDisabledGroup(!canGenerate);
        if (GUILayout.Button($"🌿 Generate All ({EnabledLayerCount()}개 레이어)", GUILayout.Height(32)))
        {
            GenerateAllLayers(selected);
        }
        EditorGUI.EndDisabledGroup();

        // Clear (선택된 오브젝트)
        bool hasDeco = hasSelection && CountContainersOnTarget(selected) > 0;
        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
        EditorGUI.BeginDisabledGroup(!hasDeco);
        if (GUILayout.Button("🗑 Clear (선택된 오브젝트)", GUILayout.Height(26)))
        {
            ClearAllLayersOnTarget(selected);
        }
        EditorGUI.EndDisabledGroup();

        // Clear All (씬 전체)
        int totalContainers = CountAllDecorationContainers();
        GUI.backgroundColor = new Color(0.7f, 0.3f, 0.3f);
        EditorGUI.BeginDisabledGroup(totalContainers == 0);
        if (GUILayout.Button($"🗑 Clear All — 씬 전체 ({totalContainers}개)", GUILayout.Height(26)))
        {
            if (EditorUtility.DisplayDialog(
                "환경 장식 일괄 삭제",
                $"씬 내 모든 장식 컨테이너 {totalContainers}개를 삭제합니다.\n이 작업은 Ctrl+Z로 되돌릴 수 있습니다.",
                "삭제", "취소"))
            {
                ClearAllInScene();
            }
        }
        EditorGUI.EndDisabledGroup();

        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);
    }

    void DrawLastResult()
    {
        if (_lastGeneratedCount > 0 && !string.IsNullOrEmpty(_lastTargetName))
        {
            EditorGUILayout.HelpBox(
                $"마지막 작업: \"{_lastTargetName}\"에 {_lastGeneratedCount}개 배치 완료",
                MessageType.None
            );
        }
    }

    #endregion

    #region Core Logic

    string GetContainerName(int layerIndex)
    {
        if (layerIndex >= 0 && layerIndex < _layers.Count)
            return $"{CONTAINER_PREFIX}_{layerIndex}_{_layers[layerIndex].name}";
        return $"{CONTAINER_PREFIX}_{layerIndex}";
    }

    int EnabledLayerCount()
    {
        int count = 0;
        foreach (var layer in _layers)
        {
            if (layer.enabled && layer.profile != null) count++;
        }
        return count;
    }

    void GenerateAllLayers(GameObject target)
    {
        Undo.SetCurrentGroupName("Environment Placer Generate All");
        ClearAllLayersOnTarget(target);

        int totalCount = 0;

        for (int i = 0; i < _layers.Count; i++)
        {
            PlacementLayer layer = _layers[i];
            if (!layer.enabled || layer.profile == null) continue;

            int count = GenerateLayer(target, layer, i, false);
            totalCount += count;
        }

        if (_fitBlockZScale)
        {
            FitBlockZScaleToSpawnedObjects(target);
        }

        _lastGeneratedCount = totalCount;
        _lastTargetName = target.name;

        Debug.Log($"[EnvironmentPlacer] \"{target.name}\": 전체 {EnabledLayerCount()}개 레이어, 총 {totalCount}개 오브젝트 배치 완료.");
    }

    int GenerateLayer(GameObject target, PlacementLayer layer, int layerIndex, bool autoFitScale = true)
    {
        // 기존 해당 레이어 컨테이너 정리
        ClearLayer(target, layerIndex);

        Bounds bounds = CalculateBounds(target);
        if (bounds.size.x <= 0f || bounds.size.z <= 0f)
        {
            Debug.LogWarning($"[EnvironmentPlacer] \"{target.name}\": 유효한 Collider 또는 Renderer를 찾을 수 없습니다.");
            return 0;
        }

        // 레이어별 컨테이너 생성 (부모 스케일 보정)
        string containerName = GetContainerName(layerIndex);
        GameObject container = new GameObject(containerName);
        container.transform.SetParent(target.transform, false);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;

        Vector3 parentLossyScale = target.transform.lossyScale;
        container.transform.localScale = new Vector3(
            1f / parentLossyScale.x,
            1f / parentLossyScale.y,
            1f / parentLossyScale.z
        );
        Undo.RegisterCreatedObjectUndo(container, "Environment Placer Generate");

        // 파라미터 결정
        float spacing = layer.profile.MinimumSpacing / layer.density;

        int seed = layer.profile.UseRandomSeed
            ? System.Environment.TickCount + layerIndex
            : layer.profile.Seed + layerIndex;

        System.Random rng = new System.Random(seed);

        List<Vector2> points;
        Quaternion sideRotation;
        Vector3 surfacePos;

        switch (layer.side)
        {
            case PlacementSide.Top:
                sideRotation = Quaternion.identity;
                surfacePos = new Vector3(0f, bounds.max.y + layer.profile.YOffset, 0f);
                if (layer.profile.IsRandomPlacement)
                {
                    points = PoissonDiskSample(
                        new Vector2(bounds.min.x, bounds.min.z),
                        new Vector2(bounds.max.x, bounds.max.z),
                        spacing,
                        layer.profile.SamplesBeforeRejection,
                        rng
                    );
                }
                else
                {
                    points = LinearSample(bounds.min.x, bounds.max.x, bounds.center.z, spacing);
                }
                break;

            case PlacementSide.Bottom:
                sideRotation = Quaternion.Euler(180f, 0f, 0f);
                surfacePos = new Vector3(0f, bounds.min.y - layer.profile.YOffset, 0f);
                if (layer.profile.IsRandomPlacement)
                {
                    points = PoissonDiskSample(
                        new Vector2(bounds.min.x, bounds.min.z),
                        new Vector2(bounds.max.x, bounds.max.z),
                        spacing,
                        layer.profile.SamplesBeforeRejection,
                        rng
                    );
                }
                else
                {
                    points = LinearSample(bounds.min.x, bounds.max.x, bounds.center.z, spacing);
                }
                break;

            case PlacementSide.Left:
                sideRotation = Quaternion.Euler(0f, 0f, 90f);
                surfacePos = new Vector3(bounds.min.x - layer.profile.YOffset, 0f, 0f);
                if (layer.profile.IsRandomPlacement)
                {
                    points = PoissonDiskSample(
                        new Vector2(bounds.min.z, bounds.min.y),
                        new Vector2(bounds.max.z, bounds.max.y),
                        spacing,
                        layer.profile.SamplesBeforeRejection,
                        rng
                    );
                }
                else
                {
                    points = LinearSample(bounds.min.z, bounds.max.z, bounds.center.y, spacing);
                }
                break;

            case PlacementSide.Right:
                sideRotation = Quaternion.Euler(0f, 0f, -90f);
                surfacePos = new Vector3(bounds.max.x + layer.profile.YOffset, 0f, 0f);
                if (layer.profile.IsRandomPlacement)
                {
                    points = PoissonDiskSample(
                        new Vector2(bounds.min.z, bounds.min.y),
                        new Vector2(bounds.max.z, bounds.max.y),
                        spacing,
                        layer.profile.SamplesBeforeRejection,
                        rng
                    );
                }
                else
                {
                    points = LinearSample(bounds.min.z, bounds.max.z, bounds.center.y, spacing);
                }
                break;

            case PlacementSide.Front:
                sideRotation = Quaternion.Euler(90f, 0f, 0f);
                surfacePos = new Vector3(0f, 0f, bounds.max.z + layer.profile.YOffset);
                if (layer.profile.IsRandomPlacement)
                {
                    points = PoissonDiskSample(
                        new Vector2(bounds.min.x, bounds.min.y),
                        new Vector2(bounds.max.x, bounds.max.y),
                        spacing,
                        layer.profile.SamplesBeforeRejection,
                        rng
                    );
                }
                else
                {
                    points = LinearSample(bounds.min.x, bounds.max.x, bounds.center.y, spacing);
                }
                break;

            case PlacementSide.Back:
                sideRotation = Quaternion.Euler(-90f, 0f, 0f);
                surfacePos = new Vector3(0f, 0f, bounds.min.z - layer.profile.YOffset);
                if (layer.profile.IsRandomPlacement)
                {
                    points = PoissonDiskSample(
                        new Vector2(bounds.min.x, bounds.min.y),
                        new Vector2(bounds.max.x, bounds.max.y),
                        spacing,
                        layer.profile.SamplesBeforeRejection,
                        rng
                    );
                }
                else
                {
                    points = LinearSample(bounds.min.x, bounds.max.x, bounds.center.y, spacing);
                }
                break;

            default:
                points = new List<Vector2>();
                sideRotation = Quaternion.identity;
                surfacePos = Vector3.zero;
                break;
        }

        int count = 0;

        foreach (Vector2 point in points)
        {
            GameObject prefab = layer.profile.PickRandomPrefab(rng);
            if (prefab == null) continue;

            EnvironmentEntry entry = layer.profile.GetEntryForPrefab(prefab);

            // 스케일 (엔트리 랜덤 범위 × 레이어 전체 스케일)
            float scaleFactor = Mathf.Lerp(
                Mathf.Max(entry.MinScale, 0.01f),
                Mathf.Max(entry.MaxScale, 0.01f),
                (float)rng.NextDouble()
            ) * layer.globalScale;

            // 회전: 배치면 회전 + Y축 랜덤 회전
            Quaternion rotation = sideRotation;
            if (entry.RandomYRotation)
            {
                rotation *= Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            }

            // 방향별 3D 위치 매핑
            Vector3 position;
            switch (layer.side)
            {
                case PlacementSide.Top:
                case PlacementSide.Bottom:
                    position = new Vector3(point.x, surfacePos.y, point.y + layer.zOffset);
                    break;
                case PlacementSide.Left:
                case PlacementSide.Right:
                    position = new Vector3(surfacePos.x, point.y, point.x + layer.zOffset);
                    break;
                case PlacementSide.Front:
                    position = new Vector3(point.x, point.y, surfacePos.z + layer.zOffset);
                    break;
                case PlacementSide.Back:
                    position = new Vector3(point.x, point.y, surfacePos.z - layer.zOffset);
                    break;
                default:
                    position = Vector3.zero;
                    break;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, container.transform);
            if (instance != null)
            {
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.transform.localScale = Vector3.one * scaleFactor;
                Undo.RegisterCreatedObjectUndo(instance, "Environment Placer Generate");
                count++;
            }
        }

        Debug.Log($"[EnvironmentPlacer] \"{target.name}\" > \"{containerName}\": {count}개 배치. (Seed: {seed})");

        if (autoFitScale && _fitBlockZScale)
        {
            FitBlockZScaleToSpawnedObjects(target);
        }

        return count;
    }

    void ClearLayer(GameObject target, int layerIndex)
    {
        if (target == null) return;

        string containerName = GetContainerName(layerIndex);
        Transform existing = target.transform.Find(containerName);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }
    }

    void ClearAllLayersOnTarget(GameObject target)
    {
        if (target == null) return;

        // 이 오브젝트 아래의 모든 _Deco_ 컨테이너 삭제
        List<Transform> toRemove = new List<Transform>();
        foreach (Transform child in target.transform)
        {
            if (child.name.StartsWith(CONTAINER_PREFIX))
            {
                toRemove.Add(child);
            }
        }

        foreach (Transform t in toRemove)
        {
            Undo.DestroyObjectImmediate(t.gameObject);
        }
    }

    void ClearAllInScene()
    {
        GameObject[] allContainers = FindDecorationContainers();
        if (allContainers.Length == 0) return;

        Undo.SetCurrentGroupName("Environment Placer Clear All");

        for (int i = allContainers.Length - 1; i >= 0; i--)
        {
            if (allContainers[i] != null)
            {
                Undo.DestroyObjectImmediate(allContainers[i]);
            }
        }

        _lastGeneratedCount = 0;
        _lastTargetName = null;

        Debug.Log($"[EnvironmentPlacer] 씬 전체에서 {allContainers.Length}개의 장식 컨테이너를 삭제했습니다.");
    }

    int CountContainersOnTarget(GameObject target)
    {
        int count = 0;
        foreach (Transform child in target.transform)
        {
            if (child.name.StartsWith(CONTAINER_PREFIX)) count++;
        }
        return count;
    }

    int CountAllDecorationContainers()
    {
        return FindDecorationContainers().Length;
    }

    static GameObject[] FindDecorationContainers()
    {
        Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        List<GameObject> containers = new List<GameObject>();

        foreach (Transform t in allTransforms)
        {
            if (t.name.StartsWith(CONTAINER_PREFIX))
            {
                containers.Add(t.gameObject);
            }
        }

        return containers.ToArray();
    }

    #endregion

    #region Bounds Calculation

    static Bounds CalculateBounds(GameObject target)
    {
        Collider col = target.GetComponent<Collider>();
        if (col != null) return col.bounds;

        Renderer rend = target.GetComponent<Renderer>();
        if (rend != null) return rend.bounds;

        Collider childCol = target.GetComponentInChildren<Collider>();
        if (childCol != null) return childCol.bounds;

        Renderer childRend = target.GetComponentInChildren<Renderer>();
        if (childRend != null) return childRend.bounds;

        return new Bounds(target.transform.position, Vector3.zero);
    }

    static bool IsDecoTransform(Transform t, Transform target)
    {
        Transform current = t;
        while (current != null && current != target)
        {
            if (current.name.StartsWith(CONTAINER_PREFIX)) return true;
            current = current.parent;
        }
        return false;
    }

    static Bounds CalculateTargetBlockBoundsOnly(GameObject target)
    {
        Collider col = target.GetComponent<Collider>();
        if (col != null) return col.bounds;

        Renderer rend = target.GetComponent<Renderer>();
        if (rend != null) return rend.bounds;

        Collider[] childCols = target.GetComponentsInChildren<Collider>();
        foreach (var c in childCols)
        {
            if (!IsDecoTransform(c.transform, target.transform))
            {
                return c.bounds;
            }
        }

        Renderer[] childRends = target.GetComponentsInChildren<Renderer>();
        foreach (var r in childRends)
        {
            if (!IsDecoTransform(r.transform, target.transform))
            {
                return r.bounds;
            }
        }

        return new Bounds(target.transform.position, Vector3.zero);
    }

    void FitBlockZScaleToSpawnedObjects(GameObject target)
    {
        if (target == null) return;

        List<Renderer> spawnedRenderers = new List<Renderer>();
        Renderer[] allRenderers = target.GetComponentsInChildren<Renderer>();

        foreach (var rend in allRenderers)
        {
            if (IsDecoTransform(rend.transform, target.transform))
            {
                spawnedRenderers.Add(rend);
            }
        }

        if (spawnedRenderers.Count == 0) return;

        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (var rend in spawnedRenderers)
        {
            minZ = Mathf.Min(minZ, rend.bounds.min.z);
            maxZ = Mathf.Max(maxZ, rend.bounds.max.z);
        }

        if (minZ >= maxZ) return;

        Bounds blockBounds = CalculateTargetBlockBoundsOnly(target);
        if (blockBounds.size.z <= 0.0001f) return;

        float currentMinZ = blockBounds.min.z;
        float currentMaxZ = blockBounds.max.z;

        float targetMinZ = Mathf.Min(currentMinZ, minZ);
        float targetMaxZ = Mathf.Max(currentMaxZ, maxZ);

        float newDepth = targetMaxZ - targetMinZ;
        float currentDepth = currentMaxZ - currentMinZ;

        if (currentDepth <= 0.0001f || Mathf.Approximately(newDepth, currentDepth)) return;

        int childCount = target.transform.childCount;
        Vector3[] oldWorldPositions = new Vector3[childCount];
        Quaternion[] oldWorldRotations = new Quaternion[childCount];
        Vector3[] oldWorldLossyScales = new Vector3[childCount];
        Transform[] children = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = target.transform.GetChild(i);
            children[i] = child;
            oldWorldPositions[i] = child.position;
            oldWorldRotations[i] = child.rotation;
            oldWorldLossyScales[i] = child.lossyScale;
            Undo.RecordObject(child, "Fit Block Z Scale - Child");
        }

        Undo.RecordObject(target.transform, "Fit Block Z Scale - Target");

        float scaleFactorZ = newDepth / currentDepth;
        Vector3 currentLocalScale = target.transform.localScale;
        target.transform.localScale = new Vector3(
            currentLocalScale.x,
            currentLocalScale.y,
            currentLocalScale.z * scaleFactorZ
        );

        float newCenterZ = (targetMinZ + targetMaxZ) * 0.5f;
        float currentCenterZ = (currentMinZ + currentMaxZ) * 0.5f;
        float shiftZ = newCenterZ - currentCenterZ;

        target.transform.position += new Vector3(0f, 0f, shiftZ);

        Vector3 parentLossyScale = target.transform.lossyScale;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = children[i];
            child.position = oldWorldPositions[i];
            child.rotation = oldWorldRotations[i];

            Vector3 prevLossy = oldWorldLossyScales[i];
            child.localScale = new Vector3(
                parentLossyScale.x != 0f ? prevLossy.x / parentLossyScale.x : child.localScale.x,
                parentLossyScale.y != 0f ? prevLossy.y / parentLossyScale.y : child.localScale.y,
                parentLossyScale.z != 0f ? prevLossy.z / parentLossyScale.z : child.localScale.z
            );
        }
    }

    #endregion

    #region Poisson Disk Sampling

    /// <summary>
    /// Bridson 알고리즘 기반 Poisson Disk Sampling.
    /// 최소 간격을 유지하면서 자연스러운 포인트 분포를 생성합니다.
    /// </summary>
    static List<Vector2> PoissonDiskSample(Vector2 regionMin, Vector2 regionMax, float minDist, int maxAttempts, System.Random rng)
    {
        float cellSize = minDist / Mathf.Sqrt(2f);
        Vector2 regionSize = regionMax - regionMin;

        int gridWidth = Mathf.CeilToInt(regionSize.x / cellSize);
        int gridHeight = Mathf.CeilToInt(regionSize.y / cellSize);

        if (gridWidth <= 0 || gridHeight <= 0) return new List<Vector2>();

        int[,] grid = new int[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                grid[x, y] = -1;

        List<Vector2> points = new List<Vector2>();
        List<int> activeList = new List<int>();

        Vector2 startPoint = new Vector2(
            regionMin.x + (float)rng.NextDouble() * regionSize.x,
            regionMin.y + (float)rng.NextDouble() * regionSize.y
        );

        AddPoint(startPoint, points, activeList, grid, regionMin, cellSize);

        while (activeList.Count > 0)
        {
            int activeIndex = rng.Next(activeList.Count);
            int pointIndex = activeList[activeIndex];
            Vector2 center = points[pointIndex];
            bool found = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                float radius = minDist + (float)rng.NextDouble() * minDist;
                Vector2 candidate = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                if (candidate.x < regionMin.x || candidate.x >= regionMax.x ||
                    candidate.y < regionMin.y || candidate.y >= regionMax.y)
                    continue;

                if (IsValidPoint(candidate, points, grid, regionMin, cellSize, minDist, gridWidth, gridHeight))
                {
                    AddPoint(candidate, points, activeList, grid, regionMin, cellSize);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                activeList.RemoveAt(activeIndex);
            }
        }

        return points;
    }

    static void AddPoint(Vector2 point, List<Vector2> points, List<int> activeList, int[,] grid, Vector2 regionMin, float cellSize)
    {
        int gridX = Mathf.FloorToInt((point.x - regionMin.x) / cellSize);
        int gridY = Mathf.FloorToInt((point.y - regionMin.y) / cellSize);

        if (gridX >= 0 && gridX < grid.GetLength(0) && gridY >= 0 && gridY < grid.GetLength(1))
        {
            grid[gridX, gridY] = points.Count;
        }

        activeList.Add(points.Count);
        points.Add(point);
    }

    static bool IsValidPoint(Vector2 candidate, List<Vector2> points, int[,] grid, Vector2 regionMin, float cellSize, float minDist, int gridWidth, int gridHeight)
    {
        int gridX = Mathf.FloorToInt((candidate.x - regionMin.x) / cellSize);
        int gridY = Mathf.FloorToInt((candidate.y - regionMin.y) / cellSize);

        int searchStartX = Mathf.Max(0, gridX - 2);
        int searchEndX = Mathf.Min(gridWidth - 1, gridX + 2);
        int searchStartY = Mathf.Max(0, gridY - 2);
        int searchEndY = Mathf.Min(gridHeight - 1, gridY + 2);

        float minDistSqr = minDist * minDist;

        for (int x = searchStartX; x <= searchEndX; x++)
        {
            for (int y = searchStartY; y <= searchEndY; y++)
            {
                int neighborIndex = grid[x, y];
                if (neighborIndex >= 0)
                {
                    Vector2 diff = candidate - points[neighborIndex];
                    if (diff.sqrMagnitude < minDistSqr)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    #endregion

    #region Linear Sampling

    /// <summary>
    /// 동일한 Z좌표 상에서 X 방향으로 일정 간격으로 포인트를 생성합니다.
    /// </summary>
    static List<Vector2> LinearSample(float minX, float maxX, float centerZ, float spacing)
    {
        List<Vector2> points = new List<Vector2>();
        if (spacing <= 0f || minX > maxX) return points;

        for (float x = minX; x <= maxX + 0.0001f; x += spacing)
        {
            points.Add(new Vector2(x, centerZ));
        }

        return points;
    }

    #endregion
}
