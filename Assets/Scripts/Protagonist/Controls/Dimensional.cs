using UnityEngine;

public class Dimensional : MonoBehaviour
{
    [SerializeField] Rigidbody _own;
    [SerializeField] Rigidbody _external;

    [SerializeField] float _originalZ;

    private void Awake()
    {
        _originalZ = _own.transform.position.z;
    }

    public void LockZ(bool _lock)
    {
        if (_lock)
        {
            _external.constraints |= RigidbodyConstraints.FreezePositionZ;
            _own.constraints |= RigidbodyConstraints.FreezePositionZ;
        }
        else
        {
            _external.constraints &= ~RigidbodyConstraints.FreezePositionZ;
            _own.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        }
    }

    public void NullifyZ()
    {
        Vector3 position;
        position = _external.transform.position;
        position.z = _originalZ;
        _external.transform.position = position;

        position = _own.transform.position;
        position.z = _originalZ;
        _own.transform.position = position;
    }
}
