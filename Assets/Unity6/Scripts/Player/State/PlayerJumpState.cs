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

        // 점프 버튼 홀드 이벤트 구독 (가변 점프)
        stateMachine.InputHandler.OnJumpHeld += HandleJumpHold;
        stateMachine.InputHandler.OnJumpReleased += HandleJumpRelease;
    }

    protected override void UpdateState()
    {
        // Jump 상태의 고유 로직
        // 가변 점프는 FixedUpdate에서 처리
    }

    public override void FixedUpdate()
    {
        // 부모의 공중 이동 로직 사용
        base.FixedUpdate();

        // 가변 점프: 버튼이 눌려있으면 추가 상승력 적용
        if (stateMachine.InputHandler.IsJumpHeld)
        {
            float holdTime = stateMachine.InputHandler.GetJumpHoldTime();
            stateMachine.Movement.ApplyJumpHoldForce(holdTime);
        }
    }

    public override void Exit()
    {
        // 점프 이벤트 구독 해제
        stateMachine.InputHandler.OnJumpHeld -= HandleJumpHold;
        stateMachine.InputHandler.OnJumpReleased -= HandleJumpRelease;
    }

    private void HandleJumpHold(float holdTime)
    {
        // OnJumpHeld 이벤트는 정보 제공용
        // 실제 로직은 FixedUpdate에서 처리
    }

    private void HandleJumpRelease()
    {
        // 점프 버튼을 뗐을 때 상승 중단
        stateMachine.Movement.CutJump();
    }
}
