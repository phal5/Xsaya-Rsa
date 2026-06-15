using UnityEngine;

public class Character_Steering : MonoBehaviour
{
    [SerializeField] Rigidbody _character;
    [SerializeField] Character_Movement _movement;
    [SerializeField] CapsuleCaster _caster;
    [SerializeField] float rotationThreshold = 0.1f;
    [SerializeField] float _rotationSpeed = 1.0f;

    RaycastHit _hit;

    public void Move(Vector3 inputDirection, Vector3 groundNormal)
    {
        Vector3 movement;
        //Motion

        Vector3 direction = Camera.main.transform.TransformVector(inputDirection);
        movement = CustomMath.PreservativeRemove(groundNormal, direction);
        if(_caster.Cast(out _hit, movement))
        {
            movement = CustomMath.CleanRemove(_hit.normal, movement);
        }
        _movement.Move(movement);

        //Rotation
        if (movement.sqrMagnitude < rotationThreshold * rotationThreshold) return;
        Quaternion targetRotation = Quaternion.LookRotation(CustomMath.RemoveY(movement), Vector3.up);
        Quaternion rotated = Quaternion.RotateTowards(_character.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        _character.MoveRotation(rotated);
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
