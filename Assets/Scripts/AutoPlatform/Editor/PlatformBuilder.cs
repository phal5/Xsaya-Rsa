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

        public string guid;

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
    public static Module[] MeasureInto(LevelRoute route, string folder)
    {
        List<Module> found = new List<Module>();

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // 하위 폴더는 뒤지지 않는다. 지정한 폴더에 발판만 모아 두는 것이 규약이다.
            string directory = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            if (directory != folder.TrimEnd('/')) continue;

            Module module = Measure(guid);
            if (module.Valid) found.Add(module);
        }

        // 좁은 것부터 늘어세운다. 역할을 이름이 아니라 순위로 고르기 위한 것이다.
        found.Sort((a, b) => a.spec.width.CompareTo(b.spec.width));

        Module[] modules = found.ToArray();
        if (route == null) return modules;

        Undo.RecordObject(route, "Measure Pads");

        route.pads = new LevelRoute.PadSpec[modules.Length];
        for (int i = 0; i < modules.Length; i++) route.pads[i] = modules[i].spec;

        return modules;
    }

    public static void Build(LevelRoute route, string folder)
    {
        if (route == null) return;

        // 판정과 배치가 같은 치수를 보게 한다. 아트를 갈아끼우면 여기서 따라 바뀐다.
        Module[] modules = MeasureInto(route, folder);

        if (modules.Length == 0)
        {
            Debug.LogError($"[PlatformBuilder] '{folder}'에서 쓸 수 있는 발판 프리팹을 찾지 못했다. "
                + "콜라이더가 있는 프리팹이 하나는 있어야 한다.");
            return;
        }

        Transform container = PrepareContainer(route);

        int built = 0;
        foreach (Run run in Runs(route))
        {
            if (run.module < 0 || run.module >= modules.Length) continue;

            Module module = modules[run.module];
            if (!module.Valid) continue;

            built += Place(container, run, module);
        }

        int overlaps = AuditOverlaps(container);

        Debug.Log($"[PlatformBuilder] '{route.name}' — 발판 {built}장 배치, 관통 {overlaps}쌍. 컨테이너: {container.name}");
        EditorUtility.SetDirty(route.gameObject);
    }

    /// <summary>가장 최근에 지은 컨테이너. 아직 짓지 않았으면 null.</summary>
    public static Transform ContainerOf(LevelRoute route)
    {
        List<Transform> all = ContainersOf(route);
        return all.Count == 0 ? null : all[all.Count - 1];
    }

    /// <summary>
    /// 이 루트가 지어 놓은 컨테이너 전부.
    ///
    /// 적어 둔 목록이 먼저다. 비어 있으면 부모 밑에서 이름으로 한 번 주워 담는다 —
    /// 목록이 생기기 전에 지어 놓은 것을 잃지 않기 위한 것이다.
    /// </summary>
    public static List<Transform> ContainersOf(LevelRoute route)
    {
        if (route == null) return new List<Transform>();

        route.builtContainers.RemoveAll(c => c == null);
        if (route.builtContainers.Count > 0) return new List<Transform>(route.builtContainers);

        if (route.buildRoot == null) return new List<Transform>();

        foreach (Transform child in route.buildRoot)
            if (child.name.StartsWith(CONTAINER_PREFIX)) route.builtContainers.Add(child);

        return new List<Transform>(route.builtContainers);
    }

    /// <summary>지어 놓은 것을 전부 걷어낸다. 손으로 고친 것도 함께 사라지므로 부를 때 각오할 것.</summary>
    public static void Clear(LevelRoute route)
    {
        List<Transform> containers = ContainersOf(route);

        Undo.RecordObject(route, "Clear Platforms");
        route.builtContainers.Clear();

        foreach (Transform container in containers)
            if (container != null) Undo.DestroyObjectImmediate(container.gameObject);

        Debug.Log($"[PlatformBuilder] 컨테이너 {containers.Count}개를 걷어냈다.", route);
    }

    static string Prefix(LevelRoute route) => CONTAINER_PREFIX + route.name;

    #region 발판 묶기

    /// <summary>이어서 지을 한 덩어리. Ground로 이어진 노드들은 하나의 발판이 된다.</summary>
    struct Run
    {
        public float minX;
        public float maxX;
        public float y;
        public int module;
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
            if (!route.SpecOf(node).Valid)
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

        // 이번에 지은 것뿐 아니라 형제 컨테이너의 발판까지 함께 본다 —
        // 덧붙여 짓는 이상, 새 발판이 예전 발판을 뚫는 것도 관통이다.
        List<Transform> tiles = new List<Transform>();
        foreach (Transform sibling in container.parent)
            foreach (Transform tile in sibling) tiles.Add(tile);

        foreach (Transform tile in tiles)
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
            module.guid = guid;
            module.spec.guid = guid;
            module.spec.name = prefab.name;
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
    /// 발판을 지어 넣을 부모를 정한다.
    ///
    /// 루트에 적어 둔 참조가 먼저다. 없으면 씬을 뒤져 이름이 맞는 것을 찾고, 그것도 없으면 만든다.
    /// 어느 쪽이든 찾은 것을 루트에 적어 두므로, 그 뒤로는 이름을 바꾸든 다른 것 밑으로 옮기든 따라간다.
    /// </summary>
    static Transform ResolveBuildRoot(LevelRoute route)
    {
        if (route.buildRoot != null) return route.buildRoot;

        Undo.RecordObject(route, "Resolve Build Root");

        // 씬 루트가 아니어도 찾는다 — 정리하다 다른 것 밑으로 들어가 있는 경우가 흔하다.
        foreach (GameObject root in route.gameObject.scene.GetRootGameObjects())
        {
            Transform found = root.name == PHYSICS_ROOT
                ? root.transform
                : FindByName(root.transform, PHYSICS_ROOT);

            if (found == null) continue;

            route.buildRoot = found;
            return found;
        }

        GameObject created = new GameObject(PHYSICS_ROOT);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(created, route.gameObject.scene);
        Undo.RegisterCreatedObjectUndo(created, "Build Platforms");

        route.buildRoot = created.transform;
        return created.transform;
    }

    static Transform FindByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;

            Transform deeper = FindByName(child, name);
            if (deeper != null) return deeper;
        }

        return null;
    }

    static Transform PrepareContainer(LevelRoute route)
    {
        Transform physics = ResolveBuildRoot(route);

        // 이미 있는 것은 건드리지 않는다. 지난번에 지은 발판에는 손으로 고친 것이 섞여 있을 수 있고,
        // 그것을 말없이 지우면 잃은 줄도 모른 채 잃는다. 치우려면 [지우기]를 따로 눌러야 한다.
        int serial = ContainersOf(route).Count + 1;
        string name = $"{Prefix(route)} {serial}";

        GameObject container = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(container, "Build Platforms");

        // 월드 기준으로 먼저 붙인 뒤 자리를 잡는다. 부모의 스케일을 되돌리지 않는다.
        container.transform.SetParent(physics, true);
        container.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        container.transform.localScale = Vector3.one;

        Undo.RecordObject(route, "Build Platforms");
        route.builtContainers.Add(container.transform);

        return container.transform;
    }

    #endregion
}
