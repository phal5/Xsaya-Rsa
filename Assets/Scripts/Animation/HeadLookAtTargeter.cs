using UnityEngine;

/// <summary>
/// <see cref="AutoTarget"/>이 잡고 있는 가장 가까운 적을 바라본다.
/// 적이 사거리를 벗어나면 조준이 풀리고 고개가 애니메이션으로 돌아간다.
/// </summary>
public class HeadLookAtTargeter : HeadLookAtBase
{
    protected override Transform Target
    {
        get
        {
            AutoTarget targeter = PlayerManager.instance != null ? PlayerManager.instance.targeter : null;
            return targeter != null ? targeter._targetTransform : null;
        }
    }
}
