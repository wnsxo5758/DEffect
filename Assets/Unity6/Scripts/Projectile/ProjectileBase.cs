using UnityEngine;
using UnityEngine.Pool;


[RequireComponent(typeof(ProjectileMovement))]
public class ProjectileBase : PoolItem<ProjectileBase>
{
    [Header("총알 설정")]
    [SerializeField] protected float speed; // 총알 속도
    [SerializeField] protected int damage;  // 총알 데미지

    private ProjectileMovement bulletMovement;

    private void Awake()
    {
        bulletMovement = GetComponent<ProjectileMovement>();
    }

    public void Fire(Vector2 dir)
    {
        bulletMovement.Fire(dir, speed);
    }

    public void SetDamage(int dmg) // 
    {
        damage = dmg;
    }

    public void DestoryBullet() // 풀로 반환
    {
        ReleaseToPool();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == gameObject) return; // 자기 자신과의 충돌은 무시
        if (collision.TryGetComponent(out IDamageable target)) // IDamageable 인터페이스를 가진 대상에게 데미지 부여
        {
            target.DecreaseHp(damage);
        }
        DestoryBullet();// 충돌한 다음 풀로 반환
    }
}
