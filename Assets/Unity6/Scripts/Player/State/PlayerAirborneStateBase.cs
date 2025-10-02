public abstract class PlayerAirborneStateBase : PlayerStateBase
{
    public PlayerAirborneStateBase(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 공중 상태 공통 전환 조건들

        // 1. 착지 시 Idle 상태로 전환 (최고 우선순위)
        AddTransition<PlayerIdleState>(
            () => stateMachine.Movement.IsGrounded(),
            priority: 100
        );
    }

    public override void FixedUpdate()
    {
        // 공중에서의 공통 물리 처리: 좌우 이동 가능
        stateMachine.Movement.Move();
    }
}
