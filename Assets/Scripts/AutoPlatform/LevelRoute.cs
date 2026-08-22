using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레벨의 <b>동선</b>. 발판이 아니라 발이 닿는 지점을 적는다.
///
/// 노드 하나가 "여기 서 있을 수 있다"는 뜻이고, 노드 사이가 한 번의 이동이다.
/// 발판은 여기서 나온다 — 반대가 아니다. 그래야 튜닝으로 점프가 바뀌었을 때
/// 무엇이 깨졌는지 눈이 아니라 판정으로 알 수 있다.
///
/// 좌표는 로컬 XY다. 주인공이 언제나 Z=0에 있으므로 Z는 쓰지 않는다.
/// </summary>
public class LevelRoute : MonoBehaviour
{
    /// <summary>
    /// 모듈 한 장의 실측 치수. 발판을 지을 때 프리팹에서 재서 갱신된다.
    ///
    /// 아트의 세로 부피를 따로 들고 있어야 한다 — 이 애셋들은 걸어 다니는 슬래브가 아니라
    /// 그 아래로 열주가 통째로 매달린 신전 조각이라, 딛는 면만 보고 배치하면 서로 관통한다.
    /// </summary>
    [System.Serializable]
    public struct PadSpec
    {
        [Tooltip("프리팹의 GUID. 목록의 순서가 바뀌어도 이것으로 다시 찾는다.")]
        public string guid;

        public string name;

        public float width;

        [Tooltip("윗면에서 아래로 뻗은 아트 길이.")]
        public float drop;

        [Tooltip("윗면 위로 솟은 아트 길이.")]
        public float rise;

        /// <summary>
        /// 윗면에서 아래로 뻗은 <b>콜라이더</b> 길이. 몸이 실제로 막히는 부피는 이것뿐이다.
        ///
        /// 종유석이나 매달린 장식은 콜라이더가 없어 지나가는 데 지장이 없다.
        /// 통행 여유를 아트로 재면 있지도 않은 벽을 요구하게 된다.
        /// </summary>
        public float colliderDrop;

        [Tooltip("윗면 위로 솟은 콜라이더 길이.")]
        public float colliderRise;

        public bool Valid => width > 0.01f;
    }

    [System.Serializable]
    public class Node
    {
        public string label;

        /// <summary>발이 닿는 지점. 로컬 XY.</summary>
        public Vector2 position;

        /// <summary>앞 노드에서 여기로 오는 수단. Auto면 풀어서 가장 싼 것을 찾는다.</summary>
        public MoveKind entry = MoveKind.Auto;

        /// <summary>발판 목록에서 몇 번째를 쓸지. <see cref="NoPad"/>이면 발판을 두지 않는다.</summary>
        public int module;

        /// <summary>발판 폭(m). 0이면 모듈 하나 크기 그대로.</summary>
        public float padWidth = 0f;

        /// <summary>
        /// 이 발판을 떠날 때 몸이 솟는 높이. 머리 위로 얼마나 비워야 하는지가 여기서 나온다.
        ///
        /// 홑점프로 떠나는 자리와 2단 점프로 떠나는 자리는 요구가 1.3 m나 다르다.
        /// 모든 발판에 같은 값을 요구하면, 넉넉한 쪽은 쓸데없이 벌어지고
        /// 빠듯한 쪽은 <b>통과 판정을 받고도 실제로는 머리가 닿는다</b>.
        /// </summary>
        public float departRise;
    }

    /// <summary>
    /// 메인에서 갈라져 나가는 곁길. 보상이나 지름길처럼 "안 가도 되는" 경로다.
    /// <see cref="fromIndex"/>에서 출발해 <see cref="rejoinIndex"/>로 돌아온다.
    /// </summary>
    [System.Serializable]
    public class Branch
    {
        public string name = "Sub";

        /// <summary>메인 루트의 몇 번 노드에서 갈라지는가.</summary>
        public int fromIndex;

        /// <summary>메인 루트의 몇 번 노드로 합류하는가. -1이면 막다른 길.</summary>
        public int rejoinIndex = -1;

        public List<Node> nodes = new List<Node>();
    }

    [Tooltip("판정에 쓸 이동 성능. 인스펙터의 [캐릭터에서 읽기]로 실제 값을 긁어온다.")]
    public MotionProfile profile = new MotionProfile();

    [Tooltip("비워두면 인스펙터가 씬에서 찾는다. 캐릭터 씬이 안 열려 있으면 마지막으로 읽은 값을 쓴다.")]
    public CharacterManager sampleCharacter;

    public List<Node> main = new List<Node>();
    public List<Branch> branches = new List<Branch>();

    /// <summary>
    /// 길이 아닌 발판. 지어지기는 하되 밟고 지나가는 자리가 아니다.
    ///
    /// 두꺼운 발판을 길목에 세워 두면 그 부피가 그대로 벽이 된다 — 넘어갈 수 없으니
    /// 옆으로 타고 올라야 하고, 그러느라 좁은 폭에서 연달아 뛰게 된다.
    /// 겹침 검사는 이것도 함께 보지만, 구간 판정은 보지 않는다.
    /// </summary>
    public List<Node> obstacles = new List<Node>();

    [Header("배치")]
    /// <summary>
    /// 발판을 지어 넣을 부모.
    ///
    /// 이름으로 찾지 않는다. "Physics"라는 이름에 기대고 있었더니, 그 오브젝트를 다른 것 밑으로
    /// 옮기거나 이름을 바꾸는 순간 지은 것을 <b>다시 찾지 못해 지우기가 조용히 아무것도 안 했다</b>.
    /// 참조로 들고 있으면 어디로 옮기든 따라간다.
    /// </summary>
    [Tooltip("발판을 지어 넣을 부모. 비워두면 처음 지을 때 찾거나 만들어 여기에 적어 둔다.")]
    public Transform buildRoot;

    /// <summary>이 루트가 지은 컨테이너들. 지우기는 정확히 이것만 없앤다.</summary>
    public List<Transform> builtContainers = new List<Transform>();

    /// <summary>
    /// 쓸 수 있는 발판 목록. 지정한 폴더를 훑어 실측한 것이며, 폭이 좁은 것부터 늘어선다.
    ///
    /// 프리팹의 정체를 이름이나 열거형으로 알지 않는다 — 폭과 두께를 재서 역할을 정한다.
    /// 그래야 폴더에 다른 애셋을 넣어도 코드를 고치지 않는다.
    /// </summary>
    public PadSpec[] pads = new PadSpec[0];

    [Header("기즈모")]
    public bool drawEnvelopes = true;
    public bool drawBody = true;
    public bool drawOnlyWhenSelected = false;

    /// <summary>
    /// 동선을 통째로 비운다. 지어 놓은 발판은 건드리지 않는다 — 그쪽은 빌더의 몫이다.
    /// </summary>
    public void ClearRoute()
    {
        main.Clear();
        branches.Clear();
        obstacles.Clear();
    }

    /// <summary>노드의 월드 위치. Z는 언제나 0으로 눌러 둔다.</summary>
    public Vector3 WorldOf(Node node)
    {
        Vector3 world = transform.TransformPoint(new Vector3(node.position.x, node.position.y, 0f));
        world.z = 0f;
        return world;
    }

    /// <summary>
    /// 발판 반폭. 폭을 안 정한 노드는 그 모듈 한 장으로 본다.
    ///
    /// 요청한 폭을 그대로 쓰면 안 된다 — 빌더는 모듈을 쪼개 놓지 못하므로 개수를 <b>올림</b>해서
    /// 짓는다. 12 m를 달라고 하면 5.01 m짜리 세 장, 즉 15.03 m가 선다.
    /// 여기서 같은 올림을 하지 않으면 판정이 아는 폭보다 실제가 넓어져, 옆 발판을 소리 없이 덮는다.
    /// </summary>
    public float HalfWidthOf(Node node)
    {
        if (!SpecOf(node).Valid) return 0f;

        float module = SpecOf(node).width;
        if (module <= 0.01f) return 0f;

        float wanted = node.padWidth > 0.01f ? node.padWidth : module;
        int tiles = Mathf.Max(1, Mathf.CeilToInt(wanted / module - 0.001f));

        return tiles * module * 0.5f;
    }

    /// <summary>
    /// 판정에 쓸 두 끝점 — 도약대의 끝과 착지대의 코앞.
    ///
    /// 노드 중심끼리 재면 안 된다. 발판이 6.95 m나 되므로 중심 거리에는 딛고 선 땅이 그대로 들어가고,
    /// 그러면 실제로 넘어야 하는 <b>틈</b>보다 훨씬 큰 값을 넘으라고 요구하게 된다.
    /// 넘어야 하는 최소량은 언제나 가장자리에서 가장자리까지다.
    /// </summary>
    public void EdgePoints(Node from, Node to, out Vector3 takeoff, out Vector3 landing)
    {
        Vector3 a = WorldOf(from);
        Vector3 b = WorldOf(to);

        float direction = Mathf.Sign(b.x - a.x == 0f ? 1f : b.x - a.x);

        takeoff = a + Vector3.right * direction * HalfWidthOf(from);
        landing = b - Vector3.right * direction * HalfWidthOf(to);

        // 발판이 서로 물려 있으면 틈이 없다. 음수 거리로 뒤집히지 않게 한 점으로 눌러 둔다.
        if ((landing.x - takeoff.x) * direction < 0f) landing.x = takeoff.x;
    }

    /// <summary>
    /// 방향 키를 놓지 않고 이 구간을 밟을 수 있는지.
    ///
    /// 손을 떼지 않으면 수평 속도가 언제나 최대라 점프를 짧게 만들지 못한다. 그래서 도달은
    /// 상한만의 문제가 아니다 — 가장 짧은 궤적으로도 목표를 지나쳐 버리면 그 발판은 못 밟는다.
    /// 도약 지점은 출발 발판 위 어디든 될 수 있으므로, 착지 가능 구간은
    /// [가까운 끝 + 최소사거리, 먼 끝 + 최대사거리]가 된다. 이것이 목표 발판과 겹쳐야 한다.
    /// </summary>
    public bool HoldReach(Node from, Node to, out Ability technique,
        out float shortest, out float longest, out float nearEdge, out float farEdge)
        => HoldReach(from, to, out technique, out _, out shortest, out longest, out nearEdge, out farEdge);

    public bool HoldReach(Node from, Node to, out Ability technique, out bool ledge,
        out float shortest, out float longest, out float nearEdge, out float farEdge)
    {
        ledge = false;
        Vector3 a = WorldOf(from);
        Vector3 b = WorldOf(to);

        float direction = b.x - a.x >= 0f ? 1f : -1f;
        float dy = b.y - a.y;

        float halfFrom = HalfWidthOf(from);
        float halfTo = HalfWidthOf(to);

        // 목표 발판을 가로지르는 구간 — 출발 발판의 먼 끝에서 잰다.
        nearEdge = (b.x - direction * halfTo - (a.x + direction * halfFrom)) * direction;
        farEdge = nearEdge + halfTo * 2f;

        technique = Ability.None;
        shortest = 0f;
        longest = 0f;

        // 싼 수단부터 묻는다. 되돌아 점프는 목록 끝에 있으니, 방향을 쥔 채로 되는 일에는 쓰이지 않는다.
        foreach (Ability plan in MotionEnvelope.Plans)
        {
            if (!MotionEnvelope.Reach(profile, plan, dy, out float lo, out float hi)) continue;

            // 되돌아 점프는 처음 뛰는 쪽을 고를 수 있다. 그래서 부호를 뒤집은 구간도 함께 본다.
            bool reversible = (plan & Ability.Reverse) != 0;
            int variants = reversible ? 2 : 1;

            for (int i = 0; i < variants; i++)
            {
                float low = i == 0 ? lo : -hi;
                float high = i == 0 ? hi : -lo;

                // 도약을 앞당기면 그만큼 짧게 떨어진다. 출발 발판 폭이 그대로 여유가 된다.
                float reachable = low - halfFrom * 2f;
                if (reachable > farEdge || high < nearEdge) continue;

                technique = plan;
                shortest = reachable;
                longest = high;
                return true;
            }
        }

        // 발로는 못 딛는 높이다. 턱을 물면 손 길이만큼 위가 더 열린다 —
        // 발이 목표보다 reachHigh 아래를 지날 때 걸리므로, 그 높이로 다시 푼다.
        float footTarget = dy - profile.ledgeReachHigh;

        foreach (Ability plan in MotionEnvelope.Plans)
        {
            if (!MotionEnvelope.Reach(profile, plan, footTarget, out float lo, out float hi)) continue;

            bool reversible = (plan & Ability.Reverse) != 0;
            int variants = reversible ? 2 : 1;

            for (int i = 0; i < variants; i++)
            {
                float low = (i == 0 ? lo : -hi) - profile.ledgeReach;
                float high = (i == 0 ? hi : -lo) + profile.ledgeReach;

                float reachable = low - halfFrom * 2f;
                if (reachable > farEdge || high < nearEdge) continue;

                technique = plan;
                shortest = reachable;
                longest = high;
                ledge = true;
                return true;
            }
        }

        return false;
    }

    /// <summary>손을 떼지 않고는 밟지 못하는 구간들.</summary>
    public IEnumerable<(Segment segment, string reason)> HoldProblems()
    {
        foreach (Segment segment in Segments())
        {
            if (segment.to.entry == MoveKind.Ground) continue;

            if (HoldReach(segment.from, segment.to, out _, out float shortest, out float longest,
                out float nearEdge, out float farEdge)) continue;

            string reason = shortest > farEdge
                ? string.Format("가장 짧은 궤적({0:0.00} m)이 발판 끝({1:0.00} m)을 지나친다 — 손을 떼야 한다", shortest, farEdge)
                : string.Format("가장 긴 궤적({0:0.00} m)으로도 발판({1:0.00} m)에 닿지 않는다", longest, nearEdge);

            yield return (segment, reason);
        }
    }

    /// <summary>메인과 곁길을 통틀어 모든 구간을 훑는다.</summary>
    public IEnumerable<Segment> Segments()
    {
        for (int i = 1; i < main.Count; i++)
            yield return new Segment(main[i - 1], main[i], null, i);

        foreach (Branch branch in branches)
        {
            Node previous = NodeAt(branch.fromIndex);
            if (previous == null) continue;

            for (int i = 0; i < branch.nodes.Count; i++)
            {
                yield return new Segment(previous, branch.nodes[i], branch, i);
                previous = branch.nodes[i];
            }

            Node rejoin = NodeAt(branch.rejoinIndex);
            if (rejoin != null && previous != rejoin)
                yield return new Segment(previous, rejoin, branch, branch.nodes.Count);
        }
    }

    public Node NodeAt(int index)
        => index >= 0 && index < main.Count ? main[index] : null;

    /// <summary>발판을 두지 않는다는 표시.</summary>
    public const int NoPad = -1;

    public PadSpec SpecOf(int module)
        => pads != null && module >= 0 && module < pads.Length ? pads[module] : default;

    /// <summary>노드가 세우는 모듈의 실측 치수.</summary>
    public PadSpec SpecOf(Node node) => SpecOf(node.module);

    /// <summary>목록에서 가장 좁은 것. 아슬아슬한 디딤돌에 쓴다.</summary>
    public int NarrowPad => pads == null || pads.Length == 0 ? NoPad : 0;

    /// <summary>
    /// 아래로 뻗은 아트가 가장 짧은 것. <b>위아래로 촘촘히 쌓을 수 있는</b> 발판이다.
    ///
    /// 층 간격을 정하는 것은 폭이 아니라 이 값이다 — 아래로 7 m 매달린 것을 디딤돌로 쓰면
    /// 아무리 좁아도 층을 10 m씩 벌려야 해서, 좁고 높은 레벨에서는 한 층도 못 쌓는다.
    /// </summary>
    public int ThinPad
    {
        get
        {
            if (pads == null || pads.Length == 0) return NoPad;

            int best = 0;
            for (int i = 1; i < pads.Length; i++)
                if (pads[i].drop < pads[best].drop) best = i;

            return best;
        }
    }

    /// <summary>가장 넓은 것.</summary>
    public int WidePad => pads == null || pads.Length == 0 ? NoPad : pads.Length - 1;

    /// <summary>
    /// 딛는 면 아래로 아트가 가장 많이 매달린 것. 벽이나 고원처럼 부피로 존재감을 내는 자리에 쓴다.
    /// 이름이 아니라 실측으로 고르므로, 폴더에 무엇이 들어오든 알아서 짚는다.
    /// </summary>
    public int ThickPad
    {
        get
        {
            if (pads == null || pads.Length == 0) return NoPad;

            int best = 0;
            for (int i = 1; i < pads.Length; i++)
                if (pads[i].drop > pads[best].drop) best = i;

            return best;
        }
    }

    /// <summary>좁은 쪽 0, 넓은 쪽 1로 놓고 그 사이를 고른다.</summary>
    public int PadByRank(float t)
    {
        if (pads == null || pads.Length == 0) return NoPad;

        return Mathf.Clamp(Mathf.RoundToInt(t * (pads.Length - 1)), 0, pads.Length - 1);
    }

    /// <summary>메인 · 곁길 · 장애물의 모든 발판을 한 줄로. 자리를 차지하는 것은 전부 여기 들어온다.</summary>
    public IEnumerable<Node> AllNodes()
    {
        foreach (Node node in main) yield return node;

        foreach (Branch branch in branches)
            foreach (Node node in branch.nodes) yield return node;

        foreach (Node node in obstacles) yield return node;
    }

    /// <summary>
    /// X가 겹치는 두 발판 사이에 남는 실제 빈 공간. 아트가 관통하면 음수가 된다.
    ///
    /// 동선만으로는 절대 안 보이는 종류의 사고다 — 두 노드가 각자 멀쩡히 닿아도,
    /// 하나가 다른 하나 위를 덮고 있으면 아트가 서로 뚫고 지나가거나, 아래쪽이 설 수도 없는 자리가 된다.
    /// 사람이 지나가려면 최소한 키만큼은 비어 있어야 한다.
    /// </summary>
    /// <summary>
    /// 발판 사이에 반드시 남겨 둘 가로 틈.
    ///
    /// 콜라이더로 잰 폭과 실제 아트의 폭이 몇 cm 어긋나서, 딱 맞닿게 놓으면 그 차이만큼 관통한다.
    /// 마지막 1 cm를 쫓는 대신 여유를 두고 물러선다.
    /// </summary>
    public const float PadGap = 0.2f;

    [Tooltip("키 위로 더 비워 둘 높이. 뛰었을 때 천장에 머리를 찧지 않으려면 점프 정점만큼은 필요하다.")]
    [Min(0f)] public float headroom = 1.3f;

    /// <summary>
    /// 발판 위에 최소한 비어 있어야 하는 높이.
    ///
    /// 키만큼만 요구하면 <b>서 있을 수는 있지만 뛸 수는 없는</b> 자리가 만들어진다.
    /// 실제로 하는 일은 서 있는 것이 아니라 뛰는 것이므로, 정점만큼을 더 비운다.
    /// </summary>
    public float RequiredClearance => profile.characterHeight + headroom;

    /// <summary>
    /// 이 발판 위에 비어 있어야 하는 높이. 그 자리를 떠날 때 무엇을 하는지에 따라 달라진다.
    /// </summary>
    public float ClearanceNeededOn(Node below)
        => profile.characterHeight + Mathf.Max(headroom, below.departRise);

    /// <summary>두 발판이 한 걸음으로 이어져 있는가. 뛰어 올라가는 목표는 천장이 아니다.</summary>
    public bool Connected(Node a, Node b)
    {
        foreach (Segment segment in Segments())
            if ((segment.from == a && segment.to == b) || (segment.from == b && segment.to == a))
                return true;

        return false;
    }

    /// <summary>두 발판의 X가 겹치는가. 겹치지 않으면 높이는 볼 것도 없다.</summary>
    public bool Shadows(Node a, Node b)
    {
        if (!SpecOf(a).Valid || !SpecOf(b).Valid) return false;

        return Mathf.Abs(a.position.x - b.position.x) < HalfWidthOf(a) + HalfWidthOf(b) + PadGap;
    }

    /// <summary>
    /// 몸이 지나갈 수 있는 빈 공간. <b>콜라이더</b>로만 잰다 —
    /// 종유석처럼 콜라이더 없는 장식은 몸을 막지 않는다.
    /// </summary>
    public float Passage(Node below, Node above)
        => (above.position.y - SpecOf(above).colliderDrop) - (below.position.y + SpecOf(below).colliderRise);

    /// <summary>
    /// 아트끼리 남는 틈. 음수면 서로 파고든 것이다.
    /// 통행과 달리 여기서는 여유가 필요 없다 — 닿지만 않으면 된다.
    /// </summary>
    public float ArtGap(Node below, Node above)
        => (above.position.y - SpecOf(above).drop) - (below.position.y + SpecOf(below).rise);

    /// <summary>예전 이름. 통행 기준으로 읽는다.</summary>
    public float Clearance(Node below, Node above) => Passage(below, above);

    /// <summary>이 자리에 발판을 놓아도 되는가 — 이미 있는 어떤 것과도 서로를 막지 않는가.</summary>
    public bool Fits(Node candidate)
    {
        foreach (Node other in AllNodes())
        {
            if (ReferenceEquals(other, candidate)) continue;
            if (!Shadows(candidate, other)) continue;

            Node below = candidate.position.y > other.position.y ? other : candidate;
            Node above = below == other ? candidate : other;

            if (Passage(below, above) < ClearanceNeededOn(below)) return false;
            if (ArtGap(below, above) < 0f) return false;
        }

        return true;
    }

    public IEnumerable<(Node below, Node above, float clearance)> ClearanceProblems()
    {
        List<Node> nodes = new List<Node>(AllNodes());

        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;

                Node below = nodes[i];
                Node above = nodes[j];
                if (!SpecOf(below).Valid || !SpecOf(above).Valid) continue;
                if (above.position.y <= below.position.y) continue;

                if (!Shadows(below, above)) continue;

                // 뛰어 올라가는 목표는 막는 것이 아니다.
                if (Connected(below, above)) continue;

                float passage = Passage(below, above);
                float art = ArtGap(below, above);

                if (passage >= ClearanceNeededOn(below) && art >= 0f) continue;

                yield return (below, above, art < 0f ? art : passage);
            }
        }
    }

    public struct Segment
    {
        public Node from;
        public Node to;

        /// <summary>메인 루트면 null.</summary>
        public Branch branch;

        public int index;

        public Segment(Node from, Node to, Branch branch, int index)
        {
            this.from = from;
            this.to = to;
            this.branch = branch;
            this.index = index;
        }
    }

    #region Gizmos

    static readonly List<Vector3> _envelope = new List<Vector3>();

    void OnDrawGizmos()
    {
        if (!drawOnlyWhenSelected) Draw();
    }

    void OnDrawGizmosSelected()
    {
        if (drawOnlyWhenSelected) Draw();
    }

    void Draw()
    {
        foreach (Segment segment in Segments()) DrawSegment(segment);

        DrawNodes(main, new Color(1f, 1f, 1f));
        foreach (Branch branch in branches)
            DrawNodes(branch.nodes, new Color(0.45f, 0.8f, 1f));
    }

    void DrawNodes(List<Node> nodes, Color color)
    {
        foreach (Node node in nodes)
        {
            Vector3 foot = WorldOf(node);

            Gizmos.color = color;
            Gizmos.DrawSphere(foot, 0.12f);

            if (!drawBody) continue;

            // 캐릭터가 서 있는 부피. 발판 폭과 천장 여유를 눈으로 재기 위한 것이다.
            Gizmos.color = new Color(color.r, color.g, color.b, 0.25f);
            Gizmos.DrawWireCube(
                foot + Vector3.up * profile.characterHeight * 0.5f,
                new Vector3(profile.characterRadius * 2f, profile.characterHeight, profile.characterRadius * 2f));
        }
    }

    void DrawSegment(Segment segment)
    {
        EdgePoints(segment.from, segment.to, out Vector3 from, out Vector3 to);

        Traversal verdict = TraversalSolver.Solve(profile, from, to, segment.to.entry);

        Gizmos.color = verdict.Color;
        Gizmos.DrawLine(WorldOf(segment.from), from);
        Gizmos.DrawLine(from, to);
        Gizmos.DrawLine(to, WorldOf(segment.to));

        if (!drawEnvelopes || verdict.kind == MoveKind.Ground) return;

        // 한계 궤적. 목표점이 이 선 안쪽에 들어와 있는지가 판정의 전부다.
        float endHeight = verdict.kind == MoveKind.Ledge
            ? to.y - from.y - profile.ledgeReachHigh
            : to.y - from.y;

        TraversalSolver.SampleEnvelope(
            profile, from, to.x - from.x, verdict.abilities, endHeight, _envelope);

        Gizmos.color = new Color(verdict.Color.r, verdict.Color.g, verdict.Color.b, 0.55f);
        for (int i = 1; i < _envelope.Count; i++)
            Gizmos.DrawLine(_envelope[i - 1], _envelope[i]);

        if (verdict.kind != MoveKind.Ledge) return;

        // 턱을 물 수 있는 구간. 손이 닿는 높이 창을 목표 발판 앞에 세워 보여준다.
        Gizmos.color = new Color(0.6f, 0.9f, 1f, 0.7f);
        Vector3 lip = to + Vector3.right * Mathf.Sign(from.x - to.x) * profile.ledgeReach;
        Gizmos.DrawLine(lip, lip - Vector3.up * profile.ledgeReachHigh);
        Gizmos.DrawLine(lip - Vector3.up * profile.ledgeReachLow,
                        lip - Vector3.up * profile.ledgeReachLow + Vector3.right * Mathf.Sign(to.x - from.x) * 0.3f);
    }

    #endregion
}
