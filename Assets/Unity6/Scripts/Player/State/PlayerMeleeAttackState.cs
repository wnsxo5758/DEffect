using UnityEngine;

/// <summary>
/// 근접 공격 상태
/// SRP: 근접 공격 동작과 상태 전환만 담당
/// </summary>
public class PlayerMeleeAttackState : PlayerStateBase
{
    public override bool ShouldLockDirection => true;

    private PlayerCombatSystem combatSystem;
    private PlayerMeleeAttack meleeAttack;

    public PlayerMeleeAttackState(StateMachine stateMachine) : base(stateMachine)
    {
        combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        meleeAttack = combatSystem?.MeleeAttack;
    }

    protected override void SetupTransitions()
    {
        // 공격이 끝나면 자동으로 전환 (애니메이션 이벤트로 처리)
    }

    public override void Enter()
    {
        base.Enter();

        if (meleeAttack == null)
        {
            Debug.LogError("[PlayerMeleeAttackState] MeleeAttack component is missing!");
            stateMachine.ChangeState<PlayerIdleState>();
            return;
        }

        // 이동 정지
        stateMachine.Movement.StopMovement();

        // 공격 시작
        meleeAttack.StartAttack();

        // 공격 종료 이벤트 구독
        meleeAttack.OnAttackFinished += OnAttackFinished;

        // 애니메이션 트리거
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.TriggerAttack();
        }
    }

    protected override void UpdateState()
    {
        // 공격 중에는 이동 불가
        // 애니메이션이 끝나면 자동으로 전환
    }

    public override void FixedUpdate()
    {
        // 공격 중에는 물리 업데이트 없음 (움직임 고정)
    }

    public override void Exit()
    {
        // 이벤트 구독 해제
        if (meleeAttack != null)
        {
            meleeAttack.OnAttackFinished -= OnAttackFinished;
        }
    }

    /// <summary>
    /// 공격 종료 시 호출
    /// </summary>
    private void OnAttackFinished()
    {
        // 입력에 따라 적절한 상태로 전환
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
