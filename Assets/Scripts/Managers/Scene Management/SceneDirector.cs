using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 캐릭터를 <b>어느 씬의 어느 자리</b>에 세울지 아는 유일한 곳.
///
/// 부르는 자리는 둘이다 — 죽어서 쉬어간 자리로 돌아갈 때(<see cref="Character_Down"/>)와
/// 관문을 넘을 때(<see cref="Gateway"/>). 둘이 하는 일이 같아서 한 문으로 모았다.
/// 씬을 넘느냐 아니냐는 <b>부르는 쪽이 알 일이 아니다</b>. 여기서 갈린다.
///
/// 캐릭터 씬에 상주한다. 배경 씬은 자기 자신을 내리면서 코루틴을 돌릴 수 없다.
///
/// <b>발판이 사라져도 떨어지지 않는 이유:</b> 배경을 내리기 전에 조작을 거두고 중력을 끈다.
/// 중력을 켜둔 채로는 보이는 자리가 멎어 있어도 외력 몸에 아래 방향 속도가 계속 쌓여,
/// 도착해서 푸는 순간 그동안 쌓인 만큼이 한꺼번에 터진다 — 매달리기가 중력부터 끄는 것과 같은 이유다.
/// </summary>
public class SceneDirector : MonoBehaviour
{
    public static SceneDirector instance;

    [Tooltip("옮길 대상. 같은 씬에 있으므로 인스펙터에서 꽂을 수 있다.")]
    [SerializeField] CharacterManager _character;

    [Tooltip("조작을 잠그고 푸는 곳. 캐릭터의 최상위 상태기계다.")]
    [SerializeField] CharacterRoot _root;

    [Header("Initial Rest")]
#if UNITY_EDITOR
    [Tooltip("처음 부활 지점이 놓인 씬. 꽂으면 아래 이름 칸이 채워진다. 비워두면 시작 무대를 쓴다.")]
    [SerializeField] UnityEditor.SceneAsset _initialSceneAsset;
#endif

    [Tooltip("처음 부활 지점이 놓인 씬의 이름. 비어 있으면 시작할 때 올라와 있던 무대를 쓴다.")]
    [SerializeField] string _initialScene;

    [Tooltip("한 번도 쉬어가기 전에 죽었을 때 설 자리.")]
    [SerializeField] Vector3 _initialPlace = new Vector3(0f, 0.75f, 0f);

    [Tooltip("그때 바라볼 <b>방향</b>. 회전각이 아니라 방향 벡터다.")]
    [SerializeField] Vector3 _initialFacing = new Vector3(1f, 0f, 0f);

#if UNITY_EDITOR
    void OnValidate()
    {
        // 씬 에셋은 빌드에 남지 않는다. 이름을 여기서 뽑아 적어둬야 빌드가 부를 수 있다.
        if (_initialSceneAsset != null) _initialScene = _initialSceneAsset.name;
    }
#endif

    /// <summary>씬을 부르는 중인지. 관문을 두 번 밟는 것을 여기서 흘린다.</summary>
    public bool Busy { get; private set; }

    void Awake()
    {
        if (instance == null) instance = this;
        else Debug.LogError($"[{name}] SceneDirector가 둘 이상입니다. 캐릭터 씬에 하나만 둔다.", this);
    }

    /// <summary>
    /// 아직 아무 데도 쉬어가지 않은 처음 상태를 채운다.
    ///
    /// 씬을 적어두면 그것을 쓰고, 비워두면 시작할 때 올라와 있던 무대를 쓴다.
    /// 비워두는 쪽이 안전한 기본값이다 — 무엇을 열어놓고 Play하든 갈 곳이 있고, 어긋날 자리가 없다.
    /// 적어두는 쪽은 <b>처음 부활 지점이 특정 스테이지에 있을 때</b> 쓴다.
    ///
    /// Awake가 아니라 Start다. 무대의 등록이 Awake에서 이뤄지므로 그보다 늦어야 한다.
    /// </summary>
    void Start()
    {
        string scene = _initialScene;

        if (string.IsNullOrEmpty(scene))
        {
            if (Stage.Current == null)
            {
                Debug.LogWarning($"[{name}] 시작 시 무대가 없어 초기 부활 지점을 잡지 못했습니다. " +
                                 "쉬어가기 전에 죽으면 쓰러진 자리에서 그대로 일어납니다.", this);
                return;
            }

            scene = Stage.Current.gameObject.scene.name;
        }

        _restScene = scene;
        _restPlace = _initialPlace;

        // 방향 벡터를 회전으로 바꾼다. 0 벡터는 LookRotation이 받지 못하므로 정면으로 둔다.
        _restFacing = _initialFacing.sqrMagnitude < 0.0001f
            ? Quaternion.identity
            : Quaternion.LookRotation(_initialFacing.normalized);
    }

    #region Checkpoint

    /// <summary>
    /// 마지막으로 쉬어간 자리. <b>체크포인트 오브젝트가 아니라 그때 몸이 서 있던 자리</b>다.
    ///
    /// 이 기록이 여기 있는 이유는 <b>쓰는 쪽이 여기</b>이기 때문이다. 다른 스테이지에 적힌 좌표는
    /// 그 씬을 부를 수 있는 쪽에서만 뜻이 있고, 씬을 부르는 것은 이 클래스뿐이다.
    /// 직렬화하지 않는다. 앱을 껐다 켜도 남아야 하는 것은 체크포인트가 아니라 세이브의 몫이다.
    /// </summary>
    string _restScene;
    Vector3 _restPlace;
    Quaternion _restFacing;

    /// <summary>
    /// 쉬어간 자리를 적는다. <see cref="Checkpoint"/>가 부른다.
    ///
    /// 몸이 지금 서 있는 자리를 적는다 — 그 땅은 방금 딛고 있었으므로 정의상 유효하다.
    /// 체크포인트 오브젝트를 제단 위에 놓든 벽에 붙이든 스폰 오프셋을 따로 맞출 일이 없다.
    /// </summary>
    /// <param name="scene">그 자리가 속한 배경 씬의 이름.</param>
    public void SetCheckpoint(string scene)
    {
        if (_character == null || _character.Body == null) return;

        _restScene = scene;
        _restPlace = _character.Body.position;
        _restFacing = _character.Body.rotation;
    }

    /// <summary>
    /// 쉬어간 자리로 돌려보낸다. <b>다운 상태가 제 시간을 다 쓰고 부른다.</b>
    ///
    /// <b>체력을 여기서 되돌리지 않는다.</b> 부르는 쪽이 이미 되돌린 뒤에 부르기 때문이다 —
    /// 그 순서가 지켜져야 다른 스테이지로 돌아가는 길이 성립한다. 체력이 0인 채로는
    /// 최상위 머신이 매 갱신마다 다시 쓰러뜨려, 씬이 올라오는 동안 몸을 붙들 수가 없다.
    ///
    /// 적어둔 자리가 없으면 아무것도 하지 않는다 — 아직 한 번도 쉬어가지 않았다는 뜻이므로
    /// 옮길 곳이 없는 것이지, 쓰러진 자리가 옳은 것은 아니다.
    /// </summary>
    /// <returns>
    /// 이 프레임에 끝났는지. <b>거짓이면 씬을 부르는 중이고, 조작은 이쪽이 돌려준다.</b>
    /// </returns>
    public bool Respawn()
    {
        if (string.IsNullOrEmpty(_restScene)) return true;

        return Go(_restScene, _restPlace, _restFacing);
    }

    #endregion

    /// <summary>
    /// 적어둔 씬의 적어둔 자리에 세운다. 그 씬이 이미 올라와 있으면 자리만 옮기고,
    /// 아니면 지금 배경을 내리고 그 씬을 불러온 뒤 옮긴다.
    /// </summary>
    /// <param name="scene">씬 <b>이름</b>. 체크포인트가 적어두는 것과 같은 단위다.</param>
    /// <returns>
    /// 이 프레임에 끝났는지. <b>거짓이면 씬을 부르는 중이고, 조작은 이쪽이 돌려준다</b> —
    /// 부르는 쪽이 뒤이어 조작을 풀면 로드가 끝나기 전에 움직이게 된다.
    /// </returns>
    public bool Go(string scene, Vector3 place, Quaternion facing)
    {
        if (_character == null || _root == null)
        {
            Debug.LogError($"[{name}] 옮길 대상이나 상태기계가 꽂혀 있지 않습니다.", this);
            return true;
        }

        if (Busy) return false;

        if (string.IsNullOrEmpty(scene))
        {
            Debug.LogError($"[{name}] 목적지 씬 이름이 비어 있습니다.", this);
            return true;
        }

        // 이미 올라와 있으면 부를 것이 없다. 같은 스테이지에서의 부활이 여기로 온다.
        if (SceneManager.GetSceneByName(scene).isLoaded)
        {
            Place(place, facing);
            return true;
        }

        // 여기서 막지 않으면 지금 배경을 내린 <b>뒤에야</b> 실패해, 캐릭터가 아무것도 없는 곳에 남는다.
        if (!InBuildSettings(scene))
        {
            Debug.LogError($"[{name}] '{scene}'이 Build Settings에 없어 부를 수 없습니다. " +
                           "File > Build Profiles의 씬 목록에 넣어야 경로로 불러올 수 있습니다.", this);
            return true;
        }

        StartCoroutine(Transit(scene, place, facing));
        return false;
    }

    IEnumerator Transit(string scene, Vector3 place, Quaternion facing)
    {
        Busy = true;

        // 조작을 먼저 거둔다. 지상·공중 축이 멎어야 발판이 사라진 것을 낙하로 읽지 않는다.
        _root.ToUI();

        // 몸을 붙든다. 중력이 꺼져 있고 두 몸의 속도가 0이면 쌓이는 것이 없어,
        // 매 프레임 다시 못박지 않아도 그 자리에 그대로 있는다.
        _character.Movement.SetGravity(false);
        _character.Movement.Pin(_character.Body.position);

        // <b>내리고 나서 부른다.</b> 겹쳐 올리면 배경 두 벌의 텍스처가 동시에 상주하고,
        // 새 스폰 자리에 옛 지오메트리가 남아 물리가 몸을 밀어낸다.
        // 붙들어 둔 덕에 이 사이가 비어도 떨어지지 않으므로, 겹칠 이유가 없다.
        Stage leaving = Stage.Current;
        if (leaving != null) yield return SceneManager.UnloadSceneAsync(leaving.gameObject.scene);

        // 씬을 내리는 것만으로는 텍스처가 풀리지 않는다. 실제로 메모리가 돌아오는 곳은 여기다.
        yield return Resources.UnloadUnusedAssets();

        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        // Stage가 액티브 씬을 가져가는 것은 Start다. 한 프레임 준다.
        yield return null;

        if (Stage.Current == null)
            Debug.LogWarning($"[{name}] '{scene}'에 Stage가 없습니다. 하늘과 환경광이 이전 씬 것으로 남습니다.", this);

        Place(place, facing);

        _character.Movement.SetGravity(true);

        // 조작을 돌려주기 전에 푼다. 순서가 바뀌면 한 프레임 동안 관문을 다시 밟을 수 있다.
        Busy = false;
        _root.ToControl();
    }

    /// <summary>
    /// 두 몸을 함께 옮기고 화면을 2D로 되돌린다.
    ///
    /// 몸의 position만 대입하면 속도를 쥔 외력 몸이 제자리에 남아, 다음 물리 프레임에
    /// 그쪽이 몸을 도로 끌고 간다. 두 몸을 함께 놓는 길은 Pin 하나뿐이다.
    ///
    /// <b>2D 전환이 여기 있는 이유:</b> 씬을 넘는 경우 리그는 스테이지와 함께 갈린다.
    /// 더 일찍 부르면 곧 사라질 옛 리그를 돌려놓게 되고, 새 리그는 제 씬에 적힌 모드로 시작한다.
    /// 자리를 놓는 이 시점에는 새 리그가 이미 스스로 등록을 마쳤으므로 여기가 유일하게 맞는 자리다.
    /// </summary>
    void Place(Vector3 place, Quaternion facing)
    {
        _character.Movement.Pin(place);
        _character.Body.rotation = facing;

        // 전투에서 3D로 열려 있었더라도 다시 세워질 때는 2D로 돌아온다.
        if (PlayerManager.instance != null) PlayerManager.instance.Set2D(true);
    }

    /// <summary>
    /// 이름으로 부를 수 있는 씬인지. 빌드 목록에 없는 씬은 경로를 알아도 불러올 수 없다.
    /// 이름만 비교하므로 프로젝트에 같은 이름의 씬이 둘 있으면 어느 쪽이 걸릴지는 목록 순서가 정한다.
    /// </summary>
    static bool InBuildSettings(string scene)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == scene) return true;
        }

        return false;
    }
}
