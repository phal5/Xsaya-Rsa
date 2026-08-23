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

    /// <summary>닿으면 넘어간다. 콜라이더는 <b>Is Trigger를 켜두어야</b> 여기가 불린다.</summary>
    void OnTriggerEnter(Collider other)
    {
        if (PlayerManager.IsPlayer(other)) Enter();
    }

    /// <summary>
    /// 관문을 넘는다. 밟아서 넘는 것은 위에서 부르고, <b>말을 걸어 여는 문</b>이면
    /// <see cref="InteractableEvent"/>의 onInteract에 이 메서드를 물린다.
    /// 목적지는 어느 쪽으로 들어오든 같은 한 벌이다.
    /// </summary>
    public void Enter()
    {
        if (SceneDirector.instance == null)
        {
            Debug.LogError($"[{name}] SceneDirector가 없어 관문을 넘을 수 없습니다. 캐릭터 씬에 하나 있어야 합니다.", this);
            return;
        }

        SceneDirector.instance.Go(_scene, _place, Quaternion.Euler(_facing));
    }
}
