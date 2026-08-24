using UnityEngine;

/// <summary>
/// 지역의 <b>표시 이름</b>을 모아둔 목록. 로딩 화면이 여기서 이름을 얻는다.
///
/// 이름을 도착할 씬에 두지 못하는 이유는 <b>시점</b>이다 — 로딩 화면은 로드가 시작되기 전에
/// 이미 그려져 있어야 하는데, 그 씬은 아직 올라오지 않았다. 반대로 관문마다 적어두면
/// 같은 지역으로 가는 문 수만큼 이름이 복제되고, 부활에는 관문이 아예 없어 물어볼 데가 없다.
///
/// 에셋은 씬이 아니라 언제든 읽히므로 그 창을 비껴간다. 그리고 지역 하나에 이름 하나만 남는다.
///
/// 씬 이름을 그대로 쓰지 않는 것은 그것이 <b>식별자</b>이기 때문이다. 파일 사정에 묶여 있어
/// 띄어쓰기도 대소문자도 마음대로 못 바꾸고, 현지화할 자리도 없다.
/// </summary>
[CreateAssetMenu(fileName = "StageDirectory", menuName = "Scene Management/Stage Directory")]
public class StageDirectory : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
#if UNITY_EDITOR
        [Tooltip("이 항목이 가리키는 씬.")]
        [SerializeField] UnityEditor.SceneAsset _sceneAsset;
#endif

        // 씬 에셋은 빌드에 남지 않는다. 이름을 뽑아 적어두고 감춘다.
        [HideInInspector]
        [SerializeField] string _scene;

        [Tooltip("화면에 띄울 지역 이름.")]
        [SerializeField] string _displayName;

        public string Scene => _scene;
        public string DisplayName => _displayName;

#if UNITY_EDITOR
        internal void Sync()
        {
            if (_sceneAsset != null) _scene = _sceneAsset.name;
        }
#endif
    }

    [SerializeField] Entry[] _stages = new Entry[0];

    /// <summary>
    /// 씬 이름으로 표시 이름을 찾는다.
    ///
    /// 목록에 없으면 <b>씬 이름을 그대로 돌려주고 경고를 남긴다.</b> 빈 문자열을 돌려주면
    /// 로딩 화면이 아무 이름 없이 떠서, 빠뜨린 것인지 원래 그런 것인지 화면으로는 구별되지 않는다.
    /// </summary>
    public string NameOf(string scene)
    {
        if (string.IsNullOrEmpty(scene)) return string.Empty;

        foreach (Entry entry in _stages)
        {
            if (entry == null || entry.Scene != scene) continue;

            return string.IsNullOrEmpty(entry.DisplayName) ? scene : entry.DisplayName;
        }

        Debug.LogWarning($"[{name}] '{scene}'의 표시 이름이 목록에 없습니다. 씬 이름을 그대로 씁니다.", this);
        return scene;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        foreach (Entry entry in _stages) entry?.Sync();
    }
#endif
}
