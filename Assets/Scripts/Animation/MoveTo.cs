using UnityEngine;

public class MoveTo : MonoBehaviour
{
    [SerializeField] Transform target;

    private void FixedUpdate()
    {
        transform.position = target.position;
    }
}
