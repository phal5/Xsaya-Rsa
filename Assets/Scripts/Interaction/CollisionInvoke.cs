using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어가 닿으면 이벤트를 쏜다. <b>트리거와 물리 충돌 양쪽</b>을 받는다.
///
/// 입력이 있어야 동작하는 <see cref="InteractableEvent"/>와 달리 이쪽은 근접만으로 발동한다.
/// 통과 가능한 볼륨으로 놓든 실제로 부딪히는 몸으로 놓든 같은 이벤트가 나간다.
///
/// 한 번만 쓰려면 <see cref="Destroy"/>를 이벤트 끝에 물리면 된다.
/// </summary>
public class CollisionInvoke : MonoBehaviour
{
    [SerializeField] UnityEvent _onCollision;

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerManager.IsPlayer(other)) _onCollision.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (PlayerManager.IsPlayer(collision.collider)) _onCollision.Invoke();
    }

    public void Destroy()
    {
        Destroy(this);
    }
}
