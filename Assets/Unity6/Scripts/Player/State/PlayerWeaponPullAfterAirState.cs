using UnityEngine;

/// <summary>
/// 공중 무기 뽑기 후 회전 낙하 상태
/// SRP: 공중에서 무기를 뽑은 후 회전하며 낙하하는 동작만 담당
/// </summary>
public class PlayerWeaponPullAfterAirState : PlayerAirborneStateBase
{
    public override bool ShouldLockDirection => true;

    [Header("Knockback Settings")]
    private const float knockbackForceX = 5f; // 수평 반동 힘
    private const float knockbackForceY = 3f; // 수직 반동 힘

    private Rigidbody2D rb;
    private PlayerRangedAttack rangedAttack;

    public PlayerWeaponPullAfterAirState(StateMachine stateMachine) : base(stateMachine)
    {
        rb = stateMachine.GetComponent<Rigidbody2D>();
        PlayerCombatSystem combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        rangedAttack = combatSystem?.RangedAttack;
    }

    protected override void SetupTransitions()
    {
        // 착지 시 즉시 Idle/Run 상태로 전환
        AddTransition<PlayerIdleState>(
            () => stateMachine.Movement.IsGrounded()
                  && Mathf.Abs(stateMachine.InputHandler.MoveInput) <= 0.1f,
            priority: 100
        );

        AddTransition<PlayerRunState>(
            () => stateMachine.Movement.IsGrounded()
                  && Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.1f,
            priority: 101
        );
    }

    public override void Enter()
    {
        base.Enter();

        // 이동 입력 무시
        stateMachine.Movement.SetMoveInput(0);

        // 반동 방향 계산: 무기 → 플레이어 방향의 반대
        ApplyKnockback();

        // AfterPull 애니메이션 트리거
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.TriggerWeaponPullAfterAir();
        }
    }

    /// <summary>
    /// 무기 뽑는 방향의 반대로 반동 적용
    /// </summary>
    private void ApplyKnockback()
    {
        if (rb == null) return;

        // 무기 뽑기 컨텍스트에서 무기 위치 가져오기
        WeaponPullContext pullContext = rangedAttack?.PendingPullContext;
        if (pullContext == null)
        {
            Debug.LogWarning("[PlayerWeaponPullAfterAirState] PullContext is null, using default knockback");
            // 기본 넉백 (플레이어가 보는 반대 방향)
            float facingDirection = stateMachine.Movement.FacingDirection;
            rb.linearVelocity = new Vector2(-facingDirection * knockbackForceX, knockbackForceY);
            return;
        }

        // 무기 위치 가져오기 (weaponPosition이 저장되어 있음)
        Vector2 weaponPosition = pullContext.weaponPosition;
        Vector2 playerPosition = stateMachine.transform.position;

        // 무기에서 플레이어로의 방향 계산
        Vector2 pullDirection = (weaponPosition - playerPosition);

        // 너무 가까우면 기본 방향 사용
        if (pullDirection.magnitude < 0.1f)
        {
            float facingDirection = stateMachine.Movement.FacingDirection;
            rb.linearVelocity = new Vector2(-facingDirection * knockbackForceX, knockbackForceY);
            return;
        }

        pullDirection.Normalize();

        // 반대 방향으로 반동 적용
        Vector2 knockbackDirection = -pullDirection;
        Vector2 knockbackForce = new Vector2(
            knockbackDirection.x * knockbackForceX,
            knockbackForceY // 위쪽으로 약간 상승
        );

        rb.linearVelocity = knockbackForce;
        Debug.Log($"[AfterPull] Knockback applied: {knockbackForce}, direction: {knockbackDirection}");
    }

    protected override void UpdateState()
    {
        // 자동 전환으로 처리
    }

    public override void FixedUpdate()
    {
        // 이동 입력 무시 - 반동과 중력만 적용
        // base.FixedUpdate()를 호출하지 않음으로써 좌우 이동 차단
    }

    public override void Exit()
    {
        base.Exit();
    }
}
