using UnityEngine;

/// <summary>
/// 부르면 무기 자리에 이펙트를 하나 세운다.
///
/// 휘두르는 <b>동안</b>이 아니라 <b>순간</b>을 맡는다.
/// 타격·발도·차징처럼 "지금"이 정해져 있는 연출은 부르는 쪽이 그 시점을 이미 알고 있으므로,
/// 여기서 상태를 되읽지 않는다 — 읽기 시작하면 스킬이 늘어날 때마다 조건이 하나씩 붙는다.
///
/// <see cref="MeleeWeapon"/>은 주인공·보스·Agnostos가 함께 쓰는 판정 코드라 건드리지 않는다.
/// 스킬 코드에서 <see cref="Play()"/>를 부르거나, 애니메이션 이벤트에 걸어둔다.
/// </summary>
public class WeaponEffectSpawner : MonoBehaviour
{
    [Tooltip("세울 이펙트 프리팹. Play에 직접 넘겨 이 자리를 대신할 수도 있다.")]
    [SerializeField] GameObject effect;

    [Tooltip("세울 자리. 비워두면 이 오브젝트 자리에 선다.")]
    [SerializeField] Transform spawnPoint;

    [Tooltip("켜면 무기의 자식이 되어 따라 움직인다. 터진 자리에 남아야 하는 임팩트는 꺼둔다.")]
    [SerializeField] bool followWeapon;

    [Tooltip("몇 초 뒤 치울지. 0이면 파티클 길이를 재서 알아서 정한다.")]
    [SerializeField] float lifetime;

    [Tooltip("끄면 불러도 아무것도 서지 않는다. 실행 중에는 EffectEnabled로 바꾼다.")]
    [SerializeField] bool effectEnabled = true;

    /// <summary>
    /// 연출을 켜고 끄는 손잡이. 꺼두면 <see cref="Play()"/>가 조용히 지나간다.
    ///
    /// 이미 서 있는 것까지 걷어가지는 않는다. 터지는 중인 이펙트가 도중에 사라지는 것은
    /// 꺼진 것이 아니라 <b>고장 난 것</b>으로 보인다.
    /// </summary>
    public bool EffectEnabled
    {
        get => effectEnabled;
        set => effectEnabled = value;
    }

    /// <summary>인스펙터에 꽂아둔 이펙트를 세운다. 애니메이션 이벤트에서 바로 부를 수 있다.</summary>
    public void Play() => Play(effect);

    /// <summary>
    /// 이번만 다른 이펙트를 세운다. 연속 베기의 1타·2타·3타처럼 <b>같은 무기가 타마다 다른 것을
    /// 뿌려야 할 때</b> 쓴다. 이것을 위해 스포너를 여러 개 붙이면 어느 것이 켜져 있는지 알기 어려워진다.
    /// </summary>
    public void Play(GameObject effectOverride) => Spawn(effectOverride, null);

    /// <summary>
    /// 무기 자리가 아니라 <b>맞은 자리</b>에 세운다. 때린 쪽이 부른다.
    ///
    /// 맞은 쪽에서 부르지 않는 이유는 그쪽이 무엇에 맞았는지 모르기 때문이다.
    /// IDamageable은 숫자 하나만 받으므로 방향도 무기도 알 수 없고,
    /// KillVolume도 같은 길로 들어와 구덩이에 떨어진 것이 베인 것으로 보이게 된다.
    /// </summary>
    public void PlayAt(Vector3 position) => Spawn(effect, position);

    /// <summary>
    /// 자리와 각도를 <b>모두</b> 받아 세운다. 무기가 있는 곳과 이펙트가 서야 할 곳이 다를 때 쓴다.
    ///
    /// 베기 궤적이 그렇다 — 그것은 칼이 지나간 <b>끝</b>에 서야 하는데, 뿌리는 시점의 칼은
    /// 아직 시작점에 있다. 각도도 마찬가지로 무기가 아니라 궤적이 정한다.
    /// </summary>
    public void PlayAt(Vector3 position, Quaternion rotation) => Spawn(effect, position, rotation);

    /// <summary>이번만 다른 이펙트를, 자리와 각도까지 정해 세운다.</summary>
    public void PlayAt(GameObject effectOverride, Vector3 position, Quaternion rotation)
        => Spawn(effectOverride, position, rotation);

    /// <summary>
    /// 맞은 콜라이더에서 무기와 가장 가까운 지점에 세운다.
    ///
    /// 트리거 충돌은 접점을 주지 않아서 — OnTriggerEnter가 넘기는 것은 상대 콜라이더뿐이다 —
    /// 여기서 가장 가까운 점으로 대신한다. 다만 상대가 <b>볼록하지 않은 MeshCollider</b>면
    /// Unity가 조용히 중심점을 돌려주므로, 그런 적에게는 몸통 한가운데에 뜬다.
    /// </summary>
    public void PlayAt(Collider target)
    {
        if (target == null) return;

        Transform where = spawnPoint != null ? spawnPoint : transform;

        PlayAt(target.ClosestPoint(where.position));
    }

    /// <summary>자리를 넘기지 않으면 무기 자리에 선다. 회전은 어느 쪽이든 무기를 따른다.</summary>
    void Spawn(GameObject prefab, Vector3? position, Quaternion? rotation = null)
    {
        if (!effectEnabled || prefab == null) return;

        Transform where = spawnPoint != null ? spawnPoint : transform;

        GameObject spawned = Instantiate(
            prefab,
            position ?? where.position,
            rotation ?? where.rotation,
            followWeapon ? where : null);

        Destroy(spawned, lifetime > 0f ? lifetime : LengthOf(spawned));

        _spawned = spawned;
    }

    /// <summary>마지막으로 세운 것. 그만 뿌리라고 이를 대상이다.</summary>
    GameObject _spawned;

    /// <summary>
    /// 뿌리던 것을 그만 뿌린다. <b>이미 나온 알갱이는 제 수명대로 사라진다</b> — 뚝 끊지 않는다.
    /// 오래 끄는 이펙트를 도중에 거둬야 하는 쪽이 쓴다.
    ///
    /// 치우는 일은 여기서 하지 않는다. <see cref="Spawn"/>이 걸어둔 Destroy가 그대로 맡으므로
    /// 방출만 멈추면 된다 — 여기서 또 지우면 같은 것을 두 곳에서 치우게 된다.
    ///
    /// 마지막 하나만 안다. 겹쳐 뿌리는 이펙트까지 거두려면 목록이 필요한데,
    /// 지금 이걸 쓰는 쪽은 한 번에 하나만 세운다.
    /// </summary>
    public void Stop()
    {
        if (_spawned == null) return;

        foreach (ParticleSystem part in _spawned.GetComponentsInChildren<ParticleSystem>(true))
            part.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        _spawned = null;
    }

    /// <summary>
    /// 다 뿌린 이펙트를 치운다.
    ///
    /// Cartoon FX 프리팹은 stopAction이 None이라 <b>다 뿌리고도 빈 오브젝트로 남는다.</b>
    /// 공격마다 하나씩 쌓이는데 보이는 것이 없으므로, 씬이 무거워질 때까지 아무도 눈치채지 못한다.
    ///
    /// 길이는 파티클에서 직접 잰다. 초를 적어두면 이펙트를 갈아끼울 때마다 같이 고쳐야 하고,
    /// 잊은 쪽은 꼬리가 잘리거나 빈 오브젝트가 오래 남는 것으로 조용히 드러난다.
    /// </summary>
    static float LengthOf(GameObject spawned)
    {
        float longest = 0f;

        foreach (ParticleSystem part in spawned.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = part.main;

            // 상수든 곡선이든 둘 중 값이 있는 쪽이 잡힌다.
            float life = Mathf.Max(main.startLifetime.constantMax, main.startLifetime.curveMultiplier);

            longest = Mathf.Max(longest, main.duration + life);
        }

        // 파티클이 하나도 없는 이펙트도 있다. 빛이나 데칼만 든 것은 여기서 길이를 알 수 없다.
        return longest > 0f ? longest : FallbackLifetime;
    }

    const float FallbackLifetime = 2f;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (lifetime < 0f) lifetime = 0f;
    }
#endif
}
