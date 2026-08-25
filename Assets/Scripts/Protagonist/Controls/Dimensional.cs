using UnityEngine;

/// <summary>
/// 플레이어 몸의 Z 정렬과 잠금. 2D로 돌아올 때 평면 위에 세우고, 그 자리에 얼린다.
///
/// <b>평면은 몸이 마지막으로 놓인 자리가 정한다.</b> 체크포인트든 관문이든 스테이지에 들어오는 길은
/// 전부 <see cref="SceneDirector.Place"/>를 지나고, 그 지점의 Z가 곧 그 무대에서 놀 평면이다.
/// 그래서 여기에는 저작할 값이 없다 — 스폰 지점에 이미 적혀 있고, 여기 또 두면 둘이 갈라진다.
///
/// 처음 등장은 Place를 지나지 않으므로 그때는 몸이 놓여 있던 Z가 그대로 평면이 된다.
/// </summary>
public class Dimensional : MonoBehaviour
{
    [SerializeField] Rigidbody _own;
    [SerializeField] Rigidbody _external;

    [Tooltip("지금 무대의 2D 평면. 스폰 지점이 정하므로 손으로 고칠 자리가 아니다 — 보기용이다.")]
    [SerializeField] float _planeZ;

    void Awake()
    {
        _planeZ = _own.transform.position.z;
    }

    /// <summary>
    /// 평면을 새로 정한다. 몸을 그 자리에 놓는 쪽이 함께 부른다.
    ///
    /// 옮기는 것과 평면을 정하는 것을 갈라놓으면, 옮겨진 뒤 2D로 돌아오는 순간
    /// 옛 평면으로 끌려간다 — 무대마다 평면이 다른데 기준만 하나 남는 것이다.
    /// </summary>
    public void SetPlane(float z)
    {
        _planeZ = z;
    }

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
        position.z = _planeZ;

        body.position = position;
        body.transform.position = position;
    }
}
