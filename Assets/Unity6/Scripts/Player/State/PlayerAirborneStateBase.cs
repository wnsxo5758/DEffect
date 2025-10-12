using UnityEngine;

public abstract class PlayerAirborneStateBase : PlayerStateBase
{
    public PlayerAirborneStateBase(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 공중 상태 공통 전환 조건들

        // 1. 긴 낙하 후 착지 → Land State (착지 경직)
        AddTransition<PlayerLandState>(
            () => stateMachine.Movement.IsGrounded()
                  && stateMachine.Movement.AirTime >= PlayerMovement.MinAirTimeForLanding,
            priority: 101
        );

        // 2. 짧은 낙하 후 착지 + 이동 입력 → Run
        AddTransition<PlayerRunState>(
            () => stateMachine.Movement.IsGrounded()
                  && stateMachine.Movement.AirTime < PlayerMovement.MinAirTimeForLanding
                  && Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.1f,
            priority: 100
        );

        // 3. 짧은 낙하 후 착지 + 입력 없음 → Idle
        AddTransition<PlayerIdleState>(
            () => stateMachine.Movement.IsGrounded()
                  && stateMachine.Movement.AirTime < PlayerMovement.MinAirTimeForLanding
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
