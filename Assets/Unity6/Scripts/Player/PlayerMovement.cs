using UnityEngine;

/// <summary>
/// 플레이어의 물리 기반 이동을 담당하는 컴포넌트
/// 단일 책임 원칙(SRP): 이동/점프/물리 로직만 담당 (입력 처리는 PlayerInputHandler에서)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f; // 웅크리기 시 속도 배수

    [Header("Jump Settings")]
    [SerializeField] private float minJumpForce = 8f; // 최소 점프 힘 (짧게 누를 때)
    [SerializeField] private float maxJumpForce = 15f; // 최대 점프 힘 (길게 누를 때)
    [SerializeField] private float jumpHoldDuration = 0.3f; // 최대 점프 힘에 도달하는 시간
    [SerializeField] private float jumpCutMultiplier = 0.5f; // 버튼 떼면 속도 감소 비율

    [Header("Collider Settings")]
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private Vector2 standingColliderSize = new(0.5f, 1.0f);
    [SerializeField] private Vector2 crouchingColliderSize = new(0.5f, 0.6f);

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Ceiling Check")]
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private Vector2 ceilingCheckSize = new(0.5f, 0.1f);

    private Rigidbody2D rb;

    // 현재 속도 (읽기 전용)
    public Vector2 Velocity => rb.linearVelocity;

    // 현재 이동 입력값 (외부에서 설정 가능)
    public float CurrentMoveInput { get; private set; }

    // 가변 점프 상태
    private bool isJumping = false;
    private bool canCutJump = false;

    // 공중 시간 추적 (착지 애니메이션 판단용)
    private float airTime = 0f;
    private bool wasGrounded = true;

    // 착지 애니메이션을 재생할 최소 공중 시간 (초)
    public const float MinAirTimeForLanding = 0.5f;

    // 공중 시간 읽기 전용 프로퍼티
    public float AirTime => airTime;

    // 캐릭터가 바라보는 방향 (1 = 오른쪽, -1 = 왼쪽)
    private float facingDirection = 1f;
    public float FacingDirection => facingDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // CapsuleCollider가 할당되지 않았으면 자동 검색
        if (capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        // 공중 시간 추적
        TrackAirTime();
    }

    /// <summary>
    /// 공중 시간 추적 (착지 애니메이션 판단용)
    /// </summary>
    private void TrackAirTime()
    {
        bool isGroundedNow = IsGrounded();

        if (!isGroundedNow)
        {
            // 공중에 있으면 시간 증가
            airTime += Time.deltaTime;
        }
        else
        {
            // 착지 시 리셋
            if (!wasGrounded)
            {
                // 방금 착지함
                airTime = 0f;
            }
        }

        wasGrounded = isGroundedNow;
    }

    /// <summary>
    /// 이동 입력값 설정 (InputHandler에서 호출)
    /// </summary>
    public void SetMoveInput(float input)
    {
        CurrentMoveInput = input;

        // 입력이 있으면 바라보는 방향 업데이트
        // 방향 고정은 PlayerAnimator에서 처리 (스프라이트 업데이트 차단)
        if (Mathf.Abs(input) > 0.01f)
        {
            facingDirection = Mathf.Sign(input);
        }
    }

    /// <summary>
    /// 방향을 즉시 업데이트 (StateMachine에서 보류된 입력 적용 시 사용)
    /// </summary>
    public void UpdateFacingDirection(float direction)
    {
        facingDirection = direction;
    }

    /// <summary>
    /// 현재 입력값에 따라 수평 이동 수행
    /// </summary>
    public void Move()
    {
        rb.linearVelocity = new Vector2(CurrentMoveInput * moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// 특정 방향으로 이동 (State에서 직접 호출 가능)
    /// </summary>
    public void MoveToDirection(float horizontalInput)
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// 웅크리기 상태에서 느린 속도로 이동
    /// </summary>
    public void CrouchMove()
    {
        rb.linearVelocity = new Vector2(CurrentMoveInput * moveSpeed * crouchSpeedMultiplier, rb.linearVelocity.y);
    }

    /// <summary>
    /// 점프 수행 (기본 - 최소 점프 힘으로 시작)
    /// </summary>
    public void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * minJumpForce, ForceMode2D.Impulse);
        isJumping = true;
        canCutJump = true;
    }

    /// <summary>
    /// 점프 버튼을 누르고 있는 동안 추가 상승력 적용 (가변 점프)
    /// </summary>
    /// <param name="holdTime">버튼을 누른 시간</param>
    public void ApplyJumpHoldForce(float holdTime)
    {
        if (!isJumping || rb.linearVelocity.y <= 0)
        {
            isJumping = false;
            return;
        }

        // holdTime에 따라 추가 힘 계산 (0 ~ jumpHoldDuration 범위)
        float normalizedHoldTime = Mathf.Clamp01(holdTime / jumpHoldDuration);
        float additionalForce = Mathf.Lerp(0, maxJumpForce - minJumpForce, normalizedHoldTime);

        // 부드러운 추가 힘 적용 (FixedUpdate에서 호출하므로 Time.fixedDeltaTime 사용)
        float forceThisFrame = additionalForce * Time.fixedDeltaTime * 10f;
        rb.AddForce(Vector2.up * forceThisFrame, ForceMode2D.Force);
    }

    /// <summary>
    /// 점프 버튼을 뗐을 때 호출 (상승 중단)
    /// </summary>
    public void CutJump()
    {
        if (canCutJump && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            canCutJump = false;
        }
        isJumping = false;
    }

    /// <summary>
    /// 점프 상태 리셋 (착지 시 호출)
    /// </summary>
    public void ResetJumpState()
    {
        isJumping = false;
        canCutJump = false;
    }

    /// <summary>
    /// 웅크리기 콜라이더 설정 (바닥 기준으로 높이만 변경)
    /// </summary>
    public void SetCrouchingCollider(bool isCrouching)
    {
        if (capsuleCollider == null) return;

        if (isCrouching)
        {
            // 웅크린 상태: 바닥 고정, 높이만 감소
            // Offset = 바닥(0) + (높이 / 2)
            capsuleCollider.size = crouchingColliderSize;
            capsuleCollider.offset = new Vector2(0f, crouchingColliderSize.y / 2f);
        }
        else
        {
            // 서있는 상태: 바닥 고정, 원래 높이
            capsuleCollider.size = standingColliderSize;
            capsuleCollider.offset = new Vector2(0f, standingColliderSize.y / 2f);
        }
    }

    /// <summary>
    /// 지면 접촉 체크
    /// </summary>
    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    /// <summary>
    /// 일어설 수 있는지 체크 (천장 감지)
    /// </summary>
    public bool CanStandUp()
    {
        return !Physics2D.OverlapBox(ceilingCheck.position, ceilingCheckSize, 0f, groundLayer);
    }

    /// <summary>
    /// 이동 정지
    /// </summary>
    public void StopMovement()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    /// <summary>
    /// 기본 중력 스케일 반환
    /// </summary>
    public float GetDefaultGravityScale()
    {
        return rb != null ? 3f : 3f; // Unity 기본 중력 스케일
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        // 지면 체크 영역
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        // 천장 체크 영역
        if (ceilingCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ceilingCheck.position, ceilingCheckSize);
        }
    }
}
