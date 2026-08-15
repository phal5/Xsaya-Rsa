using UnityEngine;

/// <summary>
/// 자원 소모형 회복. 남은 횟수를 진입 시점에 소모하고, 동작을 <b>끝까지 마쳐야</b> 체력이 오른다.
/// 중간에 피격으로 끊기면 횟수만 날아간다. 회복을 거는 타이밍이 판단의 대상이 되게 하려는 것이다.
///
/// 동작 중에는 제자리에 선다.
/// </summary>
public class Character_Heal : BaseCharacterState
{
    float _endTime;
    bool _consumed;

    public override void Enter()
    {
        _consumed = characterManager.ConsumeHealCharge();

        if (!_consumed)
        {
            // 남은 횟수가 없으면 즉시 빠져나간다.
            _endTime = 0f;
            return;
        }

        _endTime = Time.time + characterManager.HealTime;
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void FixedUpdateState()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
        Transitions();
    }

    public override void Exit()
    {
        // 끝까지 마쳤을 때만 회복시킨다. 끊겼으면 아무 일도 없다.
        if (_consumed && Time.time >= _endTime && characterManager.Damagable != null)
            characterManager.Damagable.Heal(characterManager.HealAmount);

        _consumed = false;
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (fsm is Character_Controlled controlled) controlled.ToLocomotion();
    }
}
