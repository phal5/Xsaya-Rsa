using UnityEngine;

public class Steering : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] Transform _character;
    [SerializeField] Movement _movement;
    [SerializeField] CapsuleCaster _caster;

    [SerializeField] float _rotationSpeed = 1.0f;

    public void Move(Vector3 inputDirection, Vector3 groundNormal)
    {
        Vector3 direction = _camera.transform.TransformDirection(inputDirection);
        Vector3 moveDirection = CustomMath.PreservativeRemove(groundNormal, direction);
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        
        _character.transform.rotation = Quaternion.RotateTowards(_character.transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        _movement.Move(direction);
    }

    public void Jump(float speed)
    {
        _movement.SetSamplerYVelocity(speed);
    }

    public void Drop()
    {
        _movement.Drop();
    }
}
