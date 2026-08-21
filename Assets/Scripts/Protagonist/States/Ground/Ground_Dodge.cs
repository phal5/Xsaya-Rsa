using UnityEngine;

/// <summary>
/// 지상 회피. 입력 방향(없으면 정면)으로 DashSpeed만큼 밀고 DashTime 동안 유지한다.
/// 끝나면 입력 유무에 따라 Idle이나 Run/Walk로 돌아간다.
/// </summary>
public class Ground_Dodge : BaseCharacterState
{
    float _endTime;
    Vector3 _direction;

    public override void Enter()
    {
        _endTime = Time.time + characterManager.DashTime;

        // 입력이 없으면 지금 보는 방향으로 구른다. 좌표계를 맞춰야 카메라 각도에 휘둘리지 않는다.
        Vector3 input = InputManager.CharacterMove;
        _direction = input.sqrMagnitude > 0.01f ? input.normalized : characterManager.FacingAsInput();

        // 대시 지속 동안은 궤도를 무시한다. 수직 속도도 지워 낭떠러지로 대시해도 떨어지지 않는다.
        characterManager.Movement.SetGravity(false);
        characterManager.Movement.SetSamplerYVelocity(0f);

        // 대시 중에는 피격 판정을 받지 않는다.
        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = true;
    }

    public override void FixedUpdateState()
    {
        characterManager.Steering.Move(_direction * characterManager.DashSpeed, Ground.GroundHit.normal);
        Transitions();
    }

    public override void Exit()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);

        // 중력은 반드시 되돌린다. 대시 중 축이 공중으로 넘어가도 축 머신의 Exit 전파가 여기까지 온다.
        characterManager.Movement.SetGravity(true);

        // 중간에 끊겼든 끝났든 무적은 반드시 해제한다.
        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = false;
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (Ground != null) Ground.ToLocomotion();
    }

    Character_Ground Ground => fsm as Character_Ground;
}
