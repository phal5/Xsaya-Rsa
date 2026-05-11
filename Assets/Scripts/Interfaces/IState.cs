using System.Collections;
using UnityEngine;

public interface IState
{
    public IEnumerator Enter();

    public IState Update();

    public IEnumerator Exit();

    public IState Transitions();
}
