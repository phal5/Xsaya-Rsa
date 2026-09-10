using System.Collections;
using Unity.Cinemachine;
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

    [Tooltip("지역 표시 이름을 모아둔 목록. 로딩 화면이 여기서 이름을 얻는다.")]
    [SerializeField] StageDirectory _stages;

    [Header("Initial Rest")]
#if UNITY_EDITOR
    [Tooltip("처음 부활 지점이 놓인 씬. 비워두면 첫 부활만 못 한다 — 자리와 방향은 그대로 선다.")]
    [SerializeField] UnityEditor.SceneAsset _initialSceneAsset;
#endif

    // 위 에셋에서 뽑아 적어둔다. 손으로 고칠 자리가 아니라 감춘다.
    [HideInInspector]
    [SerializeField] string _initialScene;

    [Tooltip("첫 등장과, 한 번도 쉬어가기 전에 죽었을 때 설 자리.")]
    [SerializeField] Vector3 _initialPlace = new Vector3(0f, 0.75f, 0f);

    [Tooltip("그때 바라볼 <b>방향</b>. 회전각이 아니라 방향 벡터다. 씬에 저장된 몸의 각도는 이 값으로 덮인다.")]
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
    /// 아직 아무 데도 쉬어가지 않은 처음 상태를 채우고, <b>첫 등장의 자리와 방향을 세운다.</b>
    ///
    /// <b>여기가 첫 등장의 주인이다.</b> 부활은 <see cref="Go"/>가 <see cref="Place"/>를 거쳐
    /// 자리와 방향을 몸에 싣는데, 첫 등장은 그 길을 지나지 않는다. 그래서 같은 Place를 여기서 부른다 —
    /// 스폰과 첫 부활이 한 값에서 나와야 둘이 어긋날 자리가 없다.
    /// 씬에 저장된 몸의 트랜스폼은 여기서 덮인다. 같은 뜻을 두 군데 적어두면 언젠가 한쪽만 고친다.
    ///
    /// <b>무대를 묻지 않는다.</b> 예전에는 씬 이름을 비워두면 <see cref="Stage.Current"/>에서 주워왔는데,
    /// 그것이 이 자리를 <b>로드 순서에 매달았다</b>. 타이틀에서 들어오면 배경은 캐릭터 씬보다 늦게
    /// 올라오므로 그때 무대는 없고, 아래 전부를 건너뛰어 방향이 통째로 적용되지 않았다 —
    /// 인스펙터의 Initial Facing을 무엇으로 바꾸든 화면이 그대로였던 것이 그것이다.
    /// 주워온 이름은 옳지도 않다. 자리는 특정 무대의 좌표인데 아무 무대에나 갖다 붙이는 셈이다.
    ///
    /// 씬 이름이 없으면 <b>첫 부활만</b> 못 한다. 자리도 방향도 절대값이라 그와 무관하게 선다.
    ///
    /// Awake가 아니라 Start다. 몸의 부품들이 Awake에서 자리를 잡으므로 그보다 늦어야 한다.
    /// </summary>
    void Start()
    {
        if (_character == null)
        {
            Debug.LogError($"[{name}] 옮길 대상이 꽂혀 있지 않아 첫 등장을 세우지 못했습니다.", this);
            return;
        }

        // 일어나는 동안 그림을 몸에서 떼어 놓을 거리. 인스펙터에 따로 두지 않는다 —
        // 매니저에 이미 적혀 있고, 같은 값이 두 군데 있으면 어느 쪽이 진짜인지 물어야 한다.
        _restOffset = _character.RiseOffset;

        _restPlace = _initialPlace;

        // 방향 벡터를 회전으로 바꾼다. 0 벡터는 LookRotation이 받지 못하므로 정면으로 둔다.
        _restFacing = _initialFacing.sqrMagnitude < 0.0001f
            ? Quaternion.identity
            : Quaternion.LookRotation(_initialFacing.normalized);

        if (string.IsNullOrEmpty(_initialScene))
            Debug.LogWarning($"[{name}] 처음 부활 지점의 씬이 비어 있습니다. " +
                             "쉬어가기 전에 죽으면 쓰러진 자리에서 그대로 일어납니다.", this);
        else
            _restScene = _initialScene;

        Place(_restPlace, _restFacing);

        // 화면 차원도 같은 사정이다 — 첫 등장은 Go()를 거치지 않으므로 Restore2D를 직접 부른다.
        // 지금 올라와 있는 스테이지(Stage.Current)가 정한 값을 그대로 따른다.
        Restore2D();
    }

    /// <summary>
    /// 그 씬의 지역 표시 이름. 로딩 화면이 부른다.
    ///
    /// 목록이 없으면 씬 이름으로 대신한다 — 이름이 없다고 전환을 막을 일은 아니다.
    /// </summary>
    public string RegionName(string scene)
    {
        if (_stages != null) return _stages.NameOf(scene);

        Debug.LogWarning($"[{name}] 지역 목록이 꽂혀 있지 않습니다. 씬 이름을 그대로 씁니다.", this);
        return scene;
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
    Vector3 _restOffset;

    /// <summary>
    /// 일어나는 동안 그림을 몸에서 떼어 놓을 거리. <see cref="Character_Rest"/>가 읽는다.
    ///
    /// 여기 두는 이유는 자리·방향과 같다 — 어느 체크포인트에서 일어나느냐에 딸린 값이고,
    /// 그것을 아는 것은 이쪽뿐이다. 아직 아무 데도 쉬어가지 않았으면 매니저에 적힌 값이 그대로 남는다.
    /// </summary>
    public Vector3 RestOffset => _restOffset;

    /// <summary>
    /// 쉬어간 자리를 적는다. <see cref="Checkpoint"/>가 부른다.
    ///
    /// 위치와 방향 <b>둘 다 부르는 쪽이 정해서 넘긴다.</b> 몸이 서 있던 자리를 쓰지 않는다 —
    /// 체크포인트는 제 위치에 저작자가 잡아둔 오프셋을 더해 스폰 지점을 스스로 계산하고,
    /// 여기는 그 결과를 그대로 받아 적을 뿐이다. 말을 건 각도·위치에 따라 매번 달라지면
    /// 스폰 지점이 흔들리므로, 체크포인트마다 고정해두는 편이 낫다.
    /// </summary>
    /// <param name="scene">그 자리가 속한 배경 씬의 이름.</param>
    /// <param name="place">쉬어간 뒤 설 자리. 체크포인트 위치 + 오프셋으로 이미 계산된 값이다.</param>
    /// <param name="riseOffset">일어나는 동안 그림을 몸에서 떼어 놓을 거리. 자리마다 바닥이 다르므로 함께 받는다.</param>
    public void SetCheckpoint(string scene, Vector3 place, Quaternion facing, Vector3 riseOffset)
    {
        _restScene = scene;
        _restPlace = place;
        _restFacing = facing;
        _restOffset = riseOffset;

        // 쉬어가는 것은 곧 체력을 채우는 것이다. Revive()를 쓰지 않는다 — 그건 죽음에서
        // 돌아올 때의 몫이고, 여기는 살아있는 채로 등록하는 자리라 Invulnerable을 건드릴 일이 없다.
        if (_character != null && _character.Damagable != null)
            _character.Damagable.Heal(_character.Damagable.MaxHealth);
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

        // 부활은 누운 자리에서 시작한다. 관문 통과와 갈리는 유일한 지점이라 여기서 표시를 넘긴다.
        return Go(_restScene, _restPlace, _restFacing, rest: true);
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
    /// <param name="rest">
    /// 도착한 뒤 누운 자리에서 시작할지. 부활이 참이고 관문 통과가 거짓이다.
    /// 이 씬이 이미 올라와 있으면 조작을 부르는 쪽이 돌려주므로 여기서는 쓰이지 않는다.
    /// </param>
    /// <param name="mode">
    /// Additive면 지금까지처럼 배경만 갈아끼운다 — 캐릭터·UI는 그대로 남는다. 스테이지 사이 이동이 이쪽이다.
    /// Single이면 지금 올라온 모든 씬(이 오브젝트가 있는 캐릭터 씬까지)을 내리고 지정한 씬 하나만 남긴다 —
    /// 다른 스테이지로 가는 게 아니라 <b>게임 세션 자체를 나가는 것</b>이다. 타이틀로 돌아갈 때가 이쪽이다.
    /// 이 경우 자리·방향·부활 여부는 쓰이지 않는다 — 캐릭터가 이 전환과 함께 사라지기 때문이다.
    /// </param>
    public bool Go(string scene, Vector3 place, Quaternion facing, bool rest = false, LoadSceneMode mode = LoadSceneMode.Additive)
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

        if (mode == LoadSceneMode.Single)
        {
            if (!InBuildSettings(scene))
            {
                Debug.LogError($"[{name}] '{scene}'이 Build Settings에 없어 부를 수 없습니다. " +
                               "File > Build Profiles의 씬 목록에 넣어야 경로로 불러올 수 있습니다.", this);
                return true;
            }

            // 코루틴도 커튼도 없다. 이 오브젝트가 있는 씬을 포함해 전부 내려가므로,
            // 여기서 뭔가를 더 하려 들면 그 코드가 도는 중에 자기 자신이 사라진다.
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
            return true;
        }

        // 이미 올라와 있으면 부를 것이 없다. 같은 스테이지에서의 부활이 여기로 온다.
        if (SceneManager.GetSceneByName(scene).isLoaded)
        {
            Place(place, facing);
            Restore2D();
            return true;
        }

        // 여기서 막지 않으면 지금 배경을 내린 <b>뒤에야</b> 실패해, 캐릭터가 아무것도 없는 곳에 남는다.
        if (!InBuildSettings(scene))
        {
            Debug.LogError($"[{name}] '{scene}'이 Build Settings에 없어 부를 수 없습니다. " +
                           "File > Build Profiles의 씬 목록에 넣어야 경로로 불러올 수 있습니다.", this);
            return true;
        }

        StartCoroutine(Transit(scene, place, facing, rest));
        return false;
    }

    IEnumerator Transit(string scene, Vector3 place, Quaternion facing, bool rest)
    {
        Busy = true;

        // 조작을 먼저 거둔다. 지상·공중 축이 멎어야 발판이 사라진 것을 낙하로 읽지 않는다.
        _root.ToUI();

        // 몸을 붙든다. 중력이 꺼져 있고 두 몸의 속도가 0이면 쌓이는 것이 없어,
        // 매 프레임 다시 못박지 않아도 그 자리에 그대로 있는다.
        _character.Movement.SetGravity(false);
        _character.Movement.Pin(_character.Body.position);

        // <b>덮고 나서 손대기 시작한다.</b> 배경이 내려가면 카메라가 빈 하늘을 비추는데,
        // 뒤따르는 로드가 동기라 그 화면이 얼어붙은 채로 눈에 남는다.
        // Cover는 덮인 프레임이 한 번 그려진 뒤에 돌아오므로, 그 창이 닫힌다.
        TransitionCurtain curtain = TransitionCurtain.Current;
        if (curtain != null) yield return curtain.Cover(RegionName(scene));

        // <b>내리고 나서 부른다.</b> 겹쳐 올리면 배경 두 벌의 텍스처가 동시에 상주하고,
        // 새 스폰 자리에 옛 지오메트리가 남아 물리가 몸을 밀어낸다.
        // 붙들어 둔 덕에 이 사이가 비어도 떨어지지 않으므로, 겹칠 이유가 없다.
        Stage leaving = Stage.Current;
        if (leaving != null)
        {
            Scene old = leaving.gameObject.scene;

            // <b>먼저 비활성화하고 나서 파괴한다.</b> Destroy는 프레임 끝까지 미뤄지므로
            // 그것만으로는 이 프레임 안에서 콜라이더가 사라지지 않는다 — 옛 지오메트리가
            // 물리에 남은 채로 새 자리에 몸이 놓이면 겹친 상태에서 밀려나며 튕긴다.
            // 비활성화는 부르는 즉시 먹으므로, 그 창을 닫는 것은 이쪽이다.
            foreach (GameObject root in old.GetRootGameObjects())
            {
                root.SetActive(false);
                Destroy(root);
            }

            // 껍데기만 남은 씬을 정리한다. 동기 언로드는 API 자체가 없다.
            yield return SceneManager.UnloadSceneAsync(old);
        }

        // 씬을 내리는 것만으로는 텍스처가 풀리지 않는다. 실제로 메모리가 돌아오는 곳은 여기다.
        yield return Resources.UnloadUnusedAssets();

        // <b>동기 로드다.</b> 비동기로 걸면 로드가 끝날 때까지 프레임이 계속 돌고,
        // 그동안 물리는 발판 없는 세상에서 스텝을 밟는다. 동기는 프레임 끝에 한 번에 통합되므로
        // 그 창이 아예 생기지 않는다 — 대신 그만큼 화면이 멈춘다. 어차피 조작을 거둔 구간이라 그 편이 낫다.
        // <b>부활이면 누운 상태로 여기서 들여보낸다.</b> 커튼이 걷힌 뒤에 들여보내면, 걷히는 동안에는 그 상태가
        // 세우는 것들 — 누운 자세, 몸에서 떼어 놓은 그림(RiseOffset), 비틀림(RiseTwist) — 이 하나도 서 있지 않다.
        // 사망 클립의 마지막 자세가 제자리에 선 채로 보이다가, 걷힌 뒤에 한꺼번에 튄다.
        //
        // 커튼 아래서 들여보내면 걷힐 때 이미 다 서 있다. 시간을 멈추는 것도 함께 오지만 커튼은 벽시계로 돈다.
        // 기상 입력은 전환이 끝날 때까지 Character_Rest가 스스로 막는다.
        if (rest) _root.ToRest();

        SceneManager.LoadScene(scene, LoadSceneMode.Additive);

        // <b>통합 전에 놓는다.</b> 새 리그는 태어나자마자 첫 LateUpdate에서 플레이어를 찾는데,
        // 그때 몸이 아직 떠나온 자리에 있으면 리그가 <b>그쪽으로</b> 한 번 맞췄다가 다시 돌아온다.
        // 화면이 스테이지를 가로질러 훑는 것이 그것이다. 먼저 놓으면 리그가 볼 자리가 처음부터 옳다.
        Place(place, facing);

        // 여기서 프레임이 끝나며 씬이 통합된다. 깨어나면 Stage.Start도 새 리그의 등록도 끝나 있다.
        yield return null;

        if (Stage.Current == null)
            Debug.LogWarning($"[{name}] '{scene}'에 Stage가 없습니다. 하늘과 환경광이 이전 씬 것으로 남습니다.", this);

        // <b>새 리그가 등록된 뒤라야 한다.</b> 리그는 스테이지와 함께 갈리므로,
        // 자리를 놓던 시점에 부르면 곧 사라질 옛 리그를 돌려놓게 된다.
        Restore2D();

        // 옛 리그는 씬과 함께 사라졌다. 끊어주지 않으면 브레인이 옛 자리에서 새 자리까지 화면을 훑고 온다.
        CinemachineCore.ResetCameraState();

        // 이름을 읽을 틈을 준 뒤 걷는다. 로드가 빨라도 화면이 깜빡이고 지나가지 않게.
        if (curtain != null)
        {
            yield return curtain.Hold();
            yield return curtain.Reveal();
        }

        // <b>커튼이 걷힌 뒤에 중력을 돌려준다.</b> 앞에서 돌려주면 커튼이 도는 1초 남짓 동안
        // 몸의 수직을 아무도 소유하지 않는다 - 그 사이 루트는 Character_UI이고, 그 상태는
        // 수평만 눌러둘 뿐 수직은 sampler와 중력의 몫이다. 붙들어 둔 덕에 이 사이가 비어도
        // 떨어지지 않는다는 위의 전제가 거기서 깨져, 도착 지점보다 한참 아래에서 일어나게 된다.
        _character.Movement.SetGravity(true);

        // 조작을 돌려주기 전에 푼다. 순서가 바뀌면 한 프레임 동안 관문을 다시 밟을 수 있다.
        Busy = false;

        // 부활은 커튼 아래서 이미 누운 상태로 들어갔다.
        if (!rest) _root.ToControl();
    }

    /// <summary>
    /// 두 몸을 함께 옮기고 화면을 2D로 되돌린다.
    ///
    /// 몸의 position만 대입하면 속도를 쥔 외력 몸이 제자리에 남아, 다음 물리 프레임에
    /// 그쪽이 몸을 도로 끌고 간다. 두 몸을 함께 놓는 길은 Pin 하나뿐이다.
    ///
    /// 자리만 옮긴다. 2D 복귀는 <see cref="Restore2D"/>가 따로 하는데, 부를 수 있는 시점이 다르기 때문이다 —
    /// 씬을 넘는 경우 리그가 스테이지와 함께 갈려서, 새 리그가 등록을 마친 뒤에야 돌려놓을 수 있다.
    /// </summary>
    void Place(Vector3 place, Quaternion facing)
    {
        _character.Movement.Pin(place);

        // 평면을 여기서 정하지 않는다. 스폰 지점의 Z를 평면으로 삼았더니 무대마다 -20 / 0.4 / 0으로
        // 갈렸고, 그 값이 곧 2D로 돌아올 때 몸이 끌려가는 자리가 되었다. 평면은 z=0 하나다.
        //
        // 각도도 Pin과 같은 이유로 둘 다 적는다 - Rigidbody.rotation만 적으면 그림은 다음
        // 시뮬레이션 스텝까지 옛 각도로 남고, 시간이 멎은 구간에서는 그 스텝이 오지 않는다.
        _character.Body.rotation = facing;
        _character.Body.transform.rotation = facing;
    }

    /// <summary>
    /// 전투에서 3D로 열려 있었더라도 다시 세워질 때는 2D로 돌아온다.
    /// <b>지금 등록되어 있는 리그</b>를 돌려놓으므로, 부르는 시점이 곧 대상이다.
    /// </summary>
    /// <summary>
    /// 화면 차원을 이 스테이지가 정한 시작 모드로 되돌린다.
    ///
    /// <b>전투에서 3D로 열려 있었는지와 무관하게 스테이지가 정한 값을 따른다.</b> 값은
    /// Stage에 있다 — 어느 스테이지가 3D로 시작해야 하는지는 씬마다 다르고, 아는 것은 그 씬뿐이다.
    /// 무대가 없으면(비정상적인 경로거나 아직 못 잡았을 때) 2D를 기본으로 둔다.
    /// </summary>
    void Restore2D()
    {
        if (PlayerManager.instance == null) return;

        bool startIn2D = Stage.Current == null || Stage.Current.StartIn2D;
        PlayerManager.instance.Set2D(startIn2D);
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
