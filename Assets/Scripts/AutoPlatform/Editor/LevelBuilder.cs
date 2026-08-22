using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 버튼 하나로 레벨 하나를 만든다.
///
/// 여태 흩어져 있던 순서 — 치수 재기 · 이동 성능 읽기 · 씨앗 고르기 · 동선 만들기 · 발판 짓기 · 검사 —
/// 를 한 자리에 모은 것이다. 순서를 사람이 기억해야 하면 반드시 어느 하나를 빠뜨리고,
/// 빠뜨린 것이 무엇인지는 결과가 이상해진 뒤에야 알게 된다.
/// </summary>
public static class LevelBuilder
{
    /// <summary>주인공 프리팹. 씬에 캐릭터가 없을 때 이동 성능을 여기서 읽는다.</summary>
    const string PROTAGONIST_GUID = "8544061f06e2dc448a94c2209b9dd634";

    const string ROUTE_NAME = "Level Route";

    /// <summary>
    /// 진행 창을 띄울지. 사람이 버튼을 눌렀을 때만 참이다 —
    /// 스크립트로 돌릴 때 모달을 띄우면 그쪽이 멈춰 버린다.
    /// </summary>
    public static bool ShowProgress = true;

    /// <summary>지은 결과를 한눈에 보기 위한 것. 하나라도 0이 아니면 손볼 곳이 있다.</summary>
    public struct Report
    {
        public bool ran;
        public int seed;
        public string mix;

        public int nodes;
        public int branches;
        public int tiles;

        public float width;
        public float height;

        public int unreachable;
        public int mustRelease;
        public int cramped;
        public int intersecting;
        public int outside;

        public bool Clean => unreachable == 0 && mustRelease == 0 && cramped == 0
            && intersecting == 0 && outside == 0;

        public float Slope => width <= 0.01f ? 0f : height / width;
    }

    /// <summary>
    /// 조건을 만족하는 레벨 하나를 만든다.
    ///
    /// 씨앗이 뽑기라 무엇이 나올지 정해지지 않으므로, 요구한 구간이 다 들어간 씨앗을 찾을 때까지
    /// 훑는다 — 사람이 seed를 1씩 올려가며 눈으로 고르던 일이다.
    /// </summary>
    public static Report Run(LevelBounds bounds)
    {
        Report report = default;

        if (bounds == null || !bounds.Ready)
        {
            Debug.LogError("[LevelBuilder] 시점 · 종점 · 영역 트리거가 모두 있어야 한다.", bounds);
            return report;
        }

        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[LevelBuilder] 플레이 모드에서는 짓지 않는다. 종료하면 전부 사라진다.");
            return report;
        }

        if (!AssetDatabase.IsValidFolder(bounds.moduleFolder))
        {
            Debug.LogError($"[LevelBuilder] 발판 폴더 '{bounds.moduleFolder}'를 찾을 수 없다.", bounds);
            return report;
        }

        LevelRoute route = FindOrCreateRoute(bounds);
        if (!PullProfile(route)) return report;

        report.seed = ChooseSeed(route, bounds, out report.mix);
        bounds.seed = report.seed;

        RouteFromBounds.Generate(route, bounds);
        PlatformBuilder.Build(route, bounds.moduleFolder);
        Physics.SyncTransforms();

        report = Verify(route, bounds, report);
        report.ran = true;

        Debug.Log(
            $"[LevelBuilder] 씨앗 {report.seed} · 노드 {report.nodes} · 곁길 {report.branches} · 타일 {report.tiles} · "
            + $"{report.width:0.0} × {report.height:0.0} m (기울기 {report.Slope:0.00}) · 구간 {report.mix}",
            route);

        if (!report.Clean)
            Debug.LogWarning(
                $"[LevelBuilder] 손볼 곳 — 불가 {report.unreachable} · 키를 놓아야 함 {report.mustRelease} · "
                + $"겹침 {report.cramped} · 관통 {report.intersecting} · 영역 이탈 {report.outside}", route);

        Selection.activeGameObject = route.gameObject;
        return report;
    }

    #region 준비

    static LevelRoute FindOrCreateRoute(LevelBounds bounds)
    {
        // 저작용 오브젝트는 꺼 두는 것이 정상이다. 꺼진 것도 찾아야 한다.
        LevelRoute route = Object.FindFirstObjectByType<LevelRoute>(FindObjectsInactive.Include);
        if (route != null) return route;

        GameObject created = new GameObject(ROUTE_NAME);
        Undo.RegisterCreatedObjectUndo(created, "Build Level");

        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(created, bounds.gameObject.scene);
        return created.AddComponent<LevelRoute>();
    }

    static bool PullProfile(LevelRoute route)
    {
        CharacterManager character = route.sampleCharacter != null
            ? route.sampleCharacter
            : Object.FindFirstObjectByType<CharacterManager>(FindObjectsInactive.Include);

        if (character == null)
        {
            string path = AssetDatabase.GUIDToAssetPath(PROTAGONIST_GUID);
            GameObject prefab = string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null) character = prefab.GetComponent<CharacterManager>();
        }

        if (character == null)
        {
            Debug.LogError("[LevelBuilder] CharacterManager를 찾지 못했다. 이동 성능을 읽을 수 없다.");
            return false;
        }

        Undo.RecordObject(route, "Build Level");
        route.profile.ReadFrom(character);
        MotionEnvelope.ClearCache();
        return true;
    }

    #endregion

    #region 씨앗 고르기

    /// <summary>구간 이름표에서 무엇이 뽑혔는지 읽는다.</summary>
    static readonly string[] Tags = { "대시", "턱", "고원", "낙하", "탑", "단" };

    static List<string> MixOf(LevelRoute route)
    {
        List<string> found = new List<string>();

        foreach (LevelRoute.Node node in route.main)
            foreach (string tag in Tags)
                if (node.label.StartsWith(tag) && !found.Contains(tag)) found.Add(tag);

        return found;
    }

    static bool Satisfies(LevelBounds bounds, List<string> mix)
    {
        if (bounds.requireDash && !mix.Contains("대시")) return false;
        if (bounds.requireLedge && !mix.Contains("턱")) return false;
        if (bounds.requirePlateau && !mix.Contains("고원")) return false;

        return true;
    }

    /// <summary>요구한 구간이 다 든 씨앗 중 가장 다양한 것. 못 찾으면 원래 씨앗을 쓴다.</summary>
    static int ChooseSeed(LevelRoute route, LevelBounds bounds, out string mix)
    {
        int original = bounds.seed;

        if (bounds.seedSearch <= 0)
        {
            RouteFromBounds.Generate(route, bounds);
            mix = string.Join(" · ", MixOf(route));
            return original;
        }

        int best = -1, richest = -1;
        string bestMix = "";

        // 버려질 후보들이 뱉는 경고는 묻어 둔다. 채택된 결과의 경고만 남아야 눈에 띈다.
        RouteFromBounds.Quiet = true;

        // 한 후보를 푸는 데 드는 비용이 층 수와 함께 커진다. 표시도 취소도 없이 돌리면
        // 에디터가 통째로 멎은 것처럼 보이므로, 진행을 보이고 언제든 그만둘 수 있게 한다.
        try
        {
            for (int offset = 0; offset < bounds.seedSearch; offset++)
            {
                if (ShowProgress && EditorUtility.DisplayCancelableProgressBar(
                    "레벨 만들기",
                    $"씨앗 {original + offset} 살펴보는 중 ({offset + 1}/{bounds.seedSearch})",
                    (offset + 1) / (float)bounds.seedSearch))
                {
                    Debug.Log($"[LevelBuilder] 씨앗 탐색을 {offset}개에서 멈췄다.", bounds);
                    break;
                }

                bounds.seed = original + offset;
                RouteFromBounds.Generate(route, bounds);

                List<string> got = MixOf(route);
                if (!Satisfies(bounds, got) || got.Count <= richest) continue;

                richest = got.Count;
                best = bounds.seed;
                bestMix = string.Join(" · ", got);

                // 요구한 것을 다 담았고 더 담을 것도 없으면 거기서 그만둔다.
                if (richest >= Tags.Length) break;
            }
        }
        finally
        {
            if (ShowProgress) EditorUtility.ClearProgressBar();
            RouteFromBounds.Quiet = false;
            bounds.seed = original;
        }

        if (best < 0)
        {
            Debug.LogWarning(
                $"[LevelBuilder] 씨앗 {original}부터 {bounds.seedSearch}개를 훑었지만 요구한 구간을 다 담은 것이 없다. "
                + "영역을 넓히거나 요구를 줄일 것.", bounds);

            RouteFromBounds.Generate(route, bounds);
            mix = string.Join(" · ", MixOf(route));
            return original;
        }

        mix = bestMix;
        return best;
    }

    #endregion

    #region 검사

    /// <summary>
    /// 지어진 결과를 실제 물리로 확인한다.
    ///
    /// 예측이 아니라 실측이다 — 캐릭터의 접지 캐스터와 같은 형상으로 쏘고, 아트는 렌더러 부피로 잰다.
    /// </summary>
    public static Report Verify(LevelRoute route, LevelBounds bounds, Report report)
    {
        report.nodes = route.main.Count;
        report.branches = route.branches.Count;

        foreach (LevelRoute.Segment segment in route.Segments())
        {
            route.EdgePoints(segment.from, segment.to, out Vector3 a, out Vector3 b);

            if (!TraversalSolver.Solve(route.profile, a, b, segment.to.entry).Possible) report.unreachable++;
            else if (!route.HoldReach(segment.from, segment.to, out _, out _, out _, out _, out _)) report.mustRelease++;
        }

        foreach ((LevelRoute.Node _, LevelRoute.Node _, float _) in route.ClearanceProblems()) report.cramped++;

        Bounds region = LevelBounds.WorldBoundsOf(bounds.region);
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;

        // 출발·도착은 사람이 트리거로 찍은 자리다. 영역을 넘든 말든 그것이 요구사항이므로 세지 않는다.
        LevelRoute.Node origin = route.main.Count > 0 ? route.main[0] : null;
        LevelRoute.Node finish = route.main.Count > 0 ? route.main[route.main.Count - 1] : null;

        foreach (LevelRoute.Node node in route.AllNodes())
        {
            float half = route.HalfWidthOf(node);

            if (node != origin && node != finish
                && (node.position.x - half < region.min.x - 0.01f || node.position.x + half > region.max.x + 0.01f
                || node.position.y < region.min.y - 0.01f || node.position.y > region.max.y + 0.01f)) report.outside++;

            minX = Mathf.Min(minX, node.position.x);
            maxX = Mathf.Max(maxX, node.position.x);
            minY = Mathf.Min(minY, node.position.y);
            maxY = Mathf.Max(maxY, node.position.y);
        }

        report.width = maxX - minX;
        report.height = maxY - minY;

        // 덧붙여 짓는 이상, 이번 것뿐 아니라 예전에 지은 것까지 함께 봐야 한다 —
        // 새 발판이 예전 발판을 뚫는 것도 관통이고, 콘솔은 이미 그렇게 세고 있다.
        List<Transform> tiles = new List<Transform>();
        foreach (Transform container in PlatformBuilder.ContainersOf(route))
            foreach (Transform tile in container) tiles.Add(tile);

        List<Bounds> volumes = new List<Bounds>();

        foreach (Transform tile in tiles)
        {
            bool any = false;
            Bounds art = default;

            foreach (Renderer renderer in tile.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled) continue;

                if (!any) { art = renderer.bounds; any = true; }
                else art.Encapsulate(renderer.bounds);
            }

            if (!any) continue;

            art.Expand(-0.002f);
            volumes.Add(art);
        }

        report.tiles = volumes.Count;

        for (int i = 0; i < volumes.Count; i++)
            for (int j = i + 1; j < volumes.Count; j++)
                if (volumes[i].Intersects(volumes[j])) report.intersecting++;

        return report;
    }

    #endregion
}
