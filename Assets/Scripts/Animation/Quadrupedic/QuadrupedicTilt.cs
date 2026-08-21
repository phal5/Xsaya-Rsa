using UnityEngine;

/// <summary>
/// 걸음에 맞춰 몸통을 흔든다. 어느 대각이 지금 땅을 딛고 있는지를 읽어 그쪽으로 기운다.
///
/// 신호는 발의 <b>들림</b>(<see cref="Foot.Lift"/>)이지 발의 높이가 아니다.
/// 높이에는 지형의 높낮이와, 걸음에서 이탈해 뒤에 남겨진 발까지 섞여 들어와
/// 흔들림이 아니라 풀리지 않는 기울기가 된다. 들림은 딛는 순간 0이 되고 몸이 서면
/// 0으로 잦아들므로, 흔들림도 걸음과 같이 시작하고 같이 멎는다 —
/// 공격처럼 몸이 서는 구간에서 직전 기울기를 안고 굳던 것이 이 차이다.
///
/// 진폭에는 한계를 둔다. 예전에는 발 높이차(미터)를 단위벡터에 그대로 더해서
/// 0.5m 들림이 26도짜리 목표 자세를 지시했다. 그만한 회전이 다리 뿌리를 흔들고,
/// 흔들린 뿌리가 Foot이 재는 속도를 부풀리고, 빨라진 걸음이 들림을 더 키웠다.
/// 공진의 이득이 여기서 나왔다. 되먹임의 반대쪽 절반 — 흔들림이 걸음 속도로
/// 되돌아가는 길 — 은 Foot이 몸 기준 좌표에서 속도를 재는 것으로 끊었다.
/// 루프가 열려 있으므로 아래 세 값은 이제 안정성이 아니라 <b>연출</b>의 값이다.
/// </summary>
public class QuadrupedicTilt : MonoBehaviour
{
    [Tooltip("한쪽 대각이 완전히 들렸을 때 기우는 정도. 0이면 흔들리지 않는다.")]
    [SerializeField] float _sway = 0.15f;

    [Tooltip("이만큼 들리면 흔들림이 최대가 된다. Foot의 Lift와 맞춘다.")]
    [SerializeField] float _referenceLift = 0.5f;

    [Tooltip("초당 따라갈 수 있는 최대 각도. 걸음 주기보다 느리면 흔들림이 뭉개진다.")]
    [SerializeField] float _tiltAmount;

    [Space(10f)]
    [Tooltip("대각은 (뒤왼, 앞오른)과 (앞왼, 뒤오른)이다. 자리를 바꿔 꽂으면 흔들림이 뒤집힌다.")]
    [SerializeField] Foot _rearLeft;
    [SerializeField] Foot _frontLeft;
    [SerializeField] Foot _rearRight;
    [SerializeField] Foot _frontRight;

    Vector3 up;

    void FixedUpdate()
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, TargetRotation(), Time.deltaTime * _tiltAmount);
    }

    /// <summary>들려 있는 대각이 +. -1~1로 정규화해 진폭을 <see cref="_sway"/>에 묶는다.</summary>
    float Diagonal()
    {
        float lifted = (_rearLeft.Lift + _frontRight.Lift) - (_frontLeft.Lift + _rearRight.Lift);
        return Mathf.Clamp(lifted / (2f * _referenceLift), -1f, 1f) * _sway;
    }

    Quaternion TargetRotation()
    {
        Vector3 tilt = Vector3.up * Diagonal();
        Vector3 forth = transform.parent.forward + tilt;
        Vector3 left = -transform.parent.right - tilt;

        // 진폭이 1보다 작게 묶여 있는 한 이 외적은 위를 향한다. 부호를 되돌리던
        // FaceUp은 진폭이 제한되지 않던 시절의 뒷수습이라 함께 지웠다.
        up = Vector3.Cross(left, forth).normalized;
        return Quaternion.LookRotation(up, forth);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + up);
        Gizmos.DrawSphere(transform.position + up, 0.1f);
    }
}
