using UnityEngine;

/// <summary>
/// UI가 조작을 가져갈 때와 돌려줄 때를 잇는 자리. FlipBook의
/// onSetBook -> <see cref="Lock"/>, onDialogueNull -> <see cref="Release"/>로 물린다.
///
/// <b>CharacterRoot를 인스펙터로 꽂을 수 없어서 있는 컴포넌트다.</b>
/// UI는 프리팹이라 씬 오브젝트를 참조하지 못하고, 설령 씬에 풀어놓아도
/// 캐릭터는 다른 씬에 있어 씬을 넘는 참조가 된다 — 유니티가 저장하지 못한다.
/// 그래서 PlayerManager를 거쳐 런타임에 찾는다.
///
/// 루트를 직접 들지 않는 이유이기도 하다. 캐릭터가 갈려도 매니저가 가리키는 곳만 바뀌면 된다.
/// </summary>
public class ControlLock : MonoBehaviour
{
    /// <summary>조작을 거둔다. 푸는 것은 이쪽이 아니라 <see cref="Release"/>가 한다.</summary>
    public void Lock()
    {
        CharacterRoot root = Root();
        if (root == null) return;

        root.ToUI();
    }

    public void Release()
    {
        CharacterRoot root = Root();
        if (root == null) return;

        root.ToControl();
    }

    /// <summary>
    /// 매니저가 없을 수 있다 — UI 씬만 띄워놓고 화면을 다듬는 경우가 그렇다.
    /// 그때는 잠글 대상도 없으므로 조용히 지나간다.
    /// </summary>
    static CharacterRoot Root()
    {
        if (PlayerManager.instance == null) return null;

        CharacterRoot root = PlayerManager.instance.Root;
        if (root == null)
        {
            Debug.LogWarning("[ControlLock] PlayerManager.Root가 비어 있어 조작을 잠글 수 없습니다. " +
                             "캐릭터 씬의 매니저에 CharacterRoot를 꽂아야 합니다.");
            return null;
        }

        return root;
    }
}
