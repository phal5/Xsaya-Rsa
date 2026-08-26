using UnityEngine;

/// <summary>
/// 가사 상태. 쓰러져 있지만 <b>죽지는 않았다</b>.
///
/// <see cref="Character_Down"/>과 자세가 같고 하는 일이 다르다. 그쪽은 시계를 재어 스스로
/// 부활로 넘어가는 <b>죽음</b>이고, 이쪽은 아무것도 세지 않고 <b>기다리는</b> 상태다.
/// 연출이 부르지 않는 한 나가지 않는다 — 시간이 지난다고 일어나지도, 부활하지도 않는다.
///
/// 체력을 깎지 않는다. 최상위 머신은 체력 하나로 사망을 판정하므로, 여기서 체력을 건드리면
/// 다음 갱신에 곧바로 진짜 죽음으로 끌려간다. 가사와 죽음이 갈리는 지점이 그것이다.
///
/// 무적으로 둔다. 연출이 도는 동안 스쳐 지나가는 피격이나 함정이 이야기를 끊으면 안 된다.
/// 그래서 여기서 <b>진짜로 죽이려면</b> 무적부터 풀어야 한다 — <see cref="CharacterCue.Kill"/>이 그렇게 한다.
/// </summary>
public class Character_Faint : BaseCharacterState
{
    public override void Enter()
    {
        base.Enter();

        // <b>Play가 아니라 Snap이다.</b> 연출은 시간을 멈춰두고 부르는 경우가 흔한데,
        // Play는 애니메이터가 다음에 돌 때 반영되는 부탁이라 그 "다음"이 오지 않으면
        // 직전 자세가 그대로 선 채로 남는다.
        characterManager.Animation.Snap(DownState);

        // 조종만 잃는다. 쓰러지기 직전 속도는 그대로 흘러야 한다.
        characterManager.Steering.Coast();

        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = true;
    }

    public override void FixedUpdateState()
    {
        characterManager.Steering.Coast();
    }

    public override void Exit()
    {
        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = false;
    }

    /// <summary>스스로 나가지 않는다. 깨우는 것도 죽이는 것도 밖에서 부른다.</summary>
    public override void Transitions() { }

    /// <summary>쓰러진 자세의 컨트롤러 상태 이름. <see cref="Character_Down"/>과 같은 것을 쓴다.</summary>
    const string DownState = "Down";
}
