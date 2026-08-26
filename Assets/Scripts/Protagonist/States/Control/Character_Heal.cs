using UnityEngine;

/// <summary>
/// 자원 소모형 회복. 남은 횟수를 진입 시점에 소모하고, 체력은 동작의 <b>정해진 프레임</b>에 오른다.
/// 그 프레임에 닿기 전에 피격으로 끊기면 횟수만 날아간다.
/// 회복을 거는 타이밍이 판단의 대상이 되게 하려는 것이다.
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
    float _endTime;
    bool _consumed;
    bool _restored;
    RaycastHit _hit;

    public override void Enter()
    {
        // 땅 위에서만 건다. 축 머신 밖이라 물어볼 축이 없으므로 지면 캐스터를 직접 본다.
        if (!Grounded())
        {
            Reject();
            return;
        }

        // 검사와 소모가 한 연산이라 여기 말고 다른 곳에서 미리 볼 이유가 없다.
        _consumed = characterManager.ConsumeHealCharge();

        if (!_consumed)
        {
            Reject();
            return;
        }

        _beganAt = Time.time;
        _endTime = _beganAt + characterManager.HealTime;
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
        _consumed = false;
        _restored = false;

        // 여기서 목표 속도를 실었으므로 나갈 때 내려놓는다.
        // 피격으로 끊기면 다음 상태가 이 값을 물려받아 저절로 걸어간다.
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (fsm is Character_Controlled controlled) controlled.ToLocomotion();
    }

    /// <summary>정해둔 프레임에 닿으면 한 번만 올린다.</summary>
    void Restore()
    {
        if (_restored || !_consumed) return;
        if (Time.time - _beganAt < characterManager.HealMoment) return;

        _restored = true;

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
