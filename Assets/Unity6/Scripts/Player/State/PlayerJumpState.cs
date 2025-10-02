public class PlayerJumpState : PlayerAirborneStateBase
{
    public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 부모의 공통 전환 조건 먼저 설정 (착지)
        base.SetupTransitions();

        // Jump 상태만의 고유 전환 조건

        // 1. 속도가 음수가 되면(떨어지기 시작) Fall 상태로 전환
        AddTransition<PlayerFallState>(
            () => stateMachine.Movement.Velocity.y < 0,
            priority: 50
        );
    }

    public override void Enter()
    {
        base.Enter();
        // 점프 실행
        stateMachine.Movement.Jump();
        // Jump 애니메이션 재생 등
    }

    protected override void UpdateState()
    {
        // Jump 상태의 고유 로직
        // 예: 점프 높이에 따른 효과 등
    }

    public override void FixedUpdate()
    {
        // 부모의 공중 이동 로직 사용
        base.FixedUpdate();
    }

    public override void Exit()
    {
        // Jump 상태 종료 시 정리 작업
    }
}
