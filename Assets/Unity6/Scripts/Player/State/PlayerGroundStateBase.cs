public abstract class PlayerGroundStateBase : PlayerStateBase
{
    public PlayerGroundStateBase(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 지상 상태 공통 전환 조건들

        // 1. 점프 입력 시 Jump 상태로 전환 (최고 우선순위)
        AddTransition<PlayerJumpState>(
            () => stateMachine.Movement.IsJumpPressed,
            priority: 100
        );

        // 2. 땅에서 떨어진 경우 Fall 상태로 전환
        AddTransition<PlayerFallState>(
            () => !stateMachine.Movement.IsGrounded(),
            priority: 90
        );
    }

    public override void FixedUpdate()
    {
        // 지상에서의 공통 물리 처리
        // 하위 클래스에서 필요시 override하여 추가 로직 구현
    }
}
