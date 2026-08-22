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
/// 같은 IDamageable을 쓰므로 적도 떨어지면 죽는다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KillVolume : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 트리거가 아니면 알린다.
    ///
    /// <see cref="Reset"/>은 컴포넌트를 <b>처음 붙일 때만</b> 돈다. 그 뒤에 인스펙터에서 꺼버리면
    /// 죽이는 자리가 조용히 딛고 설 수 있는 상자가 되는데, 아무것도 알려주지 않으므로
    /// 플레이해 보기 전까지는 알 길이 없다. 켜주지는 않는다 — 사람이 끈 것을 되돌리면 그것대로 당황스럽다.
    /// </summary>
    void OnValidate()
    {
        Collider volume = GetComponent<Collider>();

        if (volume != null && !volume.isTrigger)
            Debug.LogWarning($"[KillVolume] '{name}'의 콜라이더가 트리거가 아니다. 이대로는 아무도 죽지 않는다.", this);
    }
#endif

    void OnTriggerEnter(Collider other) => Kill(other);

    /// <summary>
    /// 머무는 동안 계속 묻는다.
    ///
    /// 들어오는 순간에만 물으면, 그 한 번이 무적에 걸려 흘렀을 때 <b>그 볼륨은 그 대상에게
    /// 영원히 무해해진다</b> — 겹침이 시작될 때만 오는 신호라 안에 있는 내내 다시 오지 않는다.
    /// 맞고 밀려 구덩이로 떨어지면 다운 무적이 켜진 채로 경계를 지나므로, 살아서 무한히 떨어졌다.
    ///
    /// 매 스텝 묻는 것으로 얇은 함정을 굴러 넘는 연출도 그대로 남는다.
    /// 회피가 짧아 무적이 풀리기 전에 볼륨을 빠져나가면 취약한 순간이 아예 없기 때문이다.
    /// 반대로 굴러서는 못 건널 만큼 넓으면 안에서 무적이 풀려 죽는데, 그것이 바라는 바다.
    /// </summary>
    void OnTriggerStay(Collider other) => Kill(other);

    void Kill(Collider other)
    {
        if (other.TryGetComponent(out IDamageable target))
            target.TakeDamage(float.PositiveInfinity);
    }
}
