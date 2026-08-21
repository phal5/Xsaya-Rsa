using UnityEngine;

/// <summary>피격 경직. 잠시 조작을 잃었다가 복귀한다.</summary>
public class Character_Stun : BaseCharacterState
{
    float _endTime;

    public override void Enter()
    {
        _endTime = Time.time + characterManager.StunTime;
        characterManager.Animation.Play("Stun");

        // 조종만 잃는다. 맞고 날아가던 속도는 그대로 흘러야 한다.
        // 나가는 상태가 Exit에서 남긴 "0까지 감속하라"를 첫 물리 프레임 전에 덮어쓴다.
        characterManager.Steering.Coast();
    }

    public override void FixedUpdateState()
    {
        characterManager.Steering.Coast();
        Transitions();
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (fsm is Character_Hit hit) hit.Complete();
    }
}
