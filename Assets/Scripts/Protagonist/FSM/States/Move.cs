using System.Collections;
using UnityEngine;

public class Move: IState
{
    public Move()
    {

    }

    public IEnumerator Enter()
    {
        yield return null;
    }

    public IState Update()
    {
        return this;
    }

    public IEnumerator Exit()
    {
        yield return null;
    }

    public IState Transitions()
    {

        return this;
    }

    public bool ToInteract()
    {
        
        return false;
    }
}
