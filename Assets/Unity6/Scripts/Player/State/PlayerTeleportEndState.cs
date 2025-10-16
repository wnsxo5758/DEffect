using UnityEngine;

/// <summary>
/// 텔레포트 종료 상태
/// SRP: 텔레포트 종료 애니메이션과 후속 처리만 담당
/// </summary>
public class PlayerTeleportEndState : PlayerStateBase
{
    public override bool ShouldLockDirection => true;

    private PlayerRangedAttack rangedAttack;

    public PlayerTeleportEndState(StateMachine stateMachine) : base(stateMachine)
    {
        PlayerCombatSystem combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        rangedAttack = combatSystem?.RangedAttack;
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

        // 텔레포트 종료 애니메이션
        // TODO: PlayerAnimator에 TriggerTeleportEnd() 메서드 추가 필요
        if (stateMachine.Animator != null)
        {
            // stateMachine.Animator.TriggerTeleportEnd();
        }

        // 무기 뽑기가 필요한지 확인
        if (rangedAttack != null && rangedAttack.HasPendingWeaponPull)
        {
            // 무기 뽑기 상태로 전환 예정
            // 애니메이션 종료 후 WeaponPull 상태로 이동
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
        // 무기 뽑기가 필요한 경우 WeaponPull 상태로 전환
        if (rangedAttack != null && rangedAttack.HasPendingWeaponPull)
        {
            WeaponPullContext pullContext = rangedAttack.PendingPullContext;

            if (pullContext != null)
            {
                // 지면/공중에 따라 다른 무기 뽑기 상태로 전환
                if (pullContext.isGrounded)
                {
                    stateMachine.ChangeState<PlayerWeaponPullGroundState>();
                }
                else
                {
                    stateMachine.ChangeState<PlayerWeaponPullAirState>();
                }
                return;
            }
        }

        // 무기 뽑기가 필요 없으면 일반 상태로 복귀
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
