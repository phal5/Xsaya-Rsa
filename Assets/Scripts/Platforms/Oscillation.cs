using UnityEngine;

public class Oscillation : MonoBehaviour
{
    [SerializeField] Vector3 _point1;
    [SerializeField] Vector3 _point2;
    [SerializeField] float _frequency;
    [Space(10f)]
    [SerializeField] Rigidbody _rigidbody;

    float _t = 0;
    float _prevT = 0;

    private void FixedUpdate()
    {
        _t += Time.fixedDeltaTime * _frequency;
        if (_t > 1) _t -= 1;
        float t = 0.5f - 0.5f * Mathf.Cos(_t * Mathf.PI * 2);
        Vector3 position = Vector3.Lerp(_point1, _point2, t);
        _rigidbody.MovePosition(position);
    }
}
