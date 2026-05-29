using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private Rigidbody _sampler;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _space;
    [Space(10f)]
    [SerializeField] private float _acceleration;

    Vector3 _localVelocity;

    Vector3 _targetVelocity;

    [SerializeField] bool _singular = false;

    private void FixedUpdate()
    {
        if (_singular) Singular();
        else Mutual();
    }

    private void Singular()
    {
        // singular mode does NOT utilize the concept of separated velocities
        // thus, only the SAMPLER will be utilized.
        Vector3 velocity = _sampler.linearVelocity;
        Vector3 direction = _targetVelocity.normalized;
        
        Vector3 rawAccel = _targetVelocity - velocity;
        float alignment = Vector3.Dot(rawAccel, direction); // how much 'direction' component the raw acceleration has
        float disparity = CustomMath.ReLU(-alignment);      // flip sign to derive negative alignment
        Vector3 accel = (rawAccel + disparity * direction).normalized;   // remove negative alignment factor from the acceleration vector to derive acceleration direction

        _sampler.linearVelocity += direction * Mathf.Clamp(Time.fixedDeltaTime * alignment * _acceleration, 0, alignment);

        // integrate velocity spaces
        _rigidbody.position = _sampler.position;
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

    public void SetMovementMode(bool singular)
    {
        if (singular != _singular)
        {
            _singular = singular;
            IntegrateVelocitySpace();
        }
    }

    public void Move(Vector3 targetLocalVelocity)
    {
        _targetVelocity = targetLocalVelocity;
    }

    public void IncrementSamplerVelocity(Vector3 incrementation)
    {
        _sampler.linearVelocity += incrementation;
    }

    public void SetSamplerVelocity(Vector3 velocity)
    {
        _sampler.linearVelocity = velocity;
    }

    public void SetSamplerYVelocity(float velocity)
    {
        Vector3 v = _sampler.linearVelocity;
        v.y = velocity;
        _sampler.linearVelocity = v;
    }

    public void NReluSamplerYVelocity()
    {
        Vector3 v = _sampler.linearVelocity;
        v.y = -CustomMath.ReLU(-v.y);
        _sampler.linearVelocity = v;
    }

    public void SetLocalVelocity(Vector3 velocity)
    {
        _localVelocity = velocity;
    }

    public void IntegrateVelocitySpace()
    {
        _sampler.position = _rigidbody.position;
        _sampler.linearVelocity = _rigidbody.linearVelocity;
        _targetVelocity = Vector3.zero;
        _localVelocity = Vector3.zero;
    }

    private void AccelerateTo(Vector3 targetVelocity)
    {
        if (targetVelocity == _localVelocity) return;
        
        Vector3 direction = (targetVelocity - _localVelocity).normalized;
        float difference = (targetVelocity - _localVelocity).magnitude;
        float step = _acceleration * Time.fixedDeltaTime;
        float incremence = Mathf.Min(step, difference);
        Vector3 acceleration = incremence * direction;
        Accelerate(acceleration);
    }

    private void Accelerate(Vector3 acceleration)
    {
        _localVelocity += acceleration;
    }
}
