/// <summary>
/// 체력이 절반 이하일 때. 2페이즈 진입용이자 조건 스크립트의 본보기.
///
/// 새 조건이 필요하면 이 파일을 따라 만든다.
/// <b>파일명과 클래스명이 같아야</b> 인스펙터의 Condition Script 칸에 꽂을 수 있다.
/// </summary>
public class HealthBelowHalf : ISkillCondition
{
    public bool Met(BossManager boss)
    {
        if (boss == null || boss.health == null) return false;
        if (boss.health.MaxHealth <= 0f) return false;

        return boss.health.CurrentHealth / boss.health.MaxHealth <= 0.5f;
    }
}
