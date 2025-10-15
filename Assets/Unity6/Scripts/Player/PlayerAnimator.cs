using UnityEngine;

/// <summary>
/// 플레이어 애니메이션을 관리하는 컴포넌트
/// StateMachine과 연동하여 상태에 따라 애니메이션 파라미터 업데이트
/// </summary>
[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private StateMachine stateMachine;
    private PlayerMovement movement;

    // Animation Parameters (Hash로 최적화 - string 비교보다 빠름)
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
    private static readonly int VelocityY = Animator.StringToHash("VelocityY");
    private static readonly int AirTime = Animator.StringToHash("AirTime");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Roll = Animator.StringToHash("Roll");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int AirAttack = Animator.StringToHash("AirAttack");
    private static readonly int HasWeapon = Animator.StringToHash("HasWeapon");

    private void Awake()
    {
        // 컴포넌트 참조
        stateMachine = GetComponent<StateMachine>();
        movement = GetComponent<PlayerMovement>();

        // SpriteRenderer가 할당되지 않았으면 자동 검색
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Animator가 할당되지 않았으면 자동 검색
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (animator == null)
            Debug.LogError("[PlayerAnimator] Animator component is missing!");

        if (spriteRenderer == null)
            Debug.LogError("[PlayerAnimator] SpriteRenderer is missing!");

        if (stateMachine == null)
            Debug.LogError("[PlayerAnimator] StateMachine is missing!");

        if (movement == null)
            Debug.LogError("[PlayerAnimator] PlayerMovement is missing!");
    }

    private void Update()
    {
        if (animator == null) return;

        UpdateAnimationParameters();
        UpdateSpriteDirection();
    }

    /// <summary>
    /// 애니메이션 파라미터 업데이트
    /// </summary>
    private void UpdateAnimationParameters()
    {
        // Speed: 이동 속도의 절댓값 (0 = Idle, 1 = Run)
        float speed = Mathf.Abs(movement.CurrentMoveInput);
        animator.SetFloat(Speed, speed);

        // IsGrounded: 지면에 닿아있는지
        bool isGrounded = movement.IsGrounded();
        animator.SetBool(IsGrounded, isGrounded);

        // VelocityY: Y축 속도 (Jump/Fall 구분)
        float velocityY = movement.Velocity.y;
        animator.SetFloat(VelocityY, velocityY);

        // AirTime: 공중에 있던 시간 (착지 애니메이션 판단용)
        float airTime = movement.AirTime;
        animator.SetFloat(AirTime, airTime);

        // IsCrouching: 웅크리기 상태
        bool isCrouching = stateMachine.IsCurrentState<PlayerCrouchState>();
        animator.SetBool(IsCrouching, isCrouching);

        // HasWeapon: 무기를 들고 있는지
        bool hasWeapon = stateMachine.Combat != null && stateMachine.Combat.HasWeapon;
        animator.SetBool(HasWeapon, hasWeapon);
    }

    /// <summary>
    /// 스프라이트 방향 업데이트 (좌우 반전)
    /// </summary>
    private void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;

        // 구르기 중에는 스프라이트 방향 변경 금지
        if (stateMachine.IsCurrentState<PlayerRollState>())
        {
            return;
        }

        // 공격 중에는 스프라이트 방향 변경 금지
        if (stateMachine.IsCurrentState<PlayerMeleeAttackState>())
        {
            return;
        }

        // 공중 공격 중에는 스프라이트 방향 변경 금지
        if (stateMachine.IsCurrentState<PlayerAirMeleeAttackState>())
        {
            return;
        }

        float moveInput = movement.CurrentMoveInput;

        // 입력이 있을 때만 방향 전환
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            // 왼쪽: flipX = true, 오른쪽: flipX = false
            spriteRenderer.flipX = moveInput < 0;
        }
    }

    /// <summary>
    /// 점프 애니메이션 트리거
    /// </summary>
    public void TriggerJump()
    {
        if (animator != null)
            animator.SetTrigger(Jump);
    }

    /// <summary>
    /// 구르기 애니메이션 트리거
    /// </summary>
    public void TriggerRoll()
    {
        if (animator != null)
            animator.SetTrigger(Roll);
    }

    /// <summary>
    /// 공격 애니메이션 트리거 (지상 공격)
    /// </summary>
    public void TriggerAttack()
    {
        if (animator != null)
            animator.SetTrigger(Attack);
    }

    /// <summary>
    /// 공중 공격 애니메이션 트리거
    /// </summary>
    public void TriggerAirAttack()
    {
        if (animator != null)
            animator.SetTrigger(AirAttack);
    }

    /// <summary>
    /// 무기 장착 상태 업데이트 (즉시 반영)
    /// </summary>
    public void UpdateWeaponState(bool hasWeapon)
    {
        if (animator != null)
            animator.SetBool(HasWeapon, hasWeapon);
    }

    /// <summary>
    /// 특정 방향으로 스프라이트 강제 반전
    /// </summary>
    public void SetSpriteDirection(bool facingLeft)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = facingLeft;
    }

    /// <summary>
    /// 현재 스프라이트가 왼쪽을 보고 있는지
    /// </summary>
    public bool IsFacingLeft()
    {
        return spriteRenderer != null && spriteRenderer.flipX;
    }

    // ========== 애니메이션 이벤트 메서드들 (Animator에서 호출) ==========

    /// <summary>
    /// 근접 공격 판정 타이밍 (애니메이션 이벤트)
    /// </summary>
    public void OnAttackHit()
    {
        var combatSystem = stateMachine.Combat;
        if (combatSystem != null && combatSystem.MeleeAttack != null)
        {
            combatSystem.MeleeAttack.HandleAttackCollision();
        }
    }

    /// <summary>
    /// 근접 공격 애니메이션 종료 (애니메이션 이벤트)
    /// </summary>
    public void OnAttackFinished()
    {
        var combatSystem = stateMachine.Combat;
        if (combatSystem != null && combatSystem.MeleeAttack != null)
        {
            combatSystem.MeleeAttack.FinishAttack();
        }
    }
}
