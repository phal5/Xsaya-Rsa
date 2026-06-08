using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] Vector3 _parentAxle;
    [SerializeField] float _directionScale;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + transform.parent.TransformDirection(_parentAxle), _directionScale * (_target.position - transform.position));
    }
}
