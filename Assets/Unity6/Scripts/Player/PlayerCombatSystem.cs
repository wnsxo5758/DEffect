using System;
using UnityEngine;

/// <summary>
/// 플레이어 전투 시스템의 중앙 관리자
/// 단일 책임 원칙(SRP): 전투 관련 상태 및 컴포넌트 조율만 담당
/// 개방-폐쇄 원칙(OCP): 새로운 공격 타입 추가 시 상속이 아닌 조합으로 확장
/// </summary>
public class PlayerCombatSystem : MonoBehaviour
{
    [Header("Combat Components")]
    [SerializeField] private PlayerMeleeAttack meleeAttack;
    [SerializeField] private PlayerRangedAttack rangedAttack;
    [SerializeField] private PlayerTeleportAttack teleportAttack;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject thrownWeaponPrefab;

    // 전투 이벤트
    public event Action OnCombatStateChanged;
    public event Action<WeaponBase> OnWeaponEquipped;
    public event Action OnWeaponUnequipped;

    // 현재 무기 상태
    private WeaponBase currentWeapon;
    private bool hasWeapon = false;

    // 컴포넌트 참조
    public PlayerMeleeAttack MeleeAttack => meleeAttack;
    public PlayerRangedAttack RangedAttack => rangedAttack;
    public PlayerTeleportAttack TeleportAttack => teleportAttack;

    // 무기 상태 프로퍼티
    public bool HasWeapon => hasWeapon;
    public WeaponBase CurrentWeapon => currentWeapon;
    public GameObject ThrownWeaponPrefab => thrownWeaponPrefab;

    // 내부 참조
    private PlayerAnimator playerAnimator;

    // 컴포넌트 동적 관리 플래그
    private bool isMeleeEnabled = false;
    private bool isRangedEnabled = false;
    private bool isTeleportEnabled = false;

    private void Awake()
    {
        // 기존 컴포넌트 참조 (있으면 활성화 상태 기록)
        meleeAttack = GetComponent<PlayerMeleeAttack>();
        rangedAttack = GetComponent<PlayerRangedAttack>();
        teleportAttack = GetComponent<PlayerTeleportAttack>();
        playerAnimator = GetComponent<PlayerAnimator>();

        // 시작 시 컴포넌트가 있으면 활성화 상태로 설정
        isMeleeEnabled = meleeAttack != null;
        isRangedEnabled = rangedAttack != null;
        isTeleportEnabled = teleportAttack != null;
    }

    /// <summary>
    /// 무기 장착
    /// </summary>
    public void EquipWeapon(WeaponBase weaponData)
    {
        if (currentWeapon != null)
        {
            UnequipWeapon();
        }

        currentWeapon = weaponData;
        hasWeapon = true;

        // 각 공격 컴포넌트에 무기 정보 전달
        if (meleeAttack != null)
            meleeAttack.OnWeaponEquipped(weaponData);

        if (rangedAttack != null)
            rangedAttack.OnWeaponEquipped();

        // 애니메이터에 무기 장착 상태 즉시 반영
        if (playerAnimator != null)
            playerAnimator.UpdateWeaponState(true);

        OnWeaponEquipped?.Invoke(weaponData);
        OnCombatStateChanged?.Invoke();
    }

    /// <summary>
    /// 무기 해제
    /// </summary>
    public void UnequipWeapon()
    {
        if (currentWeapon == null) return;

        currentWeapon = null;
        hasWeapon = false;

        // 각 공격 컴포넌트에 무기 해제 알림
        if (meleeAttack != null)
            meleeAttack.OnWeaponUnequipped();

        if (rangedAttack != null)
            rangedAttack.OnWeaponUnequipped();

        // 애니메이터에 무기 해제 상태 즉시 반영
        if (playerAnimator != null)
            playerAnimator.UpdateWeaponState(false);

        OnWeaponUnequipped?.Invoke();
        OnCombatStateChanged?.Invoke();
    }

    /// <summary>
    /// 근접 공격 가능 여부
    /// </summary>
    public bool CanPerformMeleeAttack()
    {
        return hasWeapon && meleeAttack != null && meleeAttack.CanAttack();
    }

    /// <summary>
    /// 원거리 공격(던지기) 가능 여부
    /// </summary>
    public bool CanPerformRangedAttack()
    {
        return hasWeapon && rangedAttack != null && rangedAttack.CanThrow();
    }

    /// <summary>
    /// 텔레포트 가능 여부
    /// </summary>
    public bool CanPerformTeleport()
    {
        return teleportAttack != null && teleportAttack.CanTeleport();
    }

    /// <summary>
    /// 리스폰 시 전투 상태 초기화
    /// </summary>
    public void ResetOnRespawn()
    {
        // 던진 무기 회수
        if (rangedAttack != null && !hasWeapon)
        {
            rangedAttack.RecallThrownWeapon();
        }

        // 각 컴포넌트 리셋
        meleeAttack?.ResetState();
        rangedAttack?.ResetState();
        teleportAttack?.ResetState();
    }

    /// <summary>
    /// 무기 픽업 처리 (레거시 시스템과의 호환성)
    /// </summary>
    public void ProcessWeaponPickup(WeaponPickup weaponPickup, ThrownWeapon thrownWeapon)
    {
        // 던져진 무기 픽업
        if (thrownWeapon != null && rangedAttack != null)
        {
            rangedAttack.ProcessThrownWeaponPickup(thrownWeapon);
            return;
        }

        // 일반 무기 픽업
        if (weaponPickup != null)
        {
            WeaponBase weaponData = weaponPickup.GetWeaponData();
            if (weaponData != null)
            {
                // 첫 무기 획득 시 기본 전투 컴포넌트 자동 활성화
                if (!hasWeapon)
                {
                    EnableBasicCombat();
                }

                thrownWeaponPrefab = weaponPickup.GetWeaponPrefab();
                EquipWeapon(weaponData);
                Destroy(weaponPickup.gameObject);
            }
        }
    }

    /// <summary>
    /// 기본 전투 능력 활성화 (첫 무기 획득 시)
    /// 근접 공격 + 원거리 던지기
    /// </summary>
    private void EnableBasicCombat()
    {
        Debug.Log("[PlayerCombatSystem] 기본 전투 능력 활성화!");

        // 근접 공격 활성화
        if (!isMeleeEnabled)
        {
            EnableMeleeAttack();
        }

        // 원거리 공격 활성화
        if (!isRangedEnabled)
        {
            EnableRangedAttack();
        }
    }

    /// <summary>
    /// 근접 공격 컴포넌트 활성화
    /// </summary>
    public void EnableMeleeAttack()
    {
        if (isMeleeEnabled) return;

        if (meleeAttack == null)
        {
            meleeAttack = gameObject.AddComponent<PlayerMeleeAttack>();
            Debug.Log("[PlayerCombatSystem] PlayerMeleeAttack 컴포넌트 추가됨!");
        }

        isMeleeEnabled = true;
    }

    /// <summary>
    /// 원거리 공격 컴포넌트 활성화
    /// </summary>
    public void EnableRangedAttack()
    {
        if (isRangedEnabled) return;

        if (rangedAttack == null)
        {
            rangedAttack = gameObject.AddComponent<PlayerRangedAttack>();
            Debug.Log("[PlayerCombatSystem] PlayerRangedAttack 컴포넌트 추가됨!");

            // StateMachine에 이벤트 구독 요청
            StateMachine stateMachine = GetComponent<StateMachine>();
            if (stateMachine != null)
            {
                stateMachine.SubscribeToRangedAttackEvents();
            }
        }

        isRangedEnabled = true;
    }

    /// <summary>
    /// 텔레포트 공격 컴포넌트 활성화 (스킬 획득 시)
    /// </summary>
    public void EnableTeleportAttack()
    {
        if (isTeleportEnabled) return;

        if (teleportAttack == null)
        {
            teleportAttack = gameObject.AddComponent<PlayerTeleportAttack>();
            Debug.Log("[PlayerCombatSystem] PlayerTeleportAttack 컴포넌트 추가됨! 텔레포트 스킬 해금!");
        }

        isTeleportEnabled = true;
    }

    /// <summary>
    /// 특정 전투 능력 비활성화 (필요시)
    /// </summary>
    public void DisableCombatAbility(System.Type componentType)
    {
        if (componentType == typeof(PlayerMeleeAttack) && meleeAttack != null)
        {
            Destroy(meleeAttack);
            meleeAttack = null;
            isMeleeEnabled = false;
        }
        else if (componentType == typeof(PlayerRangedAttack) && rangedAttack != null)
        {
            Destroy(rangedAttack);
            rangedAttack = null;
            isRangedEnabled = false;
        }
        else if (componentType == typeof(PlayerTeleportAttack) && teleportAttack != null)
        {
            Destroy(teleportAttack);
            teleportAttack = null;
            isTeleportEnabled = false;
        }
    }
}
