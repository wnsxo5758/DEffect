using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f; // 이동 속도
    [SerializeField] private float jumpForce = 10f;

    [Header("Collider Settings")] 
    [SerializeField] private BoxCollider2D standingCollider;
    [SerializeField] private BoxCollider2D crouchingCollider;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // 발밑 위치의 빈 오브젝트
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Ceiling Check")]
    [SerializeField] private Transform ceilingCheck; // 머리 위 감지 위치
    [SerializeField] private Vector2 ceilingCheckSize = new Vector2(0.5f, 0.1f);
    
    private Rigidbody2D rb;
    public Vector2 MoveInput { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsCrouchPressed { get; private set; }
    
    public Vector2 Velocity => rb.linearVelocity;
    
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        playerInputActions = new PlayerInputActions();
    }
    
    // 오브젝트 활성화 시 호출
    private void OnEnable()
    {
        // Player 액션맵 활성화
        playerInputActions.Player.Enable();
        
        // 이벤트 핸들러 등록
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;
        playerInputActions.Player.Jump.performed += OnJump;
        playerInputActions.Player.Crouch.performed += OnCrouch;
    }
    
    // 오브젝트 비활성화 시 호출
    private void OnDisable()
    {
        // Player 액션맵 비활성화
        // 메모리 해제 (메모리 누수 방지)
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Jump.performed -= OnJump;
        playerInputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        // 키를 누르면 (1, 0) 또는 (-1, 0) 값이 들어온다. 키를 떼면 (0, 0)
        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        StartCoroutine(JumpPressed());
    }
    private System.Collections.IEnumerator JumpPressed()
    {
        IsJumpPressed = true;
        yield return null;
        IsJumpPressed = false;
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        // 버튼이 눌려있는 동안 true 반환
        IsCrouchPressed = context.ReadValueAsButton();
    }
    
    public void Move()
    {
        rb.linearVelocity = new Vector2(MoveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    public void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
    
    public void SetCrouchingCollider(bool isCrouching)
    {
        // 콜라이더 제어
        standingCollider.enabled = !isCrouching;
        crouchingCollider.enabled = isCrouching;
    }
    
    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    public bool CanStandUp()
    {
        return !Physics2D.OverlapBox(ceilingCheck.position, ceilingCheckSize, 0f, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
