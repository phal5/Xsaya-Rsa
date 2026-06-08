using UnityEngine;

public class KeyInputMovement : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidBody;
    [SerializeField] float _rotationSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 movement = InputManager.CharacterMove;
        _rigidBody.AddForce(movement);
    }
}
