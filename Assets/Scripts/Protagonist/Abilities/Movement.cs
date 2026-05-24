using System.Collections;
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
    Vector3 _targetLocalVelocity;

    bool _singular = false;

    private void FixedUpdate()
    {
        if (_singular)
        {
            Singular();
        }
        else
        {
            Mutual();
        }
    }

    private void Singular()
    {
        Vector3 v = _rigidbody.linearVelocity;



        _sampler.position = _rigidbody.position;
        _sampler.linearVelocity = _rigidbody.linearVelocity;
    }

    private void Mutual()
    {
        //0. set local velocity
        AccelerateTo(_targetLocalVelocity);

        //1. set linear velocity as local + environmental velocity
        _rigidbody.linearVelocity = _sampler.linearVelocity + _localVelocity;

        //2. set sample position
        _sampler.position = _rigidbody.position;
    }

    // I'll use 2 rigidbodies - one for Environmental velocity(sampling), other for global velocity
    // I'll teleport 'environmental' to 'global' on the end of every calculation and extract the velocity in the beginning
    // on the entrance to airborne state, global velocity will override environmental velocity once for smooth conversion.

    // Local velocity will remain internal.

    public void SetMovementMode(bool singular)
    {
        if(singular !=  _singular)
        {
            _singular = singular;
            IntegrateVelocitySpace();
        }
    }

    public void Move(Vector3 targetLocalVelocity)
    {
        _targetLocalVelocity = targetLocalVelocity;
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

    public void SetLocalVelocity(Vector3 velocity)
    {
        _localVelocity = velocity;
    }

    public void IntegrateVelocitySpace()
    {
        _sampler.linearVelocity = _rigidbody.linearVelocity;
        _targetLocalVelocity = Vector3.zero;
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
        if (acceleration == Vector3.zero) return;

        Vector3 dir = acceleration.normalized;
        float spd = Vector3.Dot(_localVelocity, dir);
        _localVelocity += acceleration;
    }
}
