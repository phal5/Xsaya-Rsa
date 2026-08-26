using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Mode")]
    [Tooltip("Additive면 배경만 갈아끼운다 — 캐릭터·UI는 그대로 남는 보통의 스테이지 이동이다. " +
             "Single이면 지금 올라온 모든 씬(캐릭터·UI 포함)을 내리고 이 씬 하나만 남긴다 — " +
             "다른 스테이지가 아니라 타이틀처럼 게임 세션 자체를 나가는 목적지일 때 쓴다. " +
             "이 경우 아래 도착 자리·방향은 쓰이지 않는다 — 캐릭터가 이 전환과 함께 사라지기 때문이다.")]
    [SerializeField] LoadSceneMode _mode = LoadSceneMode.Additive;

    [Header("Arrival")]
    [Tooltip("도착해서 설 자리. 저쪽 씬의 월드 좌표다. Single 모드에서는 쓰이지 않는다.")]
    [SerializeField] Vector3 _place;

    // 위 도착 지점의 Z는 평면을 정하지 않는다. 2D 평면은 z=0 하나이고, 도착해 2D로 돌아오는
    // 순간 몸이 그쪽으로 눌린다.
    [Tooltip("도착해서 바라볼 <b>방향</b>. 회전각이 아니라 방향 벡터다. (0,0,1)이면 +Z를 본다. Single 모드에서는 쓰이지 않는다.")]
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
        // Single 모드는 도착 자리가 없다 — 캐릭터가 이 전환과 함께 사라지므로 그릴 것이 없다.
        if (_mode == LoadSceneMode.Single) return;

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

    /// <summary>대사를 기다리는 중인지. 대사가 도는 동안 다시 밟는 것을 여기서 흘린다.</summary>
    bool _waiting;

    /// <summary>
    /// 관문에 들어선다. 적어둔 대사가 있으면 <b>먼저 보여주고 끝난 뒤에</b> 넘어가고,
    /// 없으면 곧바로 넘어간다.
    ///
    /// 밟아서 넘는 것은 위에서 부르고, <b>말을 걸어 여는 문</b>이면
    /// <see cref="InteractableEvent"/>의 onInteract에 이 메서드를 물린다.
    ///
    /// 대사를 <b>도착한 뒤에</b> 띄울 수는 없다 — 전환은 옛 스테이지의 루트를 전부 파괴하므로
    /// 도착 시점에는 이 관문도 여기 적어둔 책도 이미 없다.
    /// </summary>
    public void Enter()
    {
        if (_waiting) return;

        if (_book == null || !_book.HasPages || FlipBook.Instance == null)
        {
            Cross();
            return;
        }

        // 거두는 것을 잊지 않기 위해 표시를 먼저 세운다. 이 관문은 곧 스테이지와 함께 파괴된다.
        _waiting = true;
        FlipBook.Instance.AddDialogueEndListener(OnDialogueEnd);
        FlipBook.Instance.SetBook(_book);
    }

    /// <summary>
    /// 대사가 끝나면 넘어간다.
    ///
    /// 이 이벤트에는 조작을 돌려주는 배선(onDialogueNull → ControlLock.Release)이 이미 물려 있다.
    /// 인스펙터에 적힌 것이 먼저 돌고 런타임에 건 것이 뒤에 도므로, 조작이 풀린 직후
    /// 전환이 다시 거둬간다 — 최종 상태는 전환 쪽이라 어긋나지 않는다.
    /// </summary>
    void OnDialogueEnd()
    {
        if (!_waiting) return;

        Release();
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

        SceneDirector.instance.Go(_scene, _place, Facing, mode: _mode);
    }

    /// <summary>
    /// 파괴되거나 꺼지는 길에도 반드시 거둔다. 남겨두면 FlipBook이 죽은 대상을 부른다.
    ///
    /// <b>여기서 아직 _waiting이면 대사를 못 다 본 채로 이 관문이 사라지는 것이다</b> —
    /// 이 관문이 아니라 <b>다른 경로가</b>(대표적으로 사망) 씬을 넘겨 배경째 파괴됐을 때다.
    /// 정상 종료(<see cref="OnDialogueEnd"/>)는 이미 _waiting을 꺼둔 뒤라 여기 걸리지 않는다.
    ///
    /// 그 자리를 남겨두면 FlipBook의 대사창이 새 씬 위에 그대로 떠 있는다 — Next()가 끝에 닿아야
    /// 닫히는데, 아무도 더 넘기지 않기 때문이다. 그래서 여기서 강제로 닫는다.
    /// </summary>
    void OnDisable()
    {
        bool interrupted = _waiting;

        Release();

        if (interrupted && FlipBook.Instance != null) FlipBook.Instance.ForceClose(_book);
    }

    void Release()
    {
        if (!_waiting) return;

        _waiting = false;
        if (FlipBook.Instance != null) FlipBook.Instance.RemoveDialogueEndListener(OnDialogueEnd);
    }

    /// <summary>
    /// 방향 벡터를 회전으로 바꾼다. 0 벡터는 LookRotation이 받지 못하므로 정면으로 둔다.
    /// <see cref="SceneDirector"/>의 초기 지점과 같은 규칙이다 — 한쪽만 오일러면 옮겨 적을 때 틀린다.
    /// </summary>
    Quaternion Facing => _facing.sqrMagnitude < 0.0001f
        ? Quaternion.identity
        : Quaternion.LookRotation(_facing.normalized);
}
