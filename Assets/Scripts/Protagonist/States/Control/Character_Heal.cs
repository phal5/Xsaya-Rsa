using UnityEngine;

/// <summary>
/// 자원 소모형 회복. 체력이 오르는 것도 횟수가 깎이는 것도 동작의 <b>정해진 프레임</b>이다.
/// 그 프레임에 닿기 전에 끊기면 아무 대가도 치르지 않는다 - 낸 만큼만 받는다.
///
/// <b>붙들고 있어야 낫는다.</b> 회복 키를 놓으면 그 자리에서 끊긴다.
/// 회복을 거는 타이밍이 판단의 대상이 되게 하려는 것이다.
///
/// <b>낫고 나면 곧바로 끝난다.</b> 남은 클립을 마저 돌리려고 붙들지 않는다 —
/// 그 시간은 연출일 뿐이고, 그동안 조작을 묶으면 나은 뒤에도 한참을 못 움직인다.
///
/// <b>회복 시점을 초가 아니라 클립 프레임으로 적는 이유</b>는 <see cref="SwingStrike"/>와 같다 —
/// 클립을 프레임 단위로 들여다보며 고르므로, 초로 환산해 적어두면 볼 때마다 다시 나눠야 한다.
/// 그 프레임은 <b>동작에 들어선 순간</b>부터 센다. 섞이는 동안에도 클립은 이미 돌아가고 있어,
/// 블렌드가 끝나기를 기다렸다 세면 실제 동작보다 늦는다.
///
/// 땅 위에서만 걸 수 있고, 동작 중에는 느리게 움직인다.
/// </summary>
public class Character_Heal : BaseCharacterState
{
    float _beganAt;
    bool _restored;
    RaycastHit _hit;

    /// <summary>
    /// 이 상태는 이어서 하지 않는다. 죽었다 돌아오는 길이 그 재개를 지나는데,
    /// 그때 Enter가 다시 돌면 회복이 처음부터 한 번 더 걸린다.
    /// </summary>
    public override bool ResumesOnReturn => false;

    public override void Enter()
    {
        // 땅 위에서만 건다. 축 머신 밖이라 물어볼 축이 없으므로 지면 캐스터를 직접 본다.
        if (!Grounded())
        {
            Reject();
            return;
        }

        // <b>여기서는 보기만 한다.</b> 깎는 것은 회복이 실제로 들어가는 프레임의 몫이다.
        // 남은 것이 없으면 동작 자체를 걸지 않는다 - 걸어봐야 아무 일도 일어나지 않는다.
        if (characterManager.HealCharges <= 0)
        {
            Reject();
            return;
        }

        _beganAt = Time.time;
        _restored = false;

        characterManager.Animation.Play(characterManager.HealState, characterManager.HealBlend);

        // 진입과 동시에 세운다. 회복이 걸리는 프레임을 기다리면 이미 다 낫고 나서 빛이 난다.
        if (characterManager.HealEffect != null) characterManager.HealEffect.Play();
    }

    /// <summary>
    /// 회복 시점은 여기서 본다. FixedUpdate가 아닌 이유는 그쪽이 초당 50번이라
    /// 정해둔 프레임과 최대 20ms까지 어긋나기 때문이다 — 그림은 매 프레임 그려진다.
    /// </summary>
    public override void UpdateState()
    {
        Restore();
    }

    public override void FixedUpdateState()
    {
        // 제자리에 세우지 않는다. 느리게나마 움직일 수 있다.
        Vector3 input = InputManager.CharacterMove;
        characterManager.Steering.Move(input * characterManager.HealSpeed, _hit.normal == Vector3.zero ? Vector3.up : _hit.normal);

        Transitions();
    }

    public override void Exit()
    {
        _restored = false;

        // 뿌리던 빛을 거둔다. 회복이 들어가는 순간 이 상태가 끝나므로 그 둘은 같은 시각이고,
        // 끊겨 나가는 길도 여기를 지나므로 이펙트만 혼자 끝까지 도는 일이 없다.
        if (characterManager.HealEffect != null) characterManager.HealEffect.Stop();

        // 여기서 목표 속도를 실었으므로 나갈 때 내려놓는다.
        // 피격으로 끊기면 다음 상태가 이 값을 물려받아 저절로 걸어간다.
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void Transitions()
    {
        // 붙들고 있어야 낫는다. 놓으면 그 자리에서 끊긴다 - 회복이 아직 안 들어갔으면 횟수도 그대로다.
        if (!Held())
        {
            Reject();
            return;
        }

        // 회복이 들어갔으면 거기서 끝이다. 남은 클립을 마저 돌리려고 붙들지 않는다 —
        // 그 시간은 연출일 뿐인데 그동안 조작이 묶여, 나은 뒤에도 한참을 못 움직였다.
        if (!_restored) return;

        if (fsm is Character_Controlled controlled) controlled.ToLocomotion();
    }

    /// <summary>
    /// 회복 키가 아직 눌려 있는지. <see cref="InputManager"/>에 표시를 따로 두지 않고 액션에 직접 묻는다 —
    /// 두 군데가 같은 것을 들고 있으면 어느 날 하나만 갱신된다.
    /// </summary>
    static bool Held()
    {
        if (InputManager.instance == null || InputManager.instance.move_heal == null) return false;

        return InputManager.instance.move_heal.action.IsPressed();
    }

    /// <summary>
    /// 정해둔 프레임에 닿으면 한 번만 올린다. <b>깎는 것도 여기서 한다.</b>
    ///
    /// 검사와 소모는 여기서도 한 연산이다. 진입할 때 본 것과 지금이 다를 수 있고,
    /// 그 틈에 남은 것이 사라졌다면 회복도 일어나지 않아야 한다.
    /// </summary>
    void Restore()
    {
        if (_restored) return;
        if (Time.time - _beganAt < characterManager.HealMoment) return;

        _restored = true;

        if (!characterManager.ConsumeHealCharge()) return;

        if (characterManager.Damagable != null)
            characterManager.Damagable.Heal(characterManager.HealAmount);
    }

    bool Grounded()
    {
        return characterManager.GroundCaster != null && characterManager.GroundCaster.Cast(out _hit);
    }

    void Reject()
    {
        if (fsm is Character_Controlled controlled) controlled.ToLocomotion();
    }
}
