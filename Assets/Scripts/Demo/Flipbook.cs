using UnityEngine;

public class Flipbook : MonoBehaviour
{
    [SerializeField] GameObject _attack;
    [SerializeField] GameObject _run;
    [SerializeField] GameObject _idle;

    [SerializeField] GameObject _jump;
    [SerializeField] GameObject _fall;

    [SerializeField] GameObject _stunned;
    [SerializeField] GameObject _dead;
    [Space(10f)]
    [SerializeField] Rigidbody _rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
