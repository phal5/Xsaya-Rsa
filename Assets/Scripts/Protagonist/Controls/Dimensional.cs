using UnityEngine;

public class Dimensional : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] Rigidbody _sampler;

    [SerializeField] float _originalZ;

    private void Awake()
    {
        _originalZ = _rigidbody.transform.position.z;
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
        position = _sampler.transform.position;
        position.z = _originalZ;
        _sampler.transform.position = position;

        position = _rigidbody.transform.position;
        position.z = _originalZ;
        _rigidbody.transform.position = position;
    }
}
