using UnityEngine;

public class BookTrigger : MonoBehaviour
{
    [SerializeField] Book book;

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        if (other.transform == PlayerManager.instance.player)
        {
            FlipBook.Instance.SetBook(book);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;
        if (collision.transform == PlayerManager.instance.player)
        {
            FlipBook.Instance.SetBook(book);
        }
    }
}