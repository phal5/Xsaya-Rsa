using UnityEngine;

/// <summary>
/// 보스 체력바가 그릴 값. <b>보스도 UI도 서로를 모른다</b> — 양쪽이 이 애셋 하나만 본다.
///
/// 스크립터블 오브젝트인 이유는 그것이 <b>씬이 아니라 파일</b>이기 때문이다.
/// 씬을 넘는 참조는 유니티가 저장하지 못하지만 애셋 참조는 어디서든 저장된다.
/// 지금은 보스와 바가 같은 스테이지 씬에 있어 그 문제가 없지만, 나중에 상시 UI로 옮기더라도
/// 배선만 바뀌고 코드는 그대로다.
///
/// 스테이지마다 하나씩 둔다. 그래야 두 보스가 같이 나와도 서로의 값을 덮지 않는다.
/// </summary>
[CreateAssetMenu(menuName = "Xsaya/Boss Health VM", fileName = "BossHealth")]
public class BossHealthVM : ScriptableObject
{
    /// <summary>
    /// 이 셋은 <b>저장하지 않는다.</b> 애셋은 판이 끝나도 파일로 남으므로,
    /// 저장했다가는 죽기 직전의 체력이 파일에 적혀 다음 판이 거기서 시작한다.
    /// </summary>
    [System.NonSerialized] string _displayName = "";
    [System.NonSerialized] float _normalized;
    [System.NonSerialized] bool _visible;

    public string DisplayName => _displayName;

    /// <summary>0~1. 바가 그대로 fillAmount에 넣는다.</summary>
    public float Normalized => _normalized;

    public bool Visible => _visible;

    /// <summary>값이 바뀔 때마다. 바는 이것만 듣는다.</summary>
    public event System.Action Changed;

    public void Show(string displayName, float normalized)
    {
        _displayName = displayName;
        _normalized = Mathf.Clamp01(normalized);
        _visible = true;
        Changed?.Invoke();
    }

    public void SetHealth(float normalized)
    {
        _normalized = Mathf.Clamp01(normalized);
        Changed?.Invoke();
    }

    public void Hide()
    {
        _visible = false;
        Changed?.Invoke();
    }

    /// <summary>
    /// 판을 시작할 때 씻어낸다.
    ///
    /// NonSerialized만으로 충분해 보이지만, 도메인 리로드를 끄고 플레이하면 관리 객체가 그대로 살아남아
    /// 앞 판의 값이 이어진다. TimeManager가 static 필드에 대해 같은 이유로 같은 일을 한다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetAll()
    {
        foreach (BossHealthVM vm in Resources.FindObjectsOfTypeAll<BossHealthVM>())
        {
            vm._displayName = "";
            vm._normalized = 0f;
            vm._visible = false;
        }
    }
}
