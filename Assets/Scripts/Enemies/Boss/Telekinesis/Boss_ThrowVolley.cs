using UnityEngine;

/// <summary>
/// 끌어올린 창을 하나씩 플레이어에게 던진다.
///
///   준비  바닥의 창이 떠올라 보스 주위에 고리를 이룬다. 캐스팅 자세로 읽힌다.
///   발동  발동 구간에 걸쳐 하나씩 나간다. 마지막 하나가 구간 끝에 나가도록 간격을 나눈다.
///   후딜  빈손으로 굳는다.
///
/// <b>예고는 여기서 하지 않는다.</b> 창이 스스로 한다 —
/// 던지기로 정해진 뒤 표적을 향해 돌아서고 뒤로 물러났다가 나가는 것은
/// <see cref="Boss_Throwable"/>의 Aiming 국면이 맡는다.
/// 그렇게 두면 어느 스킬이 던지든 예고가 따라오고, 공전 고리의 마무리 투척도 같은 예고를 갖는다.
/// 여기서 또 하면 두 지정이 매 프레임 서로를 덮어쓴다.
///
/// 조준은 <b>나가는 순간의 플레이어 자리</b>다. 미래 위치를 재지 않으므로,
/// 던지는 간격이 곧 피할 수 있는 창이 된다 — 개수와 발동 시간으로 난이도가 정해진다.
/// </summary>
public class Boss_ThrowVolley : Boss_Telekinesis
{
    [Header("Throw")]
    [Tooltip("던지는 속도(m/s). 0이면 창에 붙은 발사체의 기본 속도를 쓴다.\n" +
             "겨냥·예고·박힘 같은 규칙은 전부 창과 발사체가 들고 있다.")]
    [SerializeField, Min(0f)] float _throwSpeed = 18f;

    [Tooltip("준비 구간 내내 플레이어를 따라본다.")]
    [SerializeField] bool _trackDuringWindup = true;

    int _thrown;

    // 각 발이 나갈 시각. 발동에 들어올 때 한 번에 정한다.
    // 매 프레임 남은 비율로 되짚으면 프레임률에 따라 마지막 하나가 구간을 넘길 수 있다.
    float[] _launchAt;

    public override void Enter()
    {
        _thrown = 0;
        base.Enter();
    }

    protected override void OnWindup()
    {
        manager.LookTowards(manager.Player);
        SnapRing();
    }

    protected override void OnActivate()
    {
        manager.Stop();
        Schedule();
    }

    public override void UpdateState()
    {
        base.UpdateState();

        if (phase == Phase.Windup)
        {
            if (_trackDuringWindup) manager.LookTowards(manager.Player);

            // 준비 동안 고리가 천천히 돌면 떠 있는 게 눈에 들어온다.
            ringAngle += 60f * Time.deltaTime;

            // 바닥에서 딸려 오는 모습이 보여야 하므로 이 구간만 따라붙기.
            HoldRing(false);
            return;
        }

        // 그 뒤로는 고리를 정확한 반지름에 올려둔다.
        // 겨누는 중인 창은 스스로 자리를 잡으므로 여기 지정을 무시한다.
        HoldRing(true);

        if (phase == Phase.Active) ThrowOnSchedule();
    }

    #region Schedule - 언제 무엇이 나갈지 미리 정한다

    void Schedule()
    {
        _launchAt = new float[held.Count];

        float active = Mathf.Max(Profile.activeTime, 0f);
        float start = Time.time;

        for (int i = 0; i < held.Count; i++)
            _launchAt[i] = held.Count > 0 ? start + active * (i + 1f) / held.Count : start;
    }

    void ThrowOnSchedule()
    {
        if (_launchAt == null) return;

        while (_thrown < held.Count && Time.time >= _launchAt[_thrown]) ThrowNext();
    }

    void ThrowNext()
    {
        Boss_Throwable spear = held[_thrown];
        _thrown++;

        if (spear == null) return;

        // "누구를 향해, 얼마의 위력으로"만 알린다. 겨누고 나가는 것은 창이 정한다.
        spear.Throw(manager.Player, Profile.damage, _throwSpeed);
    }

    #endregion
}
