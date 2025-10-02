using System;
using System.Collections.Generic;
using System.Linq;

public abstract class PlayerStateBase : IPlayerState
{
    protected PlayerStateMachine stateMachine;
    protected List<StateTransition> transitions;

    public PlayerStateBase(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        transitions = new List<StateTransition>();
    }
    
    // 템플릿 메서드 패턴
    public virtual void Enter()
    {
        SetupTransitions(); // 전환 조건 등록
    }

    public virtual void Exit() { }

    public void Update()
    {
        CheckTransitions();  // 전환 체크
        UpdateState();       // 상태별 로직
    }

    public virtual void FixedUpdate() { }

    // 하위 클래스에서 구현
    protected abstract void SetupTransitions();
    protected virtual void UpdateState() { }

    // 전환 체크 로직 (공통)
    private void CheckTransitions()
    {
        foreach (var transition in transitions.OrderByDescending(t => t.Priority))
        {
            if (transition.ShouldTransition())
            {
                stateMachine.ChangeState(transition.TargetStateType);
                return;
            }
        }
    }

    // 전환 조건 등록 헬퍼
    protected void AddTransition<TState>(Func<bool> condition, int priority = 0) 
        where TState : IPlayerState
    {
        transitions.Add(new StateTransition(typeof(TState), condition, priority));
    }
}
