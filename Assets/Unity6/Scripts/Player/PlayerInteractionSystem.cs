using UnityEngine;
using System;

/// <summary>
/// 플레이어 상호작용 시스템
/// 단일 책임 원칙(SRP): 상호작용 가능한 오브젝트 감지 및 처리만 담당
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteractionSystem : MonoBehaviour
{
    [Header("Interaction Detection")]
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interactionOffset = 0.5f; // 플레이어로부터의 거리

    // 현재 감지된 상호작용 가능 오브젝트
    private IInteractable currentInteractable;
    private WeaponPickup currentWeaponPickup;
    private ThrownWeapon currentThrownWeapon;

    // 이벤트
    public event Action<IInteractable> OnInteractableDetected;
    public event Action OnInteractableLost;

    // 컴포넌트 참조
    private PlayerCombatSystem combatSystem;
    private PlayerMovement playerMovement;
    private StateMachine stateMachine;

    // 외부 참조용 프로퍼티
    public bool HasInteractable => currentInteractable != null || currentWeaponPickup != null || currentThrownWeapon != null;

    private void Awake()
    {
        combatSystem = GetComponent<PlayerCombatSystem>();
        playerMovement = GetComponent<PlayerMovement>();
        stateMachine = GetComponent<StateMachine>();

        // 상호작용 포인트가 없으면 자신의 위치로 설정
        if (interactionPoint == null)
            interactionPoint = transform;
    }

    private void Update()
    {
        UpdateInteractionPointPosition();
        DetectInteractables();
    }

    /// <summary>
    /// 플레이어가 바라보는 방향에 따라 InteractionPoint 위치 업데이트
    /// </summary>
    private void UpdateInteractionPointPosition()
    {
        if (interactionPoint == null || playerMovement == null) return;

        // 방향 고정이 필요한 상태에서는 InteractionPoint 방향도 고정
        if (stateMachine != null && stateMachine.CurrentState is PlayerStateBase stateBase)
        {
            if (stateBase.ShouldLockDirection)
            {
                return; // 방향 업데이트 차단
            }
        }

        // 플레이어가 바라보는 방향으로 offset만큼 이동
        float direction = playerMovement.FacingDirection;
        interactionPoint.localPosition = new Vector3(direction * interactionOffset, 0.6f, 0);
    }

    /// <summary>
    /// 상호작용 가능한 오브젝트 감지
    /// </summary>
    private void DetectInteractables()
    {
        // 이전 프레임의 상호작용 대상 초기화
        IInteractable previousInteractable = currentInteractable;
        WeaponPickup previousWeapon = currentWeaponPickup;
        ThrownWeapon previousThrownWeapon = currentThrownWeapon;

        currentInteractable = null;
        currentWeaponPickup = null;
        currentThrownWeapon = null;

        // 주변 오브젝트 감지
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            interactionPoint.position, interactionRadius, interactableLayer);

        // 우선순위: 무기 > IInteractable
        foreach (Collider2D collider in colliders)
        {
            // 무기 픽업 (최우선)
            if (currentWeaponPickup == null)
            {
                WeaponPickup weaponPickup = collider.GetComponent<WeaponPickup>();
                if (weaponPickup != null)
                {
                    currentWeaponPickup = weaponPickup;
                    continue;
                }
            }

            // 던져진 무기 (두 번째 우선순위)
            if (currentThrownWeapon == null)
            {
                ThrownWeapon thrownWeapon = collider.GetComponent<ThrownWeapon>();
                if (thrownWeapon != null && thrownWeapon.IsStuck())
                {
                    currentThrownWeapon = thrownWeapon;
                    continue;
                }
            }

            // IInteractable (일반 상호작용)
            if (currentInteractable == null)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    currentInteractable = interactable;
                }
            }
        }

        // 상태 변경 이벤트 발생
        if (HasInteractable && (previousInteractable != currentInteractable ||
                                previousWeapon != currentWeaponPickup ||
                                previousThrownWeapon != currentThrownWeapon))
        {
            OnInteractableDetected?.Invoke(currentInteractable);
        }
        else if (!HasInteractable && (previousInteractable != null || previousWeapon != null || previousThrownWeapon != null))
        {
            OnInteractableLost?.Invoke();
        }
    }

    /// <summary>
    /// 상호작용 실행 (InputHandler에서 호출)
    /// </summary>
    public void PerformInteraction()
    {
        // 무기 픽업이 최우선
        if (currentWeaponPickup != null)
        {
            PickupWeapon(currentWeaponPickup);
            return;
        }

        // 던져진 무기 픽업
        if (currentThrownWeapon != null)
        {
            PickupThrownWeapon(currentThrownWeapon);
            return;
        }

        // 일반 상호작용
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    /// <summary>
    /// 무기 픽업 처리
    /// </summary>
    private void PickupWeapon(WeaponPickup weaponPickup)
    {
        if (combatSystem == null) return;

        combatSystem.ProcessWeaponPickup(weaponPickup, null);
    }

    /// <summary>
    /// 던진 무기 픽업 처리
    /// </summary>
    private void PickupThrownWeapon(ThrownWeapon thrownWeapon)
    {
        if (combatSystem == null) return;

        combatSystem.ProcessWeaponPickup(null, thrownWeapon);
    }

    /// <summary>
    /// 디버그 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (interactionPoint == null) return;

        // 상호작용 감지 범위
        Gizmos.color = HasInteractable ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
    }

    /// <summary>
    /// 현재 상호작용 대상 이름 (디버그용)
    /// </summary>
    public string GetCurrentInteractionName()
    {
        if (currentWeaponPickup != null)
            return "Weapon Pickup";
        if (currentThrownWeapon != null)
            return "Thrown Weapon";
        if (currentInteractable != null)
            return currentInteractable.ToString();

        return "None";
    }
}
