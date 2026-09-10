using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 시작 기준으로 몸이 플레이어를 따라보는 구간. 프레임으로 적는다.
/// 여러 개 늘어놓으면 따라보기를 켜고 끄기를 되풀이한다.
///
/// 끝이 시작보다 크지 않으면 스킬이 끝날 때까지다 — 클립 구간의 끝 프레임과 같은 규약이다.
/// 그래서 (0,0)은 전 구간이다.
/// </summary>
[System.Serializable]
public struct AimWindow
{
    [Tooltip("스킬 시작(Enter) 기준 프레임. 이 프레임부터 몸이 플레이어를 따라본다.")]
    [Min(0)] public int start;

    [Tooltip("이 프레임에 따라보기를 멈춘다. 시작보다 크지 않으면 스킬 끝까지다 — (0,0)은 전 구간.")]
    [Min(0)] public int end;

    public bool Contains(int frame) => frame >= start && (end <= start || frame < end);
}

/// <summary>
/// 스킬 한 개. <b>클래스를 새로 쓰지 않고</b> 이 부품을 붙여 값만 채우면 새 스킬이 된다.
///
/// 채울 것은 다섯 가지다.
///   Profile        준비 · 발동 · 후딜 길이와 위력 · 사거리 · 쿨다운, 그리고 애니메이터 상태 이름
///   Condition      쿨다운·사거리 말고 더 걸 조건 (ISkillCondition). 비우면 조건 없음
///   Clip + Window  쓸 클립과 그 안의 시작 · 종료 프레임 · 블렌드 시간
///   Hit Windows    무기 판정을 켜고 끌 프레임 구간들. 여러 개 넣으면 연타가 된다
///   Poise          이 스킬이 어디서부터 끊기지 않는지
///
/// 여기 남은 유일한 로직은 조준이다. 그것도 프레임 구간으로 두었다 —
/// 스킬마다 다른 것은 "언제 어디를 보느냐"뿐이었기 때문이다.
///
/// <b>조준은 몸의 일이다.</b> 구간 안에서는 리지드바디가 플레이어를 따라보고, 밖에서는 그 자리를 지킨다.
/// 클립의 회전(회전 베기 같은)은 <see cref="RootMotionRelay"/>가 그 몸 <b>안에서</b> 루트에 따로 얹는다.
/// 둘은 다른 Transform에 쓰므로 싸우지 않고, 보이는 방향은 조준 × 클립 회전이 된다.
/// 몸의 조준은 돌아가는 속도에 한계가 없어, 느려지는 구간에서도 도는 동안 플레이어를 놓치지 않는다.
///
/// 도약처럼 궤적을 스크립트가 쥐어야 하는 스킬은 여전히 <see cref="Boss_SkillBase"/>를
/// 직접 상속한다. 값으로 표현되지 않는 움직임까지 여기에 욱여넣지 않는다.
/// </summary>
public class Boss_Skill : Boss_SkillBase
{
    [Header("Aiming - 몸이 언제 플레이어를 볼지")]
    [Tooltip("스킬 시작 기준으로 몸이 플레이어를 따라보는 프레임 구간들. 구간 밖에서는 그 자리를 지킨다.\n" +
             "기본값 (0,0)은 전 구간이다. 비워두면 한 번도 따라보지 않는다.\n" +
             "눈금은 Timeline의 Frame Rate다. 선딜에 따라보면 피하기 어려워지고, 회전 중에 따라보면 도는 내내 붙잡힌다.")]
    [SerializeField] List<AimWindow> _aimWindows = new List<AimWindow> { new AimWindow() };

    [Tooltip("발동 순간 그 자리에 못 박는다. 늦게 피하면 피할 수 있게 된다.")]
    [SerializeField] bool _stopOnActivate = true;

    protected override void OnWindup() => Aim();

    public override void UpdateState()
    {
        // 판단을 먼저 한다. base가 스킬을 끝내 다음 상태로 넘긴 뒤에 대상을 비우면,
        // Exit이 돌려놓은 조준을 여기서 도로 지워버린다.
        Aim();

        base.UpdateState();
    }

    protected override void OnActivate()
    {
        if (_stopOnActivate) manager.Stop();
    }

    public override void Exit()
    {
        base.Exit();

        // 따라보기를 돌려놓는다. 다른 상태들은 조준을 스스로 세우지 않고 늘 걸려 있다고 여긴다 —
        // 접근은 처음 한 번만 걸고, 후퇴·회피·방어·경직은 아예 걸지 않는다.
        // 구간 밖에서 끝난 채 비워두면 그 뒤로 보스가 영영 플레이어를 보지 않는다.
        manager.LookTowards(manager.Player);
    }

    /// <summary>
    /// 구간 안이면 따라보고, 밖이면 그 자리를 지킨다.
    /// 대상을 비우면 이동기가 "볼 방향이 없으면 회전에 손대지 않는다"로 빠진다 — 멈추는 방법은 이미 거기 있다.
    /// </summary>
    void Aim()
    {
        manager.LookTowards(Tracking() ? manager.Player : null);
    }

    bool Tracking()
    {
        if (_aimWindows == null) return false;

        int frame = Frame;

        foreach (AimWindow window in _aimWindows)
            if (window.Contains(frame)) return true;

        return false;
    }
}
