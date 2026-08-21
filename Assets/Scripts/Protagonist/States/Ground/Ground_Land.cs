using UnityEngine;

/// <summary>
/// 착지 마무리. 공중에서 세워둔 점프 클립의 남은 분량을 끝까지 재생하고 Idle로 넘긴다.
///
/// 클립을 다시 걸지 않는다 — 애니메이터는 이미 하강 자세로 멈춰 서 있으므로,
/// 배속만 풀면 그 자리에서 이어진다. 다시 걸면 섞이는 동안 앞 프레임이 비친다.
///
/// 조작을 막지 않는다. 이동 입력이 들어오면 남은 분량을 버리고 곧바로 Run으로 넘어간다.
/// 점프와 회피도 Character_Ground가 평소처럼 받는다.
/// </summary>
public class Ground_Land : BaseCharacterState
{
    /// <summary>클립 끝으로 인정하는 진행도. 마지막 한두 프레임은 눈에 띄지 않는다.</summary>
    const float Complete = 0.99f;

    /// <summary>재생을 푼 시각. 멈춰 있던 참이라 가속은 시간으로 건다.</summary>
    float _resumedAt;

    public override void Enter()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);

        _resumedAt = Time.time;
        ApplySpeed();
    }

    public override void FixedUpdateState()
    {
        // Ground_Idle과 같은 이유로 계속 0을 눌러준다. 속도는 상태 밖에 남는다.
        characterManager.Steering.Move(Vector3.zero, Ground.GroundHit.normal);

        ApplySpeed();
        Transitions();
    }

    /// <summary>
    /// 마무리 배속으로 끌어올린다. 세울 프레임이 없으므로 감속은 걸지 않는다 —
    /// 클립이 끝나면 그 자리에서 Idle로 섞여 나간다.
    /// </summary>
    void ApplySpeed()
    {
        JumpAnimation jump = characterManager.Jump;

        characterManager.Animation.SetFloat(
            jump.speedParameter,
            jump.ResumeSpeed(jump.landingSpeed, Time.time - _resumedAt));
    }

    public override void Transitions()
    {
        if (InputManager.CharacterMove.sqrMagnitude > 0.01f)
        {
            fsm.TransitTo<Ground_Move>();
            return;
        }

        if (Finished()) fsm.TransitTo<Ground_Idle>();
    }

    /// <summary>
    /// 클립이 끝났거나, 다른 무언가가 재생을 가져갔거나.
    /// 둘 다 "여기서 더 기다릴 이유가 없다"는 뜻이라 같이 묶는다.
    /// </summary>
    bool Finished()
    {
        if (!characterManager.Animation.IsPlaying(characterManager.Jump.stateName, out float normalized)) return true;
        return normalized >= Complete;
    }

    Character_Ground Ground => fsm as Character_Ground;
}
