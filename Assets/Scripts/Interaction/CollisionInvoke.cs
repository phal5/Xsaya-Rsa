using UnityEngine;
using UnityEngine.Events;

public class CollisionInvoke : MonoBehaviour
{
    [SerializeField] UnityEvent _onCollision;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform == PlayerManager.instance.player)
        {
            _onCollision.Invoke();
        }
    }

    public void Destroy()
    {
        Destroy(this);
    }
}
