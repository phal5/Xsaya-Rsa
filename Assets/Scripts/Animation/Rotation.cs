using UnityEngine;

public class Rotation : MonoBehaviour
{
    [SerializeField] Transform upTransform;
    [SerializeField] Transform lookTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(upTransform.forward + transform.position, lookTarget.position - transform.position);
    }
}
