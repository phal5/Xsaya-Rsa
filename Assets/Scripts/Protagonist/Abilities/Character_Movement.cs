using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character_Movement : MonoBehaviour, IMovement
{
    [SerializeField] private Rigidbody _sampler;
    [SerializeField] private Rigidbody _rigidbody;
    [Space(10f)]
    [SerializeField] private float _groundAcceleration;
    [SerializeField] private float _aerialAcceleration;

    Vector3 _localVelocity;
    Vector3 _targetVelocity;

    [SerializeField] bool _singular = false;

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (_singular) Singular();
        else Mutual();
        print(_localVelocity);
    }

    private void Singular()
    {
        // singular mode does NOT utilize the concept of separated velocities
        // thus, only the SAMPLER will be utilized.

        Vector3 velocity = _sampler.linearVelocity;
        Vector3 inputDirection = _targetVelocity.normalized;
        Vector3 rawAccel = _targetVelocity - velocity;
        rawAccel.y = 0;

        float alignment = Vector3.Dot(rawAccel, inputDirection);
        float disparity = CustomMath.ReLU(-alignment);
        Vector3 accel = (rawAccel + disparity * inputDirection).normalized;   // remove negative alignment factor from the acceleration vector to derive acceleration direction
        float magnitude = _aerialAcceleration * Mathf.Clamp(alignment, 0, 1) * Time.fixedDeltaTime;

        _sampler.linearVelocity += magnitude * accel;

        // integrate velocity spaces
        _rigidbody.MovePosition(_sampler.position);
        _rigidbody.linearVelocity = _sampler.linearVelocity;
    }

    private void Mutual()
    {
        //0. set local velocity
        AccelerateTo(_targetVelocity);

        //1. set linear velocity as local + environmental velocity
        _rigidbody.linearVelocity = _sampler.linearVelocity + _localVelocity;

        //2. set sample position
        _sampler.position = _rigidbody.position;
    }

    private void AccelerateTo(Vector3 targetVelocity)
    {
        if (targetVelocity == _localVelocity) return;
        
        Vector3 direction = (targetVelocity - _localVelocity).normalized;
        float difference = (targetVelocity - _localVelocity).magnitude;
        float step = _groundAcceleration * Time.fixedDeltaTime;
        float incremence = Mathf.Min(step, difference);
        Vector3 acceleration = incremence * direction;
        Accelerate(acceleration);
    }

    private void Accelerate(Vector3 acceleration)
    {
        _localVelocity += acceleration;
    }

    #region Public

    public void SetMovementMode(bool singular)
    {
        if (singular != _singular)
        {
            _singular = singular;
            if (singular) IntegrateMutualToSingular();
            else IntegrateSingularToMutual();
        }
    }

    public void ClearMovement()
    {
        _targetVelocity = Vector3.zero;
        _localVelocity = Vector3.zero;
        _rigidbody.linearVelocity = _sampler.linearVelocity;
        _rigidbody.angularVelocity = _sampler.angularVelocity;
    }

    public void Move(Vector3 targetLocalVelocity)
    {
        _targetVelocity = targetLocalVelocity;
    }

    public void Drop()
    {
        Vector3 v = _sampler.linearVelocity;
        v.y = -CustomMath.ReLU(-v.y);
        _sampler.linearVelocity = v;
    }

    public void SetSamplerYVelocity(float velocity)
    {
        Vector3 v = _sampler.linearVelocity;
        v.y = velocity;
        _sampler.linearVelocity = v;
    }

    public void IncrementSamplerVelocity(Vector3 incrementation)
    {
        _sampler.linearVelocity += incrementation;
    }

    public void SetSamplerVelocity(Vector3 velocity)
    {
        _sampler.linearVelocity = velocity;
    }

    public void SetLocalVelocity(Vector3 velocity)
    {
        _localVelocity = velocity;
    }

    #endregion

    #region VelocitySpace Integration

    private void IntegrateMutualToSingular()
    {
        _sampler.position = _rigidbody.position;
        _sampler.linearVelocity = _rigidbody.linearVelocity;
        _targetVelocity = Vector3.zero;
        _localVelocity = Vector3.zero;
    }

    private void IntegrateSingularToMutual()
    {
        _targetVelocity = _sampler.linearVelocity;
        _localVelocity = _sampler.linearVelocity;
        _sampler.position = _rigidbody.position;
        _sampler.linearVelocity = Vector3.zero;
    }

    #endregion
}
