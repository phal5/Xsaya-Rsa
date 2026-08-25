using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이번 판에서 이미 쓰러진 보스의 명부. 여기 적힌 보스는 무대를 다시 불러도 돌아오지 않는다.
///
/// 이런 것이 필요한 이유는, 보스가 <b>배경 씬에 놓인 인스턴스</b>이기 때문이다.
/// 플레이어가 죽어 부활하면 <see cref="SceneDirector"/>가 배경을 통째로 내렸다 다시 부르므로,
/// 죽여둔 보스도 새 오브젝트로 태어난다. 씬 밖에 기억이 있어야 그 자리에서 걸러낼 수 있다.
///
/// MonoBehaviour가 아니다. 씬에 놓을 오브젝트도, 인스턴스 null 검사도 필요 없다 —
/// <see cref="TimeManager"/>와 같은 사정이다.
///
/// <b>한 판까지다.</b> 직렬화하지 않으므로 앱을 껐다 켜면 비어 있다. 이는 체크포인트와 같은 규칙이고,
/// 세이브가 생기면 적어야 할 것은 이 집합 하나뿐이다.
/// </summary>
public static class BossGraveyard
{
    static readonly HashSet<string> _fallen = new HashSet<string>();

    /// <summary>지금까지 묻힌 이름표.</summary>
    public static IEnumerable<string> Roll => _fallen;

    public static int Count => _fallen.Count;

    /// <summary>
    /// 이 이름표가 이미 묻혔는지.
    ///
    /// 빈 이름표는 <b>묻히지 않은 것으로 본다.</b> 빈 문자열 하나가 명부에 들어가면
    /// 이름표를 적지 않은 보스가 전부 함께 사라지기 때문이다.
    /// </summary>
    public static bool Fallen(string id) => !string.IsNullOrEmpty(id) && _fallen.Contains(id);

    /// <summary>묻는다. <see cref="Boss_Dead"/>가 부른다.</summary>
    public static void Bury(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[BossGraveyard] 이름표 없는 보스가 죽었습니다. " +
                             "BossManager의 bossId를 적어야 다음에 이 무대를 불러도 다시 나오지 않습니다.");
            return;
        }

        _fallen.Add(id);
    }

    /// <summary>
    /// 하나를 되살린다.
    ///
    /// <b>지금 서 있는 무대에 다시 세우지는 않는다.</b> 그 오브젝트는 이미 파괴되었고,
    /// 다시 세우려면 어디에 어떤 자세로 놓을지를 아는 쪽이 있어야 하는데 그것은 씬이다.
    /// 그 무대를 <b>다음에 불러올 때부터</b> 다시 나온다.
    /// </summary>
    /// <returns>명부에 있어서 실제로 지웠는지.</returns>
    public static bool Exhume(string id) => _fallen.Remove(id);

    /// <summary>명부를 통째로 비운다. 되살아나는 시점은 <see cref="Exhume"/>와 같다.</summary>
    public static void Clear() => _fallen.Clear();

    /// <summary>
    /// static 필드는 씬을 넘겨도, (도메인 리로드를 끈 경우) 플레이를 다시 눌러도 남는다.
    /// 지난 판에 죽인 보스가 다음 판까지 따라오지 않도록 여기서 비운다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState() => _fallen.Clear();

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/보스 명부/들여다보기")]
    static void Dump()
    {
        if (_fallen.Count == 0)
        {
            Debug.Log("[BossGraveyard] 비어 있습니다.");
            return;
        }

        Debug.Log($"[BossGraveyard] {_fallen.Count}구: {string.Join(", ", _fallen)}");
    }

    [UnityEditor.MenuItem("Tools/보스 명부/비우기")]
    static void ClearFromMenu()
    {
        int had = _fallen.Count;
        _fallen.Clear();

        Debug.Log($"[BossGraveyard] {had}구를 비웠습니다. 무대를 다시 불러야 실제로 나옵니다.");
    }
#endif
}
