using UnityEngine;

public class PlayerRunState : PlayerGroundStateBase
{
    public PlayerRunState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 부모의 공통 전환 조건 먼저 설정 (점프, 낙하)
        base.SetupTransitions();

        // Run 상태만의 고유 전환 조건

        // 1. 이동 입력이 없으면 Idle 상태로 전환
        AddTransition<PlayerIdleState>(
            () => Mathf.Abs(stateMachine.InputHandler.MoveInput) < 0.1f,
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
        // Run 애니메이션 재생 등
    }

    protected override void UpdateState()
    {
        // Run 상태의 고유 로직
        // 예: 속도에 따른 애니메이션 속도 조절 등
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 실제 이동 처리
        stateMachine.Movement.Move();
    }

    public override void Exit()
    {
        base.Exit(); // 부모의 Exit() 호출하여 점프 이벤트 구독 해제
        // Run 상태 종료 시 정리 작업
    }
}
