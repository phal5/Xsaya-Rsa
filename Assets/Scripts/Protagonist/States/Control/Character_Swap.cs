using UnityEngine;

/// <summary>
/// 무기 교체. 지금은 껍데기다 — 자리와 시간만 잡아두고 실제 교체는 비워뒀다.
///
/// TODO: 교체할 무기 세트가 정해지면 OnSwap()을 채운다.
///       CharacterManager에 무기 목록을 두고 인덱스를 돌리는 형태가 될 것이다.
///
/// 동작 중에는 제자리에 선다.
/// </summary>
public class Character_Swap : BaseCharacterState
{
    float _endTime;

    public override void Enter()
    {
        _endTime = Time.time + characterManager.SwapTime;
        characterManager.Steering.Move(Vector3.zero, Vector3.up);

        OnSwap();
    }

    public override void FixedUpdateState()
    {
        characterManager.Steering.Move(Vector3.zero, Vector3.up);
        Transitions();
    }

    public override void Transitions()
    {
        if (Time.time < _endTime) return;

        if (fsm is Character_Controlled controlled) controlled.ToLocomotion();
    }

    /// <summary>실제 교체가 일어날 자리.</summary>
    void OnSwap()
    {
        // TODO: 무기 교체 구현
    }
}
