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
    [SerializeField] private float teleportDuration = 0.15f; // 텔레포트 이동 시간 (초)
    [SerializeField] private float safeDistanceOffset = 0.8f; // 무기로부터 안전 거리 (플레이어 쪽으로)

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
    private Rigidbody2D rb;

    private void Awake()
    {
        rangedAttack = GetComponent<PlayerRangedAttack>();
        combatSystem = GetComponent<PlayerCombatSystem>();
        rb = GetComponent<Rigidbody2D>();
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
        // TODO: 나중에 스킬 획득 시스템 구현 시 활성화
        // 현재는 컴포넌트가 있으면 사용 가능
        /*
        if (SkillManager.Instance == null || !SkillManager.Instance.HasSkill(SkillType.Teleport))
        {
            Debug.Log("텔레포트 스킬을 보유하고 있지 않습니다.");
            return false;
        }
        */

        // 던진 무기가 없으면 실패
        if (rangedAttack == null || rangedAttack.LastThrownWeapon == null)
        {
            Debug.Log("[PlayerTeleportAttack] 던진 무기가 없습니다.");
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
            Debug.Log("[PlayerTeleportAttack] 텔레포트 불가능한 위치입니다 (장애물 감지).");
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
    /// 텔레포트 이동 코루틴 - 고정 시간 보간 방식
    /// </summary>
    private IEnumerator TeleportMovementCoroutine()
    {
        ThrownWeapon targetWeapon = pendingTeleportWeapon;

        // 무기가 파괴되었는지 확인
        if (targetWeapon == null)
        {
            CompleteTeleport(false);
            yield break;
        }

        // 안전한 목표 위치 계산
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = CalculateSafeTeleportPosition(targetWeapon);

        Debug.Log($"[Teleport] 시작 위치: {startPosition}, 무기 위치: {targetWeapon.transform.position}, 목표 위치: {targetPosition}");
        Debug.Log($"[Teleport] 무기 상태 - IsStuck: {targetWeapon.IsStuck()}, IsStuckOnGround: {targetWeapon.IsStuckOnGround()}, IsStuckToEnemy: {targetWeapon.IsStuckToEnemy()}");

        // 중력 비활성화 (텔레포트 중 낙하 방지)
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        // 시간 기반 보간 이동
        float elapsedTime = 0f;

        while (elapsedTime < teleportDuration)
        {
            // 무기가 파괴되었거나 사라지면 중단
            if (targetWeapon == null)
            {
                rb.gravityScale = originalGravity;
                Debug.LogWarning("[Teleport] 무기가 사라져서 중단!");
                CompleteTeleport(false);
                yield break;
            }

            elapsedTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / teleportDuration);

            // EaseOutCubic으로 부드러운 감속 (빠르게 시작 → 부드럽게 도착)
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            // 위치 보간
            transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);

            yield return new WaitForFixedUpdate();
        }

        // 도착 - 정확한 위치로 스냅
        transform.position = targetPosition;
        rb.linearVelocity = Vector2.zero;

        Debug.Log($"[Teleport] 도착 완료! 최종 위치: {transform.position}, 무기와의 거리: {Vector3.Distance(transform.position, targetWeapon.transform.position):F2}");

        // 중력 복원
        rb.gravityScale = originalGravity;

        // 무기가 박혀있는지 확인 (무기 뽑기 필요 여부 판단)
        bool isWeaponStuck = targetWeapon.IsStuck();

        // 무기 처리
        HandleWeaponDuringTeleport(targetWeapon, isWeaponStuck);

        // 텔레포트 완료 처리
        CompleteTeleport(isWeaponStuck);
    }

    /// <summary>
    /// 안전한 텔레포트 위치 계산
    /// - 땅에 박힌 무기: 법선 벡터(위쪽) 방향으로 안전 거리
    /// - 벽/오브젝트에 박힌 무기: 법선 벡터(벽에서 나오는) 방향으로 안전 거리
    /// - 적에게 박힌 무기: 플레이어→무기 반대 방향으로 안전 거리
    /// - 날아가는 무기: 무기 위치
    /// </summary>
    private Vector3 CalculateSafeTeleportPosition(ThrownWeapon weapon)
    {
        if (weapon == null) return transform.position;

        Vector3 weaponPosition = weapon.transform.position;
        Vector3 playerPosition = transform.position;

        // 무기가 박힌 경우 - 법선 벡터 활용
        if (weapon.IsStuck())
        {
            Vector2 contactNormal = weapon.GetContactNormal();

            // 땅에 박힌 경우 - 법선 벡터는 위쪽
            if (weapon.IsStuckOnGround())
            {
                // 법선 벡터 방향(위쪽)으로 안전 거리만큼 떨어진 위치
                Vector3 safePosition = weaponPosition + (Vector3)contactNormal * safeDistanceOffset;
                return safePosition;
            }
            // 적에게 박힌 경우 - 플레이어 방향으로 오프셋
            else if (weapon.IsStuckToEnemy())
            {
                // 플레이어 → 무기 방향의 반대로 안전 거리
                Vector3 directionToWeapon = (weaponPosition - playerPosition).normalized;
                Vector3 safePosition = weaponPosition - directionToWeapon * safeDistanceOffset;
                return safePosition;
            }
            // 벽/오브젝트 옆면/윗면에 박힌 경우 - 법선 벡터 방향 활용
            else
            {
                // 법선 벡터 방향(벽에서 나오는 방향)으로 안전 거리
                Vector3 safePosition = weaponPosition + (Vector3)contactNormal * safeDistanceOffset;
                return safePosition;
            }
        }
        // 날아가는 중 - 무기 위치 그대로
        else
        {
            return weaponPosition;
        }
    }

    /// <summary>
    /// 텔레포트 중 무기 처리 - 무기 위치 도착 후 처리
    /// </summary>
    private void HandleWeaponDuringTeleport(ThrownWeapon weapon, bool isWeaponStuck)
    {
        // 무기가 박혀있으면 뽑기 애니메이션을 위해 보이지 않게 처리
        if (isWeaponStuck)
        {
            weapon.SetVisible(false);
        }
        else
        {
            // 박히지 않은 무기 (날아가는 중) - 보이지 않게 처리
            weapon.SetVisible(false);
        }
    }

    /// <summary>
    /// 텔레포트 완료
    /// </summary>
    private void CompleteTeleport(bool isWeaponStuck)
    {
        isTeleporting = false;

        // 무기가 박혀있는 경우 → 무기 뽑기 필요
        if (isWeaponStuck && pendingTeleportWeapon != null)
        {
            // 무기 뽑기 컨텍스트 생성
            PlayerMovement movement = GetComponent<PlayerMovement>();
            bool isGrounded = movement != null && movement.IsGrounded();
            float facingDirection = movement != null ? movement.FacingDirection : 1f;

            WeaponPullContext pullContext = WeaponPullContext.CreateTeleportPull(
                pendingTeleportWeapon,
                transform.position,
                isGrounded,
                facingDirection // 텔레포트 시작 시 설정된 방향 전달
            );

            // 무기 뽑기 시작 (PlayerRangedAttack에 컨텍스트 저장)
            pendingTeleportWeapon.StartPullFromEnemy();

            // RangedAttack에 컨텍스트 전달 (Pull 상태들이 사용할 수 있도록)
            if (rangedAttack != null)
            {
                rangedAttack.SetPendingPullContext(pullContext);
            }

            Debug.Log($"[PlayerTeleportAttack] 무기 뽑기 이벤트 발생! isStuck: {isWeaponStuck}, facingDirection: {facingDirection}");
            OnTeleportPullRequired?.Invoke(pullContext);
        }
        // 무기가 박히지 않은 경우 → 즉시 장착
        else
        {
            // 날아가는 중이거나 바닥에 떨어진 무기 → 즉시 장착
            if (pendingTeleportWeapon != null)
            {
                WeaponBase weaponData = pendingTeleportWeapon.GetWeaponData();
                if (weaponData != null)
                {
                    combatSystem.EquipWeapon(weaponData);
                    Destroy(pendingTeleportWeapon.gameObject);
                }
            }

            Debug.Log($"[PlayerTeleportAttack] 텔레포트 완료 이벤트 발생! isStuck: {isWeaponStuck}");
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
    /// 디버그용 기즈모 (플레이어와 무기 사이 직선 경로 + 도착 위치 표시)
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

        // 실제 도착 위치 계산 및 표시
        Vector3 safePosition = CalculateSafeTeleportPosition(weapon);

        // 도착 위치 표시 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(safePosition, 0.4f);
        Gizmos.DrawSphere(safePosition, 0.15f);

        // 무기 → 도착 위치 연결선 (파란색 점선)
        Gizmos.color = Color.blue;
        DrawDottedLine(weaponPos, safePosition, 0.2f);

        // 무기 상태 텍스트 표시
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(weaponPos + Vector3.up * 0.5f,
            $"Weapon: {(weapon.IsStuck() ? "Stuck" : "Flying")}\n" +
            $"Ground: {weapon.IsStuckOnGround()}\n" +
            $"Enemy: {weapon.IsStuckToEnemy()}");

        UnityEditor.Handles.Label(safePosition + Vector3.up * 0.5f,
            $"Target Position\n" +
            $"Offset: {safeDistanceOffset:F2}");
        #endif
    }

    /// <summary>
    /// 점선 그리기 헬퍼 메서드
    /// </summary>
    private void DrawDottedLine(Vector3 start, Vector3 end, float spacing)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        for (float i = 0; i < distance; i += spacing * 2)
        {
            Vector3 dotStart = start + direction * i;
            Vector3 dotEnd = start + direction * Mathf.Min(i + spacing, distance);
            Gizmos.DrawLine(dotStart, dotEnd);
        }
    }
}
