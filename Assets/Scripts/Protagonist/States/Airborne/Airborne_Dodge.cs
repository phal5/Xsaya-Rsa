using UnityEngine;

/// <summary>
/// 공중 회피. 티켓을 한 장 소모하고 입력 방향으로 대시한다.
/// 지상 회피와 마찬가지로 지속 동안 피격 판정을 받지 않는다.
/// </summary>
public class Airborne_Dodge : BaseCharacterState
{
    float _endTime;
    Vector3 _direction;

    public override void Enter()
    {
        base.Enter();

        Character_Airborne airborne = fsm as Character_Airborne;
        if (airborne == null || !airborne.ConsumeDodgeTicket())
        {
            _endTime = 0f;   // 티켓이 없으면 즉시 빠져나간다
            return;
        }

        _endTime = Time.time + characterManager.DashTime;

        Vector3 input = InputManager.CharacterMove;
        _direction = input.sqrMagnitude > 0.01f ? input.normalized : characterManager.Rigidbody.transform.forward;

        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = true;
    }

    public override void FixedUpdateState()
    {
        characterManager.Steering.Move(_direction * characterManager.DashSpeed, Vector3.up);
        Transitions();
    }

    public override void Exit()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);

        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = false;
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (fsm is Character_Airborne airborne) airborne.ToLocomotion();
    }
}
