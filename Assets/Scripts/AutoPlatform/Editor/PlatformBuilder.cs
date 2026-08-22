using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// <see cref="LevelRoute"/>의 동선을 받아 발판을 짓는다.
///
/// 프리팹을 <b>늘리지 않고 타일링</b>한다. Deck 계열의 장식은 Prefab Filler가 로컬 공간 격자로 채워 넣은
/// <c>_Fill_*</c> 컨테이너라, 루트를 X로 늘리면 석조 장식이 함께 늘어나 뭉개진다.
/// 그래서 폭은 모듈 개수로만 만든다.
///
/// 모듈 크기는 하드코딩하지 않고 프리팹의 콜라이더에서 잰다. 아트가 바뀌면 결과도 따라 바뀐다.
/// </summary>
public static class PlatformBuilder
{
    /// <summary><see cref="LevelRoute.PadModule"/>과 같은 순서. None은 프리팹이 없다.</summary>
    static readonly string[] ModuleGuids =
    {
        "871aede768aa4754e94c585030a98742", // Deck
        "7fa25047730505f4999e62f14bff1df4", // Deck(5 slabs)
        "5426ad51f75b168498c391f46c9c03e2", // Deck(3 slabs)
        "7fb84d039373eb44aa497c1e01c5b36b", // ThickerDeck
        null,                               // None
    };

    /// <summary>씬의 물리 지오메트리가 모이는 루트. 기존 배경 씬과 같은 이름을 쓴다.</summary>
    const string PHYSICS_ROOT = "Physics";

    const string CONTAINER_PREFIX = "_Route_";

    /// <summary>발판 하나로 묶어 볼 단차. 이보다 크면 따로 짓는다.</summary>
    const float RUN_STEP_TOLERANCE = 0.05f;

    /// <summary>프리팹에서 한 번만 재고 재사용하는 모듈 치수.</summary>
    public struct Module
    {
        public GameObject prefab;

        /// <summary>루트 원점에서 윗면까지의 높이. 발이 닿는 면을 노드에 맞추는 데 쓴다.</summary>
        public float topOffset;

        /// <summary>Z 중심 어긋남. 발판을 Z=0에 맞추기 위해 상쇄한다.</summary>
        public float centerZ;

        /// <summary>폭과 아트의 세로 부피. 루트가 판정에 그대로 쓴다.</summary>
        public LevelRoute.PadSpec spec;

        /// <summary>월드 기준 폭. 타일 간격이 된다.</summary>
        public float width => spec.width;

        public bool Valid => prefab != null && spec.width > 0.01f;
    }

    /// <summary>Prefab Filler가 만든 장식 컨테이너 안에 있는가.</summary>
    static bool IsDecoration(Transform collider, Transform root)
    {
        for (Transform t = collider; t != null && t != root; t = t.parent)
            if (t.name.StartsWith("_Fill")) return true;

        return false;
    }

    /// <summary>
    /// 프리팹에서 모듈 치수를 재어 루트에 채운다.
    ///
    /// <see cref="Build"/>보다 먼저 불러야 하는 경우가 있다 — 동선을 <b>만드는 동안</b> 이미
    /// 발판 폭과 아트 두께가 필요하기 때문이다. 갓 만든 루트는 이 표가 비어 있어서,
    /// 채우지 않고 배치하면 모든 발판을 폭 0으로 보고 겹침 검사가 통째로 무력해진다.
    /// </summary>
    public static Module[] MeasureInto(LevelRoute route)
    {
        Module[] modules = new Module[ModuleGuids.Length];
        for (int i = 0; i < ModuleGuids.Length; i++)
            modules[i] = ModuleGuids[i] == null ? default : Measure(ModuleGuids[i]);

        if (route == null) return modules;

        Undo.RecordObject(route, "Measure Pads");

        if (route.pads == null || route.pads.Length != ModuleGuids.Length)
            route.pads = new LevelRoute.PadSpec[ModuleGuids.Length];

        for (int i = 0; i < modules.Length; i++)
            if (modules[i].Valid) route.pads[i] = modules[i].spec;

        return modules;
    }

    public static void Build(LevelRoute route)
    {
        if (route == null) return;

        // 판정과 배치가 같은 치수를 보게 한다. 아트를 갈아끼우면 여기서 따라 바뀐다.
        Module[] modules = MeasureInto(route);

        if (!modules[(int)LevelRoute.PadModule.Deck].Valid)
        {
            Debug.LogError("[PlatformBuilder] Deck 프리팹을 찾지 못했다. GUID가 바뀌었는지 확인할 것.");
            return;
        }

        Transform container = PrepareContainer(route);

        int built = 0;
        foreach (Run run in Runs(route))
        {
            Module module = modules[(int)run.module];
            if (!module.Valid) continue;

            built += Place(container, run, module);
        }

        int overlaps = AuditOverlaps(container);

        Debug.Log($"[PlatformBuilder] '{route.name}' — 발판 {built}장 배치, 관통 {overlaps}쌍. 컨테이너: {container.name}");
        EditorUtility.SetDirty(route.gameObject);
    }

    /// <summary>이 루트가 지어 놓은 발판들이 담긴 컨테이너. 아직 짓지 않았으면 null.</summary>
    public static Transform ContainerOf(LevelRoute route)
    {
        if (route == null) return null;

        Transform physics = FindPhysicsRoot(route);
        return physics == null ? null : physics.Find(CONTAINER_PREFIX + route.name);
    }

    public static void Clear(LevelRoute route)
    {
        if (route == null) return;

        Transform physics = FindPhysicsRoot(route);
        if (physics == null) return;

        Transform existing = physics.Find(CONTAINER_PREFIX + route.name);
        if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);
    }

    #region 발판 묶기

    /// <summary>이어서 지을 한 덩어리. Ground로 이어진 노드들은 하나의 발판이 된다.</summary>
    struct Run
    {
        public float minX;
        public float maxX;
        public float y;
        public LevelRoute.PadModule module;
    }

    static IEnumerable<Run> Runs(LevelRoute route)
    {
        foreach (Run run in RunsOf(route, route.main)) yield return run;

        foreach (LevelRoute.Branch branch in route.branches)
            foreach (Run run in RunsOf(route, branch.nodes)) yield return run;

        // 장애물도 지어야 벽 노릇을 한다. 길이 아닐 뿐 물건은 물건이다.
        foreach (Run run in RunsOf(route, route.obstacles)) yield return run;
    }

    static IEnumerable<Run> RunsOf(LevelRoute route, List<LevelRoute.Node> nodes)
    {
        Run? open = null;

        for (int i = 0; i < nodes.Count; i++)
        {
            LevelRoute.Node node = nodes[i];
            if (node.module == LevelRoute.PadModule.None)
            {
                if (open.HasValue) { yield return open.Value; open = null; }
                continue;
            }

            Vector3 world = route.WorldOf(node);
            float half = route.HalfWidthOf(node);

            bool joins = open.HasValue
                && node.entry == MoveKind.Ground
                && Mathf.Abs(open.Value.y - world.y) <= RUN_STEP_TOLERANCE
                && open.Value.module == node.module;

            if (joins)
            {
                Run run = open.Value;
                run.minX = Mathf.Min(run.minX, world.x - half);
                run.maxX = Mathf.Max(run.maxX, world.x + half);
                open = run;
                continue;
            }

            if (open.HasValue) yield return open.Value;

            open = new Run
            {
                minX = world.x - half,
                maxX = world.x + half,
                y = world.y,
                module = node.module,
            };
        }

        if (open.HasValue) yield return open.Value;
    }

    #endregion

    #region 배치

    static int Place(Transform container, Run run, Module module)
    {
        float span = run.maxX - run.minX;

        // 타일은 절대 겹치지 않는다. 요구한 폭을 모듈 개수로 올림하고, 딱 맞물려 이어 붙인다.
        // 겹쳐 놓으면 이 애셋에서는 기둥이 기둥을 뚫고 지나간다.
        int count = Mathf.Max(1, Mathf.CeilToInt(span / module.width - 0.001f));

        float center = (run.minX + run.maxX) * 0.5f;
        float first = center - (count - 1) * module.width * 0.5f;

        for (int i = 0; i < count; i++)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(module.prefab, container);
            Undo.RegisterCreatedObjectUndo(instance, "Build Platforms");

            instance.transform.SetPositionAndRotation(
                new Vector3(
                    first + i * module.width,
                    run.y - module.topOffset,
                    -module.centerZ),
                Quaternion.identity);
        }

        return count;
    }

    #endregion

    #region 검사

    /// <summary>
    /// 지어 놓은 것끼리 실제로 관통하는지 렌더러 부피로 확인한다.
    ///
    /// 노드 단계의 <see cref="LevelRoute.ClearanceProblems"/>는 모듈 치수를 가정한 예측이고,
    /// 이쪽은 지어진 결과를 그대로 잰다. 둘이 어긋나면 믿을 것은 이쪽이다.
    /// </summary>
    static int AuditOverlaps(Transform container)
    {
        List<Bounds> volumes = new List<Bounds>();
        List<Transform> owners = new List<Transform>();

        foreach (Transform tile in container)
        {
            bool any = false;
            Bounds bounds = default;

            foreach (Renderer renderer in tile.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled) continue;

                if (!any) { bounds = renderer.bounds; any = true; }
                else bounds.Encapsulate(renderer.bounds);
            }

            if (!any) continue;

            // 맞물려 이어 붙인 타일은 경계면이 정확히 닿는다. 닿는 것과 파고드는 것을 가른다.
            bounds.Expand(-0.002f);

            volumes.Add(bounds);
            owners.Add(tile);
        }

        int found = 0;
        for (int i = 0; i < volumes.Count; i++)
        {
            for (int j = i + 1; j < volumes.Count; j++)
            {
                if (!volumes[i].Intersects(volumes[j])) continue;

                found++;
                Debug.LogError(
                    $"[PlatformBuilder] 아트가 관통한다: '{owners[i].name}' @{owners[i].position} " +
                    $"× '{owners[j].name}' @{owners[j].position}", owners[i].gameObject);
            }
        }

        return found;
    }

    #endregion

    #region 치수 재기 · 컨테이너

    /// <summary>프리팹을 한 번 띄워 콜라이더로 치수를 재고 곧바로 버린다.</summary>
    static Module Measure(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        GameObject prefab = string.IsNullOrEmpty(path)
            ? null
            : AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null) return default;

        GameObject probe = Object.Instantiate(prefab);
        probe.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        probe.hideFlags = HideFlags.HideAndDontSave;

        Module module = default;
        bool any = false;
        Bounds bounds = default;

        foreach (Collider collider in probe.GetComponentsInChildren<Collider>())
        {
            // 장식은 빼고 잰다. _Fill_*은 Prefab Filler가 채워 넣은 석조 장식이고, 데크 아랫면에 매달려
            // 폭도 조금 넘친다 — 그걸 모듈 치수로 삼으면 판정이 실제 발판과 어긋난다.
            if (IsDecoration(collider.transform, probe.transform)) continue;

            if (!any) { bounds = collider.bounds; any = true; }
            else bounds.Encapsulate(collider.bounds);
        }

        if (any)
        {
            module.prefab = prefab;
            module.spec.width = bounds.size.x;
            module.topOffset = bounds.max.y;
            module.centerZ = bounds.center.z;

            // 아트의 부피는 콜라이더와 전혀 다르다. 이 애셋들은 딛는 면 아래로 열주가 통째로 매달려 있어,
            // 딛는 면만 보고 배치하면 서로 뚫고 지나간다. 관통 검사는 렌더러로 해야 한다.
            bool drawn = false;
            Bounds art = default;

            foreach (Renderer renderer in probe.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled) continue;

                if (!drawn) { art = renderer.bounds; drawn = true; }
                else art.Encapsulate(renderer.bounds);
            }

            if (drawn)
            {
                module.spec.drop = bounds.max.y - art.min.y;
                module.spec.rise = art.max.y - bounds.max.y;

                // 장식이 딛는 면보다 몇 cm 넓다. 콜라이더 폭으로 이어 붙이면 그만큼 아트가 물린다 —
                // 이어 붙이는 간격은 넓은 쪽을 따른다. 남는 콜라이더 틈은 발 반지름보다 훨씬 좁아 빠지지 않는다.
                module.spec.width = Mathf.Max(module.spec.width, art.size.x);
            }
        }
        else
        {
            Debug.LogWarning($"[PlatformBuilder] '{prefab.name}'에서 발판으로 쓸 콜라이더를 찾지 못했다.");
        }

        Object.DestroyImmediate(probe);
        return module;
    }

    /// <summary>
    /// 루트가 속한 씬에서만 찾는다. Character 씬이 함께 열려 있을 때 남의 Physics 루트에
    /// 발판을 쌓지 않도록.
    /// </summary>
    static Transform FindPhysicsRoot(LevelRoute route)
    {
        foreach (GameObject root in route.gameObject.scene.GetRootGameObjects())
            if (root.name == PHYSICS_ROOT) return root.transform;

        return null;
    }

    static Transform PrepareContainer(LevelRoute route)
    {
        Transform physics = FindPhysicsRoot(route);
        if (physics == null)
        {
            GameObject created = new GameObject(PHYSICS_ROOT);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(created, route.gameObject.scene);
            Undo.RegisterCreatedObjectUndo(created, "Build Platforms");
            physics = created.transform;
        }

        string name = CONTAINER_PREFIX + route.name;
        Transform existing = physics.Find(name);
        if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);

        GameObject container = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(container, "Build Platforms");

        // 월드 기준으로 먼저 붙인 뒤 자리를 잡는다. 부모의 스케일을 되돌리지 않는다.
        container.transform.SetParent(physics, true);
        container.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        container.transform.localScale = Vector3.one;

        return container.transform;
    }

    #endregion
}
