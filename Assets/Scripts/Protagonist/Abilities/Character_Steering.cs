using UnityEngine;

public class Character_Steering : MonoBehaviour
{
    [SerializeField] Transform _character;
    [SerializeField] Character_Movement _movement;
    [SerializeField] CapsuleCaster _caster;

    [SerializeField] float _rotationSpeed = 1.0f;

    public void Move(Vector3 inputDirection, Vector3 groundNormal)
    {
        //Motion
        Vector3 direction = Camera.main.transform.TransformVector(inputDirection);
        Vector3 moveDirection = CustomMath.PreservativeRemove(groundNormal, direction);
        _movement.Move(moveDirection);

        //Rotation
        if (moveDirection == Vector3.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(CustomMath.RemoveY(moveDirection), Vector3.up);
        _character.transform.rotation = Quaternion.RotateTowards(_character.transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        
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
