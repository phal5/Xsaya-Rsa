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

        // 검사와 소모가 한 연산이라 여기 말고 다른 곳에서 미리 볼 이유가 없다.
        Character_Airborne airborne = fsm as Character_Airborne;
        if (airborne == null || !airborne.ConsumeDodgeTicket())
        {
            // 티켓이 없다. 공중 이동 축으로 돌려보낸다.
            if (airborne != null) airborne.ToLocomotion();
            return;
        }

        _endTime = Time.time + characterManager.DashTime;

        // 입력이 없으면 지금 보는 방향으로 구른다. 좌표계를 맞춰야 카메라 각도에 휘둘리지 않는다.
        Vector3 input = InputManager.CharacterMove;
        _direction = input.sqrMagnitude > 0.01f ? input.normalized : characterManager.FacingAsInput();

        // 대시 지속 동안은 궤도를 무시한다. 낙하 중에 걸어도 같은 거리를 수평으로 나간다.
        characterManager.Movement.SetGravity(false);
        characterManager.Movement.SetSamplerYVelocity(0f);

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

        // 티켓이 없어 거부됐을 때도 불리지만, 그땐 이미 켜져 있어 아무 일도 없다.
        characterManager.Movement.SetGravity(true);

        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = false;
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (fsm is Character_Airborne airborne) airborne.ToLocomotion();
    }
}
