using UnityEngine;

/// <summary>피격 경직. 잠시 조작을 잃었다가 복귀한다.</summary>
public class Character_Stun : BaseCharacterState
{
    float _endTime;

    public override void Enter()
    {
        _endTime = Time.time + characterManager.StunTime;

        // 속도는 상태 밖에 남으므로 직접 세운다.
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void FixedUpdateState()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
        Transitions();
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (fsm is Character_Hit hit) hit.Complete();
    }
}
