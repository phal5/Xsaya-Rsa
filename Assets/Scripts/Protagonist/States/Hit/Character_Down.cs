using UnityEngine;

/// <summary>
/// 다운. 체력이 0이 되면 들어오고, RespawnDelay 뒤에 부활해 조작으로 돌아간다.
///
/// DamagableBase의 Destroy On Death를 꺼두어야 오브젝트가 살아남아 이 상태가 성립한다.
/// 다운 동안에는 추가 피격을 받지 않는다.
/// </summary>
public class Character_Down : BaseCharacterState
{
    float _reviveTime;

    public override void Enter()
    {
        _reviveTime = Time.time + characterManager.RespawnDelay;

        characterManager.Steering.Move(Vector3.zero, Vector3.up);

        // 쓰러져 있는 동안 계속 맞아 피격 상태가 헛돌지 않게 한다.
        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = true;
    }

    public override void FixedUpdateState()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
        Transitions();
    }

    public override void Exit()
    {
        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = false;
    }

    public override void Transitions()
    {
        if (Time.time < _reviveTime) return;

        Respawn();

        if (fsm is Character_Hit hit) hit.Complete();
    }

    void Respawn()
    {
        if (characterManager.Damagable != null) characterManager.Damagable.Revive();

        Transform point = characterManager.RespawnPoint;
        if (point == null) return;

        // 부활 지점이 지정돼 있으면 옮긴다. 남은 속도도 함께 지운다.
        characterManager.Rigidbody.position = point.position;
        characterManager.Rigidbody.linearVelocity = Vector3.zero;
    }
}
