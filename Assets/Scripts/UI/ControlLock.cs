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

    /// <summary>
    /// 조작을 돌려준다. <b>씬 전환이 도는 중이면 아무것도 하지 않는다.</b>
    ///
    /// 관문은 대사가 끝나길 기다리지 않고 곧바로 넘어가므로, 대사가 전환 도중에
    /// 스스로 끝에 닿는 경우가 흔하다 — 그때 여기서 조작을 돌려주면 캐릭터가 여전히
    /// 중력이 꺼진 채 붙들려 있거나 사망 연출 중인 상태에서 조작이 풀려버린다.
    /// 그 자리는 이미 <see cref="SceneDirector"/>가 맡고 있으므로, 전환이 끝나며
    /// 스스로 정할 최종 상태에 맡긴다.
    /// </summary>
    public void Release()
    {
        if (SceneDirector.instance != null && SceneDirector.instance.Busy) return;

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
