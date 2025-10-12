using UnityEngine;

public class PlayerCrouchState : PlayerGroundStateBase
{
    public PlayerCrouchState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // Crouch는 낙하해도 상태 유지 (base.SetupTransitions 호출하지 않음)
        // 점프 입력은 상속받음
        stateMachine.InputHandler.OnJumpPressed += HandleJumpInput;

        // Crouch 상태만의 고유 전환 조건

        // 1. 웅크리기 입력 해제 + 일어설 수 있음 + 정지 → Idle
        AddTransition<PlayerIdleState>(
            () => !stateMachine.InputHandler.IsCrouchHeld
                  && stateMachine.Movement.CanStandUp()
                  && Mathf.Abs(stateMachine.InputHandler.MoveInput) < 0.1f,
            priority: 10
        );

        // 2. 웅크리기 입력 해제 + 일어설 수 있음 + 이동 → Run
        AddTransition<PlayerRunState>(
            () => !stateMachine.InputHandler.IsCrouchHeld
                  && stateMachine.Movement.CanStandUp()
                  && Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.1f,
            priority: 10
        );
    }

    private void HandleJumpInput()
    {
        // 천장이 있으면 점프 불가 (일어서기가 불가능하면 점프도 불가)
        if (!stateMachine.Movement.CanStandUp())
        {
            // 천장이 있어서 점프 불가
            #if UNITY_EDITOR
            Debug.Log("[Crouch] Cannot jump - ceiling detected!");
            #endif
            return;
        }

        // 천장이 없으면 일어서면서 점프
        // 또는 나중에 Roll State로 전환
        if (stateMachine.Movement.IsGrounded())
        {
            // 일어서기
            stateMachine.Movement.SetCrouchingCollider(false);

            // 점프 수행
            stateMachine.Movement.PerformJump();

            // 점프 애니메이션 트리거
            if (stateMachine.Animator != null)
                stateMachine.Animator.TriggerJump();

            stateMachine.ChangeState<PlayerJumpState>();
        }
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
        // 웅크린 상태에서도 좌우 이동 가능
        // 나중에 느린 속도 구현 시 CrouchMove() 사용
        stateMachine.Movement.Move();
    }

    public override void Exit()
    {
        // 점프 이벤트 구독 해제
        stateMachine.InputHandler.OnJumpPressed -= HandleJumpInput;

        // 일어서기 콜라이더로 복원
        stateMachine.Movement.SetCrouchingCollider(false);
    }
}
