using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 남은 회복 횟수를 칸으로 보여준다. 칸 하나가 회복 한 번이다.
///
/// <b>칸은 인스펙터에서 authoring한다.</b> 런타임에 만들지 않는 이유는, 개수가 거의 바뀌지 않는데
/// 만들어 붙이는 코드를 두면 레이아웃을 눈으로 맞출 수 없게 되기 때문이다.
/// 칸 수와 <see cref="CharacterManager.HealChargeMax"/>가 어긋나면 한 번 경고한다.
///
/// <b>캐릭터는 런타임에 찾는다.</b> UI는 씬을 따로 쓰므로 인스펙터로 꽂을 수 없다 —
/// 씬을 넘는 참조는 유니티가 저장하지 못한다. 캐릭터 씬은 내려가지 않으므로 한 번 찾으면 들고 있는다.
///
/// 값이 바뀔 때만 다시 그린다. HPbar처럼 매 프레임 칠하지 않는 것은, 이쪽은 시간 배속 같은
/// 곁다리 계산이 없어 바뀐 순간만 반영하면 충분하기 때문이다.
/// </summary>
public class HealChargeBar : MonoBehaviour
{
    [Tooltip("칸마다 하나씩, 왼쪽부터 순서대로. 남은 횟수만큼 앞에서부터 켜진다. " +
             "프레임은 늘 보이고 이쪽만 켜졌다 꺼진다.")]
    [SerializeField] List<Image> _fills = new();

    CharacterManager _character;
    bool _warned;

    /// <summary>마지막으로 그린 값. -1은 아직 한 번도 안 그렸다는 뜻이다.</summary>
    int _shown = -1;

    void OnEnable()
    {
        // 다시 켜질 때는 반드시 한 번 그린다. 꺼져 있는 동안 값이 바뀌었을 수 있다.
        _shown = -1;
    }

    void Update()
    {
        CharacterManager character = Character;
        if (character == null) return;

        if (character.HealCharges == _shown) return;

        _shown = character.HealCharges;
        Render(_shown);
    }

    /// <summary>
    /// 주인공의 매니저. 없으면 조용히 지나간다 —
    /// UI 씬만 띄워놓고 화면을 다듬는 경우가 있기 때문이다.
    /// </summary>
    CharacterManager Character
    {
        get
        {
            if (_character != null) return _character;

            Transform player = PlayerManager.instance != null ? PlayerManager.instance.player : null;
            if (player == null) return null;

            // player가 몸일 수도, 그 위의 루트일 수도 있다. 루트에서 훑으면 어느 쪽이든 걸린다.
            _character = player.root.GetComponentInChildren<CharacterManager>(true);

            if (_character != null && !_warned && _character.HealChargeMax != _fills.Count)
            {
                _warned = true;
                Debug.LogWarning($"[{name}] 칸이 {_fills.Count}개인데 최대 회복 횟수는 " +
                                 $"{_character.HealChargeMax}입니다. 칸 수를 맞춰야 합니다.", this);
            }

            return _character;
        }
    }

    void Render(int charges)
    {
        for (int i = 0; i < _fills.Count; i++)
        {
            if (_fills[i] == null) continue;

            _fills[i].enabled = i < charges;
        }
    }
}
