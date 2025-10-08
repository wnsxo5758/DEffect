public class PlayerIdleState : PlayerGroundStateBase
{
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 부모의 공통 전환 조건 먼저 설정 (점프, 낙하)
        base.SetupTransitions();

        // Idle 상태만의 고유 전환 조건

        // 1. 이동 입력 시 Run 상태로 전환
        AddTransition<PlayerRunState>(
            () => stateMachine.InputHandler.MoveInput.sqrMagnitude > 0.1f,
            priority: 10
        );

        // 2. 웅크리기 입력 시 Crouch 상태로 전환
        AddTransition<PlayerCrouchState>(
            () => stateMachine.InputHandler.IsCrouchHeld,
            priority: 10
        );
    }

    public override void Enter()
    {
        base.Enter();
        // Idle 애니메이션 재생 등
    }

    protected override void UpdateState()
    {
        // Idle 상태의 고유 로직
        // 예: 대기 시간에 따른 특별 애니메이션 등
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // Idle 상태에서는 움직이지 않으므로 속도를 0으로
        // stateMachine.Movement.Stop(); // 필요시 구현
    }

    public override void Exit()
    {
        // Idle 상태 종료 시 정리 작업
    }
}
