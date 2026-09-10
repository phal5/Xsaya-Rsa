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

        // 한 장짜리 자세를 세운다. 섞는 시간을 따로 주는 이유는 DashBlend의 툴팁에 있다.
        // 되돌리는 것은 이 상태가 할 일이 아니다 — 끝나면 Idle·Move가, 발판을 벗어나면 공중 축이 제 자세를 건다.
        characterManager.Animation.Play(characterManager.DashState, characterManager.DashBlend);

        // 지나온 길을 남긴다. 먼저 비우는 이유는, 앞선 회피의 꼬리가 아직 남아 있으면
        // 그 끝에서 여기까지를 한 줄로 이어 그어 버리기 때문이다.
        if (characterManager.DashTrail != null)
        {
            characterManager.DashTrail.Clear();
            characterManager.DashTrail.emitting = true;
        }

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
        // 뿌리기만 멈춘다. 이미 남긴 자리는 걷지 않는다 — 트레일 수명대로 스스로 잦아드는 것이 곧 연출이다.
        if (characterManager.DashTrail != null) characterManager.DashTrail.emitting = false;

        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = false;
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (Ground != null) Ground.ToLocomotion();
    }

    Character_Ground Ground => fsm as Character_Ground;
}
