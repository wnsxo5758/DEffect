using UnityEngine;

/// <summary>
/// 착지 상태 - 긴 낙하 후 착지 시 짧은 경직
/// </summary>
public class PlayerLandState : PlayerGroundStateBase
{
    [SerializeField] private float landingSpeedMultiplier = 0.65f; // 착지 시 이동 속도 감소 비율
    private float landingDuration = 0.2f; // 착지 경직 시간
    private float landingTimer = 0f;

    public PlayerLandState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 부모의 공통 전환 조건 (점프, 낙하)
        base.SetupTransitions();

        // Land 상태는 애니메이션이 끝나면 자동으로 Idle/Run으로 전환
        // 애니메이션 시간 기반으로 전환
    }

    public override void Enter()
    {
        base.Enter();

        landingTimer = 0f;

        // 착지 시 속도 감소
        stateMachine.Movement.StopMovement();
    }

    protected override void UpdateState()
    {
        landingTimer += Time.deltaTime;

        // 착지 경직 시간이 끝나면 Idle 또는 Run으로 전환
        if (landingTimer >= landingDuration)
        {
            // 이동 입력이 있으면 Run, 없으면 Idle
            if (Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.1f)
            {
                stateMachine.ChangeState<PlayerRunState>();
            }
            else
            {
                stateMachine.ChangeState<PlayerIdleState>();
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 착지 경직 중에는 이동 속도 감소 적용
        if (landingTimer < landingDuration && Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.01f)
        {
            // 입력 방향으로 감소된 속도로 이동
            float reducedSpeed = stateMachine.InputHandler.MoveInput * landingSpeedMultiplier;
            stateMachine.Movement.MoveToDirection(reducedSpeed);
        }
        else if (landingTimer < landingDuration)
        {
            // 입력이 없으면 정지
            stateMachine.Movement.StopMovement();
        }
    }

    public override void Exit()
    {
        base.Exit();
        // 착지 상태 종료
    }
}
