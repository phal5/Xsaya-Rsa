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

        // 티켓을 쓴 뒤라야 자세를 건다. 위에서 거부되면 아무 자세도 바꾸지 않고 돌아간다.
        characterManager.Animation.Play(characterManager.DashState, characterManager.DashBlend);

        // 지나온 길을 남긴다. 먼저 비우는 이유는, 앞선 회피의 꼬리가 아직 남아 있으면
        // 그 끝에서 여기까지를 한 줄로 이어 그어 버리기 때문이다.
        if (characterManager.DashTrail != null)
        {
            characterManager.DashTrail.Clear();
            characterManager.DashTrail.emitting = true;
        }

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

        // 뿌리기만 멈춘다. 이미 남긴 자리는 걷지 않는다 — 트레일 수명대로 스스로 잦아드는 것이 곧 연출이다.
        if (characterManager.DashTrail != null) characterManager.DashTrail.emitting = false;

        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = false;
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (fsm is Character_Airborne airborne)
        {
            // <b>점프 클립을 돌려주고 나간다.</b> 공중 축은 클립이 남의 것으로 갈려 있으면 손대지 않고,
            // Fall·Move는 애니메이션을 걸지 않는다 — 여기서 돌려놓지 않으면 떨어지는 내내 대시 자세로 굳는다.
            // 끝까지 채운 경우에만 한다. 도중에 끊기면(착지·피격) 다음 주인이 제 자세를 건다.
            airborne.BeginJumpAnimation();
            airborne.ToLocomotion();
        }
    }
}
