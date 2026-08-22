using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 이 씬이 지금의 무대라고 알리는 표식. 배경 씬 루트에 하나만 둔다.
///
/// 씬을 겹쳐 올리면 하늘·환경광·안개는 <b>액티브 씬</b>의 것 한 벌만 쓰인다.
/// 액티브 씬은 가장 먼저 올라온 씬이고 배경은 늘 나중에 얹히므로, 스테이지마다 라이팅 창에서
/// 잡아둔 값은 손도 대지 못한 채 Character 씬의 것 — 지금은 스카이박스가 비어 있다 — 이 그려진다.
/// 그래서 배경이 그 자리를 <b>스스로 가져간다.</b> 카메라가 캐릭터 씬에 있어 반대로 꽂을 수 없는 것은
/// <see cref="PlayerCameraRig"/>가 매니저에 스스로 등록하는 것과 같은 사정이다.
///
/// 스카이박스 하나가 아니라 RenderSettings 한 벌이 통째로 따라온다 — 환경광·안개·반사·태양.
/// 그래서 여기에는 필드가 없다. 값은 씬이 이미 들고 있고, 여기 또 적어두면 둘이 갈라진다.
///
/// <b>딸려 오는 것 하나:</b> 액티브 씬은 런타임에 Instantiate된 오브젝트가 들어갈 자리이기도 하다.
/// 이 표식이 붙은 뒤로 이펙트·투사체는 캐릭터 씬이 아니라 이 배경 씬에 태어나고,
/// 배경이 내려갈 때 함께 정리된다. 스테이지에 속한 것들이므로 그 편이 맞다.
/// </summary>
[DisallowMultipleComponent]
public class Stage : MonoBehaviour
{
    /// <summary>
    /// Awake가 아니라 Start다. 비동기 Additive 로드는 씬이 <b>로드 완료로 표시되기 전에</b> Awake를 돌리는데,
    /// SetActiveScene은 아직 로드되지 않은 씬을 거부한다. 그 창 안에서 부르면 조용히 실패한다.
    ///
    /// 늦어지지는 않는다. Start는 그 프레임이 그려지기 전에 돌므로 옛 하늘이 한 프레임 비칠 일은 없다.
    /// </summary>
    void Start()
    {
        Scene scene = gameObject.scene;

        if (!SceneManager.SetActiveScene(scene))
        {
            Debug.LogError($"[{name}] '{scene.name}'을 액티브 씬으로 세우지 못했습니다. 이 씬의 라이팅이 적용되지 않습니다.", this);
            return;
        }

        // 하늘이 갈렸다고 환경광이 저절로 다시 구워지지는 않는다.
        // 이 씬들은 Ambient Mode가 Skybox라, 이걸 부르지 않으면 하늘만 바뀌고 빛은 이전 것이 남는다.
        DynamicGI.UpdateEnvironment();
    }
}
