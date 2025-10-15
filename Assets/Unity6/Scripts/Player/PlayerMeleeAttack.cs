using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 근접 공격 전담 컴포넌트
/// 단일 책임 원칙(SRP): 근접 공격 로직만 담당
/// </summary>
public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Vector2 attackOffset = new Vector2(0, 0.6f);
    [SerializeField] private Vector2[] attackPolygonPoints = new Vector2[]
    {
        new Vector2(-0.2f, -0.5f),  // 왼쪽 아래
        new Vector2(-0.2f, 1.5f),   // 왼쪽 위
        new Vector2(1.2f, 1.0f),    // 오른쪽 위
        new Vector2(1.2f, -0.5f)       // 오른쪽 아래
    };

    [Header("Collision Detection")]
    [SerializeField] private LayerMask enemyLayer;

    // 공격 콜라이더
    private PolygonCollider2D attackCollider;
    private GameObject attackColliderObject;

    // 현재 무기 정보
    private WeaponBase currentWeapon;

    // 공격 상태
    private bool canAttack = true;
    private bool isAttacking = false;

    // 이벤트
    public event Action OnAttackStarted;
    public event Action OnAttackFinished;

    // 외부 참조용 프로퍼티
    public bool IsAttacking => isAttacking;

    // 컴포넌트 참조
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    private void Awake()
    {
        // 컴포넌트 참조
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();

        // enemyLayer가 설정되지 않았으면 기본값 설정
        if (enemyLayer == 0)
        {
            InitializeDefaultSettings();
        }

        InitializeAttackCollider();
    }

    /// <summary>
    /// 기본 설정 초기화 (동적 생성 시 호출)
    /// </summary>
    private void InitializeDefaultSettings()
    {
        // Enemy 레이어 자동 설정
        int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        if (enemyLayerIndex != -1)
        {
            enemyLayer = 1 << enemyLayerIndex;
            Debug.Log($"[PlayerMeleeAttack] Enemy 레이어 자동 설정됨: {enemyLayerIndex}");
        }
        else
        {
            Debug.LogWarning("[PlayerMeleeAttack] Enemy 레이어를 찾을 수 없습니다!");
        }

        // 기본 공격 범위 설정 (비어있을 경우) - 더 넓게 조정
        if (attackPolygonPoints == null || attackPolygonPoints.Length == 0)
        {
            attackPolygonPoints = new Vector2[]
            {
                new Vector2(-0.2f, -0.5f),  // 왼쪽 아래
                new Vector2(-0.2f, 1.5f),   // 왼쪽 위
                new Vector2(1.2f, 1.0f),    // 오른쪽 위
                new Vector2(1.2f, -0.5f)       // 오른쪽 아래
            };
        }
    }

    /// <summary>
    /// 외부에서 설정 초기화 (PlayerCombatSystem에서 호출)
    /// </summary>
    public void Initialize(LayerMask enemyMask, Vector2 offset, Vector2[] polygonPoints)
    {
        enemyLayer = enemyMask;
        attackOffset = offset;
        attackPolygonPoints = polygonPoints;

        // 이미 생성된 콜라이더가 있으면 재설정
        if (attackCollider != null)
        {
            attackCollider.SetPath(0, attackPolygonPoints);
        }

        if (attackColliderObject != null)
        {
            attackColliderObject.transform.localPosition = attackOffset;
        }
    }

    /// <summary>
    /// 공격 콜라이더 초기화
    /// </summary>
    private void InitializeAttackCollider()
    {
        attackColliderObject = new GameObject("MeleeAttackCollider")
        {
            transform =
            {
                parent = transform,
                localPosition = attackOffset
            }
        };

        attackCollider = attackColliderObject.AddComponent<PolygonCollider2D>();
        attackCollider.isTrigger = true;
        attackCollider.SetPath(0, attackPolygonPoints);

        attackColliderObject.SetActive(false);
    }

    /// <summary>
    /// 무기 장착 시 호출
    /// </summary>
    public void OnWeaponEquipped(WeaponBase weaponData)
    {
        currentWeapon = weaponData;
        canAttack = true;

        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(true);
        }
    }

    /// <summary>
    /// 무기 해제 시 호출
    /// </summary>
    public void OnWeaponUnequipped()
    {
        currentWeapon = null;
        canAttack = false;

        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(false);
        }
    }

    /// <summary>
    /// 공격 가능 여부
    /// </summary>
    public bool CanAttack()
    {
        return currentWeapon != null && canAttack && !isAttacking;
    }

    /// <summary>
    /// 근접 공격 시작 (State에서 호출)
    /// </summary>
    public void StartAttack()
    {
        if (!CanAttack()) return;

        canAttack = false;
        isAttacking = true;

        OnAttackStarted?.Invoke();

        // 쿨다운 시작
        StartCoroutine(AttackCooldownTimer());
    }

    /// <summary>
    /// 공격 종료 (애니메이션 이벤트에서 호출)
    /// </summary>
    public void FinishAttack()
    {
        isAttacking = false;
        OnAttackFinished?.Invoke();
    }

    /// <summary>
    /// 공격 판정 (애니메이션 이벤트에서 호출)
    /// </summary>
    public void HandleAttackCollision()
    {
        if (currentWeapon == null) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;

        List<Collider2D> results = new List<Collider2D>();
        attackCollider.Overlap(filter, results);

        foreach (Collider2D enemyCollider in results)
        {
            ProcessEnemyHit(enemyCollider);
        }
    }

    /// <summary>
    /// 적 피격 처리 (Unity6 버전 - IDamageable 인터페이스 사용)
    /// </summary>
    private void ProcessEnemyHit(Collider2D enemyCollider)
    {
        // Unity6의 IDamageable 인터페이스 사용
        IDamageable damageable = enemyCollider.GetComponent<IDamageable>();

        if (damageable != null && currentWeapon != null)
        {
            int damage = currentWeapon.Damage;

            // TODO: 시간 정지 기능은 나중에 Unity6으로 이식 후 추가
            // ITimeAffected timeAffected = enemyCollider.GetComponent<ITimeAffected>();
            // if (TimeManager.Instance != null && TimeManager.Instance.IsTimeFrozen() && timeAffected != null)
            // {
            //     TimeManager.Instance.ApplyDamageInFrozenTime(timeAffected, damage, attackDirection);
            // }

            // 일반 데미지 적용
            damageable.DecreaseHp(damage);

            // 카메라 쉐이크 (있으면 실행)
            if (CameraController.Instance != null)
            {
                CameraController.Instance.ShakeScreen();
            }
        }
    }

    /// <summary>
    /// 공격 쿨타임 코루틴
    /// </summary>
    private IEnumerator AttackCooldownTimer()
    {
        if (currentWeapon != null)
        {
            yield return new WaitForSeconds(currentWeapon.AttackCooldown);
        }
        else
        {
            yield return new WaitForSeconds(0.5f); // 기본 쿨타임
        }

        canAttack = true;
    }

    /// <summary>
    /// 상태 리셋 (리스폰 시 호출)
    /// </summary>
    public void ResetState()
    {
        isAttacking = false;
        canAttack = currentWeapon != null;

        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(currentWeapon != null);
        }
    }

    /// <summary>
    /// 현재 무기 정보 반환
    /// </summary>
    public WeaponBase GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
