using UnityEngine;

/// <summary>
/// 깊은 잠. 플레이어가 awakeRadius 안에 들어오면 전투로 넘어간다.
/// 한 번 깨어난 뒤에는 여기로 돌아오지 않는다.
/// </summary>
public class Boss_Idle : BaseEntityState<BossManager>
{
    public override void Enter()
    {
        manager.Stop();
        manager.PlayAnimation(manager.idleTrigger);
    }

    public override void UpdateState()
    {
        Transitions();
    }

    public override void Transitions()
    {
        ToAlert();
    }

    void ToAlert()
    {
        if (!manager.HasPlayer) return;

        if (manager.DistanceToPlayer() <= manager.awakeRadius)
        {
            fsm.TransitTo<Boss_Alert>();
        }
    }
}
