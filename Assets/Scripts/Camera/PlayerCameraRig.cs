using UnityEngine;

/// <summary>
/// 플레이어를 따라가는 리그. 대상을 <b>실행 중에</b> PlayerManager에서 받아온다.
///
/// 배경 씬에 놓인 리그는 플레이어를 직렬화할 수 없다 — 씬을 넘는 참조는 유니티가 저장하지 못하므로,
/// 인스펙터에 꽂아둔 것처럼 보여도 로드되면 비어 있다. 스테이지마다 리그를 따로 두면서
/// 스테이지별 경계 콜라이더를 같은 씬에서 꽂으려면 이 길뿐이다.
///
/// 대상을 찾는 일만 여기 있고 카메라 수식은 전부 <see cref="HybridCameraRig"/>의 것이다.
/// 그쪽을 복사해 오면 2D 오프셋도 전환 블렌딩도 3D 궤도도 두 벌이 되어, 한쪽만 고치는 날이 온다.
/// </summary>
public class PlayerCameraRig : HybridCameraRig
{
    bool _registered;

    protected override void LateUpdate()
    {
        if (!Resolve()) return;

        base.LateUpdate();
    }

    /// <summary>
    /// 플레이어를 찾는다. <b>비어 있는 것만 채운다</b> — 인스펙터에 꽂아둔 것이 있으면 그대로 쓴다.
    ///
    /// Start가 아니라 매 프레임 확인하는 것은 씬이 올라오는 순서를 가정하지 않기 위해서다.
    /// 배경이 먼저 올라오면 그 순간에는 플레이어가 아직 없다. 한 번 찾고 나면 첫 줄에서 끝난다.
    ///
    /// 찾는 김에 이쪽을 매니저에 등록한다. 반대로 매니저가 리그를 꽂을 수는 없기 때문이다.
    /// </summary>
    bool Resolve()
    {
        if (character != null && targeter != null && _registered) return true;

        PlayerManager manager = PlayerManager.instance;
        if (manager == null) return false;

        if (character == null) character = manager.player;
        if (targeter == null) targeter = manager.targeter;

        // 2D/3D 전환이 이 리그로 오게 한다. 스테이지가 바뀌면 새 리그가 그 자리를 이어받는다.
        manager.UseCameraRig(this);
        _registered = true;

        return character != null && targeter != null;
    }
}
