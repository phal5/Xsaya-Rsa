using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteInEditMode]
public class ComboRecorder : MonoBehaviour
{
    [SerializeField] Key[] key;
    //[SerializeField] 
    [Space(10f)]
    [SerializeField] bool copy = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (copy)
        {
            copy = false;

        }
    }
}
