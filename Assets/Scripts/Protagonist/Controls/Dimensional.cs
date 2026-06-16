using UnityEngine;

public class Dimensional : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] Rigidbody _sampler;

    float _originalZ;

    private void Awake()
    {
        _originalZ = _rigidbody.position.z;
    }

    public void LockZ(bool _lock)
    {
        if (_lock)
        {
            _sampler.constraints |= RigidbodyConstraints.FreezePositionZ;
            _rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
        }
        else
        {
            _sampler.constraints &= ~RigidbodyConstraints.FreezePositionZ;
            _rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        }
    }

    public void NullifyZ()
    {
        Vector3 position;
        position = _sampler.position;
        position.z = _originalZ;
        _sampler.position = position;

        position = _rigidbody.position;
        position.z = _originalZ;
        _rigidbody.position = position;
    }
}
