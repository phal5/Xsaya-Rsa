using UnityEngine;

public class ToDo : MonoBehaviour
{
    [SerializeField][TextArea(10, 50)] string memo;
    [SerializeField] Memo[] _memos;
}

[System.Serializable]
public class Memo
{
    [SerializeField][TextArea(10, 10)] string memo;
}
