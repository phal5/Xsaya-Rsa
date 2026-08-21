using UnityEngine;

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
/// 여기 남은 유일한 로직은 조준이다. 그것도 켜고 끄는 값으로 두었다 —
/// 스킬마다 다른 것은 "언제 어디를 보느냐"뿐이었기 때문이다.
///
/// 도약처럼 궤적을 스크립트가 쥐어야 하는 스킬은 여전히 <see cref="Boss_SkillBase"/>를
/// 직접 상속한다. 값으로 표현되지 않는 움직임까지 여기에 욱여넣지 않는다.
/// </summary>
public class Boss_Skill : Boss_SkillBase
{
    [Header("Aiming - 언제 어디를 볼지")]
    [Tooltip("준비 구간에 들어갈 때 플레이어를 향해 돈다.")]
    [SerializeField] bool _aimOnWindup = true;

    [Tooltip("준비 구간 내내 플레이어를 따라본다. 끄면 진입 시 한 번만 조준한다.")]
    [SerializeField] bool _trackDuringWindup;

    [Tooltip("발동 순간 그 자리에 못 박는다. 늦게 피하면 피할 수 있게 된다.")]
    [SerializeField] bool _stopOnActivate = true;

    protected override void OnWindup()
    {
        if (_aimOnWindup) manager.LookTowards(manager.Player);
    }

    public override void UpdateState()
    {
        base.UpdateState();

        // 선딜이 길수록 읽히지만, 그동안 따라보면 피하기가 어려워진다.
        if (_trackDuringWindup && phase == Phase.Windup) manager.LookTowards(manager.Player);
    }

    protected override void OnActivate()
    {
        if (_stopOnActivate) manager.Stop();
    }
}
