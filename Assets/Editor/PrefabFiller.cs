using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 지정한 오브젝트의 부피를 프리팹으로 균일하게 채우는 유틸리티.
///
/// Environment Placer가 여섯 개의 "면"을 훑는다면 이쪽은 "속"을 채운다.
/// 배치가 난수(Poisson)가 아니라 일정 간격의 격자라서 결과가 항상 같고, 간격만 보면 밀도를 안다.
///
/// 격자는 대상의 로컬 공간에서 만든다. 대상이 회전해 있어도 격자가 함께 따라 돌고,
/// 간격은 월드 단위로 유지되도록 대상 스케일로 나눠 보정한다.
///
/// 결과물은 대상 아래 컨테이너 하나에 모인다. Clear로 통째로 지울 수 있고, Undo도 한 덩어리다.
/// </summary>
public class PrefabFillerWindow : EditorWindow
{
    const string CONTAINER_PREFIX = "_Fill";

    /// <summary>안전장치. 이보다 많이 나오면 경고하고 실행을 막는다.</summary>
    const int HARD_LIMIT = 20000;
    const int WARN_LIMIT = 2000;

    enum FillMode
    {
        Spacing,   // 간격을 정하면 개수가 정해진다
        Count,     // 개수를 정하면 간격이 정해진다
    }

    GameObject _prefab;

    FillMode _mode = FillMode.Spacing;
    float _spacing = 1f;
    Vector3Int _counts = new Vector3Int(4, 4, 4);

    float _padding = 0f;
    bool _insideColliderOnly = true;
    bool _followTargetRotation = false;

    float _minScale = 1f;
    float _maxScale = 1f;
    bool _randomYRotation = false;

    bool _useRandomSeed = false;
    int _seed = 42;

    Vector2 _scrollPos;
    bool _canceled;

    int _lastCount;
    string _lastTargetName;

    [MenuItem("Tools/Prefab Filler")]
    public static void ShowWindow()
    {
        PrefabFillerWindow window = GetWindow<PrefabFillerWindow>("Prefab Filler");
        window.minSize = new Vector2(340, 460);
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
        DrawPrefab();
        DrawGrid();
        DrawVariation();
        DrawActions();
        DrawLastResult();

        EditorGUILayout.EndScrollView();
    }

    #region UI

    void DrawHeader()
    {
        GUILayout.Space(5);
        GUILayout.Label("🧊 Prefab Filler", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "선택한 오브젝트의 부피를 프리팹으로 균일하게 채웁니다.\n" +
            "격자는 대상의 로컬 공간에 놓이므로 대상이 회전해 있어도 따라 돕니다.",
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
            EditorGUILayout.HelpBox("Hierarchy에서 채울 오브젝트를 선택해주세요.", MessageType.Warning);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("선택됨", selected, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();

            if (!TryGetLocalBounds(selected, out Bounds local))
            {
                EditorGUILayout.HelpBox("이 오브젝트에서 Renderer나 Collider를 찾지 못했습니다.", MessageType.Warning);
            }
            else
            {
                Vector3 lossy = Abs(selected.transform.lossyScale);
                Vector3 world = new Vector3(local.size.x * lossy.x, local.size.y * lossy.y, local.size.z * lossy.z);
                EditorGUILayout.LabelField($"   부피 (월드): {world.x:F2} × {world.y:F2} × {world.z:F2}", EditorStyles.miniLabel);

                int existing = CountContainers(selected);
                if (existing > 0)
                {
                    EditorGUILayout.LabelField($"   기존 컨테이너: {existing}개", EditorStyles.miniLabel);
                }
            }
        }

        GUILayout.Space(8);
    }

    void DrawPrefab()
    {
        GUILayout.Label("채울 프리팹", EditorStyles.miniBoldLabel);
        _prefab = (GameObject)EditorGUILayout.ObjectField("프리팹", _prefab, typeof(GameObject), false);
        GUILayout.Space(8);
    }

    void DrawGrid()
    {
        GUILayout.Label("격자", EditorStyles.miniBoldLabel);

        _mode = (FillMode)EditorGUILayout.EnumPopup(
            new GUIContent("기준", "간격을 정하면 개수가 따라오고, 개수를 정하면 간격이 따라온다."),
            _mode
        );

        if (_mode == FillMode.Spacing)
        {
            _spacing = EditorGUILayout.FloatField(
                new GUIContent("간격 (월드)", "세 축 모두 같은 간격을 쓴다. 대상 스케일과 무관하게 월드 단위로 유지된다."),
                _spacing
            );
            _spacing = Mathf.Max(_spacing, 0.01f);
        }
        else
        {
            _counts = EditorGUILayout.Vector3IntField(
                new GUIContent("개수 (X, Y, Z)", "축마다 몇 개를 놓을지. 1이면 그 축 가운데에 한 줄만 놓인다."),
                _counts
            );
            _counts = new Vector3Int(
                Mathf.Max(_counts.x, 0),
                Mathf.Max(_counts.y, 0),
                Mathf.Max(_counts.z, 0)
            );
        }

        _padding = EditorGUILayout.FloatField(
            new GUIContent("여백 (월드)", "경계에서 안쪽으로 띄울 거리. 벽면에 딱 붙는 걸 피할 때 쓴다."),
            _padding
        );

        _insideColliderOnly = EditorGUILayout.Toggle(
            new GUIContent("콜라이더 안쪽만", "대상 콜라이더 내부에 들어가는 지점만 남긴다. 상자가 아닌 모양을 채울 때 쓴다. 볼록하지 않은 MeshCollider는 판정이 불가능해 무시된다."),
            _insideColliderOnly
        );

        GUILayout.Space(8);
    }

    void DrawVariation()
    {
        GUILayout.Label("변형", EditorStyles.miniBoldLabel);

        _followTargetRotation = EditorGUILayout.Toggle(
            new GUIContent("대상 회전 따라가기", "켜면 대상이 기울어진 만큼 프리팹도 같이 기운다. 끄면 똑바로 선 자세를 유지한다."),
            _followTargetRotation
        );

        _randomYRotation = EditorGUILayout.Toggle(
            new GUIContent("Y축 랜덤 회전", "같은 프리팹이 반복되는 티를 줄인다."),
            _randomYRotation
        );

        EditorGUILayout.BeginHorizontal();
        _minScale = EditorGUILayout.FloatField("최소 스케일", _minScale);
        _maxScale = EditorGUILayout.FloatField("최대 스케일", _maxScale);
        EditorGUILayout.EndHorizontal();

        _minScale = Mathf.Max(_minScale, 0.01f);
        _maxScale = Mathf.Max(_maxScale, _minScale);

        _useRandomSeed = EditorGUILayout.Toggle(
            new GUIContent("매번 다른 시드", "끄면 같은 설정이 항상 같은 결과를 낸다."),
            _useRandomSeed
        );

        if (!_useRandomSeed)
        {
            EditorGUI.indentLevel++;
            _seed = EditorGUILayout.IntField("시드", _seed);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(8);
    }

    void DrawActions()
    {
        GameObject target = Selection.activeGameObject;

        bool hasTarget = target != null && TryGetLocalBounds(target, out _);
        bool hasPrefab = _prefab != null;

        // 실행 전에 몇 개가 나올지 먼저 보여준다. 눌러보고 나서 굳는 걸 막기 위해서다.
        int estimate = 0;
        if (hasTarget && TryGetLocalBounds(target, out Bounds local))
        {
            estimate = EstimateCount(target, local);
        }

        GUILayout.Label("실행", EditorStyles.miniBoldLabel);

        if (hasTarget && hasPrefab)
        {
            EditorGUILayout.LabelField($"   생성 예상: {estimate}개", EditorStyles.miniLabel);
        }

        if (estimate > HARD_LIMIT)
        {
            EditorGUILayout.HelpBox(
                $"{estimate}개는 너무 많습니다 (상한 {HARD_LIMIT}개).\n" +
                "간격을 늘리거나 개수를 줄여주세요.",
                MessageType.Error
            );
        }
        else if (estimate > WARN_LIMIT)
        {
            EditorGUILayout.HelpBox(
                $"{estimate}개는 생성에 시간이 걸립니다. 진행 중 취소할 수 있습니다.",
                MessageType.Warning
            );
        }

        bool canFill = hasTarget && hasPrefab && estimate > 0 && estimate <= HARD_LIMIT;

        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        EditorGUI.BeginDisabledGroup(!canFill);
        if (GUILayout.Button("🧊 Fill", GUILayout.Height(32)))
        {
            RunCancelable("Prefab Filler Fill", () => Fill(target));
        }
        EditorGUI.EndDisabledGroup();

        bool hasContainer = target != null && CountContainers(target) > 0;
        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
        EditorGUI.BeginDisabledGroup(!hasContainer);
        if (GUILayout.Button("🗑 Clear (선택된 오브젝트)", GUILayout.Height(26)))
        {
            RunCancelable("Prefab Filler Clear", () => ClearContainers(target));
        }
        EditorGUI.EndDisabledGroup();

        GUI.backgroundColor = Color.white;
        GUILayout.Space(5);
    }

    void DrawLastResult()
    {
        if (_lastCount > 0 && !string.IsNullOrEmpty(_lastTargetName))
        {
            EditorGUILayout.HelpBox($"마지막 작업: \"{_lastTargetName}\"에 {_lastCount}개 배치 완료", MessageType.None);
        }
    }

    #endregion

    #region Core

    /// <summary>
    /// Environment Placer와 같은 방식. 취소하면 그룹 전체를 되돌려 절반짜리 결과를 남기지 않는다.
    /// </summary>
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
            Debug.Log("[PrefabFiller] 취소되었습니다. 작업 전 상태로 되돌렸습니다.");
            return;
        }

        Undo.CollapseUndoOperations(group);
    }

    void Fill(GameObject target)
    {
        if (!TryGetLocalBounds(target, out Bounds local)) return;

        ClearContainers(target);

        List<Vector3> localPoints = BuildGrid(target, local);
        if (localPoints.Count == 0)
        {
            Debug.LogWarning("[PrefabFiller] 조건에 맞는 지점이 없습니다.");
            return;
        }

        // 컨테이너는 손대지 않는다. 배치물을 월드에서 완성한 뒤 편입시키는 방식이라
        // 부모 스케일을 역수로 상쇄할 필요가 없다.
        GameObject container = new GameObject($"{CONTAINER_PREFIX}_{_prefab.name}");
        container.transform.SetParent(target.transform, false);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;
        Undo.RegisterCreatedObjectUndo(container, "Prefab Filler Fill");

        Collider containment = _insideColliderOnly ? ContainmentCollider(target) : null;

        // Physics.autoSyncTransforms는 기본이 꺼짐이다. 에디터에서 대상을 막 움직이거나 스케일한
        // 직후면 콜라이더가 아직 물리 쪽에 반영되지 않아, ClosestPoint가 예전 위치로 답한다.
        // 그대로 두면 안쪽 지점이 바깥으로 판정돼 태반이 사라진다.
        if (containment != null) Physics.SyncTransforms();

        int seed = _useRandomSeed ? System.Environment.TickCount : _seed;
        System.Random rng = new System.Random(seed);

        Quaternion baseRotation = _followTargetRotation ? target.transform.rotation : Quaternion.identity;

        int count = 0;

        for (int i = 0; i < localPoints.Count; i++)
        {
            if ((i & 0x3F) == 0 &&
                PollCancel("Prefab Filler", $"배치 중 {i}/{localPoints.Count}", (float)i / localPoints.Count))
            {
                return;
            }

            Vector3 world = target.transform.TransformPoint(localPoints[i]);

            if (containment != null && !IsInside(containment, world)) continue;

            // 씬 루트에 먼저 만들어 월드 값을 그대로 준다. 루트에서는 localScale이 곧 월드 스케일이다.
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(_prefab, target.scene);
            if (instance == null) continue;

            Quaternion rotation = baseRotation;
            if (_randomYRotation)
            {
                rotation *= Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            }

            float scale = Mathf.Lerp(_minScale, _maxScale, (float)rng.NextDouble());

            instance.transform.SetPositionAndRotation(world, rotation);
            instance.transform.localScale = Vector3.one * scale;

            // 다 잡은 뒤 편입. 로컬 값 역산은 유니티가 한다.
            instance.transform.SetParent(container.transform, true);

            Undo.RegisterCreatedObjectUndo(instance, "Prefab Filler Fill");
            count++;
        }

        _lastCount = count;
        _lastTargetName = target.name;

        Debug.Log($"[PrefabFiller] \"{target.name}\": {count}개 배치 완료. (지점 {localPoints.Count}개, Seed: {seed})");
    }

    bool PollCancel(string title, string info, float progress)
    {
        if (_canceled) return true;

        if (EditorUtility.DisplayCancelableProgressBar(title, info, progress))
        {
            _canceled = true;
        }

        return _canceled;
    }

    #endregion

    #region Grid

    /// <summary>대상 로컬 공간의 격자 지점들.</summary>
    List<Vector3> BuildGrid(GameObject target, Bounds local)
    {
        List<Vector3> points = new List<Vector3>();

        Vector3 lossy = Abs(target.transform.lossyScale);

        // 간격과 여백은 월드 단위로 받는다. 로컬로 환산해야 대상 스케일이 달라도 같은 간격이 나온다.
        Vector3 pad = new Vector3(
            _padding / SafeAxis(lossy.x),
            _padding / SafeAxis(lossy.y),
            _padding / SafeAxis(lossy.z)
        );

        Vector3 min = local.min + pad;
        Vector3 max = local.max - pad;

        AxisPlan x = PlanAxis(min.x, max.x, _spacing / SafeAxis(lossy.x), _counts.x);
        AxisPlan y = PlanAxis(min.y, max.y, _spacing / SafeAxis(lossy.y), _counts.y);
        AxisPlan z = PlanAxis(min.z, max.z, _spacing / SafeAxis(lossy.z), _counts.z);

        for (int ix = 0; ix < x.count; ix++)
        {
            for (int iy = 0; iy < y.count; iy++)
            {
                for (int iz = 0; iz < z.count; iz++)
                {
                    points.Add(new Vector3(
                        x.start + x.step * ix,
                        y.start + y.step * iy,
                        z.start + z.step * iz
                    ));
                }
            }
        }

        return points;
    }

    struct AxisPlan
    {
        public int count;
        public float start;
        public float step;
    }

    /// <summary>
    /// 한 축의 지점 개수와 시작점을 정한다.
    ///
    /// 간격 기준이면 들어가는 만큼 넣고 남는 자리를 양쪽에 반씩 나눠 격자를 가운데에 둔다.
    /// 개수 기준이면 양 끝에 딱 맞춰 균등 분할한다.
    /// </summary>
    AxisPlan PlanAxis(float min, float max, float localSpacing, int requestedCount)
    {
        AxisPlan plan = new AxisPlan { count = 0, start = (min + max) * 0.5f, step = 0f };

        float span = max - min;
        if (span < 0f) return plan;

        if (_mode == FillMode.Count)
        {
            if (requestedCount <= 0) return plan;

            plan.count = requestedCount;

            if (requestedCount == 1)
            {
                plan.start = (min + max) * 0.5f;
                plan.step = 0f;
            }
            else
            {
                plan.step = span / (requestedCount - 1);
                plan.start = min;
            }

            return plan;
        }

        if (localSpacing <= 0f) return plan;

        int fit = Mathf.FloorToInt(span / localSpacing) + 1;
        if (fit <= 0) return plan;

        plan.count = fit;
        plan.step = localSpacing;

        // 남는 자리를 반씩 나눠 격자를 가운데로
        float used = localSpacing * (fit - 1);
        plan.start = min + (span - used) * 0.5f;

        return plan;
    }

    int EstimateCount(GameObject target, Bounds local)
    {
        Vector3 lossy = Abs(target.transform.lossyScale);

        Vector3 pad = new Vector3(
            _padding / SafeAxis(lossy.x),
            _padding / SafeAxis(lossy.y),
            _padding / SafeAxis(lossy.z)
        );

        Vector3 min = local.min + pad;
        Vector3 max = local.max - pad;

        AxisPlan x = PlanAxis(min.x, max.x, _spacing / SafeAxis(lossy.x), _counts.x);
        AxisPlan y = PlanAxis(min.y, max.y, _spacing / SafeAxis(lossy.y), _counts.y);
        AxisPlan z = PlanAxis(min.z, max.z, _spacing / SafeAxis(lossy.z), _counts.z);

        // 콜라이더 안쪽만 남기는 경우 실제로는 이보다 적게 나온다.
        return x.count * y.count * z.count;
    }

    #endregion

    #region Bounds & Containment

    /// <summary>
    /// 대상의 로컬 공간 경계. 격자를 로컬에서 만들어야 대상 회전을 따라가기 때문에 로컬로 받는다.
    /// </summary>
    static bool TryGetLocalBounds(GameObject target, out Bounds bounds)
    {
        bounds = default;
        if (target == null) return false;

        if (target.TryGetComponent(out Renderer renderer))
        {
            bounds = renderer.localBounds;
            return true;
        }

        if (target.TryGetComponent(out BoxCollider box))
        {
            bounds = new Bounds(box.center, box.size);
            return true;
        }

        if (target.TryGetComponent(out SphereCollider sphere))
        {
            bounds = new Bounds(sphere.center, Vector3.one * (sphere.radius * 2f));
            return true;
        }

        // 자식까지 훑을 때는 월드 경계밖에 못 얻으므로 대상 로컬로 되돌린다.
        Renderer childRenderer = target.GetComponentInChildren<Renderer>();
        if (childRenderer != null)
        {
            Bounds world = childRenderer.bounds;
            Vector3 localCenter = target.transform.InverseTransformPoint(world.center);
            Vector3 lossy = Abs(target.transform.lossyScale);
            Vector3 localSize = new Vector3(
                world.size.x / SafeAxis(lossy.x),
                world.size.y / SafeAxis(lossy.y),
                world.size.z / SafeAxis(lossy.z)
            );
            bounds = new Bounds(localCenter, localSize);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 내부 판정에 쓸 콜라이더. 볼록하지 않은 MeshCollider는 ClosestPoint를 지원하지 않아 제외한다.
    /// </summary>
    static Collider ContainmentCollider(GameObject target)
    {
        if (!target.TryGetComponent(out Collider collider)) return null;

        if (collider is MeshCollider mesh && !mesh.convex)
        {
            Debug.LogWarning("[PrefabFiller] 볼록하지 않은 MeshCollider는 내부 판정을 할 수 없어 경계 전체를 채웁니다.");
            return null;
        }

        return collider;
    }

    static bool IsInside(Collider collider, Vector3 worldPoint)
    {
        return (collider.ClosestPoint(worldPoint) - worldPoint).sqrMagnitude < 1e-8f;
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

    #region Utility

    static Vector3 Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    static float SafeAxis(float v)
    {
        return Mathf.Max(v, 1e-4f);
    }

    #endregion
}
