using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보스 체력바. <see cref="BossHealthVM"/>만 보고 그린다 — 보스도 DamagableBase도 모른다.
///
/// <b>이 컴포넌트는 늘 켜져 있어야 한다.</b> 껐다 켜는 것은 <see cref="_root"/>다.
/// 자기 자신을 끄면 OnDisable에서 구독이 끊겨, 보스가 나타나도 다시 켜줄 사람이 없다.
///
/// 매 프레임 묻지 않는다. 값이 바뀔 때만 다시 그린다 —
/// 주인공 체력바(HPbar)는 Update마다 폴링하지만, 그쪽은 시간 배속까지 함께 계산하는 사정이 있다.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Tooltip("그릴 값. 보스 쪽 Presenter와 같은 애셋을 꽂는다.")]
    [SerializeField] BossHealthVM _vm;

    [Tooltip("켜고 끌 대상. 이 컴포넌트가 붙은 오브젝트가 아니어야 한다.")]
    [SerializeField] GameObject _root;

    [Tooltip("Image Type을 Filled로 두어야 fillAmount가 먹는다.")]
    [SerializeField] Image _fill;

    [Tooltip("보스 이름. 없으면 비워둔다.")]
    [SerializeField] TextMeshProUGUI _label;

    void OnEnable()
    {
        if (_vm == null)
        {
            Debug.LogWarning($"[{name}] VM이 비어 있어 체력바가 그려지지 않습니다.", this);
            return;
        }

        _vm.Changed += Render;

        // 구독만 하고 끝내면 안 된다. 보스가 이미 깨어난 뒤에 이 바가 켜지는 경우
        // (스테이지가 늦게 올라오거나 UI를 다시 켠 경우) 다음 피격까지 빈 바가 남는다.
        Render();
    }

    void OnDisable()
    {
        if (_vm != null) _vm.Changed -= Render;
    }

    void Render()
    {
        if (_root != null) _root.SetActive(_vm.Visible);

        if (_fill != null) _fill.fillAmount = _vm.Normalized;

        if (_label != null) _label.text = _vm.DisplayName;
    }
}
