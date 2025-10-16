using UnityEngine;

/// <summary>
/// 텔레포트 종료 상태
/// SRP: 바닥 무기로 텔레포트 시 착지 애니메이션만 담당
/// (적에 박힌 무기는 TeleportStartState에서 직접 WeaponPull 상태로 전환)
/// </summary>
public class PlayerTeleportEndState : PlayerStateBase
{
    public override bool ShouldLockDirection => true;

    public PlayerTeleportEndState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 애니메이션 이벤트로 전환 처리
    }

    public override void Enter()
    {
        base.Enter();

        // 이동 입력 무시
        stateMachine.Movement.StopMovement();

        // 텔레포트 종료 애니메이션 (착지)
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.TriggerTeleportEnd();
        }
    }

    protected override void UpdateState()
    {
        // 애니메이션이 끝나면 자동으로 전환
    }

    public override void FixedUpdate()
    {
        // 텔레포트 종료 중에는 물리 업데이트 없음
    }

    public override void Exit()
    {
        // 정리 작업 없음
    }

    /// <summary>
    /// 텔레포트 종료 애니메이션 완료 시 호출 (애니메이션 이벤트)
    /// </summary>
    public void OnTeleportEndAnimationFinished()
    {
        // 바닥 무기로 텔레포트했으므로 일반 상태로 복귀
        TransitionToIdleOrFall();
    }

    /// <summary>
    /// Idle 또는 Fall 상태로 전환
    /// </summary>
    private void TransitionToIdleOrFall()
    {
        if (!stateMachine.Movement.IsGrounded())
        {
            stateMachine.ChangeState<PlayerFallState>();
        }
        else if (Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.1f)
        {
            stateMachine.ChangeState<PlayerRunState>();
        }
        else
        {
            stateMachine.ChangeState<PlayerIdleState>();
        }
    }
}
