using UnityEngine;

public abstract class PlayerGroundStateBase : PlayerStateBase
{
    public PlayerGroundStateBase(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 지상 상태 공통 전환 조건들

        // Note: 점프는 이벤트 기반으로 처리하므로 전환 조건에서 제거
        // (InputHandler의 OnJumpPressed 이벤트를 구독)

        // 1. 땅에서 떨어진 경우 Fall 상태로 전환
        AddTransition<PlayerFallState>(
            () => !stateMachine.Movement.IsGrounded(),
            priority: 90
        );
    }

    public override void Enter()
    {
        base.Enter();

        // 점프 이벤트 구독
        stateMachine.InputHandler.OnJumpPressed += HandleJumpInput;
    }

    public override void Exit()
    {
        // 점프 이벤트 구독 해제 (메모리 누수 방지)
        stateMachine.InputHandler.OnJumpPressed -= HandleJumpInput;
    }

    private void HandleJumpInput()
    {
        // 지면에 있을 때만 점프 가능
        if (stateMachine.Movement.IsGrounded())
        {
            stateMachine.Movement.PerformJump();
            stateMachine.ChangeState<PlayerJumpState>();
        }
    }

    public override void FixedUpdate()
    {
        // 지상에서의 공통 물리 처리
        // 하위 클래스에서 필요시 override하여 추가 로직 구현
    }
}
