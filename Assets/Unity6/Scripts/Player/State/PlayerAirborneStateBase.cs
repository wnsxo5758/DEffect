using UnityEngine;

public abstract class PlayerAirborneStateBase : PlayerStateBase
{
    public PlayerAirborneStateBase(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 공중 상태 공통 전환 조건들

        // 1. 착지 시 이동 입력이 있으면 Run 상태로 전환
        AddTransition<PlayerRunState>(
            () => stateMachine.Movement.IsGrounded()
                  && Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.1f,
            priority: 100
        );

        // 2. 착지 시 이동 입력이 없으면 Idle 상태로 전환
        AddTransition<PlayerIdleState>(
            () => stateMachine.Movement.IsGrounded()
                  && Mathf.Abs(stateMachine.InputHandler.MoveInput) <= 0.1f,
            priority: 99
        );
    }

    public override void Exit()
    {
        // 착지 시 점프 상태 리셋
        if (stateMachine.Movement.IsGrounded())
        {
            stateMachine.Movement.ResetJumpState();
        }
    }

    public override void FixedUpdate()
    {
        // 공중에서의 공통 물리 처리: 좌우 이동 가능
        stateMachine.Movement.Move();
    }
}
