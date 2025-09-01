using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [Header("�Ѿ� ����")]
    [SerializeField]
    protected int damage; // �Ѿ� ������
    [SerializeField]
    protected float speed; // �Ѿ� �ӵ�
    [SerializeField]
    protected AudioClip hitSound;
    [SerializeField]
    protected LayerMask hitLayer; // �ǰݴ�� ���̾�


    private Vector2 moveDir;
    private Vector2 lastVelocity;

    Rigidbody2D rigid;

    protected AudioSource audioSource;
    protected Animator animator;
    private MemoryPool memoryPool;
    private Collider2D collider2D;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        audioSource = GetComponentInChildren<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        collider2D = GetComponent<Collider2D>();
    }

    public virtual void SetUp(Vector2 direction, MemoryPool _memoryPool)
    {
        this.memoryPool = _memoryPool;

        moveDir = direction.normalized;

        // Rigidbody2D �̵� ����
        if (rigid != null)
        {
            rigid.linearVelocity = moveDir * speed;
        }

        // ȸ�� ����
        RotateToDirection(moveDir);

        // Collider Ȱ��ȭ (Ǯ���� ���� �� �ʱ�ȭ �ʼ�)
        if (collider2D != null)
        {
            collider2D.enabled = true;
        }


    }

    private  void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            PlayerHp playerHp = collision.GetComponent<PlayerHp>();
            if (playerHp != null)
            {
                DeathData deathData = new DeathData(DeathCause.RangedAttack);
                playerHp.DecreaseHp(damage, deathData,true, true);
            }
        }
        Destroy(this.gameObject);
        //StartCoroutine(nameof(DestroyBullet));

        //if ((hitLayer.value & (1 << collision.gameObject.layer)) > 0)
        //{
        //    StartCoroutine(nameof(DestroyBullet));
        //}
    }

    //�Ѿ� ��Ȱ��ȭ(Destroy �ƴ�)
    protected virtual IEnumerator DestroyBullet()
    {
        GetComponent<Collider2D>().enabled = false;

        PlaySound(hitSound);
        Destroy(gameObject);
        //memoryPool.DeactivatePoolItems(gameObject);
        yield return null;
    }

    //����
    private void PlaySound(AudioClip _clip)
    {
        audioSource.Stop();
        audioSource.clip = _clip;
        audioSource.Play();
    }
    
    // �Ѿ� ȸ���� ��������Ʈ ȸ��
    protected void RotateToDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


}
