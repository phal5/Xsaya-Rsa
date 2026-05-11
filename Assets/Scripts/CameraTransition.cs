using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraTransition : MonoBehaviour
{
    [SerializeField] private bool _toggle;  //true = 3D, false = 2D
    [SerializeField] Transform _protagonist;
    [SerializeField] Transform _target;
    [SerializeField] float _duration;
    
    float t0;
    float t1;

    public IEnumerator Move()
    {
        while(Time.time < t1)
        {
            yield return null;
        }

    }
}
