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

        Vector3 input = InputManager.CharacterMove;
        _direction = input.sqrMagnitude > 0.01f ? input.normalized : characterManager.Rigidbody.transform.forward;

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
