using UnityEngine;

/// <summary>
/// 주인공을 바라본다. 씬을 넘는 참조를 피해 <see cref="PlayerManager.instance"/>에서 매번 받아온다.
/// </summary>
public class HeadLookAtPlayer : HeadLookAtBase
{
    protected override Transform Target =>
        PlayerManager.instance != null ? PlayerManager.instance.player : null;
}
