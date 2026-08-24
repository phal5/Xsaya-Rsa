using UnityEngine;

/// <summary>
/// 스테이지와 스테이지를 잇는 관문. 닿으면 적어둔 씬의 적어둔 자리로 넘어간다.
///
/// 좌표는 <b>저쪽 씬의 월드 좌표</b>를 손으로 적는다. 저쪽에 마커를 놓고 꽂을 수는 없다 —
/// 씬을 넘는 참조는 유니티가 저장하지 못해, 꽂아둔 것처럼 보여도 로드되면 비어 있다.
/// 대신 멀티씬 편집으로 양쪽을 함께 열면 좌표계가 하나이므로, 선택했을 때 그려지는
/// 도착 표시를 저쪽 씬의 바닥에 맞춰 보며 적을 수 있다.
///
/// 넘어가는 일 자체는 <see cref="SceneDirector"/>가 한다. 여기 있는 것은 <b>어디로</b>뿐이다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Gateway : MonoBehaviour
{
#if UNITY_EDITOR
    [Tooltip("넘어갈 씬.")]
    [SerializeField] UnityEditor.SceneAsset _sceneAsset;
#endif

    // 씬 에셋은 빌드에 남지 않으므로 이름을 뽑아 여기 적어둔다. 빌드가 실제로 쓰는 값이다.
    // 손으로 고칠 자리가 아니라 감춘다 — 위의 에셋과 갈라지면 조용히 다른 씬을 부른다.
    [HideInInspector]
    [SerializeField] string _scene;

    [Header("Arrival")]
    [Tooltip("도착해서 설 자리. 저쪽 씬의 월드 좌표다.")]
    [SerializeField] Vector3 _place;

    [Tooltip("도착해서 바라볼 <b>방향</b>. 회전각이 아니라 방향 벡터다. (0,0,1)이면 +Z를 본다.")]
    [SerializeField] Vector3 _facing = new Vector3(0f, 0f, 1f);

    [Header("Dialogue")]
    [Tooltip("넘어가기 전에 띄울 대사. 페이지를 비워두면 곧바로 넘어간다.")]
    [SerializeField] Book _book;

#if UNITY_EDITOR
    void OnValidate()
    {
        // 씬 에셋은 빌드에 남지 않는다. 이름을 여기서 뽑아 적어둬야 빌드가 부를 수 있다.
        if (_sceneAsset != null) _scene = _sceneAsset.name;
    }

    /// <summary>
    /// 도착 자리를 씬 뷰에 그린다. 저쪽 씬을 함께 열어두면 그 바닥 위에 놓인 것이 보인다.
    /// 함께 열지 않았으면 허공에 뜬 표시가 되는데, 그 자체가 아직 맞춰보지 않았다는 뜻이다.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.9f, 0.4f);
        Gizmos.DrawWireSphere(_place, 0.5f);
        Gizmos.DrawLine(transform.position, _place);
        Gizmos.DrawRay(_place, Facing * Vector3.forward);
    }
#endif

    /// <summary>닿으면 넘어간다. 콜라이더는 <b>Is Trigger를 켜두어야</b> 여기가 불린다.</summary>
    void OnTriggerEnter(Collider other)
    {
        if (PlayerManager.IsPlayer(other)) Enter();
    }

    /// <summary>
    /// 관문에 들어선다. 적어둔 대사가 있으면 <b>띄우기만 하고 끝나길 기다리지 않는다</b> —
    /// 넘어가는 것은 이 자리에서 곧바로 시작된다.
    ///
    /// 대사는 화면을 덮는 커튼(sortingOrder가 더 높다) 아래서 계속 돈다. 이 관문이
    /// 스테이지와 함께 파괴되는 순간 <see cref="OnDisable"/>이 강제로 닫는다 — 그때는
    /// 이미 커튼에 가려진 뒤라 눈에 띄지 않는다.
    ///
    /// 밟아서 넘는 것은 위에서 부르고, <b>말을 걸어 여는 문</b>이면
    /// <see cref="InteractableEvent"/>의 onInteract에 이 메서드를 물린다.
    ///
    /// 대사를 <b>도착한 뒤에</b> 띄울 수는 없다 — 전환은 옛 스테이지의 루트를 전부 파괴하므로
    /// 도착 시점에는 이 관문도 여기 적어둔 책도 이미 없다.
    /// </summary>
    public void Enter()
    {
        if (_book != null && _book.HasPages && FlipBook.Instance != null)
            FlipBook.Instance.SetBook(_book);

        Cross();
    }

    /// <summary>
    /// 대사 없이 지금 넘어간다. 대사를 건너뛰는 문이나 다른 연출에서 부를 수 있다.
    /// </summary>
    public void Cross()
    {
        if (SceneDirector.instance == null)
        {
            Debug.LogError($"[{name}] SceneDirector가 없어 관문을 넘을 수 없습니다. 캐릭터 씬에 하나 있어야 합니다.", this);
            return;
        }

        SceneDirector.instance.Go(_scene, _place, Facing);
    }

    /// <summary>
    /// 스테이지와 함께 사라질 때, 이 관문이 연 대사가 아직 떠 있으면 닫는다.
    ///
    /// 끝나길 기다리지 않으므로 <b>거의 항상 여기서 닫힌다</b> — Next()가 끝까지 갈 겨를이
    /// 아직 없다. ForceClose는 지금 열려 있는 책이 이것일 때만 움직이므로, 드물게
    /// 그새 다 읽고 자연히 닫힌 뒤라도 안전하게 아무 일도 하지 않는다.
    /// </summary>
    void OnDisable()
    {
        if (_book != null && FlipBook.Instance != null) FlipBook.Instance.ForceClose(_book);
    }

    /// <summary>
    /// 방향 벡터를 회전으로 바꾼다. 0 벡터는 LookRotation이 받지 못하므로 정면으로 둔다.
    /// <see cref="SceneDirector"/>의 초기 지점과 같은 규칙이다 — 한쪽만 오일러면 옮겨 적을 때 틀린다.
    /// </summary>
    Quaternion Facing => _facing.sqrMagnitude < 0.0001f
        ? Quaternion.identity
        : Quaternion.LookRotation(_facing.normalized);
}
