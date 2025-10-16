using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 텔레포트 공격 전담 컴포넌트
/// 단일 책임 원칙(SRP): 텔레포트 로직만 담당
/// </summary>
public class PlayerTeleportAttack : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private float teleportCooldown = 3f;
    [SerializeField] private float teleportDelay = 0.1f; // 텔레포트 이펙트 딜레이

    // 텔레포트 상태
    private bool canTeleport = false;
    private bool isTeleporting = false;
    private ThrownWeapon pendingTeleportWeapon;

    // 이벤트
    public event Action OnTeleportStarted;
    public event Action OnTeleportCompleted;
    public event Action<WeaponPullContext> OnTeleportPullRequired; // 적에 박힌 무기로 텔레포트한 경우

    // 외부 참조용 프로퍼티
    public bool IsTeleporting => isTeleporting;
    public bool CanTeleport() => canTeleport;

    // 컴포넌트 참조
    private PlayerRangedAttack rangedAttack;
    private PlayerCombatSystem combatSystem;

    private void Awake()
    {
        rangedAttack = GetComponent<PlayerRangedAttack>();
        combatSystem = GetComponent<PlayerCombatSystem>();
    }

    /// <summary>
    /// 텔레포트 가능 상태로 변경 (무기 던진 후)
    /// </summary>
    public void EnableTeleport()
    {
        canTeleport = true;
    }

    /// <summary>
    /// 텔레포트 불가 상태로 변경
    /// </summary>
    public void DisableTeleport()
    {
        canTeleport = false;
    }

    /// <summary>
    /// 텔레포트 시작 (State에서 호출)
    /// </summary>
    public bool StartTeleport()
    {
        // 스킬 보유 확인
        if (SkillManager.Instance == null || !SkillManager.Instance.HasSkill(SkillType.Teleport))
        {
            Debug.Log("텔레포트 스킬을 보유하고 있지 않습니다.");
            return false;
        }

        // 던진 무기가 없으면 실패
        if (rangedAttack.LastThrownWeapon == null)
        {
            Debug.Log("던진 무기가 없습니다.");
            return false;
        }

        // 텔레포트 가능 여부 확인
        if (!canTeleport || isTeleporting)
        {
            return false;
        }

        // 텔레포트 가능 위치 검증
        if (!IsTeleportPossible(rangedAttack.LastThrownWeapon))
        {
            Debug.Log("텔레포트 불가능한 위치입니다.");
            return false;
        }

        // 텔레포트 시작
        isTeleporting = true;
        canTeleport = false;
        pendingTeleportWeapon = rangedAttack.LastThrownWeapon;

        OnTeleportStarted?.Invoke();
        return true;
    }

    /// <summary>
    /// 텔레포트 실행 (애니메이션 이벤트에서 호출)
    /// </summary>
    public void ExecuteTeleport()
    {
        if (pendingTeleportWeapon == null)
        {
            CompleteTeleport(false);
            return;
        }

        StartCoroutine(TeleportMovementCoroutine());
    }

    /// <summary>
    /// 텔레포트 이동 코루틴
    /// </summary>
    private IEnumerator TeleportMovementCoroutine()
    {
        ThrownWeapon targetWeapon = pendingTeleportWeapon;

        // 적에게 박힌 무기인지 확인
        bool wasAttachedToEnemy = targetWeapon != null &&
                                   targetWeapon.transform.parent != null &&
                                   targetWeapon.transform.parent.CompareTag("Enemy");

        // 무기가 파괴되었는지 확인
        if (targetWeapon == null)
        {
            CompleteTeleport(false);
            yield break;
        }

        // 텔레포트 위치 계산
        Vector3 teleportPosition = CalculateTeleportPosition(targetWeapon);

        // 플레이어 위치 이동
        transform.position = teleportPosition;

        // 짧은 대기 시간 (텔레포트 이펙트용)
        yield return new WaitForSeconds(teleportDelay);

        // 무기 처리
        HandleWeaponDuringTeleport(targetWeapon, wasAttachedToEnemy);

        // 텔레포트 완료 처리
        CompleteTeleport(wasAttachedToEnemy);
    }

    /// <summary>
    /// 텔레포트 위치 계산 - 무기 위치로 직접 이동
    /// </summary>
    private Vector3 CalculateTeleportPosition(ThrownWeapon weapon)
    {
        // 박힌 무기든 아니든 무기 위치로 직접 이동
        return weapon.transform.position;
    }

    /// <summary>
    /// 텔레포트 중 무기 처리 - 무기 위치 도착 후 처리
    /// </summary>
    private void HandleWeaponDuringTeleport(ThrownWeapon weapon, bool wasAttachedToEnemy)
    {
        // 적에게 붙어있던 무기는 뽑기 애니메이션을 위해 보이지 않게 처리
        if (wasAttachedToEnemy)
        {
            weapon.SetVisible(false);
        }
        else
        {
            // 적에게 붙어있지 않은 경우 (바닥에 떨어진 무기)
            // 무기는 CompleteTeleport에서 장착되고 제거됨
            weapon.SetVisible(false);
        }
    }

    /// <summary>
    /// 텔레포트 완료
    /// </summary>
    private void CompleteTeleport(bool wasAttachedToEnemy)
    {
        isTeleporting = false;

        // 적에게 박힌 무기로 텔레포트한 경우 무기 뽑기 필요
        if (wasAttachedToEnemy && pendingTeleportWeapon != null)
        {
            // 무기 뽑기 컨텍스트 생성
            WeaponPullContext pullContext = WeaponPullContext.CreateTeleportPull(
                pendingTeleportWeapon,
                transform.position,
                GetComponent<PlayerMovement>()?.IsGrounded() ?? false
            );

            OnTeleportPullRequired?.Invoke(pullContext);
        }
        else
        {
            // 바닥에 떨어진 무기로 텔레포트한 경우 즉시 장착
            if (pendingTeleportWeapon != null)
            {
                WeaponBase weaponData = pendingTeleportWeapon.GetWeaponData();
                if (weaponData != null)
                {
                    combatSystem.EquipWeapon(weaponData);
                }
            }

            OnTeleportCompleted?.Invoke();
        }

        // 정리
        pendingTeleportWeapon = null;

        // 쿨다운 시작
        StartCoroutine(TeleportCooldownTimer());
    }

    /// <summary>
    /// 텔레포트 가능 위치인지 검증 - 플레이어와 무기 사이 직선 경로 체크
    /// </summary>
    private bool IsTeleportPossible(ThrownWeapon weapon)
    {
        if (weapon == null) return false;

        Vector2 playerPosition = transform.position;
        Vector2 weaponPosition = weapon.transform.position;

        // Object와 Ground 레이어 마스크 (충돌 체크할 레이어)
        int objectLayer = LayerMask.NameToLayer("Object");
        int groundLayer = LayerMask.NameToLayer("Ground");
        LayerMask obstacleLayers = (1 << objectLayer) | (1 << groundLayer);

        // 플레이어와 무기 사이의 직선 경로에 장애물이 있는지 체크
        RaycastHit2D[] hits = Physics2D.LinecastAll(playerPosition, weaponPosition, obstacleLayers);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && !IsWeaponOrItsParent(hit.collider.gameObject, weapon.gameObject))
            {
                Debug.Log($"텔레포트 경로 상 장애물 감지: {hit.collider.gameObject.name}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 무기 또는 무기의 부모인지 확인
    /// </summary>
    private bool IsWeaponOrItsParent(GameObject obj, GameObject weapon)
    {
        if (obj == weapon) return true;

        Transform weaponParent = weapon.transform.parent;
        if (weaponParent != null && obj == weaponParent.gameObject) return true;

        return false;
    }

    /// <summary>
    /// 텔레포트 쿨타임 코루틴
    /// </summary>
    private IEnumerator TeleportCooldownTimer()
    {
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }

    /// <summary>
    /// 상태 리셋 (리스폰 시 호출)
    /// </summary>
    public void ResetState()
    {
        isTeleporting = false;
        canTeleport = false;
        pendingTeleportWeapon = null;
    }

    /// <summary>
    /// 디버그용 기즈모 (플레이어와 무기 사이 직선 경로 표시)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (rangedAttack == null || rangedAttack.LastThrownWeapon == null) return;

        ThrownWeapon weapon = rangedAttack.LastThrownWeapon;
        Vector3 playerPos = transform.position;
        Vector3 weaponPos = weapon.transform.position;

        // 텔레포트 가능 여부에 따라 색상 변경
        bool canTeleportToWeapon = IsTeleportPossible(weapon);
        Gizmos.color = canTeleportToWeapon ? Color.green : Color.red;

        // 플레이어 → 무기 직선 경로 표시
        Gizmos.DrawLine(playerPos, weaponPos);

        // 무기 위치 표시
        Gizmos.DrawWireSphere(weaponPos, 0.3f);

        // 플레이어 위치 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerPos, 0.2f);
    }
}
