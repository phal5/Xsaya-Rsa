using UnityEngine;

/// <summary>
/// 대사 한 권을 들고 있다가 열어주는 자리. <b>여는 입구가 하나뿐이라 발동 방식과 무관하다.</b>
///
/// 밟아서 열려면 <see cref="CollisionInvoke"/>의 onCollision에,
/// 말을 걸어 열려면 <see cref="InteractableEvent"/>의 onInteract에 <see cref="Open"/>을 물리면 된다.
/// <see cref="Gateway"/>가 Enter() 하나로 두 길을 받는 것과 같은 모양이다.
///
/// <b>FlipBook은 인스펙터로 꽂을 수 없다.</b> UI 씬에 있어 씬을 넘는 참조가 되는데,
/// 그건 유니티가 저장하지 못한다 — 꽂아둔 것처럼 보여도 로드되면 비어 있다.
/// 그래서 싱글턴으로 런타임에 찾는다. 인터랙션 탐지가 물리로 대상을 찾는 것과 같은 이유다.
/// </summary>
public class BookHolder : MonoBehaviour
{
    [Tooltip("열었을 때 넘길 대사. 페이지와 페이지별 이벤트를 여기서 쓴다.")]
    [SerializeField] Book _book = new();

    public void Open()
    {
        if (FlipBook.Instance == null)
        {
            Debug.LogError($"[{name}] FlipBook이 없어 대사를 열 수 없습니다. UI 씬이 올라와 있어야 합니다.", this);
            return;
        }

        FlipBook.Instance.SetBook(_book);
    }
}
