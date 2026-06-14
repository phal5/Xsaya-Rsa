using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public interface IState
{
    public void Init(EntityManager manager, FiniteStateMachine stateMachine) { }

    public void Enter() { }

    public void UpdateState();

    public void Exit();

    public void Transitions();
}