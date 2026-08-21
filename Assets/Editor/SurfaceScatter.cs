using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 레이캐스트로 실제 표면을 찾아 프리팹을 심는 배치 유틸리티.
///
/// Environment Placer는 대상의 AABB 여섯 면을 평면으로 보고 그 위에 뿌린다. 축에 정렬된 상자에는
/// 잘 맞지만, 기울어진 벽이나 굴곡진 지형에는 배치물이 허공에 뜨거나 파묻힌다.
///
/// 이쪽은 평면에서 광선을 쏴 맞은 지점과 법선을 그대로 쓴다. 덩굴처럼 표면에 붙어야 하는 것,
/// 경사면에만 심어야 하는 것에 쓴다.
///
/// 프리팹 목록·가중치·스케일 범위·시드는 EnvironmentPlacerProfile을 그대로 재사용한다.
/// 같은 프로파일을 두 툴이 나눠 쓴다.
/// </summary>
public class SurfaceScatterWindow : EditorWindow
{
    const string CONTAINER_PREFIX = "_Scatter";
    const int HARD_LIMIT = 20000;

    /// <summary>광선을 쏘는 방향.</summary>
    public enum CastDirection
    {
        Down,     // -Y  바닥/지형에 심을 때
        Up,       // +Y  천장
        Left,     // -X
        Right,    // +X
        Forward,  // +Z
        Back,     // -Z
        Custom,
    }

    EnvironmentPlacerProfile _profile;

    CastDirection _direction = CastDirection.Down;
    Vector3 _customDirection = new Vector3(0f, -1f, 0f);
    float _originMargin = 1f;

    LayerMask _layerMask = ~0;
    bool _restrictToTarget = true;

    float _density = 1f;
    float _globalScale = 1f;

    // 경사 필터 (월드 up 기준 각도). 0=바닥, 90=수직벽, 180=천장
    float _minSlope = 0f;
    float _maxSlope = 180f;

    [Range(0f, 1f)] float _normalBlend = 1f;
    float _surfaceOffset = 0f;

    Vector2 _scrollPos;
    bool _canceled;

    int _lastCount;
    int _lastMissCount;
    int _lastFilteredCount;
    string _lastTargetName;

    [MenuItem("Tools/Surface Scatter")]
    public static void ShowWindow()
    {
        SurfaceScatterWindow window = GetWindow<SurfaceScatterWindow>("Surface Scatter");
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
        DrawTarget();
        DrawProfile();
        DrawCast();
        DrawFilter();
        DrawOrientation();
        DrawActions();
        DrawLastResult();

        EditorGUILayout.EndScrollView();
    }

    #region UI

    void DrawHeader()
    {
        GUILayout.Space(5);
        GUILayout.Label("🌿 Surface Scatter (Raycast)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "광선을 쏴 실제 표면에 심습니다. 기울어진 면과 굴곡진 지형에 씁니다.\n" +
            "프리팹 목록은 Environment Placer 프로파일을 그대로 씁니다.",
            MessageType.Info
        );
        GUILayout.Space(5);
    }

    void DrawTarget()
    {
        GUILayout.Label("대상", EditorStyles.miniBoldLabel);

        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            EditorGUILayout.HelpBox("Hierarchy에서 심을 대상을 선택해주세요.", MessageType.Warning);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("선택됨", selected, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();

            if (!TryGetWorldBounds(selected, out Bounds bounds))
            {
                EditorGUILayout.HelpBox("이 오브젝트에서 Renderer나 Collider를 찾지 못했습니다.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField($"   경계 (월드): {bounds.size.x:F2} × {bounds.size.y:F2} × {bounds.size.z:F2}", EditorStyles.miniLabel);
            }

            int existing = CountContainers(selected);
            if (existing > 0)
            {
                EditorGUILayout.LabelField($"   기존 컨테이너: {existing}개", EditorStyles.miniLabel);
            }
        }

        GUILayout.Space(8);
    }

    void DrawProfile()
    {
        GUILayout.Label("프로파일", EditorStyles.miniBoldLabel);

        _profile = (EnvironmentPlacerProfile)EditorGUILayout.ObjectField("프로파일", _profile, typeof(EnvironmentPlacerProfile), false);

        if (_profile != null)
        {
            int entries = _profile.Entries != null ? _profile.Entries.Length : 0;
            EditorGUILayout.LabelField($"   프리팹 {entries}종 · 최소 간격 {_profile.MinimumSpacing:F2}", EditorStyles.miniLabel);

            if (!_profile.IsRandomPlacement)
            {
                EditorGUILayout.HelpBox("이 툴은 항상 Poisson 분포를 씁니다. 프로파일의 '고정 간격' 설정은 무시됩니다.", MessageType.None);
            }
        }

        _density = EditorGUILayout.Slider(
            new GUIContent("밀도", "프로파일의 최소 간격을 이 값으로 나눈다. 높을수록 촘촘하다."),
            _density, 0.1f, 3f
        );

        _globalScale = EditorGUILayout.Slider("전체 스케일", _globalScale, 0.1f, 5f);

        GUILayout.Space(8);
    }

    void DrawCast()
    {
        GUILayout.Label("광선", EditorStyles.miniBoldLabel);

        _direction = (CastDirection)EditorGUILayout.EnumPopup(
            new GUIContent("방향", "광선을 쏘는 방향. 경계의 반대쪽 면에서 이 방향으로 쏜다."),
            _direction
        );

        if (_direction == CastDirection.Custom)
        {
            EditorGUI.indentLevel++;
            _customDirection = EditorGUILayout.Vector3Field("커스텀 방향", _customDirection);
            EditorGUI.indentLevel--;
        }

        _originMargin = EditorGUILayout.FloatField(
            new GUIContent("시작 여유", "경계 밖으로 이만큼 물러나서 쏜다. 표면에 딱 붙은 시작점이 안쪽에서 시작되는 걸 막는다."),
            _originMargin
        );
        _originMargin = Mathf.Max(_originMargin, 0f);

        _layerMask = EditorGUILayout.MaskField(
            new GUIContent("레이어 마스크", "광선이 맞을 레이어."),
            _layerMask, UnityEditorInternal.InternalEditorUtility.layers
        );

        _restrictToTarget = EditorGUILayout.Toggle(
            new GUIContent("대상에만 맞히기", "선택한 오브젝트와 그 자식에 맞은 것만 인정한다. 끄면 마스크에 걸리는 아무 표면에나 심는다."),
            _restrictToTarget
        );

        GUILayout.Space(8);
    }

    void DrawFilter()
    {
        GUILayout.Label("경사 필터", EditorStyles.miniBoldLabel);

        EditorGUILayout.LabelField(
            "월드 위쪽 기준 각도. 0=바닥, 90=수직벽, 180=천장",
            EditorStyles.miniLabel
        );

        EditorGUILayout.MinMaxSlider(
            new GUIContent("허용 각도"),
            ref _minSlope, ref _maxSlope, 0f, 180f
        );

        EditorGUILayout.LabelField($"   {_minSlope:F0}° ~ {_maxSlope:F0}°", EditorStyles.miniLabel);

        GUILayout.Space(8);
    }

    void DrawOrientation()
    {
        GUILayout.Label("자세", EditorStyles.miniBoldLabel);

        _normalBlend = EditorGUILayout.Slider(
            new GUIContent("법선 정렬", "1이면 표면 법선을 그대로 따라 눕는다. 0이면 월드 위쪽으로 똑바로 선다. 덩굴처럼 표면에 붙는 것은 1, 나무처럼 중력 방향이 정해진 것은 0."),
            _normalBlend, 0f, 1f
        );

        _surfaceOffset = EditorGUILayout.FloatField(
            new GUIContent("표면 오프셋", "법선 방향으로 띄우는 거리. 음수면 표면에 박힌다."),
            _surfaceOffset
        );

        GUILayout.Space(8);
    }

    void DrawActions()
    {
        GameObject target = Selection.activeGameObject;

        bool hasTarget = target != null && TryGetWorldBounds(target, out _);
        bool hasProfile = _profile != null && _profile.Entries != null && _profile.Entries.Length > 0;

        GUILayout.Label("실행", EditorStyles.miniBoldLabel);

        if (!hasProfile && _profile != null)
        {
            EditorGUILayout.HelpBox("프로파일에 등록된 프리팹이 없습니다.", MessageType.Warning);
        }

        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        EditorGUI.BeginDisabledGroup(!hasTarget || !hasProfile);
        if (GUILayout.Button("🌿 Scatter", GUILayout.Height(32)))
        {
            RunCancelable("Surface Scatter", () => Scatter(target));
        }
        EditorGUI.EndDisabledGroup();

        bool hasContainer = target != null && CountContainers(target) > 0;
        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
        EditorGUI.BeginDisabledGroup(!hasContainer);
        if (GUILayout.Button("🗑 Clear (선택된 오브젝트)", GUILayout.Height(26)))
        {
            RunCancelable("Surface Scatter Clear", () => ClearContainers(target));
        }
        EditorGUI.EndDisabledGroup();

        GUI.backgroundColor = Color.white;
        GUILayout.Space(5);
    }

    void DrawLastResult()
    {
        if (_lastCount <= 0 && _lastMissCount <= 0) return;
        if (string.IsNullOrEmpty(_lastTargetName)) return;

        EditorGUILayout.HelpBox(
            $"마지막 작업: \"{_lastTargetName}\"\n" +
            $"심음 {_lastCount}개 · 빗나감 {_lastMissCount}개 · 경사 필터로 제외 {_lastFilteredCount}개",
            MessageType.None
        );
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
            Debug.Log("[SurfaceScatter] 취소되었습니다. 작업 전 상태로 되돌렸습니다.");
            return;
        }

        Undo.CollapseUndoOperations(group);
    }

    bool PollCancel(string info, float progress)
    {
        if (_canceled) return true;

        if (EditorUtility.DisplayCancelableProgressBar("Surface Scatter", info, progress))
        {
            _canceled = true;
        }

        return _canceled;
    }

    void Scatter(GameObject target)
    {
        if (!TryGetWorldBounds(target, out Bounds bounds)) return;

        Vector3 dir = ResolveDirection();
        if (dir.sqrMagnitude < 1e-6f)
        {
            Debug.LogWarning("[SurfaceScatter] 광선 방향이 영벡터입니다.");
            return;
        }
        dir.Normalize();

        // 방향에 직교하는 두 축. 이 평면 위에서 시작점을 뿌린다.
        BuildBasis(dir, out Vector3 u, out Vector3 v);

        float extentU = ProjectedExtent(bounds.extents, u);
        float extentV = ProjectedExtent(bounds.extents, v);
        float extentD = ProjectedExtent(bounds.extents, dir);

        float spacing = _profile.MinimumSpacing / Mathf.Max(_density, 0.01f);

        int seed = _profile.UseRandomSeed ? System.Environment.TickCount : _profile.Seed;
        System.Random rng = new System.Random(seed);

        List<Vector2> plane = PoissonDisk.Sample(
            new Vector2(-extentU, -extentV),
            new Vector2(extentU, extentV),
            spacing,
            _profile.SamplesBeforeRejection,
            rng,
            found => PollCancel($"배치 지점 계산 중… {found}개", -1f)
        );

        if (_canceled) return;

        if (plane.Count == 0)
        {
            Debug.LogWarning("[SurfaceScatter] 배치 지점이 나오지 않았습니다. 간격이 경계보다 큰지 확인하세요.");
            return;
        }

        if (plane.Count > HARD_LIMIT)
        {
            Debug.LogError($"[SurfaceScatter] 지점이 {plane.Count}개로 상한({HARD_LIMIT})을 넘습니다. 밀도를 낮추세요.");
            return;
        }

        ClearContainers(target);

        // 컨테이너는 씬 루트에서 만들어 다 채운 뒤, 맨 마지막에 통째로 편입시킨다.
        //
        // 대상의 자식으로 먼저 만들면 대상의 비균일 스케일이 위에서 내려온다. 그 아래에서
        // 비스듬히 회전한 배치물은 전단(shear)으로 찌그러지고, 축 사이 각도가 90도를 벗어난다.
        // 스케일은 축 길이만 바꿀 뿐 각도를 되돌리지 못하므로 나중에 손쓸 방법이 없다.
        //
        // 회전은 처음부터 대상에 맞춰 둔다. 항등으로 두면 편입할 때 대상과의 상대 회전이 남아
        // 그것만으로 전단이 생긴다. 상대 회전이 0이어야 대상의 스케일이 축 대 축으로 대응된다.
        GameObject container = new GameObject($"{CONTAINER_PREFIX}_{_profile.name}");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(container, target.scene);
        container.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
        container.transform.localScale = Vector3.one;
        Undo.RegisterCreatedObjectUndo(container, "Surface Scatter");

        // 콜라이더가 물리 쪽에 반영되기 전이면 광선이 예전 위치를 때린다.
        // autoSyncTransforms는 기본이 꺼져 있다.
        Physics.SyncTransforms();

        float maxDistance = 2f * extentD + 2f * _originMargin;
        Vector3 originBase = bounds.center - dir * (extentD + _originMargin);

        int count = 0;
        int missed = 0;
        int filtered = 0;

        for (int i = 0; i < plane.Count; i++)
        {
            if ((i & 0x3F) == 0 && PollCancel($"광선 {i}/{plane.Count}", (float)i / plane.Count)) return;

            Vector3 origin = originBase + u * plane[i].x + v * plane[i].y;

            if (!Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, _layerMask, QueryTriggerInteraction.Ignore))
            {
                missed++;
                continue;
            }

            if (_restrictToTarget && !IsPartOf(hit.transform, target.transform))
            {
                missed++;
                continue;
            }

            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope < _minSlope || slope > _maxSlope)
            {
                filtered++;
                continue;
            }

            GameObject prefab = _profile.PickRandomPrefab(rng);
            if (prefab == null) continue;

            EnvironmentEntry entry = _profile.GetEntryForPrefab(prefab);

            // 자세: 법선과 월드 위쪽 사이를 섞는다.
            Vector3 up = _normalBlend >= 1f
                ? hit.normal
                : Vector3.Slerp(Vector3.up, hit.normal, _normalBlend).normalized;

            if (up.sqrMagnitude < 1e-6f) up = hit.normal;

            Quaternion rotation = UprightRotation(up);

            if (entry.RandomYRotation)
            {
                rotation = Quaternion.AngleAxis((float)rng.NextDouble() * 360f, up) * rotation;
            }

            float scale = Mathf.Lerp(
                Mathf.Max(entry.MinScale, 0.01f),
                Mathf.Max(entry.MaxScale, 0.01f),
                (float)rng.NextDouble()
            ) * _globalScale;

            // 월드에서 완성한 뒤 편입한다. 대상 스케일이 섞여 들어오지 않는다.
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, target.scene);
            if (instance == null) continue;

            // 회전 -> 스케일 -> 위치 순으로 잡는다.
            instance.transform.rotation = rotation;
            instance.transform.localScale = Vector3.one * scale;
            instance.transform.position = hit.point + hit.normal * _surfaceOffset;
            instance.transform.SetParent(container.transform, true);

            Undo.RegisterCreatedObjectUndo(instance, "Surface Scatter");

            ApplyInstancing(instance, _profile.UsesGpuInstancing(entry));

            count++;
        }

        // 다 심었으니 통째로 편입시킨다.
        container.transform.SetParent(target.transform, true);

        _lastCount = count;
        _lastMissCount = missed;
        _lastFilteredCount = filtered;
        _lastTargetName = target.name;

        Debug.Log($"[SurfaceScatter] \"{target.name}\": {count}개 심음. (지점 {plane.Count}개, 빗나감 {missed}, 경사 제외 {filtered}, Seed: {seed})");
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

    #endregion

    #region Math

    Vector3 ResolveDirection()
    {
        switch (_direction)
        {
            case CastDirection.Down: return Vector3.down;
            case CastDirection.Up: return Vector3.up;
            case CastDirection.Left: return Vector3.left;
            case CastDirection.Right: return Vector3.right;
            case CastDirection.Forward: return Vector3.forward;
            case CastDirection.Back: return Vector3.back;
            default: return _customDirection;
        }
    }

    /// <summary>방향에 직교하는 두 축을 만든다.</summary>
    static void BuildBasis(Vector3 dir, out Vector3 u, out Vector3 v)
    {
        Vector3 reference = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.99f ? Vector3.right : Vector3.up;

        u = Vector3.Cross(dir, reference).normalized;
        v = Vector3.Cross(dir, u).normalized;
    }

    /// <summary>AABB 반경을 주어진 축에 투영한 길이.</summary>
    static float ProjectedExtent(Vector3 extents, Vector3 axis)
    {
        return Mathf.Abs(extents.x * axis.x) + Mathf.Abs(extents.y * axis.y) + Mathf.Abs(extents.z * axis.z);
    }

    /// <summary>주어진 up을 Y축으로 삼는 회전.</summary>
    static Quaternion UprightRotation(Vector3 up)
    {
        Vector3 forward = Vector3.Cross(up, Vector3.right);
        if (forward.sqrMagnitude < 1e-6f) forward = Vector3.Cross(up, Vector3.forward);

        return Quaternion.LookRotation(forward.normalized, up);
    }

    static bool IsPartOf(Transform hit, Transform target)
    {
        Transform current = hit;
        while (current != null)
        {
            if (current == target) return true;
            current = current.parent;
        }
        return false;
    }

    static bool TryGetWorldBounds(GameObject target, out Bounds bounds)
    {
        bounds = default;
        if (target == null) return false;

        bool any = false;

        foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null) continue;
            if (IsScatterChild(renderer.transform)) continue;   // 이전 배치물은 경계에서 뺀다

            if (!any) { bounds = renderer.bounds; any = true; }
            else bounds.Encapsulate(renderer.bounds);
        }

        if (any) return true;

        foreach (Collider collider in target.GetComponentsInChildren<Collider>(true))
        {
            if (collider == null) continue;
            if (IsScatterChild(collider.transform)) continue;

            if (!any) { bounds = collider.bounds; any = true; }
            else bounds.Encapsulate(collider.bounds);
        }

        return any;
    }

    static bool IsScatterChild(Transform t)
    {
        Transform current = t;
        while (current != null)
        {
            if (current.name.StartsWith(CONTAINER_PREFIX)) return true;
            current = current.parent;
        }
        return false;
    }

    #endregion

    #region Containers

    static int CountContainers(GameObject target)
    {
        if (target == null) return 0;

        int count = 0;
        foreach (Transform child in target.transform)
        {
            if (child.name.StartsWith(CONTAINER_PREFIX)) count++;
        }
        return count;
    }

    static void ClearContainers(GameObject target)
    {
        if (target == null) return;

        List<Transform> toRemove = new List<Transform>();
        foreach (Transform child in target.transform)
        {
            if (child.name.StartsWith(CONTAINER_PREFIX)) toRemove.Add(child);
        }

        foreach (Transform t in toRemove)
        {
            Undo.DestroyObjectImmediate(t.gameObject);
        }
    }

    #endregion
}
