using UnityEngine;

/// <summary>
/// 들어오면 죽는 자리. 구덩이·용암·화면 밖에 붙인다.
///
/// <b>얼마나 깎을지 적어두지 않는다.</b> 체력이 얼마든 여기 들어온 것은 죽은 것이므로,
/// 수치를 두면 최대 체력이 바뀌거나 방어력이 붙는 순간 조용히 살아남는 구덩이가 된다.
///
/// 사망 판정을 여기서 하지 않는 이유는 CharacterRoot가 이미 체력 하나만 보기 때문이다.
/// 때린 주체가 없는 죽음도 같은 길로 들어가야 부활도 연출도 한 벌로 끝난다.
///
/// 같은 IDamageable을 쓰므로 적도 떨어지면 죽는다. 회피 무적 중에는 통하지 않는데,
/// 틈을 굴러 넘는 것이 그렇게 읽히는 편이 자연스러워 그대로 둔다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KillVolume : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable target))
            target.TakeDamage(float.PositiveInfinity);
    }
}
