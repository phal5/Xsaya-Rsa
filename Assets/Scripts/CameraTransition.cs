using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraTransition : MonoBehaviour
{
    [SerializeField] private bool _toggle;  //true = 3D, false = 2D
    [SerializeField] private bool _previousToggle;
    [SerializeField] private Transform _protagonist;
    [SerializeField] private Transform _target;
    [SerializeField] private float _duration;

    private float t0;
    private float t1;

    private void Start()
    {
        
    }

    public IEnumerator Move()
    {
        while(Time.time < t1)
        {
            yield return null;
        }

    }
}
