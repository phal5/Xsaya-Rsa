using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public abstract class BaseCharacterState : IState
{
    protected CharacterManager characterManager;
    protected FiniteStateMachine fsm;

    public void Init(EntityManager characterManager, FiniteStateMachine machine)
    {
        if (characterManager.GetType() != typeof(CharacterManager)) return;
        this.characterManager = (CharacterManager)characterManager;
        this.fsm = machine;
        Bootstrap();
    }

    public virtual void Bootstrap() { }

    /// <summary>
    /// 부모 머신이 돌아올 때 이 상태를 <b>이어서</b> 할지.
    ///
    /// 부모는 떠났던 자리를 기억했다가 돌아오며 그 상태의 Enter를 다시 부른다 — 구독처럼
    /// 되살려야 할 것이 있는 상태를 위해서다. 그런데 Enter에서 <b>한 번만 일어나야 할 일</b>을
    /// 하는 상태에게는 그 재개가 곧 그 일을 한 번 더 하는 것이 된다.
    /// 회복이 횟수를 다시 쓰고, 상호작용이 말을 다시 거는 것이 그것이었다.
    ///
    /// 타입 목록을 부모에 박아두는 대신 상태가 스스로 말하게 둔다. 같은 성격의 상태가 늘어도
    /// 고칠 곳은 그 파일 한 줄이다.
    /// </summary>
    public virtual bool ResumesOnReturn => true;

    public virtual void Enter() { Debug.Log($"Entered {this.GetType().Name}"); }

    public virtual void UpdateState() { }

    public virtual void FixedUpdateState() { }

    public virtual void Exit() { }

    public virtual void Transitions() { }
}
/*
    public override void Bootstrap()
    {

    }

    public override void Enter()
    {

    }

    public override void UpdateState()
    {

    }

    public override void FixedUpdateState()
    {

    }

    public override void Exit()
    {

    }

    public override void Transitions()
    {

    }
 */