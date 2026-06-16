using UnityEngine;

public class BookTrigger : MonoBehaviour
{
    [SerializeField] Book book;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform == PlayerManager.instance.player)
        {
            FlipBook.Instance.SetBook(book);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == PlayerManager.instance.player)
        {
            FlipBook.Instance.SetBook(book);
        }
    }
}