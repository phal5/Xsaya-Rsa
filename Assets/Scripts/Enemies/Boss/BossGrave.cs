using UnityEngine;

/// <summary>
/// 이미 쓰러진 보스라면 무대가 올라오는 순간 스스로 물러난다. <b>보스 루트에 붙인다.</b>
///
/// 판정을 보스 쪽에 두는 이유는, 보스가 어떻게 씬에 들어왔는지 아는 쪽이 따로 없기 때문이다.
/// 배경 씬에 처음부터 놓여 있든 나중에 누가 세우든, 깨어나는 자리는 언제나 자기 Awake다 —
/// <see cref="Stage"/>가 스스로 등록하는 것과 같은 사정이다.
/// </summary>
[RequireComponent(typeof(BossManager))]
[DisallowMultipleComponent]
public class BossGrave : MonoBehaviour
{
    void Awake()
    {
        BossManager boss = GetComponent<BossManager>();

        if (!BossGraveyard.Fallen(boss.bossId)) return;

        // <b>먼저 비활성화하고 나서 파괴한다.</b> Destroy는 프레임 끝까지 미뤄지므로
        // 그것만으로는 이 프레임 안에서 사라지지 않는다 — 그 사이 FSM의 Start가 돌아
        // 죽었어야 할 보스가 한 번 깨어나고, 체력바까지 떴다가 사라진다.
        // 비활성화는 부르는 즉시 먹으므로 그 창을 닫는 것은 이쪽이다.
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
