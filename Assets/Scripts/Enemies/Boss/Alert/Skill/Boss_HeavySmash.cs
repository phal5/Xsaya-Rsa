/// <summary>
/// 선딜 동안 플레이어를 조준하는 타격. 준비가 긴 슬롯에 어울린다.
/// </summary>
public class Boss_HeavySmash : Boss_SkillBase
{
    protected override void OnWindup()
    {
        // 선딜이 긴 만큼 그동안 몸을 돌려둔다.
        manager.LookTowards(manager.Player);
    }
}
