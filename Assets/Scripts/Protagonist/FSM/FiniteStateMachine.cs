using UnityEngine;

public class FiniteStateMachine : MonoBehaviour
{
    private IState _state;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _state.Update();
    }
}
