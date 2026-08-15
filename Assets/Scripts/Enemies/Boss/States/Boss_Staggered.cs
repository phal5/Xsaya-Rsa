using UnityEngine;

/// <summary>
/// 피격 경직. 진행 중이던 행동을 거두고 잠시 굳는다.
/// 진입/이탈은 BossRoot가 관리한다.
/// </summary>
public class Boss_Staggered : BaseEntityState<BossManager>
{
    float _timer;

    public override void Enter()
    {
        _timer = manager.staggerTime;

        // 속도는 상태 밖(SingularFlatMovement)에 남으므로 직접 거둔다.
        // 히트박스도 스킬이 끊긴 경우를 대비해 한 번 더 내린다.
        manager.Stop();
        if (manager.weapon != null) manager.weapon.EndAttack();

        manager.PlayAnimation(manager.staggerTrigger);
    }

    public override void UpdateState()
    {
        _timer -= Time.deltaTime;
        Transitions();
    }

    public override void Exit()
    {
        // 경직에서 빠져나온 직후 다시 경직되지 않도록 간격을 둔다.
        manager.StartStaggerCooldown();
    }

    public override void Transitions()
    {
        if (_timer > 0f) return;

        if (fsm is Boss_DamageMachine machine) machine.Complete();
    }
}
