using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class SmoothMove : MonoBehaviour
{
    [SerializeField] Transform? _targetSpace;
    [SerializeField] Vector3 _relativeTargetPosition;
    [SerializeField] float _duration;
    [SerializeField] bool _align = true;

    [Space(10)]
    [SerializeField]bool _move = false;

    [SerializeField] Quaternion _startRotation;
    [SerializeField] Vector3 _startPosition;
    [SerializeField] float _t0;
    [SerializeField] float _t1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_move)
        {
            _move = false;
            Move();
        }
    }

    IEnumerator MotionAround()
    {
        float t = 0;
        Vector3 targetOrigin = _targetSpace.position;
        while(t < 1)
        {
            //Set t
            t = CustomMath.smooth01((Time.time - _t0) / _duration);

            //Calculate World-space Directions
            Vector3 dir0 = _startPosition - targetOrigin;
            Vector3 dir1 = _relativeTargetPosition;

            GetRotation(dir0, dir1, targetOrigin, t, out Quaternion relativeRotation, out Vector3 relativePosition);

            transform.position = targetOrigin + relativePosition;
            transform.rotation = _startRotation * relativeRotation;
            yield return null;
        }

        Quaternion InitialPositionalRotation = Quaternion.LookRotation(_startPosition - targetOrigin, Vector3.up);
        transform.position = targetOrigin + _relativeTargetPosition;
        transform.rotation = _startRotation * Quaternion.LookRotation(_relativeTargetPosition, Vector3.up) * Quaternion.Inverse(InitialPositionalRotation);
    }

    private void GetRotation(Vector3 dir0, Vector3 dir1, Vector3 origin, float t, out Quaternion relativeRotation, out Vector3 relativePosition)
    {
        relativePosition = Vector3.Slerp(dir0, dir1, t);
        Quaternion initialPositionalRotation = Quaternion.LookRotation(dir0, Vector3.up);

        Quaternion positionalRotation = Quaternion.LookRotation(relativePosition, Vector3.up);
        relativeRotation = positionalRotation * Quaternion.Inverse(initialPositionalRotation);
    }

    public void Move()
    {
        //set start position
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        //reset _t
        _t0 = Time.time;
        _t1 = Time.time + _duration;

        StartCoroutine(MotionAround());
    }
}
