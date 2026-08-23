using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
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

    [Header("Transition")]
    [Tooltip("화면을 가릴 시간. 0이면 기다리지 않는다. UIGroupFader의 Fade Duration과 맞춘다.")]
    [SerializeField] float _leaveTime = 0.3f;

    [Tooltip("떠나기 직전. 페이드 아웃을 여기 문다.")]
    [SerializeField] UnityEvent _onLeave;

    [Tooltip("도착해서 조작을 돌려주기 직전. 페이드 인을 여기 문다.")]
    [SerializeField] UnityEvent _onArrive;

    /// <summary>씬을 부르는 중인지. 관문을 두 번 밟는 것을 여기서 흘린다.</summary>
    public bool Busy { get; private set; }

    void Awake()
    {
        if (instance == null) instance = this;
        else Debug.LogError($"[{name}] SceneDirector가 둘 이상입니다. 캐릭터 씬에 하나만 둔다.", this);
    }

    /// <summary>
    /// 적어둔 씬의 적어둔 자리에 세운다. 그 씬이 이미 올라와 있으면 자리만 옮기고,
    /// 아니면 지금 배경을 내리고 그 씬을 불러온 뒤 옮긴다.
    /// </summary>
    /// <param name="scene">씬 <b>이름</b>. 체크포인트가 적어두는 것과 같은 단위다.</param>
    /// <param name="rest">도착한 자리를 쉬어간 자리로도 적을지.</param>
    /// <returns>
    /// 이 프레임에 끝났는지. <b>거짓이면 씬을 부르는 중이고, 조작은 이쪽이 돌려준다</b> —
    /// 부르는 쪽이 뒤이어 조작을 풀면 로드가 끝나기 전에 움직이게 된다.
    /// </returns>
    public bool Go(string scene, Vector3 place, Quaternion facing, bool rest = false)
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
            Place(scene, place, facing, rest);
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

        _onLeave.Invoke();

        // 실시간으로 센다. 일시정지가 걸린 채로 관문을 넘어도 여기서 멎지 않게.
        if (_leaveTime > 0f) yield return new WaitForSecondsRealtime(_leaveTime);

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

        Place(scene, place, facing, rest);

        _character.Movement.SetGravity(true);

        // 옛 리그는 씬과 함께 사라졌다. 끊어주지 않으면 브레인이 옛 자리에서 새 자리까지
        // 화면을 훑고 온다 — 스테이지 하나를 가로지르는 길이다.
        CinemachineCore.ResetCameraState();

        _onArrive.Invoke();

        // 조작을 돌려주기 전에 푼다. 순서가 바뀌면 한 프레임 동안 관문을 다시 밟을 수 있다.
        Busy = false;
        _root.ToControl();
    }

    /// <summary>
    /// 두 몸을 함께 옮긴다.
    ///
    /// 몸의 position만 대입하면 속도를 쥔 외력 몸이 제자리에 남아, 다음 물리 프레임에
    /// 그쪽이 몸을 도로 끌고 간다. 두 몸을 함께 놓는 길은 Pin 하나뿐이다.
    /// </summary>
    void Place(string scene, Vector3 place, Quaternion facing, bool rest)
    {
        _character.Movement.Pin(place);
        _character.Body.rotation = facing;

        // 자리를 옮긴 <b>뒤에</b> 적는다. SetCheckpoint는 몸이 지금 있는 곳을 적으므로
        // 먼저 부르면 떠나온 자리가 적힌다.
        if (rest) _character.SetCheckpoint(scene);
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
