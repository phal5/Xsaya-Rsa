using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [field: SerializeField] public Transform player {  get; private set; }
    [Tooltip("자동 조준. 3D 모드에서 카메라가 이 대상을 향해 돈다.")]
    [field: SerializeField] public AutoTarget targeter { get; private set; }
    [field: SerializeField] public DamagableBase playerDamagable {  get; private set; }
    [Tooltip("적이 공격 시작 시점을 감지하는 통로. 회피/방어 판정에 쓰인다.")]
    [field: SerializeField] public MeleeWeapon playerWeapon { get; private set; }

    [Header("Dimension")]
    [Tooltip("2D/3D를 전환할 카메라 리그. 배경 씬에 있으면 여기서 꽂을 수 없고 리그가 스스로 등록한다.")]
    [field: SerializeField] public HybridCameraRig cameraRig { get; private set; }

    [Tooltip("플레이어 몸의 Z 정렬과 잠금을 담당한다.")]
    [field: SerializeField] public Dimensional dimension { get; private set; }

    public static PlayerManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(this);
    }

    /// <summary>
    /// 리그가 스스로 등록한다. <b>반대 방향으로는 꽂을 수 없다.</b>
    ///
    /// 리그를 배경 씬에 두면 캐릭터 씬의 이 매니저가 그것을 직렬화할 수 없다 —
    /// 씬을 넘는 참조는 유니티가 저장하지 못하므로, 인스펙터에 꽂아둔 것처럼 보여도 로드되면 비어 있다.
    /// 찾아 나서는 쪽을 리그로 뒤집으면 스테이지마다 리그가 갈려도 이 자리가 저절로 맞는다.
    /// </summary>
    public void UseCameraRig(HybridCameraRig rig)
    {
        if (rig != null) cameraRig = rig;
    }

    /// <summary>
    /// 화면 차원을 전환한다. 전투를 열고 닫는 쪽이 부른다.
    ///
    /// 2D로 돌아갈 때는 잠그기 <b>전에</b> Z를 제자리로 옮긴다. 순서가 바뀌면 어긋난 채로 굳는다.
    /// 3D로 풀 때는 옮길 것이 없다 — 그 자리에서 Z가 열리기만 하면 된다.
    /// </summary>
    public void Set2D(bool to2D)
    {
        if (cameraRig != null) cameraRig.ToggleMode(to2D);
        if (dimension == null) return;

        if (to2D) dimension.NullifyZ();
        dimension.LockZ(to2D);
    }
}
