using UnityEngine;

/// <summary>
/// UI 전체를 접었다 펴는 자리. <b>UI 바깥의 씬에서 부르라고 있는 컴포넌트다.</b>
///
/// UI는 씬을 따로 쓰므로 스테이지의 트리거나 대사에서 페이더를 인스펙터로 꽂을 수 없다 —
/// 씬을 넘는 참조는 유니티가 저장하지 못한다. 그래서 <see cref="HudService"/>를 거쳐
/// 런타임에 찾는다. <see cref="ControlLock"/>이 조작을 거두는 것과 같은 방식이다.
///
/// <b>무엇을 접는지는 여기서 정하지 않는다.</b> UI가 페이더 몇 개로 이루어져 있는지는
/// HudService만 알고, 이쪽은 "접어라 / 펴라"만 말한다. 묶음이 늘어도 이 파일은 그대로다.
///
/// 컷신·연출·보스 등장처럼 화면을 비워야 할 때 트리거의 UnityEvent에 물리면 된다.
/// </summary>
public class UiVisibility : MonoBehaviour
{
    /// <summary>UI를 편다.</summary>
    public void FadeIn() => SetVisible(true);

    /// <summary>UI를 접는다.</summary>
    public void FadeOut() => SetVisible(false);

    /// <summary>
    /// 한 메서드로 켜고 끈다. UnityEvent에 bool 인자로 물릴 때 쓴다.
    /// 같은 상태로 다시 불러도 안전하다 — 페이더가 이미 그 상태면 아무것도 하지 않는다.
    /// </summary>
    public void SetVisible(bool visible)
    {
        HudService service = HudService.Instance;

        if (service == null)
        {
            // UI 씬을 띄우지 않고 스테이지만 열어 레벨을 다듬는 경우가 있다.
            // 접을 UI가 없는 것뿐이므로 경고만 남기고 지나간다.
            Debug.LogWarning($"[{name}] HudService가 없어 UI를 여닫을 수 없습니다. UI 씬이 올라와 있어야 합니다.", this);
            return;
        }

        service.SetHudVisible(visible);
    }
}
