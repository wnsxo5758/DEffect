using UnityEngine;

/// <summary>
/// 공중 근접 공격 상태
/// 공격 시작 시 완전히 정지, 공격 판정 후 감소된 속도로 복귀
/// </summary>
public class PlayerAirMeleeAttackState : PlayerStateBase
{
    public override bool ShouldLockDirection => true;

    private PlayerCombatSystem combatSystem;
    private PlayerMeleeAttack meleeAttack;
    private Rigidbody2D rb;

    // 공격 시작 시 속도 저장
    private float savedVelocityX;  // 수평 속도만 저장
    private bool hasAttackHit = false;  // 공격 판정이 실행되었는지 여부

    // 공격 판정 후 수평 속도 감소 비율 (0.5 = 절반 속도)
    private const float PostAttackVelocityMultiplier = 0.5f;

    public PlayerAirMeleeAttackState(StateMachine stateMachine) : base(stateMachine)
    {
        combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        meleeAttack = combatSystem?.MeleeAttack;
        rb = stateMachine.GetComponent<Rigidbody2D>();
    }

    protected override void SetupTransitions()
    {
        // 공중 공격 상태에서는 수동 전환만 허용
        // 공격이 끝나면 OnAttackFinished 이벤트에서 전환
    }

    public override void Enter()
    {
        base.Enter();

        if (meleeAttack == null)
        {
            Debug.LogError("[PlayerAirMeleeAttackState] MeleeAttack component is missing!");
            stateMachine.ChangeState<PlayerFallState>();
            return;
        }

        // 현재 수평 속도를 저장 (관성 유지용)
        if (rb != null)
        {
            savedVelocityX = rb.linearVelocity.x;
        }

        hasAttackHit = false;

        // 공중 공격 시작
        meleeAttack.StartAirAttack();

        // 공격 판정 이벤트 구독
        meleeAttack.OnAttackHit += OnAttackHit;

        // 공격 종료 이벤트 구독
        meleeAttack.OnAttackFinished += OnAttackFinished;

        // 애니메이션 트리거
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.TriggerAirAttack();
        }
    }

    protected override void UpdateState()
    {
        // 공격 중 로직 (필요시 추가)
    }

    public override void FixedUpdate()
    {
        if (rb == null) return;

        // 공격 판정 전: 완전히 정지 (X, Y 모두 0)
        // 공격 판정 후: 감소된 수평 속도로 복원 + 중력 적용
        if (!hasAttackHit)
        {
            // 공중에 완전히 정지 (X, Y 모두 0)
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // 공격 판정 후 원래 속도의 50%로 복귀 + 중력 적용
            float reducedVelocityX = savedVelocityX * PostAttackVelocityMultiplier;
            rb.linearVelocity = new Vector2(reducedVelocityX, rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        // 이벤트 구독 해제
        if (meleeAttack != null)
        {
            meleeAttack.OnAttackHit -= OnAttackHit;
            meleeAttack.OnAttackFinished -= OnAttackFinished;
        }
    }

    /// <summary>
    /// 공격 판정 시 호출 (애니메이션 이벤트)
    /// </summary>
    private void OnAttackHit()
    {
        // 공격 판정이 실행되었음을 표시
        hasAttackHit = true;
    }

    /// <summary>
    /// 공격 종료 시 호출
    /// </summary>
    private void OnAttackFinished()
    {
        // 착지 여부에 따라 상태 전환
        if (stateMachine.Movement.IsGrounded())
        {
            // 착지했으면 입력에 따라 전환
            if (Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.1f)
            {
                stateMachine.ChangeState<PlayerRunState>();
            }
            else
            {
                stateMachine.ChangeState<PlayerIdleState>();
            }
        }
        else
        {
            // 아직 공중이면 Fall 상태로
            stateMachine.ChangeState<PlayerFallState>();
        }
    }
}
