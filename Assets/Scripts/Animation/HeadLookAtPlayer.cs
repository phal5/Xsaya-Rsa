using UnityEngine;

/// <summary>
/// 트랜스폼이 플레이어를 바라보도록 회전한다.
/// 애니메이션 평가(애니메이터) 이후에 회전이 덮어씌워지지 않도록 LateUpdate에서 LookAt을 수행한다.
/// </summary>
public class HeadLookAtPlayer : MonoBehaviour
{
    [Tooltip("바라볼 대상 트랜스폼. 비어있으면 PlayerManager.instance.player를 자동으로 참조한다.")]
    [SerializeField] private Transform _target;

    [Tooltip("대상 위치 기준 시선 높이/위치 보정값.")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1.5f, 0f);

    private void LateUpdate()
    {
        Transform target = _target != null ? _target : (PlayerManager.instance != null ? PlayerManager.instance.player : null);
        if (target == null) return;

        transform.LookAt(target.position + _offset);
    }
}
