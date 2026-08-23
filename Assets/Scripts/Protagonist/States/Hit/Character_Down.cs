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

        // 옮기기가 이 프레임에 끝났을 때만 조작을 돌려준다.
        // 쉬어간 자리가 다른 스테이지면 씬이 올라오는 동안 director가 붙들고 있다가 직접 푼다.
        if (!Respawn()) return;

        // 이제 부모는 최상위다. 피격을 거쳐 들어왔든 낙사로 들어왔든 여기로 돌아간다.
        if (fsm is CharacterRoot root) root.ToControl();
    }

    /// <summary>
    /// 체력을 되돌리고 마지막으로 쉬어간 자리로 옮긴다.
    ///
    /// 옮기는 일 자체는 <see cref="SceneDirector"/>가 한다 — 쉬어간 자리가 다른 스테이지면
    /// 그 씬을 먼저 불러와야 하는데, 여기서 그것까지 알 필요는 없다. 같은 스테이지면 자리만 옮긴다.
    ///
    /// <b>체력을 먼저 되돌린다.</b> 최상위 머신은 매 갱신마다 체력으로 사망을 판정하므로,
    /// 되돌리기 전에 씬을 부르면 그 사이 매 갱신마다 이 상태로 도로 끌려온다.
    /// </summary>
    /// <returns>조작을 이 자리에서 돌려줘도 되는지. 거짓이면 director가 돌려준다.</returns>
    bool Respawn()
    {
        if (characterManager.Damagable != null) characterManager.Damagable.Revive();

        if (SceneDirector.instance == null)
        {
            Debug.LogError("[Character_Down] SceneDirector가 없어 쉬어간 자리로 돌아갈 수 없습니다.");
            return true;
        }

        return SceneDirector.instance.Respawn();
    }
}
