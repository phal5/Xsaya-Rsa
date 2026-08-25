using UnityEngine;

/// <summary>
/// 끌어올린 물건들을 하나의 고리로 묶어 보스 주위를 공전시킨다.
///
///   준비  물건이 떠올라 고리를 짠다. 아직 아프지 않다 — 들어갈 틈을 읽을 시간이다.
///   발동  고리가 정해진 각속도로 돈다. 이 동안 스치면 아프다.
///   후딜  고리가 풀려 떨어진다.
///
/// 던지기와 달리 <b>보스가 움직여도 고리가 따라온다</b> — 자리를 매 스텝 보스 기준으로 다시 잡기
/// 때문이다. 그래서 이 스킬이 도는 동안 보스가 다가오면 고리도 같이 밀고 들어온다.
///
/// 마무리를 던지기로 돌릴 수도 있다. 그러면 공전이 곧 조준 시간이 된다.
/// </summary>
public class Boss_OrbitRing : Boss_Telekinesis
{
    [Header("Orbit")]
    [Tooltip("고리가 도는 각속도(도/초). 음수면 반대로 돈다.")]
    [SerializeField] float _angularSpeed = 160f;

    [Tooltip("준비 구간에서 고리가 벌어지는 동안의 각속도. 천천히 돌면 짜이는 게 보인다.")]
    [SerializeField] float _windupAngularSpeed = 50f;

    [Tooltip("고리가 위아래로 기울어 도는 각도. 0이면 수평으로 돈다.")]
    [SerializeField, Range(0f, 60f)] float _tilt = 15f;

    [Header("Orbit - 스치면")]
    [Tooltip("고리에 스쳤을 때의 피해. 0이면 프로파일의 피해를 쓴다.")]
    [SerializeField, Min(0f)] float _contactDamage;

    [Tooltip("같은 대상을 다시 갈기까지의 간격. 고리는 사라지지 않으므로 이걸로 조절한다.")]
    [SerializeField, Min(0.05f)] float _rehitDelay = 0.8f;

    [Header("Orbit - 끝낼 때")]
    [Tooltip("후딜에 들어가며 고리를 플레이어에게 던진다. 끄면 그 자리에 떨어뜨린다.")]
    [SerializeField] bool _throwOnFinish;

    [Tooltip("마무리로 던질 때의 속도. 0이면 발사체의 기본 속도를 쓴다.")]
    [SerializeField, Min(0f)] float _finishThrowSpeed = 16f;

    [Tooltip("하나씩 떨어져 나가는 간격(초). 0이면 한꺼번에 쏟아진다.\n" +
             "개수 × 간격이 후딜보다 길면 남은 것은 스킬이 끝나는 순간 한꺼번에 나간다.")]
    [SerializeField, Min(0f)] float _finishInterval = 0.15f;

    float Damage => _contactDamage > 0f ? _contactDamage : Profile.damage;

    protected override void OnWindup()
    {
        manager.LookTowards(manager.Player);

        // 진입하는 순간 고리를 짜둔다. 바닥에서 끌려오는 모습은 Follow가 만든다.
        HoldRing();
    }

    protected override void OnActivate()
    {
        manager.Stop();

        // 이제부터 고리가 아프다.
        foreach (Boss_Throwable stone in held)
            if (stone != null) stone.SetHarmful(true, Damage, _rehitDelay);
    }

    protected override void OnRecover()
    {
        if (!_throwOnFinish) { DropAll(); return; }

        // 첫 발은 곧바로. 나머지는 UpdateState가 간격을 두고 내보낸다.
        _thrown = 0;
        _nextThrowAt = Time.time;
    }

    public override void UpdateState()
    {
        base.UpdateState();

        // 구간마다 다른 빠르기로 돈다. 준비는 느리게, 발동은 제 속도로.
        float speed = phase == Phase.Windup ? _windupAngularSpeed : _angularSpeed;
        ringAngle += speed * Time.deltaTime;

        // 후딜에도 계속 자리를 지정한다. 아직 안 나간 것들은 도는 채로 제 차례를 기다린다.
        // 이미 날아간 것은 Held가 아니라 이 지정을 무시하므로, 남은 것들의 각도는 그대로 유지된다.
        //
        // 준비 구간에만 따라붙기를 쓴다 — 바닥에서 딸려 오는 모습이 보여야 하기 때문이다.
        // 그 뒤로는 정확히 올려두어야 인스펙터의 반지름이 곧 실제 궤도가 된다.
        HoldRing(phase != Phase.Windup);

        if (phase == Phase.Recovery && _throwOnFinish) ThrowOnSchedule();
    }

    /// <summary>
    /// 받아들인 창도 <b>이미 도는 고리라면</b> 아프게 만든다.
    /// OnActivate가 그때 들고 있던 것에만 걸어두므로, 나중에 합류한 것은 여기서 따로 걸어야 한다.
    /// </summary>
    public override bool Absorb(Boss_Throwable throwable)
    {
        if (!base.Absorb(throwable)) return false;

        if (phase != Phase.Windup) throwable.SetHarmful(true, Damage, _rehitDelay);

        return true;
    }

    /// <summary>고리를 기울인다. 수평으로만 돌면 평면적으로 보인다.</summary>
    protected override Vector3 SlotOf(int index, int total) => Tilted(base.SlotOf(index, total));

    Vector3 Tilted(Vector3 slot)
    {
        if (Mathf.Approximately(_tilt, 0f) || manager == null || manager.character == null) return slot;

        Vector3 centre = manager.character.position + Vector3.up * RingHeight;
        Vector3 offset = slot - centre;

        // 보스가 보는 방향을 축으로 기울인다. 정면에서 봤을 때 타원으로 읽힌다.
        Quaternion lean = Quaternion.AngleAxis(_tilt, manager.character.right);

        return centre + lean * offset;
    }

    #region Finish - 하나씩 떼어 보낸다

    int _thrown;
    float _nextThrowAt;

    public override void Enter()
    {
        _thrown = 0;
        base.Enter();
    }

    /// <summary>
    /// 제 차례가 된 것부터 내보낸다.
    /// 간격이 0이면 while이 한 바퀴에 다 돌아 예전처럼 한꺼번에 쏟아진다 — 그 동작도 남겨둔다.
    /// </summary>
    void ThrowOnSchedule()
    {
        while (_thrown < held.Count && Time.time >= _nextThrowAt)
        {
            ThrowNext();
            _nextThrowAt = Time.time + _finishInterval;
        }
    }

    void ThrowNext()
    {
        Boss_Throwable stone = held[_thrown];
        _thrown++;

        if (stone != null) stone.Throw(manager.Player, Damage, _finishThrowSpeed);
    }

    /// <summary>
    /// 후딜이 짧아 미처 못 나간 것이 남았으면 여기서 마저 내보낸다.
    /// 그러지 않으면 기반 클래스가 원위치로 돌려보내, 플레이어에게는 남은 돌이 조용히 사라진 것으로 보인다.
    /// </summary>
    public override void Exit()
    {
        if (_throwOnFinish) while (_thrown < held.Count) ThrowNext();

        base.Exit();
    }

    #endregion

    void DropAll()
    {
        foreach (Boss_Throwable stone in held)
            if (stone != null) stone.Release();

        held.Clear();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (!_throwOnFinish || _finishInterval <= 0f) return;

        // 개수는 여기서 정확히 알 수 없으므로 집기로 한 최대치로 잰다.
        float needed = MaxCount * _finishInterval;
        if (needed <= Profile.recoveryTime) return;

        Debug.LogWarning(
            $"[{name}] 마무리 발사에 {needed:0.00}초가 필요한데 후딜이 {Profile.recoveryTime:0.00}초입니다. " +
            $"남은 것은 스킬이 끝나는 순간 한꺼번에 나갑니다. 후딜을 늘리거나 간격을 줄이세요.", this);
    }
#endif
}
