using UnityEngine;

/// <summary>
/// 플레이어가 입력으로 말을 걸 수 있는 대상.
/// 근접해서 자동으로 발동하는 CollisionInvoke·BookTrigger와 달리 이쪽은 입력이 있어야 동작한다.
///
/// 탐지기가 최우선 후보를 고를 때마다 Focus/Unfocus를 보내므로,
/// 대상은 그걸로 "상호작용 가능" 표시를 띄우거나 내리면 된다.
/// </summary>
public interface IInteractable
{
    /// <summary>UI에 띄울 안내 문구. "살펴보기", "들어가기" 등.</summary>
    string Prompt { get; }

    /// <summary>지금 상호작용할 수 있는 상태인지. 이미 열린 문 등은 false를 돌려주면 된다.</summary>
    bool Available { get; }

    /// <summary>최우선 후보가 되었다. 표시를 띄운다.</summary>
    void Focus();

    /// <summary>더 이상 최우선 후보가 아니다. 표시를 내린다.</summary>
    void Unfocus();

    void Interact(CharacterManager character);
}
