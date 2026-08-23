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
    /// 깨어나며 화면을 3D로 연다. <b>닫는 쪽은 <see cref="Boss_Dead"/>가 맡는다.</b>
    ///
    /// 여는 자리를 Boss_Alert에 두지 않았다. 그 상태는 경직에서 복귀할 때마다 다시 Enter되므로
    /// 거기서 열면 경직마다 다시 열리고, 짝을 맞추려 Exit에 닫기를 걸면 경직마다 2D로 되돌아간다.
    /// 이 상태는 한 번 떠나면 돌아오지 않아, 전투가 열리는 순간과 정확히 한 번 겹친다.
    /// </summary>
    void ToAlert()
    {
        if (!manager.HasPlayer) return;

        if (manager.DistanceToPlayer() <= manager.awakeRadius)
        {
            if (PlayerManager.instance != null) PlayerManager.instance.Set2D(false);

            fsm.TransitTo<Boss_Alert>();
        }
    }
}
