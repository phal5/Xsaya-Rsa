using UnityEngine;

/// <summary>
/// 씬 전환. <see cref="SceneDirector"/>가 커튼을 덮고 배경을 갈아끼우는 동안 머문다.
///
/// <b>맞지도 죽지도 않는다.</b> 이 동안 몸은 떠나온 자리에 붙들려 있어, 거기 남은 것(낙사 볼륨, 적의 공격)이
/// 되살린 몸을 다시 죽이거나 경직으로 끌어내 전환 도중 조작이 튀어나왔다. 무적이면 피해가 들어가지 않으므로
/// 피격 표시도 사망도 생기지 않는다.
///
/// 대사(<see cref="Character_UI"/>)와 따로 둔 이유가 이것이다 — 대사 중에는 맞고 죽을 수 있어야 하지만(보스 연출)
/// 전환 중에는 아니다.
///
/// 그림과 몸을 멈춘다. 대사를 기다릴 때(UI_Idle)와 같은 방식이다.
/// 전환이 몸을 새 자리에 놓으면(Pin) 멈추기 전에 적어 둔 속도는 버려진다.
///
/// 스스로 나가지 않는다. 부활이면 커튼 아래서 Character_Rest로, 관문이면 전환이 끝나며 조작으로 넘어간다.
/// </summary>
public class Character_Transit : BaseCharacterState
{
    public override void Enter()
    {
        base.Enter();

        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = true;

        characterManager.Animation.Freeze(true);
        characterManager.Movement.Freeze(true);
    }

    public override void Exit()
    {
        characterManager.Movement.Freeze(false);
        characterManager.Animation.Freeze(false);

        if (characterManager.Damagable != null) characterManager.Damagable.Invulnerable = false;
    }

    public override void Transitions() { }
}
