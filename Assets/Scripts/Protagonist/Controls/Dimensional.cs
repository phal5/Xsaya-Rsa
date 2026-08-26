using UnityEngine;

/// <summary>
/// 플레이어 몸의 Z 정렬과 잠금. 2D로 돌아올 때 평면 위에 세우고, 그 자리에 얼린다.
///
/// <b>평면은 z = 0 하나다.</b> 저작할 값이 아니라 세계의 규약이다 —
/// 무대의 놀이 평면도, 스폰 지점도, 관문의 도착 지점도 전부 그 위에 놓인다.
///
/// 한때는 스폰 지점의 Z를 평면으로 삼았다. 그러면 무대마다 평면이 갈리는데,
/// 실제로 -20 / 0.4 / 0 세 값이 나왔고 2D로 돌아올 때마다 몸이 그 자리로 끌려갔다.
/// 스폰 지점이 평면 위에 정확히 놓여 있어야만 맞는 규칙이었던 셈이라, 규칙 쪽을 없앴다.
/// 스폰이 평면을 벗어나 있어도 2D로 돌아오는 순간 여기서 제자리로 눌린다.
/// </summary>
public class Dimensional : MonoBehaviour
{
    /// <summary>2D로 놀 때 몸이 서는 평면.</summary>
    const float Plane = 0f;

    [SerializeField] Rigidbody _own;
    [SerializeField] Rigidbody _external;

    public void LockZ(bool _lock)
    {
        if (_lock)
        {
            _external.constraints |= RigidbodyConstraints.FreezePositionZ;
            _own.constraints |= RigidbodyConstraints.FreezePositionZ;
        }
        else
        {
            _external.constraints &= ~RigidbodyConstraints.FreezePositionZ;
            _own.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        }
    }

    /// <summary>두 몸을 평면 위로 옮긴다.</summary>
    public void NullifyZ()
    {
        Flatten(_own);
        Flatten(_external);
    }

    /// <summary>
    /// <b>물리 자세와 Transform 둘 다에 쓴다.</b> Transform에만 쓰면 되돌아온다 —
    /// Character_Movement가 매 FixedUpdate마다 두 몸을 <see cref="Rigidbody.position"/>으로 맞추는데,
    /// 그것이 읽는 물리 자세는 아직 옛 Z다. 유니티가 Transform 변경을 물리로 밀어 넣는 시점은
    /// FixedUpdate 스크립트가 다 돈 뒤라, 그 대입이 마지막 말을 한다.
    ///
    /// 읽는 것도 Transform이 아니라 물리 자세에서 읽는다. 둘이 어긋나 있을 때 실제로 쓰이는 쪽이 그쪽이다.
    /// </summary>
    void Flatten(Rigidbody body)
    {
        Vector3 position = body.position;
        position.z = Plane;

        body.position = position;
        body.transform.position = position;
    }
}
