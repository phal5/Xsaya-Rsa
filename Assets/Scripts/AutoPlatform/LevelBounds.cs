using UnityEngine;

/// <summary>
/// 레벨이 놓일 자리를 정육면체 세 개로 준다 — 시점, 종점, 그리고 그 사이를 채울 영역.
///
/// 동선을 손으로 찍는 대신 이 셋과 지그재그 횟수만 주면 나머지는 계산으로 나온다.
/// 영역이 좁고 높을수록 요구되는 수직성이 올라가고, 그에 따라 쓰는 기술이 한 단씩 올라간다:
/// 홑점프 → 2단 점프 → 되돌아 점프 → 턱 잡기.
/// </summary>
public class LevelBounds : MonoBehaviour
{
    [Header("정육면체 트리거")]
    [Tooltip("주인공이 서서 시작하는 자리. 바닥면 한가운데가 첫 발판이 된다.")]
    public BoxCollider startTrigger;

    [Tooltip("도달해야 하는 자리. 바닥면 한가운데가 마지막 발판이 된다.")]
    public BoxCollider endTrigger;

    [Tooltip("발판이 벗어나면 안 되는 부피. 좁을수록 경로가 수직으로 선다.")]
    public BoxCollider region;

    [Header("동선")]
    [Tooltip("지그재그 층 수. 홀수면 나아가던 방향으로 빠져나온다.")]
    [Min(1)] public int zigzags = 3;

    [Tooltip("모양을 뽑는 씨앗. 같은 값이면 언제나 같은 레벨이 나온다.")]
    public int seed = 1;

    [Tooltip("지그재그 사이에 끼워 넣을 다른 구간의 비율. 0이면 지그재그만 반복한다.")]
    [Range(0f, 1f)] public float variety = 0.5f;

    /// <summary>
    /// 걸음을 도달 구간의 어디에 놓을지. 발판이 촘촘한지 성긴지를 정하는 값이다.
    ///
    /// 0이면 닿는 <b>최소 거리</b>에 붙여 발판이 빽빽해지고 홑점프로 끝난다.
    /// 1로 갈수록 한계까지 벌어져 성기고, 능력을 더 요구한다.
    /// </summary>
    [Tooltip("0이면 최소 거리에 붙어 촘촘하고, 1로 갈수록 한계까지 벌어져 성기다.")]
    [Range(0f, 1f)] public float spacing = 0.3f;

    [Tooltip("두꺼운 발판을 길목에 세워 넘어가지 못하게 한다. 옆으로 타고 올라야 지나갈 수 있다.")]
    public bool walls = true;

    [Header("반드시 들어가야 할 구간")]
    [Tooltip("씨앗은 뽑기라 무엇이 나올지 정해지지 않는다. 여기 켠 것이 다 나올 때까지 씨앗을 훑는다.")]
    public bool requireDash = true;

    public bool requireLedge = true;

    public bool requirePlateau = true;

    [Tooltip("조건을 만족하는 씨앗을 찾을 때 훑어볼 개수. 0이면 seed를 그대로 쓴다.")]
    [Min(0)] public int seedSearch = 40;

    [Header("시작 · 도착 발판")]
    [Tooltip("출발 발판의 모듈. 아래로 열주가 뻗은 것을 쓰면 지면에 선 것처럼 읽힌다.")]
    public LevelRoute.PadModule startModule = LevelRoute.PadModule.ThickerDeck;

    [Tooltip("도착 발판의 모듈. 공중에 있으므로 아래가 얇은 것을 쓴다.")]
    public LevelRoute.PadModule endModule = LevelRoute.PadModule.Deck5;

    [Tooltip("발판 가장자리와 영역 벽 사이에 남길 여백.")]
    [Min(0f)] public float margin = 1f;

    [Tooltip("갈라져 나가는 곁길의 수. 층을 걸어 돌지 않고 곧게 올라 건너뛰는 지름길이 된다.")]
    [Min(0)] public int branches = 2;

    [Tooltip("곁길 하나가 건너뛸 주 경로 노드의 최소 개수. 작으면 지름길 티가 나지 않는다.")]
    [Min(2)] public int branchSkip = 4;

    public LevelRoute.PadModule module = LevelRoute.PadModule.Deck3;

    public bool Ready => startTrigger != null && endTrigger != null && region != null;

    /// <summary>
    /// 트리거의 월드 부피. <see cref="Collider.bounds"/>를 쓰지 않는다.
    ///
    /// 저작용 트리거는 게임에 살려둘 이유가 없어 꺼 두는 것이 정상인데, 꺼진 콜라이더는
    /// <c>bounds</c>가 0을 돌려준다. 그러면 발판 폭도 영역 크기도 전부 0으로 읽혀 레벨이 무너진다.
    /// 그래서 트랜스폼과 상자 치수에서 직접 잰다 — 회전은 보지 않으니 축에 맞춰 두어야 한다.
    /// </summary>
    public static Bounds WorldBoundsOf(BoxCollider box)
    {
        Transform transform = box.transform;

        Vector3 center = transform.TransformPoint(box.center);
        Vector3 size = Vector3.Scale(box.size, transform.lossyScale);

        return new Bounds(center, new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z)));
    }

    /// <summary>주인공의 발이 놓일 자리 — 트리거 바닥면 한가운데. Z는 언제나 0이다.</summary>
    public static Vector2 FootOf(BoxCollider box)
    {
        Bounds bounds = WorldBoundsOf(box);
        return new Vector2(bounds.center.x, bounds.min.y);
    }

    /// <summary>
    /// 트리거의 폭이 그대로 발판의 폭이 된다.
    ///
    /// 트리거를 자리 표시로만 쓰면 옮기든 키우든 결과가 거의 같아진다 — 크기까지 읽어야
    /// "여기가 출발점이다"라고 정한 것이 레벨에 나타난다.
    /// </summary>
    public static float WidthOf(BoxCollider box) => WorldBoundsOf(box).size.x;

    void OnDrawGizmos()
    {
        if (startTrigger != null) Draw(startTrigger, new Color(0.4f, 1f, 0.5f, 0.35f));
        if (endTrigger != null) Draw(endTrigger, new Color(1f, 0.85f, 0.3f, 0.35f));
        if (region != null) Draw(region, new Color(0.5f, 0.7f, 1f, 0.18f));
    }

    static void Draw(BoxCollider box, Color color)
    {
        Bounds bounds = WorldBoundsOf(box);

        Gizmos.color = color;
        Gizmos.DrawCube(bounds.center, bounds.size);

        Gizmos.color = new Color(color.r, color.g, color.b, 1f);
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
