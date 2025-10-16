using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 원거리 공격(무기 던지기) 전담 컴포넌트
/// 단일 책임 원칙(SRP): 무기 던지기 로직만 담당
/// </summary>
public class PlayerRangedAttack : MonoBehaviour
{
    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 7f;      // 수평 던지기 힘 (낮춤)
    [SerializeField] private float throwUpwardForce = 1.5f; // 상승 던지기 힘 (낮춤)
    [SerializeField] private Vector2 throwOffset = new Vector2(0, 0.6f);

    [Header("Cooldown")]
    [SerializeField] private float throwCooldown = 1f;

    // 던진 무기 추적
    private ThrownWeapon lastThrownWeapon;

    // 던지기 상태
    private bool canThrow = false;
    private bool isThrowingWeapon = false;
    private float throwDirection = 1f; // 던지기 시작 시 캡처된 방향 (1: 오른쪽, -1: 왼쪽)

    // 무기 뽑기 상태
    private WeaponPullContext pendingPullContext;
    private bool hasPendingWeaponPull = false;

    // 이벤트
    public event Action OnThrowStarted;
    public event Action OnThrowFinished;
    public event Action<WeaponPullContext> OnWeaponPullStarted;

    // 외부 참조용 프로퍼티
    public bool IsThrowingWeapon => isThrowingWeapon;
    public ThrownWeapon LastThrownWeapon => lastThrownWeapon;
    public bool HasPendingWeaponPull => hasPendingWeaponPull;
    public WeaponPullContext PendingPullContext => pendingPullContext;

    // 컴포넌트 참조
    private PlayerCombatSystem combatSystem;
    private PlayerMovement movement;

    private void Awake()
    {
        combatSystem = GetComponent<PlayerCombatSystem>();
        movement = GetComponent<PlayerMovement>();
    }

    /// <summary>
    /// 무기 장착 시 호출
    /// </summary>
    public void OnWeaponEquipped()
    {
        canThrow = true;
        lastThrownWeapon = null;
    }

    /// <summary>
    /// 무기 해제 시 호출
    /// </summary>
    public void OnWeaponUnequipped()
    {
        canThrow = false;
    }

    /// <summary>
    /// 던지기 가능 여부
    /// </summary>
    public bool CanThrow()
    {
        return combatSystem.HasWeapon && canThrow && !isThrowingWeapon;
    }

    /// <summary>
    /// 무기 던지기 시작 (State에서 호출)
    /// </summary>
    public void StartThrow()
    {
        if (!CanThrow()) return;

        isThrowingWeapon = true;
        canThrow = false;

        // 던지기 시작 시 방향 캡처 (애니메이션 중 입력 변경에 영향 받지 않도록)
        throwDirection = movement != null ? movement.FacingDirection : 1f;

        OnThrowStarted?.Invoke();
    }

    /// <summary>
    /// 무기 던지기 실행 (애니메이션 이벤트에서 호출)
    /// </summary>
    public void ExecuteThrow()
    {
        if (combatSystem.CurrentWeapon == null || combatSystem.ThrownWeaponPrefab == null)
        {
            FinishThrow();
            return;
        }

        // 던지기 시작 시 캡처된 방향 사용 (애니메이션 중 입력 변경에 영향 받지 않음)
        Vector2 direction = new Vector2(throwDirection, 0).normalized;
        Vector2 spawnPosition = (Vector2)transform.position + throwOffset;

        // 던진 무기 생성
        GameObject thrownWeaponObj = Instantiate(
            combatSystem.ThrownWeaponPrefab,
            spawnPosition,
            Quaternion.identity
        );

        ThrownWeapon thrownWeapon = thrownWeaponObj.GetComponent<ThrownWeapon>();

        if (thrownWeapon != null)
        {
            // 던지는 힘 계산 (포물선)
            Vector2 throwForceVector = direction * throwForce + Vector2.up * throwUpwardForce;

            // 무기 초기화
            thrownWeapon.Initialize(
                combatSystem.CurrentWeapon,
                throwForceVector,
                direction,
                transform.position
            );

            lastThrownWeapon = thrownWeapon;
        }

        // 무기 해제
        combatSystem.UnequipWeapon();

        // 텔레포트 활성화 (무기 던진 후)
        if (combatSystem.TeleportAttack != null)
        {
            combatSystem.TeleportAttack.EnableTeleport();
        }

        // 쿨다운 시작
        StartCoroutine(ThrowCooldownTimer());
    }

    /// <summary>
    /// 던지기 종료 (애니메이션 이벤트에서 호출)
    /// </summary>
    public void FinishThrow()
    {
        isThrowingWeapon = false;
        OnThrowFinished?.Invoke();
    }

    /// <summary>
    /// 던진 무기 픽업 처리
    /// </summary>
    public void ProcessThrownWeaponPickup(ThrownWeapon thrownWeapon)
    {
        // 무기를 이미 가지고 있거나, 이미 뽑기 진행 중이면 중단
        if (combatSystem.HasWeapon) return;
        if (hasPendingWeaponPull) return;
        if (thrownWeapon == null) return;

        bool playerGrounded = movement != null && movement.IsGrounded();

        // 무기 뽑기 컨텍스트 생성
        WeaponPullContext pullContext = new WeaponPullContext(
            thrownWeapon,
            transform.position,
            playerGrounded
        );

        // 뽑기 시작
        thrownWeapon.StartPullFromEnemy();

        // 상태 저장
        pendingPullContext = pullContext;
        hasPendingWeaponPull = true;

        OnWeaponPullStarted?.Invoke(pullContext);
    }

    /// <summary>
    /// 무기 뽑기 완료 (애니메이션 이벤트에서 호출)
    /// </summary>
    public void CompleteWeaponPull()
    {
        if (pendingPullContext == null || pendingPullContext.targetWeapon == null)
        {
            ClearPendingPull();
            return;
        }

        // 무기 장착
        WeaponBase weaponData = pendingPullContext.targetWeapon.GetWeaponData();
        if (weaponData != null)
        {
            combatSystem.EquipWeapon(weaponData);
        }

        // 무기 오브젝트 제거
        Destroy(pendingPullContext.targetWeapon.gameObject);

        // 텔레포트로 뽑은 경우 추적 정리
        if (pendingPullContext.isTeleportPull)
        {
            lastThrownWeapon = null;
        }

        // 무기 회수 시 텔레포트 비활성화
        if (combatSystem.TeleportAttack != null)
        {
            combatSystem.TeleportAttack.DisableTeleport();
        }

        ClearPendingPull();
    }

    /// <summary>
    /// 무기 뽑기 데미지 적용 (애니메이션 이벤트에서 호출)
    /// </summary>
    public void ApplyWeaponPullDamage()
    {
        if (pendingPullContext?.targetWeapon != null)
        {
            pendingPullContext.targetWeapon.ApplyPullDamage();
        }
    }

    /// <summary>
    /// pending 상태 초기화
    /// </summary>
    private void ClearPendingPull()
    {
        pendingPullContext = null;
        hasPendingWeaponPull = false;
    }

    /// <summary>
    /// 무기 뽑기 컨텍스트 설정 (텔레포트에서 호출)
    /// </summary>
    public void SetPendingPullContext(WeaponPullContext context)
    {
        pendingPullContext = context;
        hasPendingWeaponPull = context != null;
    }

    /// <summary>
    /// 낙사한 무기 즉시 회수
    /// </summary>
    public void RecallWeaponFromFall(ThrownWeapon weapon)
    {
        if (weapon == null) return;

        // 이미 무기를 가지고 있으면 파괴만
        if (combatSystem.HasWeapon)
        {
            Destroy(weapon.gameObject);
            return;
        }

        // 즉시 무기 장착
        WeaponBase weaponData = weapon.GetWeaponData();
        if (weaponData != null)
        {
            combatSystem.EquipWeapon(weaponData);
        }

        if (lastThrownWeapon == weapon)
        {
            lastThrownWeapon = null;
        }

        // 텔레포트 비활성화
        if (combatSystem.TeleportAttack != null)
        {
            combatSystem.TeleportAttack.DisableTeleport();
        }

        Destroy(weapon.gameObject);
    }

    /// <summary>
    /// 던진 무기 회수 (리스폰 시)
    /// </summary>
    public void RecallThrownWeapon()
    {
        if (lastThrownWeapon != null && !combatSystem.HasWeapon)
        {
            RecallWeaponFromFall(lastThrownWeapon);
        }
    }

    /// <summary>
    /// 던지기 쿨타임 코루틴
    /// </summary>
    private IEnumerator ThrowCooldownTimer()
    {
        yield return new WaitForSeconds(throwCooldown);
        canThrow = !isThrowingWeapon; // 던지는 중이 아니면 다시 활성화
    }

    /// <summary>
    /// 상태 리셋 (리스폰 시 호출)
    /// </summary>
    public void ResetState()
    {
        isThrowingWeapon = false;
        canThrow = combatSystem.HasWeapon;

        ClearPendingPull();
    }
}
