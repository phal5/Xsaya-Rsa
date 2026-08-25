using UnityEngine;

/// <summary>
/// 쉬어가는 자리. 말을 걸면 플레이어가 다음에 여기서 일어난다.
///
/// 적어두는 것은 <b>말을 건 순간 플레이어가 서 있던 자리가 아니라 이 오브젝트의 자리</b>다.
/// <see cref="_offset"/>을 이 오브젝트 기준으로 더해 스폰 지점을 잡는다 — 말을 건 각도·위치에
/// 따라 스폰 지점이 흔들리지 않고, 제단 위든 바닥이든 저작자가 저작 시점에 눈으로 맞출 수 있다.
///
/// 씬 이름을 SceneManager.GetActiveScene()으로 묻지 않는다. 씬을 겹쳐 올리는 구성에서는
/// 활성 씬이 무엇인지가 로드 순서에 딸려 흔들린다. 체크포인트는 자기가 놓인 씬에 있으므로
/// gameObject.scene이 물어볼 것도 없이 맞다.
///
/// 적는 곳은 <see cref="SceneDirector"/>다. 캐릭터가 아니라 거기인 이유는, 다른 스테이지에 적힌
/// 좌표는 그 씬을 부를 수 있는 쪽에서만 뜻이 있기 때문이다.
///
/// 표시·문구·이벤트는 <see cref="InteractableEvent"/>가 이미 하는 일이라 그대로 물려받는다.
/// 여기서 더하는 것은 자리를 적는 한 줄뿐이다.
///
/// <b>단발 입력으로는 발동하지 않는다.</b> <see cref="Interact"/>는 비워두고,
/// 실제 발동은 <see cref="Activate"/>가 맡는다 — <see cref="InteractHold"/>가 붙들린 시간을
/// 재고 다 차면 그것을 부른다. move_interact 액션 하나를 게임의 모든 상호작용이 공유하므로,
/// 그 액션 자체에 Hold를 걸면 체크포인트만이 아니라 전부가 그렇게 된다. 그래서 여기서 갈랐다.
/// </summary>
public class Checkpoint : InteractableEvent
{
    [Tooltip("스폰 지점. 이 오브젝트의 <b>로컬</b> 좌표계 기준 오프셋이다 — 오브젝트를 돌리거나 옮기면 함께 따라온다.")]
    [SerializeField] Vector3 _offset = Vector3.zero;

    [Tooltip("일어나서 볼 <b>방향</b>. 회전각이 아니라 방향 벡터다. 말을 건 각도와 무관하게 항상 이쪽을 본다.")]
    [SerializeField] Vector3 _facing = new Vector3(1f, 0f, 0f);

    [Tooltip("여기서 일어날 때 누운 <b>그림</b>을 몸에서 떼어 놓을 거리. 위의 스폰 오프셋과 다르다 — " +
             "그쪽은 몸이 설 자리고, 이쪽은 그 자리에 선 몸에서 그림만 떼어 놓는 거리다. " +
             "몸 기준이라 위의 방향과 함께 돈다.")]
    [SerializeField] Vector3 _riseOffset = new Vector3(-0.5f, 0.8f, 1f);

    /// <summary>
    /// 단발 입력은 아무것도 하지 않는다. <see cref="Character_Interact"/>가 여전히 이걸 부르지만
    /// (그 상태는 건드리지 않는다), 붙든 시간을 재는 건 <see cref="InteractHold"/>의 몫이다.
    /// </summary>
    public override void Interact(CharacterManager character) { }

    /// <summary>
    /// 실제 발동. <see cref="InteractHold"/>가 다 붙든 순간 부른다.
    /// </summary>
    public void Activate()
    {
        if (!Available) return;

        // 이벤트보다 먼저 적는다. 이벤트가 씬을 부르거나 대화를 여는 경우,
        // 그 뒤에 적으면 적기도 전에 이 오브젝트가 사라져 있을 수 있다.
        if (SceneDirector.instance != null)
            SceneDirector.instance.SetCheckpoint(gameObject.scene.name, transform.TransformPoint(_offset), Facing, _riseOffset);
        else
            Debug.LogError($"[{name}] SceneDirector가 없어 쉬어간 자리를 적지 못했습니다.", this);

        // character를 쓰지 않는다 — base.Interact()도 받기만 하고 참조하지 않는다.
        base.Interact(null);
    }

    /// <summary>
    /// 방향 벡터를 회전으로 바꾼다. 0 벡터는 LookRotation이 받지 못하므로 정면으로 둔다.
    /// <see cref="Gateway"/>·<see cref="SceneDirector"/>의 초기 지점과 같은 규칙이다.
    /// </summary>
    Quaternion Facing => _facing.sqrMagnitude < 0.0001f
        ? Quaternion.identity
        : Quaternion.LookRotation(_facing.normalized);

#if UNITY_EDITOR
    /// <summary>스폰 지점을 씬 뷰에 그린다. 선택하지 않아도 보이도록 항상 그린다 — 배치할 때 바닥과 맞춰봐야 한다.</summary>
    void OnDrawGizmos()
    {
        Vector3 spawn = transform.TransformPoint(_offset);

        Gizmos.color = new Color(0.3f, 0.9f, 0.4f);
        Gizmos.DrawWireSphere(spawn, 0.3f);
        Gizmos.DrawLine(transform.position, spawn);
        Gizmos.DrawRay(spawn, Facing * Vector3.forward * 0.5f);
    }
#endif
}
