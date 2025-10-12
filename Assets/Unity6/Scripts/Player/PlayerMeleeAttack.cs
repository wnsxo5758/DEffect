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
        new Vector2(0, 0),
        new Vector2(0, 1f),
        new Vector2(1f, 0.5f),
        new Vector2(1f, -0.5f)
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

    private void Awake()
    {
        InitializeAttackCollider();
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
    /// 적 피격 처리
    /// </summary>
    private void ProcessEnemyHit(Collider2D enemyCollider)
    {
        EnemyBT enemy = enemyCollider.GetComponent<EnemyBT>();
        ITimeAffected timeAffected = enemyCollider.GetComponent<ITimeAffected>();

        if (enemy != null)
        {
            // 공격 방향 계산
            Vector2 attackDirection = (enemyCollider.transform.position - transform.position).normalized;
            int damage = currentWeapon.Damage;

            // 시간 정지 중인지 확인
            if (TimeManager.Instance != null && TimeManager.Instance.IsTimeFrozen() && timeAffected != null)
            {
                // 시간 정지 중 데미지 누적
                TimeManager.Instance.ApplyDamageInFrozenTime(timeAffected, damage, attackDirection);

                // 시각 효과만 표시
                if (CameraController.Instance != null)
                {
                    CameraController.Instance.ShakeScreen(0.1f, 0.05f, 0.05f);
                }
            }
            else
            {
                // 일반 데미지 적용
                enemy.DecreaseHp(damage);

                if (CameraController.Instance != null)
                {
                    CameraController.Instance.ShakeScreen();
                }
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
