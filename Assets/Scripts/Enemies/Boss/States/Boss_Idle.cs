using UnityEngine;

/// <summary>
/// 깊은 잠. 플레이어가 awakeRadius 안에 들어오면 전투로 넘어간다.
/// 한 번 깨어난 뒤에는 여기로 돌아오지 않는다.
///
/// 전투가 열리는 유일한 지점이라, 화면을 3D로 여는 것도 여기서 한다.
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

    /// <summary>
    /// 깨어나며 싸울 화면으로 바꾼다. <b>되돌리는 쪽은 <see cref="Boss_Dead"/>가 맡는다.</b>
    ///
    /// 여는 자리를 Boss_Alert에 두지 않았다. 그 상태는 경직에서 복귀할 때마다 다시 Enter되므로
    /// 거기서 열면 경직마다 다시 열리고, 짝을 맞추려 Exit에 닫기를 걸면 경직마다 되돌아간다.
    /// 이 상태는 한 번 떠나면 돌아오지 않아, 전투가 열리는 순간과 정확히 한 번 겹친다.
    /// </summary>
    void ToAlert()
    {
        if (!manager.HasPlayer) return;
        if (manager.DistanceToPlayer() > manager.awakeRadius) return;

        Open();

        // 전투가 열렸다고 알린다. <b>무엇이 듣는지는 여기서 모른다</b> —
        // 체력바든 음악이든 인스펙터에서 꽂는다. 이 상태는 한 번 떠나면 돌아오지 않으므로
        // 이 알림도 판마다 정확히 한 번이다.
        manager.NotifyCombatStart();

        fsm.TransitTo<Boss_Alert>();
    }

    /// <summary>
    /// 싸울 화면으로 바꾸고, 들어오기 전이 어느 쪽이었는지 적어둔다.
    ///
    /// <b>어느 쪽으로 바꿀지는 보스가 정한다.</b> 예전에는 여기서 3D를 박아 두어,
    /// 2D로 싸워야 하는 보스도 깨어나는 순간 Z가 열렸다.
    /// </summary>
    void Open()
    {
        if (manager.combatView == BossCombatView.Keep) return;
        if (PlayerManager.instance == null) return;

        manager.viewBefore = PlayerManager.instance.Is2D;
        PlayerManager.instance.Set2D(manager.combatView == BossCombatView.Flat2D);
    }
}
