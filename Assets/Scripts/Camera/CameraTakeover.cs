using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 부르면 화면을 <b>지정한 시네머신 카메라</b>에게 넘긴다. 되돌리는 것도 여기서 한다.
///
/// 넘기는 방법은 우선순위다. 카메라를 껐다 켜는 쪽이 더 단순해 보이지만, 그러면 브레인이
/// 블렌딩할 상대를 잃어 화면이 뚝 끊긴다. 우선순위는 브레인이 정해둔 블렌드를 그대로 타므로
/// 넘어가는 모습과 돌아오는 모습이 저작한 대로 나온다.
///
/// <b>이 컴포넌트는 넘겨받을 카메라 쪽에 붙이지 않아도 된다.</b> 부르는 쪽(연출·트리거·대사 이벤트)에
/// 붙이고 대상을 꽂는 편이 대개 낫다 — 한 카메라를 여러 곳에서 부르는 일이 흔하기 때문이다.
///
/// 씬을 넘는 참조는 담지 못하므로, 대상은 <b>같은 씬</b>에 있어야 한다.
/// 배경이 갈리면 그 카메라도 함께 사라지고 이 컴포넌트도 대개 같이 사라진다.
/// </summary>
public class CameraTakeover : MonoBehaviour
{
    [Tooltip("화면을 넘겨받을 카메라. 같은 씬에 있어야 한다.")]
    [SerializeField] CinemachineCamera _camera;

    [Tooltip("넘겨받는 동안 쓸 우선순위. 평소 카메라보다 높아야 한다 — 스테이지의 리그는 0으로 둔다.")]
    [SerializeField] int _priority = 100;

    [Tooltip("켜면 이 오브젝트가 켜질 때 스스로 넘겨받는다. 트리거 볼륨처럼 켜지는 것 자체가 신호일 때 쓴다.")]
    [SerializeField] bool _onEnable;

    /// <summary>넘겨받기 전의 우선순위. 돌려줄 때 그대로 되돌린다.</summary>
    PrioritySettings _before;
    bool _held;

    void OnEnable()
    {
        if (_onEnable) Show();
    }

    /// <summary>
    /// 화면을 이 카메라에게 넘긴다. 이미 넘겨받은 상태면 아무것도 하지 않는다 —
    /// 두 번 부르면 두 번째가 <b>이미 올려둔 값</b>을 원래 값으로 적어, 돌려줄 곳을 잃는다.
    /// </summary>
    public void Show()
    {
        if (_held || !Ready()) return;

        _held = true;
        _before = _camera.Priority;

        PrioritySettings priority = _camera.Priority;
        priority.Enabled = true;
        priority.Value = _priority;
        _camera.Priority = priority;
    }

    /// <summary>화면을 원래 카메라에게 돌려준다.</summary>
    public void Hide()
    {
        if (!_held || !Ready()) return;

        _held = false;
        _camera.Priority = _before;
    }

    /// <summary>UnityEvent에서 bool 하나로 오갈 때. 켜기와 끄기를 따로 물릴 필요가 없어진다.</summary>
    public void Toggle(bool show)
    {
        if (show) Show();
        else Hide();
    }

    /// <summary>
    /// 넘겨받은 채로 사라지지 않는다. 이 오브젝트가 꺼지거나 씬과 함께 내려갈 때,
    /// 올려둔 우선순위가 남으면 그 카메라가 영영 화면을 쥔다.
    /// </summary>
    void OnDisable()
    {
        Hide();
    }

    bool Ready()
    {
        if (_camera != null) return true;

        Debug.LogWarning($"[{name}] 넘겨줄 카메라가 꽂혀 있지 않습니다.", this);
        return false;
    }
}
