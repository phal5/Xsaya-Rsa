using UnityEngine;

/// <summary>
/// 사망(다운). 체력이 0이 되면 들어오고, RespawnDelay 뒤에 마지막 체크포인트에서 일어나 조작으로 돌아간다.
///
/// <b>CharacterRoot 직속</b>이다. 피격을 거쳐 들어오는 것이 보통이지만 피격에 속하지 않으므로,
/// 낙사나 함정처럼 때린 주체가 없는 죽음도 <see cref="CharacterRoot.ToDown"/> 하나로 들어온다.
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
        characterManager.Animation.Play("Down");

        // 조종만 잃는다. 쓰러지기 직전 속도는 그대로 흘러야 한다.
        characterManager.Steering.Coast();

        // 쓰러져 있는 동안 계속 맞아 피격 상태가 헛돌지 않게 한다.
        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = true;
    }

    public override void FixedUpdateState()
    {
        characterManager.Steering.Coast();
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

        // 이제 부모는 최상위다. 피격을 거쳐 들어왔든 낙사로 들어왔든 여기로 돌아간다.
        if (fsm is CharacterRoot root) root.ToControl();
    }

    /// <summary>
    /// 체력을 되돌리고 마지막으로 쉬어간 자리로 옮긴다.
    ///
    /// <b>Pin으로 옮긴다.</b> 몸의 position만 대입하면 속도를 쥔 외력 몸이 제자리에 남아,
    /// 다음 물리 프레임에 그쪽이 몸을 도로 끌고 간다. 두 몸을 함께 놓는 길은 Pin 하나뿐이다.
    ///
    /// 적어둔 자리가 없거나 그 씬이 지금 올라와 있지 않으면 옮기지 않는다 —
    /// 아직 한 번도 쉬어가지 않았다는 뜻이므로, 옮길 곳이 없는 것이지 그 자리가 옳은 것은 아니다.
    /// </summary>
    void Respawn()
    {
        if (characterManager.Damagable != null) characterManager.Damagable.Revive();

        if (!characterManager.TryCheckpoint(out Vector3 place, out Quaternion facing)) return;

        characterManager.Movement.Pin(place);
        characterManager.Body.rotation = facing;
    }
}
