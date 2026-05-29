using UnityEngine;

public class ObjectBasedMotion : MonoBehaviour
{
    [SerializeField] Movement _movement;
    [SerializeField] Transform _transform;

    public void Move(Vector2 controls, Vector3 groundNormal, float speed)
    {
        //Transforms vec2 controls to vec3 movement using ground normal

    }
}
