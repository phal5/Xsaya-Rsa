using UnityEngine;

/// <summary>
/// 쉬어가는 자리. 말을 걸면 플레이어가 다음에 여기서 일어난다.
///
/// 적어두는 것은 <b>이 오브젝트의 자리가 아니라 말을 건 순간 플레이어가 서 있던 자리</b>다.
/// 그 땅은 플레이어가 방금 딛고 있었으므로 정의상 유효하다 — 제단 위에 놓든 벽에 붙이든
/// 스폰 오프셋을 따로 맞출 일이 없고, 맞추다 틀릴 자리도 생기지 않는다.
///
/// 씬 이름을 SceneManager.GetActiveScene()으로 묻지 않는다. 씬을 겹쳐 올리는 구성에서는
/// 활성 씬이 무엇인지가 로드 순서에 딸려 흔들린다. 체크포인트는 자기가 놓인 씬에 있으므로
/// gameObject.scene이 물어볼 것도 없이 맞다.
///
/// 표시·문구·이벤트는 <see cref="InteractableEvent"/>가 이미 하는 일이라 그대로 물려받는다.
/// 여기서 더하는 것은 자리를 적는 한 줄뿐이다.
/// </summary>
public class Checkpoint : InteractableEvent
{
    public override void Interact(CharacterManager character)
    {
        if (!Available || character == null) return;

        // 이벤트보다 먼저 적는다. 이벤트가 씬을 부르거나 대화를 여는 경우,
        // 그 뒤에 적으면 적기도 전에 이 오브젝트가 사라져 있을 수 있다.
        character.SetCheckpoint(gameObject.scene.name);

        base.Interact(character);
    }
}
