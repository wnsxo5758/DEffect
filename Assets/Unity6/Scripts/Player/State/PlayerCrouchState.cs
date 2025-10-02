public class PlayerCrouchState : PlayerGroundStateBase
{
    public PlayerCrouchState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 부모의 공통 전환 조건 먼저 설정 (점프, 낙하)
        base.SetupTransitions();

        // Crouch 상태만의 고유 전환 조건

        // 1. 웅크리기 입력 해제 시
        //    - 머리 위에 장애물이 없으면 Idle 상태로
        //    - 이동 입력이 있으면 Run 상태로
        AddTransition<PlayerIdleState>(
            () => !stateMachine.Movement.IsCrouchPressed
                  && stateMachine.Movement.CanStandUp()
                  && stateMachine.Movement.MoveInput.sqrMagnitude < 0.1f,
            priority: 10
        );

        AddTransition<PlayerRunState>(
            () => !stateMachine.Movement.IsCrouchPressed
                  && stateMachine.Movement.CanStandUp()
                  && stateMachine.Movement.MoveInput.sqrMagnitude > 0.1f,
            priority: 10
        );
    }

    public override void Enter()
    {
        base.Enter();
        // 웅크리기 콜라이더로 변경
        stateMachine.Movement.SetCrouchingCollider(true);
        // Crouch 애니메이션 재생 등
    }

    protected override void UpdateState()
    {
        // Crouch 상태의 고유 로직
        // 예: 웅크린 상태에서 느린 이동 등
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 웅크린 상태에서도 좌우 이동 가능 (느린 속도)
        // stateMachine.Movement.CrouchMove(); // 필요시 구현
    }

    public override void Exit()
    {
        // 일어서기 콜라이더로 복원
        stateMachine.Movement.SetCrouchingCollider(false);
    }
}
