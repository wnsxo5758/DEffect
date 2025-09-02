using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // 이동 속도

    private Rigidbody2D rb;
    private Vector2 moveInput;
    
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
    }
    
    // 오브젝트 비활성화 시 호출
    private void OnDisable()
    {
        // Player 액션맵 비활성화
        // 메모리 해제 (메모리 누수 방지)
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        // 키를 누르면 (1, 0) 또는 (-1, 0) 값이 들어온다. 키를 떼면 (0, 0)
        moveInput = context.ReadValue<Vector2>();
    }
    
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }
}
