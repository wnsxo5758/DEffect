using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private StageData stageData;

    [Header("����� ����")] 
    [SerializeField] private bool enableDeveloperDebug = false;
    
    [Header("��ũ����")] 
    [SerializeField] private float crouchCheckDistance = 0.5f;
    [SerializeField] private LayerMask aboveLayer;

    [Header("������")] 
    [SerializeField] private float rollCooldown = 1f;

    [Header("������ ������")] 
    [SerializeField] private float respawnDelay = 2f;
    
    private MovementRigidbody2D movement;
    private PlayerHp playerHp;
    private PlayerAttack playerAttack;
    private PlayerInteraction playerInteraction;
    private PlayerStateMachine<PlayerController> stateMachine;
    private PlayerAnim animator;
    private PlayerSound playerSound;
    // �̵� �Է� ����� ����
    private Vector2 moveInput;

    private PlayerInteraction.InteractionType currentInteractionType = PlayerInteraction.InteractionType.None;
    private bool canInteract = false;
    private bool canRoll = true;
    
    public bool IsOnLadder { get; set; } //��ٸ� 
    public bool WantToStand { get; set; } // �ɾ��� �� �Ͼ �� �ִ� ����
    
    private void Awake()
    {
        playerSound = GetComponentInChildren<PlayerSound>();
        movement = GetComponent<MovementRigidbody2D>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHp = GetComponent<PlayerHp>();
        playerInteraction = GetComponent<PlayerInteraction>();
        animator = GetComponentInChildren<PlayerAnim>();
        stateMachine = new PlayerStateMachine<PlayerController>();
    }

    private void Start()
    {
        // ���� �ӽ� �ʱ�ȭ
        stateMachine.Setup(this, new PlayerStates.Idle());
        stateMachine.SetGlobalState(new PlayerStates.StateGlobal());
        if (playerHp != null)
        {
            playerHp.OnPlayerDeath += OnPlayerDeath;
        }
    }
    
    private void Update()
    {
        if (playerHp != null && playerHp.IsDead) return;
        
        stateMachine.Execute();
    }

    // Move �̺�Ʈ
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    // Jump �̺�Ʈ
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // ���� ���¿� ���� ���� ó��
            if (GetCurrentState() is PlayerStates.Idle || GetCurrentState() is PlayerStates.Run)
            {
                if (movement.IsGrounded)
                {
                    movement.Jump();
                    playerSound.JumpSound();
                    if (animator != null)
                    {
                        animator.JumpAnim();
                    }
                    
                    ChangeState(new PlayerStates.Jump());
                }
            }
            // ��ٸ����� �����ϴ� ���
            else if (GetCurrentState() is PlayerStates.Climb)
            {
                OnLadderJump();
            }
        }
    }

    // Crouch �̺�Ʈ
    public void OnCrouch(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                OnCrouchDown();
                break;
            case InputActionPhase.Canceled:
                OnCrouchUp();
                break;
        }
    }

    // Roll �̺�Ʈ
    public void OnRoll(InputAction.CallbackContext context) 
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (GetCurrentState() is PlayerStates.Idle or PlayerStates.Run)
            {
                if (movement.IsGrounded && canRoll)
                {
                    ChangeState(new PlayerStates.Roll());
                }
            }
        }
    }
    
    // Interact �̺�Ʈ
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (playerInteraction != null)
        {
            if (context.phase == InputActionPhase.Started ||
                context.phase == InputActionPhase.Canceled)
            {
                playerInteraction.ProcessInteraction(context);
            }
        }
    }
    
    // ���� ���� �̺�Ʈ
    public void OnMeleeAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (playerAttack != null)
            {
                playerAttack.PerformMeleeAttack();
            }
        }
    }
    
    // �ڷ���Ʈ �̺�Ʈ
    public void OnTeleport(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (playerAttack != null)
            {
                playerAttack.PerformTeleport();
            }
        }
    }
    
    // ThrowWeapon �̺�Ʈ
    public void OnThrowWeapon(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (playerAttack != null)
            {
                playerAttack.PerformThrowWeapon();
            }
        }
    }
    
    
    // �̵� ������Ʈ
    public void UpdateMove(float input) // �̵�
    {
        if (GetCurrentState() is PlayerStates.Crawl)
        {
            movement.Crawl(input);
        }
        else
        {
            movement.MoveTo(input);
        }

        float xPos = Mathf.Clamp(transform.position.x, stageData.PlayerLimitMinX, stageData.PlayerLimitMaxX);
        transform.position = new Vector2(xPos, transform.position.y);
    }

    // ��������Ʈ ���� ����
    public void SpriteFlipX(float direction)
    {
        if (direction != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * Mathf.Sign(direction),
                transform.localScale.y,
                transform.localScale.z);
        }
    }
    
    // ���� �Է� �� ��ȯ
    public float VerticalInput()
    {
        return moveInput.y;
    }

    // ���� �Է� �� ��ȯ
    public float HorizontalInput()
    {
        return moveInput.x;
    }
    
    // ��ũ���� ����
    private void OnCrouchDown()
    {
        if (GetCurrentState() is PlayerStates.Idle || GetCurrentState() is PlayerStates.Run)
        {
            WantToStand = false;
            ChangeState(new PlayerStates.Crawl());
        }
    }
    
    // ��ũ���� ����
    private void OnCrouchUp()
    {
        if (GetCurrentState() is PlayerStates.Crawl)
        {
            if (!HasSpaceAbove())
            {
                WantToStand = true;
                
                return;
            }
            
            ChangeState(new PlayerStates.Idle());
        }
    }

    public void SetInteractionAvailable(bool available, PlayerInteraction.InteractionType type)
    {
        canInteract = available;
        currentInteractionType = type;
    }
    
    // ��ٸ����� ����
   private void OnLadderJump()
    {
        if (GetCurrentState() is PlayerStates.Climb)
        {
            animator.LadderJumpAnim();
            movement.LadderJump(HorizontalInput());
            IsOnLadder = false;
            ChangeState(new PlayerStates.Jump());
        }
    }

    public IEnumerator StartRollCoroutine()
    {
        canRoll = false;
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    public bool OnAttackReceived(bool canFreeze)
    {
        if (GetCurrentState() is PlayerStates.Roll rollState)
        {
            if (canFreeze)
            {
                return rollState.CheckDodgeAndTriggerTimeStop(this);
            }

            return true;
        }

        return false;
    }

    // ���� ������ �ִ��� Ȯ�� (��ũ���� ���� ���� ����)
    public bool HasSpaceAbove() 
    {
        Vector3 rayOrigin = transform.position + new Vector3(0, 0.7f, 0);
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, 
                            crouchCheckDistance, aboveLayer);
        
        Debug.DrawRay(rayOrigin, Vector2.up * crouchCheckDistance, hit ? Color.red : Color.green);

        return hit.collider == null;
    }
    
    // ���� �浹
    public void UpdateBelowCollision() 
    {
        if (movement.HitBelowObject != null)
        {
            if (movement.HitBelowObject.TryGetComponent<PlatformBase>(out var platform))
            {
                platform.UpdateCollision(gameObject);
            }
        }
    }
    
    private void OnPlayerDeath()
    {
        DisablePlayerControl();
    }

    public void ResetOnRespawn()
    {
        EnablePlayerControl();
        
        ResetAllPlayerStates();

        ResetAnimationsToCurrentState();
        
        StartCoroutine(DelayedRespawn());
    }

    private IEnumerator DelayedRespawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        
        if (playerHp != null)
        {
            playerHp.ResetDeathState();
        }
        
        ChangeState(new PlayerStates.Idle());
    }
    
    private void DisablePlayerControl()
    {
        // ���� �̵� ����
        if (movement != null)
        {
            movement.DisableRigidbody();
        }
        
        // �ٸ� ������Ʈ ��Ȱ��ȭ
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }
        
        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }
    }

    public void EnablePlayerControl()
    {
        // ���� �Ӽ� �ٽ� Ȱ��ȭ
        if (movement != null)
        {
            movement.EnableRigidbody();
        }

        if (playerAttack != null)
        {
            playerAttack.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }
    }
    
    private void ResetAllPlayerStates()
    {
        // �Է� ���� �ʱ�ȭ
        moveInput = Vector2.zero;
        UpdateMove(0);
        
        // ��ȣ�ۿ� ���� �ʱ�ȭ
        canInteract = false;
        currentInteractionType = PlayerInteraction.InteractionType.None;
        
        // ���� ���� �ʱ�ȭ
        playerAttack.ResetOnRespawn();
        
        // �׼� ���� �ʱ�ȭ
        canRoll = true;
        
        // Ư�� ���� �ʱ�ȭ
        IsOnLadder = false;
        WantToStand = false;
    }

    private void ResetAnimationsToCurrentState()
    {
        if (animator != null)
        {
            animator.MovementAnim(0);
            animator.SetCrouchAnim(false);
            animator.SetClimbAnim(false);
            animator.SetHasWeapon(playerAttack != null && playerAttack.HasWeapon());

            animator.ResetAllAnimationStates();
        }
    }
    
    public void ChangeState(State<PlayerController> newState)
    {
        stateMachine.ChangeState(newState);
    }

    public void RevertToPreviousState()
    {
        stateMachine.RevertToPreviousState();
    }

    public State<PlayerController> GetCurrentState()
    {
        return stateMachine.CurrentState;
    }
    
    public bool IsGrounded() => movement.IsGrounded;

    public void DebugDeath(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed && enableDeveloperDebug)
        {
            playerHp.TriggerDebugDeath();
        }
    }
        
    public void OnDestroy()
    {
        if (playerHp != null)
        {
            playerHp.OnPlayerDeath -= OnPlayerDeath;
        }
    }

    private void OnGUI()
    {
        // ������ ����װ� Ȱ��ȭ�Ǿ� ���� ���� ǥ��
        if (!enableDeveloperDebug) return;
    
        // GUI ��Ÿ�� ����
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 16;
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;
    
        // ��� �ڽ� ��Ÿ��
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.7f));
        
        // ���� ���� ���� ����
        string currentStateName = GetCurrentStateName();
    
        // ȭ�� ���� ��ܿ� ���� ���� ǥ��
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
    
        GUILayout.BeginVertical(boxStyle);
    
        GUILayout.Label("=== Player Debug Info ===", labelStyle);
        GUILayout.Space(5);
    
        GUILayout.Label($"Current State: {currentStateName}", labelStyle);
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    // ���� ���� �̸��� ���ڿ��� ��ȯ
    public string GetCurrentStateName()
    {
        var currentState = GetCurrentState();
        if (currentState == null) return "None";
    
        // ���� Ÿ�� �̸����� ���ӽ����̽� ����
        string fullName = currentState.GetType().Name;
        return fullName;
    }
    
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = color;
    
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
