using UnityEngine;

public class OscillatioryPlatform : MonoBehaviour
{
    [SerializeField] Vector3 _point1;
    [SerializeField] Vector3 _point2;
    [SerializeField] float _frequency;
    [Space(10f)]
    [SerializeField] Rigidbody _rigidbody;

    float _t = 0;

    // 가정: 발판은 한 프레임만에 한 주기를 초과한 분량을 이동하지 않을 것이다
    // * 만일 물리 프레임 하나에 걸쳐 이동 발판이 한 주기(원래 위치로 돌아오는 시간) 이상을 움직인다면 그건 애초에 발판이 아니거나 이 유형으로 구현할 물건이 아니다
    
    private void FixedUpdate()
    {
        _t += Time.fixedDeltaTime * _frequency;
        if (_t > 1) _t -= 1;
        float t = 0.5f - 0.5f * Mathf.Cos(_t * Mathf.PI * 2);
        Vector3 position = Vector3.Lerp(_point1, _point2, t);
        _rigidbody.MovePosition(position);
    }
}
