using UnityEngine;

public class Treadmill : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] Vector3 _velocity;
    Vector3 _position;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 movement = _velocity * Time.fixedDeltaTime;
        _rigidbody.position = _position - movement;
        _rigidbody.MovePosition(_position);
    }
}
