using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f; // 이동 속도
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // 발밑 위치의 빈 오브젝트
    // TODO: 땅 감지 (사각형 or 원)
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody2D rb;
    public Vector2 MoveInput { get; private set; }
    public bool IsJumpPressed { get; private set; }
    
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
        // TODO: JUMP 추가
    }
    
    // 오브젝트 비활성화 시 호출
    private void OnDisable()
    {
        // Player 액션맵 비활성화
        // 메모리 해제 (메모리 누수 방지)
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Disable();
        // TODO: JUMP 추가
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
    
    public void Move()
    {
        rb.linearVelocity = new Vector2(MoveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    public void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public bool IsGrounded()
    {
        // TODO: 감지 정하고 수정
        return true;
    }
}
