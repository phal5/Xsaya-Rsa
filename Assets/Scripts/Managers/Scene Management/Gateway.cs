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
    [Tooltip("넘어갈 씬. 꽂으면 아래 이름 칸이 채워진다. 빌드에는 이름만 남는다.")]
    [SerializeField] UnityEditor.SceneAsset _sceneAsset;
#endif

    [Tooltip("넘어갈 씬의 이름. 빌드가 실제로 쓰는 값이다.")]
    [SerializeField] string _scene;

    [Header("Arrival")]
    [Tooltip("도착해서 설 자리. 저쪽 씬의 월드 좌표다.")]
    [SerializeField] Vector3 _place;

    [Tooltip("도착해서 바라볼 방향. 오일러 각.")]
    [SerializeField] Vector3 _facing;

    [Tooltip("도착한 자리를 쉬어간 자리로도 적는다. 끄면 관문을 넘은 직후에 죽었을 때 이전 스테이지로 되돌아간다.")]
    [SerializeField] bool _restOnArrival = true;

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
        Gizmos.DrawRay(_place, Quaternion.Euler(_facing) * Vector3.forward);
    }
#endif

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other)) Enter();
    }

    /// <summary>
    /// 관문을 넘는다. 밟아서 넘는 것이 아니라 말을 걸어 여는 문이면
    /// <see cref="InteractableEvent"/>의 onInteract에 이 메서드를 물리면 된다.
    /// </summary>
    public void Enter()
    {
        if (SceneDirector.instance == null)
        {
            Debug.LogError($"[{name}] SceneDirector가 없어 관문을 넘을 수 없습니다. 캐릭터 씬에 하나 있어야 합니다.", this);
            return;
        }

        SceneDirector.instance.Go(_scene, _place, Quaternion.Euler(_facing), _restOnArrival);
    }

    /// <summary>
    /// 들어온 것이 플레이어인지. 외력 몸에는 콜라이더가 없으므로 여기 걸리는 것은 캡슐뿐이지만,
    /// 캡슐이 몸에 직접 붙어 있지 않은 구성까지 보도록 리지드바디 쪽도 함께 본다.
    /// </summary>
    static bool IsPlayer(Collider other)
    {
        Transform player = PlayerManager.instance != null ? PlayerManager.instance.player : null;
        if (player == null) return false;

        if (other.transform == player) return true;

        return other.attachedRigidbody != null && other.attachedRigidbody.transform == player;
    }
}
