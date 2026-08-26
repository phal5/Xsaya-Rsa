using UnityEngine;

/// <summary>
/// 연출이 주인공에게 거는 신호. UnityEvent에 그대로 물린다.
///
/// <b>CharacterRoot를 인스펙터로 꽂을 수 없어서 있는 컴포넌트다.</b> 부르는 쪽은 배경 씬이나
/// UI 프리팹에 있고 주인공은 캐릭터 씬에 있어, 씬을 넘는 참조가 되어 유니티가 저장하지 못한다.
/// <see cref="ControlLock"/>이 조작 잠금을 같은 이유로 중계하는 것과 같은 자리다.
///
/// 부르는 쪽이 있는 곳에 붙이고 필요한 함수를 물리면 된다.
/// </summary>
public class CharacterCue : MonoBehaviour
{
    /// <summary>
    /// 쓰러뜨린다. <b>죽이지는 않는다</b> — 체력은 그대로고, 깨우거나 죽이는 것은 다음 신호의 몫이다.
    /// </summary>
    public void Faint()
    {
        CharacterRoot root = Root();
        if (root == null) return;

        root.ToFaint();
    }

    /// <summary>
    /// 죽인다. <b>상태를 직접 바꾸지 않고 체력을 비운다.</b>
    ///
    /// 최상위 머신이 매 갱신마다 체력 하나로 사망을 판정하도록 되어 있고, 그 규칙 밖에
    /// "죽여라"는 문을 하나 더 내면 두 길이 갈린다 — 낙사·함정이 지나는 길과 다른 길이 생기고,
    /// 죽은 뒤에도 그 문이 열려 있어 아무나 사망을 취소할 수 있게 된다.
    /// 그래서 낙사(KillVolume)와 같은 방식으로 체력만 비운다.
    ///
    /// 가사 상태는 무적이므로 그것부터 푼다. 안 그러면 피해가 통째로 흘려져 아무 일도 일어나지 않는다.
    /// </summary>
    public void Kill()
    {
        DamagableBase damagable = Damagable();
        if (damagable == null) return;

        damagable.Invulnerable = false;
        damagable.TakeDamage(Mathf.Infinity);
    }

    static CharacterRoot Root()
    {
        if (PlayerManager.instance == null) return null;

        CharacterRoot root = PlayerManager.instance.Root;
        if (root == null)
        {
            Debug.LogWarning("[CharacterCue] PlayerManager.Root가 비어 있어 주인공에게 신호를 보내지 못했습니다. " +
                             "캐릭터 씬의 매니저에 CharacterRoot를 꽂아야 합니다.");
            return null;
        }

        return root;
    }

    static DamagableBase Damagable()
    {
        if (PlayerManager.instance == null) return null;

        DamagableBase damagable = PlayerManager.instance.playerDamagable;
        if (damagable == null)
        {
            Debug.LogWarning("[CharacterCue] PlayerManager.playerDamagable이 비어 있어 주인공을 죽이지 못했습니다.");
            return null;
        }

        return damagable;
    }
}
