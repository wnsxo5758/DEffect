using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ThrownWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float stuckDuration = 0.5f;
    [SerializeField] private int extraDamageMultiplier = 2;
    [SerializeField] private LayerMask stickLayers;
    [SerializeField] private float hitStunDuration = 3f; // 던진 무기 피격 시간

    [Header("Collision Settings")]
    [SerializeField] private float colliderRadiusMultiplier = 0.4f; // 콜라이더 크기 배율
    [SerializeField] private float maxAngleForCustomNormal = 100f; // 법선 벡터 커스텀 적용 최대 각도
    
    private WeaponBase weaponData;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;

    private bool isPullDamageApplied = false; // 뽑기 데미지 적용 여부
    private bool isStuck = false;
    private bool canDealDamage = true;
    private Transform stuckTarget;
    private Vector2 contactNormal; // 충돌 표면의 법선 벡터
    private Vector2 throwDirection; // 던지는 방향 저장
    private Vector2 playerPositionOnThrow; // 던진 시점의 플레이어 위치
    public LayerMask StickLayers => stickLayers;

    [SerializeField]
    private AudioClip throwClip;
    AudioSource audioSource;

    private static readonly Vector2[] possibleNormals =
    {
        Vector2.right,
        Vector2.left,
        Vector2.up,
        Vector2.down,
    };
    
    private void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
    }

    public void Initialize(WeaponBase weaponData, Vector2 throwForce, Vector2 direction, Vector2 playerPosition)
    {
        this.weaponData = weaponData;
        
        throwDirection = direction.normalized;

        playerPositionOnThrow = playerPosition;
        
        // 무기 데이터 적용
        if (spriteRenderer != null && weaponData.GetComponent<SpriteRenderer>() != null)
        {
            spriteRenderer.sprite = weaponData.GetComponent<SpriteRenderer>().sprite;
            spriteRenderer.flipX = direction.x < 0;
            PlaySound(throwClip);
            // 콜라이더 크기 설정
            float radius = Mathf.Max(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y) * colliderRadiusMultiplier;
            circleCollider.radius = radius;
            circleCollider.offset = Vector2.zero;
        }
        
        // Rigidbody 설정
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.angularDamping = 0.1f;
            rb.gravityScale = 1f;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(throwForce, ForceMode2D.Impulse);
            rb.AddTorque(direction.x > 0 ? -rotationSpeed : rotationSpeed);
        }

        gameObject.layer = LayerMask.NameToLayer("Weapon");
        
        canDealDamage = true;
        isStuck = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isStuck) return;

        bool canStick = ((1 << collision.gameObject.layer) & stickLayers) != 0;

        // 1. 박히기 처리
        if (canStick)
        {
            StickTo(collision);
        }

        // 2. 초기 충돌 데미지 처리
        HandleInitialCollisionDamage(collision.gameObject);
    }

    /// <summary>
    /// 초기 충돌 시 데미지 처리 (던진 무기가 적에게 처음 충돌)
    /// </summary>
    private void HandleInitialCollisionDamage(GameObject target)
    {
        if (!canDealDamage || !target.CompareTag("Enemy"))
            return;

        ApplyDamage(target, weaponData.Damage);
        canDealDamage = false;
    }

    /// <summary>
    /// 대상에게 데미지 적용 (일반 로직)
    /// </summary>
    private void ApplyDamage(GameObject target, int damage)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();

        if (damageable != null && weaponData != null)
        {
            // TODO: 시간 정지 기능은 나중에 Unity6으로 이식 후 추가
            // ITimeAffected timeAffected = target.GetComponent<ITimeAffected>();
            // if (TimeManager.Instance != null && TimeManager.Instance.IsTimeFrozen() && timeAffected != null)
            // {
            //     TimeManager.Instance.ApplyDamageInFrozenTime(timeAffected, damage, Vector2.zero);
            // }

            // 일반 데미지 적용
            damageable.DecreaseHp(damage);
        }
    }

    private void StickTo(Collision2D collision)
    {
        if (isStuck) return;
        
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 impactPoint = contact.point;
        Vector2 impactNormal = contact.normal;
        StopSound();
        // 법선 벡터 저장
        contactNormal = impactNormal;
        
        if (throwDirection != Vector2.zero && Mathf.Abs(impactNormal.x) > Mathf.Abs(impactNormal.y))
        {
            Vector2 inversedThrowDir = -throwDirection;

            float angleWithPhysicsNormal = Vector2.Angle(impactNormal, inversedThrowDir);

            if (angleWithPhysicsNormal < maxAngleForCustomNormal)
            {
                contactNormal = GetClosestCardinalDirection(inversedThrowDir);
            }
        }
        
        ValidateNormalWithPlayerPosition(impactPoint);
        
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        circleCollider.isTrigger = true;
        
        // 위치 & 회전 보정
        transform.position = impactPoint;
        
        float angle = Mathf.Atan2(impactNormal.y, impactNormal.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // 충돌 대상에 부착
        if (collision.rigidbody != null)
        {
            transform.SetParent(collision.transform);
            stuckTarget = collision.transform;
        }
        
        isStuck = true;
    }

    private void ValidateNormalWithPlayerPosition(Vector2 impactPoint)
    {
        if (Mathf.Abs(contactNormal.y) > Mathf.Abs(contactNormal.x))
        {
            return;
        }

        // 플레이어와 무기의 X 위치 차이
        float xDifference = impactPoint.x - playerPositionOnThrow.x;
        
        // 법선 벡터의 예상 X 방향
        float expectedNormalX = Mathf.Sign(xDifference);
        
        // 법선 벡터의 현재 X 방향
        float currentNormalX = contactNormal.x;

        // 만약 법선 벡터의 X 방향이 예상과 다르면 반전
        if (expectedNormalX * currentNormalX > 0)
        {
            contactNormal = new Vector2(-contactNormal.x, contactNormal.y);
        }
    }
    
    private Vector2 GetClosestCardinalDirection(Vector2 inputVector)
    {
        Vector2 normalizedInput = inputVector.normalized;
        
        float maxDot = float.MinValue;
        Vector2 closestDirection = Vector2.right;
        
        foreach (Vector2 direction in possibleNormals)
        {
            float dot = Vector2.Dot(normalizedInput, direction);
            if (dot > maxDot)
            {
                maxDot = dot;
                closestDirection = direction;
            }
        }
        
        return closestDirection;
    }
    
    public Vector2 GetContactNormal()
    {
        return contactNormal;
    }
    
    public bool IsStuck()
    {
        return isStuck;
    }

    public WeaponBase GetWeaponData()
    {
        return weaponData;
    }

    public void StartPullFromEnemy()
    {
        if (isStuck && stuckTarget != null)
        {
            // 무기 숨김 (애니메이션에서 무기가 표현되므로)
            SetVisible(false);
        
            // 데미지는 아직 적용하지 않음
            isPullDamageApplied = false;
        }
    }

    public void ApplyPullDamage()
    {
        if (isPullDamageApplied) return;

        ApplyDamageToStuckEnemy(extraDamageMultiplier);
        isPullDamageApplied = true;
    }

    public void PullOutFromEnemy()
    {
        if (isPullDamageApplied) return;

        ApplyDamageToStuckEnemy(extraDamageMultiplier);
        isPullDamageApplied = true;
    }

    /// <summary>
    /// 박힌 적에게 데미지 적용 (무기 뽑을 때)
    /// </summary>
    private void ApplyDamageToStuckEnemy(int damageMultiplier)
    {
        if (!isStuck || stuckTarget == null || !stuckTarget.CompareTag("Enemy"))
            return;

        int damage = weaponData.Damage * damageMultiplier;
        ApplyDamage(stuckTarget.gameObject, damage);
    }
    
    // 뽑기 완료 처리
    public void CompletePull()
    {
        // 무기 다시 보이게 하기 (잠시만)
        SetVisible(true);
        
        // 적에서 분리
        if (isStuck && stuckTarget != null)
        {
            transform.SetParent(null);
        
            isStuck = false;
            stuckTarget = null;
            isPullDamageApplied = false;
        }
    }

    public void DetachFromEnemy(Vector2 position)
    {
        if (isStuck && stuckTarget != null)
        {
            transform.SetParent(null);
            transform.position = position;
            
            circleCollider.isTrigger = false;
            circleCollider.enabled = true;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            rb.linearVelocity = Vector2.zero;
            rb.AddTorque(rotationSpeed);
            rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), 5f);
            
            isStuck = false;
            stuckTarget = null;
            canDealDamage = true;
        }
    }

    public void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (isStuck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
    }

    public void PlaySound(AudioClip _clip)
    {
        audioSource.Stop();
        audioSource.clip = _clip;
        audioSource.Play();
    }
    public void StopSound()
    {
        audioSource.Stop();
    }
}
