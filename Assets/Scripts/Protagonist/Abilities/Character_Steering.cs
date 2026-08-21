using UnityEngine;

public class Character_Steering : MonoBehaviour
{
    [SerializeField] CharacterManager _characterManager;
    [SerializeField] CapsuleCaster _caster;
    [SerializeField] float rotationThreshold = 0.1f;
    [Tooltip("초당 회전 각도.")]
    [SerializeField] float _rotationSpeed = 360f;

    Rigidbody _character;
    Character_Movement _movement;
    RaycastHit _hit;

    private void Awake()
    {
        _character = _characterManager.Rigidbody;
        _movement = _characterManager.Movement;
    }

    public void Move(Vector3 inputDirection, Vector3 groundNormal)
    {
        Vector3 movement;
        //Motion

        Vector3 direction = Camera.main.transform.TransformVector(inputDirection);
        movement = CustomMath.PreservativeRemove(groundNormal, direction);

        if (_caster.Cast(out _hit, movement))
        {
            movement = CustomMath.CleanRemove(_hit.normal, movement);
        }
        _movement.Move(movement);

        // 회전은 이동이 아니라 입력이 정한다.
        // movement로 판정하면 벽을 정면으로 밀 때 성분이 깎여 임계값 아래로 떨어지고,
        // 방향을 누르고 있는데도 몸이 돌지 않는다.
        Rotate(direction);
    }

    /// <summary>누른 방향을 바라본다. 실제로 그쪽으로 갈 수 있는지는 보지 않는다.</summary>
    void Rotate(Vector3 direction)
    {
        Vector3 facing = CustomMath.RemoveY(direction);
        if (facing.sqrMagnitude < rotationThreshold * rotationThreshold) return;

        Quaternion targetRotation = Quaternion.LookRotation(facing, Vector3.up);
        Quaternion rotated = Quaternion.RotateTowards(_character.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        _character.MoveRotation(rotated);
    }

    /// <summary>
    /// 조향 정지. Move(Vector3.zero, ...)와 다르다 — 감속시키지 않고 지금 속도를 관성으로 흘린다.
    /// 회전도 걸지 않는다. 경직처럼 조작을 잃었지만 날아가던 건 유지되어야 하는 상태가 쓴다.
    /// </summary>
    public void Coast()
    {
        _movement.Coast();
    }

    public void Jump(float speed)
    {
        _movement.SetSamplerYVelocity(speed);
    }

    public void Drop()
    {
        _movement.Drop();
    }

    public void Ground()
    {
        _movement.SetMovementMode(false);
    }

    public void Airborne()
    {
        _movement.SetMovementMode(true);
    }
}
