using UnityEngine;

/// <summary>
/// 쓰러진 보스가 몸을 일으킨다. <b>연출용 흡수 상태</b>다 — 한 번 들어오면 나가지 않는다.
///
/// 일어난 뒤에 싸우지 않는다. 그래서 <see cref="BossManager.isDead"/>도 체력도 명부도 건드리지 않는다.
/// 되살아나는 것이 아니라 <b>일어서는 그림</b>만 필요한 자리이기 때문이다.
///
/// 새 클립을 쓰지 않는다. 쓰러지는 클립을 거꾸로 훑으면 그것이 곧 일어나는 동작이고,
/// 컨트롤러에 상태를 하나 더 만들 필요도 없다. 훑는 속도는
/// <see cref="BossManager.riseTime"/>이 정한다 — 느리게 둘수록 무겁게 일어선다.
/// </summary>
public class Boss_Rise : BaseEntityState<BossManager>
{
    float _elapsed;

    public override void Enter()
    {
        _elapsed = 0f;

        // 일어나는 동안 발이 미끄러지지 않게 붙들어 둔다.
        manager.Stop();
    }

    public override void UpdateState()
    {
        _elapsed += Time.deltaTime;

        float t = manager.riseTime <= 0f ? 1f : Mathf.Clamp01(_elapsed / manager.riseTime);

        // 1에서 0으로. 클립의 끝(쓰러진 자세)에서 시작해 처음(선 자세)으로 되감는다.
        //
        // 트리거를 걸고 배속을 음수로 두는 방법도 있지만 그러지 않았다. 배속은 애니메이터 전체의
        // 것이라 다른 레이어까지 거꾸로 돌고, 어디서 멈출지를 이쪽이 알 수 없다.
        // 프레임을 직접 지정하면 시작도 끝도 여기서 정해진다.
        if (manager.animator != null && !string.IsNullOrEmpty(manager.deathTrigger))
            manager.animator.Play(manager.deathTrigger, 0, 1f - t);
    }

    /// <summary>나가지 않는다. 이 상태에 들어온 보스는 선 채로 남는다.</summary>
    public override void Transitions() { }
}
