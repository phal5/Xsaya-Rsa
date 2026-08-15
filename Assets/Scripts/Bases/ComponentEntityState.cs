using UnityEngine;

/// <summary>
/// BaseEntityState의 MonoBehaviour 쌍.
/// 인스턴스별로 인스펙터에서 값을 조절해야 하는 상태는 이쪽을 상속한다.
///
/// FSM의 Component States 목록에 등록해서 쓴다.
/// 등록되면 부모 FSM이 Awake에서 enabled를 꺼 구동권을 가져가므로,
/// 이 컴포넌트에 Update/FixedUpdate를 직접 두지 않는다.
/// </summary>
public abstract class ComponentEntityState<managerType> : MonoBehaviour, IState
    where managerType : EntityManager
{
    protected managerType manager;
    protected FiniteStateMachine fsm;

    public void Init(EntityManager entityManager, FiniteStateMachine machine)
    {
        if (entityManager.GetType() != typeof(managerType)) return;

        this.manager = (managerType)entityManager;
        this.fsm = machine;
        Bootstrap();
    }

    public virtual void Bootstrap() { }

    public virtual void Enter() { }

    public virtual void UpdateState() { }

    public virtual void FixedUpdateState() { }

    public virtual void Exit() { }

    public virtual void Transitions() { }
}
