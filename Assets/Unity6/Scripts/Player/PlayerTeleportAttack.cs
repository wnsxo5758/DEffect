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
    /// 텔레포트 위치 계산
    /// </summary>
    private Vector3 CalculateTeleportPosition(ThrownWeapon weapon)
    {
        // 무기가 박혀있지 않으면 무기 위치로 이동
        if (!weapon.IsStuck())
        {
            return weapon.transform.position;
        }

        // 박힌 무기의 법선 벡터 기반 위치 계산
        Vector2 contactNormal = weapon.GetContactNormal();
        Vector2 weaponPos = weapon.transform.position;

        Collider2D playerCollider = GetComponent<Collider2D>();
        Vector2 playerSize = playerCollider.bounds.size;

        float absNormalX = Mathf.Abs(contactNormal.x);
        float absNormalY = Mathf.Abs(contactNormal.y);

        Vector2 teleportDirection;
        float teleportDistance;

        // 수평 벽
        if (absNormalX > absNormalY)
        {
            float diagonalX = Mathf.Sign(contactNormal.x);
            teleportDirection = new Vector2(diagonalX, 1f).normalized;
            teleportDistance = playerSize.x * 2f;
        }
        // 수직 벽
        else
        {
            teleportDirection = contactNormal.y < 0 ? Vector2.down : Vector2.up;
            teleportDistance = playerSize.y;
        }

        return weaponPos + teleportDirection * teleportDistance;
    }

    /// <summary>
    /// 텔레포트 중 무기 처리
    /// </summary>
    private void HandleWeaponDuringTeleport(ThrownWeapon weapon, bool wasAttachedToEnemy)
    {
        // 무기 데이터 백업 (필요 시)
        WeaponBase weaponData = weapon.GetWeaponData();

        // 적에게 붙어있지 않은 경우에만 즉시 무기 제거
        if (!wasAttachedToEnemy)
        {
            Destroy(weapon.gameObject);
        }
        else
        {
            // 적에게 붙어있던 무기는 보이지 않게 처리
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
    /// 텔레포트 가능 위치인지 검증
    /// </summary>
    private bool IsTeleportPossible(ThrownWeapon weapon)
    {
        if (weapon == null) return false;

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return false;

        Vector2 playerSize = playerCollider.bounds.size;

        // 무기가 박혀있지 않으면 무기 위치로 이동 가능
        if (!weapon.IsStuck())
        {
            return true;
        }

        Vector2 contactNormal = weapon.GetContactNormal();
        float absNormalX = Mathf.Abs(contactNormal.x);
        float absNormalY = Mathf.Abs(contactNormal.y);

        Vector2 teleportDirection;
        float teleportDistance;

        // 법선 벡터가 수평 방향인 경우
        if (absNormalX > absNormalY)
        {
            float diagonalX = Mathf.Sign(contactNormal.x);
            teleportDirection = new Vector2(diagonalX, 1f).normalized;
            teleportDistance = playerSize.x * 2f;
        }
        // 법선 벡터가 수직 방향인 경우
        else
        {
            teleportDirection = contactNormal.y < 0 ? Vector2.down : Vector2.up;
            teleportDistance = playerSize.y;
        }

        // 텔레포트 위치 계산
        Vector2 weaponPosition = weapon.transform.position;
        Vector2 basePosition = weaponPosition + teleportDirection * teleportDistance;

        // 안전한 위치인지 확인
        return IsSafeLocation(basePosition, weapon);
    }

    /// <summary>
    /// 안전한 텔레포트 위치인지 확인
    /// </summary>
    private bool IsSafeLocation(Vector2 position, ThrownWeapon weapon)
    {
        Vector2 weaponPosition = weapon.transform.position;
        Vector2 contactNormal = weapon.GetContactNormal();

        float offsetDistance = 0.2f;
        Vector2 adjustedStartPosition;

        if (weapon.IsStuck() && contactNormal != Vector2.zero)
        {
            adjustedStartPosition = weaponPosition - contactNormal * offsetDistance;
        }
        else
        {
            adjustedStartPosition = weaponPosition + Vector2.up;
        }

        // 경로 상의 장애물 체크
        RaycastHit2D[] pathHits = Physics2D.LinecastAll(
            adjustedStartPosition,
            position,
            weapon.StickLayers
        );

        foreach (RaycastHit2D hit in pathHits)
        {
            if (hit.collider != null && !IsWeaponOrItsParent(hit.collider.gameObject, weapon.gameObject))
            {
                Debug.Log($"물체 감지: {hit.collider.gameObject}");
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
    /// 디버그용 기즈모 (무기 위치와 텔레포트 위치 표시)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (rangedAttack == null || rangedAttack.LastThrownWeapon == null) return;

        ThrownWeapon weapon = rangedAttack.LastThrownWeapon;
        if (!weapon.IsStuck()) return;

        Vector2 normal = weapon.GetContactNormal();
        Vector3 weaponPos = weapon.transform.position;

        if (normal == Vector2.zero) return;

        // 법선 벡터 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(weaponPos, normal);

        // 텔레포트 위치 계산 및 표시
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return;

        Vector2 playerSize = playerCollider.bounds.size;
        float absNormalX = Mathf.Abs(normal.x);
        float absNormalY = Mathf.Abs(normal.y);

        Vector2 teleportDirection;
        float teleportDistance;

        if (absNormalX > absNormalY)
        {
            float diagonalX = Mathf.Sign(normal.x);
            teleportDirection = new Vector2(diagonalX, 1f).normalized;
            teleportDistance = playerSize.x * 2f;
        }
        else
        {
            teleportDirection = normal.y < 0 ? Vector2.down : Vector2.up;
            teleportDistance = playerSize.y;
        }

        Vector2 teleportPos = (Vector2)weaponPos + teleportDirection * teleportDistance;

        // 안전한 위치면 녹색, 아니면 빨간색
        Gizmos.color = IsTeleportPossible(weapon) ? Color.green : Color.red;
        Gizmos.DrawLine(weaponPos, teleportPos);
        Gizmos.DrawWireSphere(teleportPos, 0.4f);
    }
}
