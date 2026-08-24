using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// 리그의 Multi-Aim Constraint로 고개를 돌리는 공통 뼈대. 무엇을 바라볼지만
/// <see cref="Target"/>에서 갈라지고, 조준점을 옮기고 웨이트를 여닫는 일은 여기서 한다.
///
/// <b>회전은 이 스크립트가 하지 않는다.</b> 리그 잡이 애니메이션 포즈 위에 조준 자세를
/// 웨이트만큼 섞으므로(웨이트 0이면 애니메이션 그대로), 캐릭터가 클립을 재생하는 중에도
/// 서로 싸우지 않는다. 여기서 transform.rotation을 직접 쓰면 그 합성이 깨진다.
///
/// 겨냥 대상은 <see cref="_aimTarget"/> 하나로 고정하고 그것을 옮긴다.
/// Constraint의 Source Object는 RigBuilder가 그래프를 지을 때 한 번만 바인딩되어
/// 런타임에 다른 Transform으로 갈아 끼울 수 없고, 애초에 플레이어는 Character 씬에 있어
/// 배경 씬의 적이 인스펙터로 직접 참조할 수도 없다. 프록시가 두 문제를 함께 푼다.
///
/// 리그 잡은 Update와 LateUpdate 사이의 애니메이션 단계에서 돌므로,
/// 조준점과 웨이트는 Update에서 맞춰야 이번 프레임에 반영된다.
/// </summary>
public abstract class HeadLookAtBase : MonoBehaviour
{
    [Tooltip("고개를 돌릴 Multi-Aim Constraint. 이 스크립트는 그 웨이트만 만진다.")]
    [SerializeField] private MultiAimConstraint _constraint;

    [Tooltip("위 Constraint의 Source Object로 꽂아 둔 빈 오브젝트. 매 프레임 여기를 대상 위치로 옮긴다. " +
             "애니메이션이 건드리는 본 계층 바깥에 두어야 한다.")]
    [SerializeField] private Transform _aimTarget;

    [Tooltip("대상 트랜스폼 기준 시선 높이 보정. 루트가 발밑에 있으면 얼굴 높이만큼 올린다.")]
    [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.5f, 0f);

    [Tooltip("대상이 있을 때 초당 웨이트 증가량. 빠르게 돌아보도록 하강보다 크게 잡는다.")]
    [SerializeField] private float _riseSpeed = 6f;

    [Tooltip("대상을 놓쳤을 때 초당 웨이트 감소량.")]
    [SerializeField] private float _fallSpeed = 2f;

    /// <summary>바라볼 대상. 없으면 null — 웨이트가 내려가 고개가 애니메이션으로 돌아간다.</summary>
    protected abstract Transform Target { get; }

    private void Update()
    {
        Transform target = Target;

        // 놓쳤을 때 조준점은 그 자리에 남긴다. 웨이트만 빠지므로 고개가
        // 마지막으로 보던 쪽에서 애니메이션 포즈로 풀린다 — 원점으로 튀지 않는다.
        if (target != null) _aimTarget.position = target.position + _targetOffset;

        float weight = target != null ? 1f : 0f;
        float speed = target != null ? _riseSpeed : _fallSpeed;
        _constraint.weight = Mathf.MoveTowards(_constraint.weight, weight, speed * Time.deltaTime);
    }
}
