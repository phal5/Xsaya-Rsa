using UnityEngine;

/// <summary>
/// 자원 소모형 회복. 남은 횟수를 진입 시점에 소모하고, 동작을 <b>끝까지 마쳐야</b> 체력이 오른다.
/// 중간에 피격으로 끊기면 횟수만 날아간다. 회복을 거는 타이밍이 판단의 대상이 되게 하려는 것이다.
///
/// 땅 위에서만 걸 수 있고, 동작 중에는 느리게 움직인다.
/// </summary>
public class Character_Heal : BaseCharacterState
{
    float _endTime;
    bool _consumed;
    RaycastHit _hit;

    public override void Enter()
    {
        // 땅 위에서만 건다. 축 머신 밖이라 물어볼 축이 없으므로 지면 캐스터를 직접 본다.
        if (!Grounded())
        {
            Reject();
            return;
        }

        // 검사와 소모가 한 연산이라 여기 말고 다른 곳에서 미리 볼 이유가 없다.
        _consumed = characterManager.ConsumeHealCharge();

        if (!_consumed)
        {
            Reject();
            return;
        }

        _endTime = Time.time + characterManager.HealTime;
    }

    public override void FixedUpdateState()
    {
        // 제자리에 세우지 않는다. 느리게나마 움직일 수 있다.
        Vector3 input = InputManager.CharacterMove;
        characterManager.Steering.Move(input * characterManager.HealSpeed, _hit.normal == Vector3.zero ? Vector3.up : _hit.normal);

        Transitions();
    }

    public override void Exit()
    {
        // 끝까지 마쳤을 때만 회복시킨다. 끊겼으면 아무 일도 없다.
        if (_consumed && Time.time >= _endTime && characterManager.Damagable != null)
            characterManager.Damagable.Heal(characterManager.HealAmount);

        _consumed = false;

        // 여기서 목표 속도를 실었으므로 나갈 때 내려놓는다.
        // 피격으로 끊기면 다음 상태가 이 값을 물려받아 저절로 걸어간다.
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (fsm is Character_Controlled controlled) controlled.ToLocomotion();
    }

    bool Grounded()
    {
        return characterManager.GroundCaster != null && characterManager.GroundCaster.Cast(out _hit);
    }

    void Reject()
    {
        if (fsm is Character_Controlled controlled) controlled.ToLocomotion();
    }
}
