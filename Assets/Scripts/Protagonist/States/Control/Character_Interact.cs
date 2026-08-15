using UnityEngine;

/// <summary>
/// 환경 오브젝트와의 상호작용. 진입 즉시 대상에게 말을 걸고 짧게 멈췄다 복귀한다.
///
/// 상호작용이 대화를 여는 경우, FlipBook의 onSetBook에서 CharacterRoot.ToUI()를 부르면
/// 루트가 UI로 넘어가 조작이 잠긴다. 이 상태가 대화를 붙들고 있지 않는 이유다.
/// </summary>
public class Character_Interact : BaseCharacterState
{
    float _endTime;

    public override void Enter()
    {
        _endTime = Time.time + characterManager.InteractTime;
        characterManager.Steering.Move(Vector3.zero, Vector3.up);

        InteractionDetector detector = characterManager.Interactor;
        if (detector == null || !detector.HasTarget) return;

        detector.Current.Interact(characterManager);
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
}
