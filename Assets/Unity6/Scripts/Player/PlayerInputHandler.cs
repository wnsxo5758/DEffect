using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 입력을 처리하고 이벤트로 전달하는 핸들러
/// 단일 책임 원칙(SRP): 입력 처리만 담당
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    // 입력 이벤트 - 다른 컴포넌트들이 구독 가능
    public event Action<Vector2> OnMoveInput;
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action<float> OnJumpHeld; // 점프 버튼이 눌린 시간 (가변 점프용)
    public event Action<bool> OnCrouchChanged;
    public event Action OnAttackPressed;
    public event Action OnThrowWeaponPressed;
    public event Action OnTeleportPressed;
    public event Action OnInteractPressed;
    public event Action OnInteractReleased;
    public event Action OnRollPressed;

    private PlayerInputActions inputActions;

    // 현재 입력 상태 (읽기 전용 프로퍼티)
    public Vector2 MoveInput { get; private set; }
    public bool IsCrouchHeld { get; private set; }
    public bool IsInteractHeld { get; private set; }
    public bool IsJumpHeld { get; private set; }

    // 가변 점프를 위한 타이머
    private float jumpHoldTime = 0f;
    private bool isTrackingJumpHold = false;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // 입력 액션 활성화
        inputActions.Player.Enable();

        // 이벤트 바인딩
        BindInputEvents();
    }

    private void OnDisable()
    {
        // 이벤트 언바인딩 (메모리 누수 방지)
        UnbindInputEvents();

        // 입력 액션 비활성화
        inputActions.Player.Disable();
    }

    private void Update()
    {
        // 가변 점프: 버튼이 눌려있는 동안 시간 추적
        if (isTrackingJumpHold)
        {
            jumpHoldTime += Time.deltaTime;
            OnJumpHeld?.Invoke(jumpHoldTime);
        }
    }

    private void BindInputEvents()
    {
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Jump.canceled += OnJump;
        inputActions.Player.Crouch.performed += OnCrouch;
        inputActions.Player.Crouch.canceled += OnCrouch;
        inputActions.Player.Attack.performed += OnAttack;
        inputActions.Player.ThrowWeapon.performed += OnThrowWeapon;
        inputActions.Player.Teleport.performed += OnTeleport;
        inputActions.Player.Interact.performed += OnInteract;
        inputActions.Player.Interact.canceled += OnInteract;
        inputActions.Player.Roll.performed += OnRoll;
    }

    private void UnbindInputEvents()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Jump.canceled -= OnJump;
        inputActions.Player.Crouch.performed -= OnCrouch;
        inputActions.Player.Crouch.canceled -= OnCrouch;
        inputActions.Player.Attack.performed -= OnAttack;
        inputActions.Player.ThrowWeapon.performed -= OnThrowWeapon;
        inputActions.Player.Teleport.performed -= OnTeleport;
        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Player.Interact.canceled -= OnInteract;
        inputActions.Player.Roll.performed -= OnRoll;
    }

    // 입력 콜백 메서드들
    private void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        OnMoveInput?.Invoke(MoveInput);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // 점프 시작
            IsJumpHeld = true;
            isTrackingJumpHold = true;
            jumpHoldTime = 0f;
            OnJumpPressed?.Invoke();
        }
        else if (context.canceled)
        {
            // 점프 버튼 뗌
            IsJumpHeld = false;
            isTrackingJumpHold = false;
            OnJumpReleased?.Invoke();
        }
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        bool isPressed = context.ReadValueAsButton();
        IsCrouchHeld = isPressed;
        OnCrouchChanged?.Invoke(isPressed);
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        OnAttackPressed?.Invoke();
    }

    private void OnThrowWeapon(InputAction.CallbackContext context)
    {
        OnThrowWeaponPressed?.Invoke();
    }

    private void OnTeleport(InputAction.CallbackContext context)
    {
        OnTeleportPressed?.Invoke();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        bool isPressed = context.ReadValueAsButton();
        IsInteractHeld = isPressed;

        if (context.performed)
        {
            OnInteractPressed?.Invoke();
        }
        else if (context.canceled)
        {
            OnInteractReleased?.Invoke();
        }
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        OnRollPressed?.Invoke();
    }

    /// <summary>
    /// 점프 홀드 타이머를 가져옵니다
    /// </summary>
    public float GetJumpHoldTime() => jumpHoldTime;

    /// <summary>
    /// 모든 입력을 비활성화 (컷신, 일시정지 등에 사용)
    /// </summary>
    public void DisableInput()
    {
        inputActions.Player.Disable();

        // 입력 상태 초기화
        MoveInput = Vector2.zero;
        IsCrouchHeld = false;
        IsInteractHeld = false;
        IsJumpHeld = false;
        isTrackingJumpHold = false;
    }

    /// <summary>
    /// 입력을 다시 활성화
    /// </summary>
    public void EnableInput()
    {
        inputActions.Player.Enable();
    }
}
