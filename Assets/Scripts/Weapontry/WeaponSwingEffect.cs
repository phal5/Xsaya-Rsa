using UnityEngine;

/// <summary>
/// 판정이 열리는 순간 <see cref="WeaponEffectSpawner"/>를 부른다.
///
/// 실시간 궤적(TrailRenderer)을 걷어내고 <b>미리 만들어진 반원</b>으로 갈아탄 자리다.
/// 트레일은 프레임당 점을 하나만 남기므로 빠른 스윙에서는 대여섯 점짜리 각진 띠가 됐다.
/// 완성된 호를 한 장 세우면 프레임레이트와 무관하게 매끈하다 —
/// 대신 <b>실제 스윙 경로를 따라가지 않는다.</b> 팔이 어떻게 움직이든 정해진 호가 뜬다.
///
/// <see cref="MeleeWeapon"/>은 주인공·보스·Agnostos가 함께 쓰므로 건드리지 않는다.
/// 여기서는 그 무기가 여닫는 콜라이더를 밖에서 지켜볼 뿐이다.
///
/// 타마다 다른 것을 뿌려야 하면 이 컴포넌트를 빼고 스킬 코드에서
/// <see cref="WeaponEffectSpawner.Play(GameObject)"/>를 직접 부르는 쪽이 낫다.
/// 여기는 "휘두르면 뜬다" 하나만 맡는다.
/// </summary>
public class WeaponSwingEffect : MonoBehaviour
{
    [Tooltip("MeleeWeapon이 여닫는 그 콜라이더. 열리는 순간마다 한 번씩 세운다.")]
    [SerializeField] Collider attackWindow;

    [Tooltip("실제로 이펙트를 세우는 쪽. 비워두면 같은 오브젝트에서 찾는다.")]
    [SerializeField] WeaponEffectSpawner spawner;

    [Tooltip("끄면 휘둘러도 아무것도 뜨지 않는다. 실행 중에는 EffectEnabled로 바꾼다.")]
    [SerializeField] bool effectEnabled = true;

    bool wasOpen;

    /// <summary>연출을 켜고 끄는 손잡이. 이미 떠 있는 것은 제 수명대로 사라진다.</summary>
    public bool EffectEnabled
    {
        get => effectEnabled;
        set => effectEnabled = value;
    }

    void Awake()
    {
        if (spawner == null) spawner = GetComponent<WeaponEffectSpawner>();
    }

    /// <summary>
    /// 지금 상태를 그대로 받아두고 시작한다. 닫힘으로 단정하면 마침 열려 있던 프레임을
    /// <b>방금 휘두른 것</b>으로 읽어, 가만히 선 채로 베기가 한 번 뜬다.
    /// </summary>
    void OnEnable() => wasOpen = attackWindow != null && attackWindow.enabled;

    /// <summary>
    /// Update가 아니라 여기서 본다. 판정을 여는 쪽이 애니메이션 이벤트일 수 있는데,
    /// 그것은 Update <b>다음</b>에 돌아서 매번 한 프레임씩 늦게 뜬다.
    /// </summary>
    void LateUpdate()
    {
        if (attackWindow == null) return;

        bool open = attackWindow.enabled;

        // 보스처럼 한 스킬 안에서 판정을 여러 번 여닫는 경우, 열릴 때마다 한 장씩 뜬다.
        if (open && !wasOpen && effectEnabled && spawner != null) spawner.Play();

        wasOpen = open;
    }

    /// <summary>붙이는 순간 같은 무기의 콜라이더와 스포너를 찾아 꽂아둔다.</summary>
    void Reset()
    {
        spawner = GetComponent<WeaponEffectSpawner>();

        MeleeWeapon weapon = GetComponentInParent<MeleeWeapon>();
        if (weapon != null) attackWindow = weapon.GetComponent<Collider>();
    }

#if UNITY_EDITOR
    /// <summary>
    /// 비어 있으면 알린다. 이대로는 <b>한 번도</b> 뜨지 않는데, 오류도 없이 조용해서
    /// 이펙트가 안 붙은 것인지 세팅이 빈 것인지 구분할 길이 없다.
    /// </summary>
    void OnValidate()
    {
        if (attackWindow == null)
            Debug.LogWarning($"[WeaponSwingEffect] '{name}'에 지켜볼 콜라이더가 없다. 이대로는 베기가 뜨지 않는다.", this);
    }
#endif
}
