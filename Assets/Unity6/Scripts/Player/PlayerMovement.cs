using UnityEngine;

/// <summary>
/// 플레이어의 물리 기반 이동을 담당하는 컴포넌트
/// 단일 책임 원칙(SRP): 이동/점프/물리 로직만 담당 (입력 처리는 PlayerInputHandler에서)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump Settings")]
    [SerializeField] private float minJumpForce = 8f; // 최소 점프 힘 (짧게 누를 때)
    [SerializeField] private float maxJumpForce = 15f; // 최대 점프 힘 (길게 누를 때)
    [SerializeField] private float jumpHoldDuration = 0.3f; // 최대 점프 힘에 도달하는 시간
    [SerializeField] private float jumpCutMultiplier = 0.5f; // 버튼 떼면 속도 감소 비율

    [Header("Collider Settings")]
    [SerializeField] private BoxCollider2D standingCollider;
    [SerializeField] private BoxCollider2D crouchingCollider;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Ceiling Check")]
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private Vector2 ceilingCheckSize = new Vector2(0.5f, 0.1f);

    private Rigidbody2D rb;

    // 현재 속도 (읽기 전용)
    public Vector2 Velocity => rb.linearVelocity;

    // 현재 이동 입력값 (외부에서 설정 가능)
    public Vector2 CurrentMoveInput { get; private set; }

    // 가변 점프 상태
    private bool isJumping = false;
    private bool canCutJump = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 이동 입력값 설정 (InputHandler에서 호출)
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        CurrentMoveInput = input;
    }

    /// <summary>
    /// 현재 입력값에 따라 수평 이동 수행
    /// </summary>
    public void Move()
    {
        rb.linearVelocity = new Vector2(CurrentMoveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// 특정 방향으로 이동 (State에서 직접 호출 가능)
    /// </summary>
    public void MoveToDirection(float horizontalInput)
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
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
    /// 웅크리기 콜라이더 설정
    /// </summary>
    public void SetCrouchingCollider(bool isCrouching)
    {
        standingCollider.enabled = !isCrouching;
        crouchingCollider.enabled = isCrouching;
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
