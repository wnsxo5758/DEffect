using UnityEngine;

/// <summary>
/// 구르기 상태 - 빠른 회피 동작
/// SRP: 구르기 동작과 무적 판정만 담당
/// </summary>
public class PlayerRollState : PlayerGroundStateBase
{
    public override bool ShouldLockDirection => true;

    // Roll 설정값
    private const float RollDuration = 0.4f; // 구르기 지속 시간
    private const float RollSpeedMultiplier = 1.5f; // 구르기 속도 배수 (일반 이동 속도 대비)
    private const float InvincibilityDuration = 0.3f; // 무적 시간 (롤 시간보다 짧게)

    // 상태 추적
    private float rollTimer = 0f;
    private float rollDirection = 0f; // 구르기 방향 (1 or -1)
    private bool isInvincible = false;

    public PlayerRollState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // Roll 상태는 자동으로 끝나므로 전환 조건 없음
        // 부모의 낙하 전환 조건(base.SetupTransitions)을 호출하지 않음
        // 구르기 도중 낙하해도 Roll이 완료될 때까지 상태 유지
        // UpdateState()에서 타이머 기반으로 전환
    }

    // 구르기 중에는 점프 불가
    protected override void SubscribeToJumpEvent()
    {
        // 점프 이벤트를 구독하지 않음 (점프 불가)
    }

    // 구르기 중에는 구르기 불가 (중복 방지)
    protected override void SubscribeToRollEvent()
    {
        // Roll 이벤트를 구독하지 않음
    }

    public override void Enter()
    {
        base.Enter();

        // 타이머 초기화
        rollTimer = 0f;
        isInvincible = true;

        // 구르기 방향 결정 (현재 입력 방향, 없으면 바라보는 방향)
        float moveInput = stateMachine.InputHandler.MoveInput;
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            rollDirection = Mathf.Sign(moveInput);
        }
        else
        {
            // 입력이 없으면 현재 바라보는 방향으로 구르기
            rollDirection = stateMachine.Movement.FacingDirection;
        }

        // Roll 애니메이션 트리거
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.TriggerRoll();
        }
    }

    protected override void UpdateState()
    {
        rollTimer += Time.deltaTime;

        // 무적 시간 종료 체크
        if (isInvincible && rollTimer >= InvincibilityDuration)
        {
            isInvincible = false;
        }

        // 구르기 종료 체크
        if (rollTimer >= RollDuration)
        {
            // 구르기 종료 후 적절한 상태로 전환
            if (!stateMachine.Movement.IsGrounded())
            {
                // 공중에 있으면 Fall 상태로
                stateMachine.ChangeState<PlayerFallState>();
            }
            else if (Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.1f)
            {
                // 지면에 있고 이동 입력이 있으면 Run 상태로
                stateMachine.ChangeState<PlayerRunState>();
            }
            else
            {
                // 지면에 있고 입력이 없으면 Idle 상태로
                stateMachine.ChangeState<PlayerIdleState>();
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 구르기 이동 (일반 속도의 1.5배로 고정 방향 이동)
        stateMachine.Movement.MoveToDirection(rollDirection * RollSpeedMultiplier);
    }

    public override void Exit()
    {
        base.Exit();

        // 무적 상태 해제
        isInvincible = false;
    }

    // 무적 상태 확인 (추후 데미지 시스템에서 사용)
    public bool IsInvincible() => isInvincible;
}
