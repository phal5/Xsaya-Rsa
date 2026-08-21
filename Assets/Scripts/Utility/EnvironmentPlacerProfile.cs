using UnityEngine;

/// <summary>
/// 환경 오브젝트 절차적 배치를 위한 프로파일 설정.
/// Assets > Create > Xsaya > Environment Placer Profile 에서 생성 가능.
/// </summary>
[CreateAssetMenu(fileName = "NewEnvironmentProfile", menuName = "Xsaya/Environment Placer Profile")]
public class EnvironmentPlacerProfile : ScriptableObject
{
    [Header("Prefab Entries")]
    [Tooltip("배치할 프리팹 목록. 가중치가 높을수록 더 자주 선택됩니다.")]
    [SerializeField] EnvironmentEntry[] _entries;

    [Header("Placement")]
    [Tooltip("오브젝트 간 최소 간격 (월드 단위).")]
    [SerializeField, Min(0.1f)] float _minimumSpacing = 1f;

    [Tooltip("랜덤한 위치에 스폰할지 여부. 비활성화 시 동일한 Z좌표 상에 일정한 간격으로 X 방향으로 배치됩니다.")]
    [SerializeField] bool _isRandomPlacement = true;

    [Tooltip("배치 시도 밀도. 높을수록 빈 공간 없이 촘촘하게 채웁니다. (1~50 권장)")]
    [SerializeField, Range(1, 50)] int _samplesBeforeRejection = 15;

    [Tooltip("블록 표면으로부터의 Y 오프셋.")]
    [SerializeField] float _yOffset = 0f;

    [Tooltip("배치된 면의 방향을 따라 오브젝트를 눕히거나 세웁니다. 끄면 어느 면에 붙든 똑바로 선 자세를 유지합니다. 기본값은 끔.")]
    [SerializeField] bool _alignToSurface = false;

    [Header("Rendering")]

    [Tooltip("이 프로파일로 배치되는 모든 오브젝트에 GPU 인스턴싱을 적용합니다.\n끄면 엔트리별 체크박스를 각각 따릅니다.\n머티리얼은 공유 에셋이므로 배치물 밖의 오브젝트에도 함께 적용됩니다.")]
    [SerializeField] bool _gpuInstancing = false;

    [Header("Randomization")]
    [Tooltip("활성화하면 매번 다른 시드를 사용합니다.")]
    [SerializeField] bool _useRandomSeed = false;

    [Tooltip("고정 시드 값. 동일한 시드는 동일한 배치를 보장합니다.")]
    [SerializeField] int _seed = 42;

    #region Public Accessors

    public EnvironmentEntry[] Entries => _entries;
    public float MinimumSpacing => _minimumSpacing;
    public bool IsRandomPlacement => _isRandomPlacement;
    public int SamplesBeforeRejection => _samplesBeforeRejection;
    public float YOffset => _yOffset;
    public bool AlignToSurface => _alignToSurface;
    public bool UseRandomSeed => _useRandomSeed;
    public int Seed => _seed;
    public bool GpuInstancing => _gpuInstancing;

    #endregion

    /// <summary>
    /// 전체 가중치 합계 대비 확률로 프리팹을 선택합니다.
    /// </summary>
    public GameObject PickRandomPrefab(System.Random rng)
    {
        if (_entries == null || _entries.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var entry in _entries)
        {
            totalWeight += Mathf.Max(entry.Weight, 0f);
        }

        if (totalWeight <= 0f) return null;

        float roll = (float)rng.NextDouble() * totalWeight;
        float cumulative = 0f;

        foreach (var entry in _entries)
        {
            cumulative += Mathf.Max(entry.Weight, 0f);
            if (roll <= cumulative && entry.Prefab != null)
            {
                return entry.Prefab;
            }
        }

        // Fallback: 마지막 유효한 엔트리 반환
        for (int i = _entries.Length - 1; i >= 0; i--)
        {
            if (_entries[i].Prefab != null) return _entries[i].Prefab;
        }

        return null;
    }

    /// <summary>
    /// 엔트리 중 하나라도 GPU 인스턴싱을 요구하는지. 배치 툴이 설정 충돌 경고에 씁니다.
    /// </summary>
    public bool AnyGpuInstancing
    {
        get
        {
            if (_entries == null) return false;
            if (_gpuInstancing) return true;

            foreach (var entry in _entries)
            {
                if (entry.GpuInstancing) return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 이 엔트리를 GPU 인스턴싱할지 결정합니다.
    /// 프로파일 전체 설정이 켜져 있으면 엔트리 개별 설정과 무관하게 적용됩니다.
    /// </summary>
    public bool UsesGpuInstancing(EnvironmentEntry entry)
    {
        return _gpuInstancing || entry.GpuInstancing;
    }

    /// <summary>
    /// 해당 프리팹에 대응하는 EnvironmentEntry를 반환합니다.
    /// </summary>
    public EnvironmentEntry GetEntryForPrefab(GameObject prefab)
    {
        if (_entries == null) return default;

        foreach (var entry in _entries)
        {
            if (entry.Prefab == prefab) return entry;
        }

        return default;
    }
}

/// <summary>
/// 개별 환경 오브젝트 엔트리. 프리팹, 가중치, 스케일/회전 랜덤 범위를 정의합니다.
/// </summary>
[System.Serializable]
public struct EnvironmentEntry
{
    [Tooltip("배치할 프리팹.")]
    public GameObject Prefab;

    [Tooltip("선택 가중치. 높을수록 자주 등장합니다.")]
    [Min(0f)] public float Weight;

    [Tooltip("최소 균일 스케일.")]
    [Min(0.01f)] public float MinScale;

    [Tooltip("최대 균일 스케일.")]
    [Min(0.01f)] public float MaxScale;

    [Tooltip("Y축 랜덤 회전 여부.")]
    public bool RandomYRotation;

    [Tooltip("이 프리팹이 쓰는 머티리얼에 GPU 인스턴싱을 켠다.\n머티리얼은 공유 에셋이므로, 같은 머티리얼을 쓰는 배치물 밖의 오브젝트에도 함께 적용된다.\nBatching Static이 걸린 렌더러는 정적 배칭이 우선하므로 인스턴싱을 타지 않는다.")]
    public bool GpuInstancing;
}
