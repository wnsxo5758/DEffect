using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementRigidbody2D : MonoBehaviour
{
    [Header("���̾� ����ũ")]
    [SerializeField] private LayerMask groundCheckLayer;
    [SerializeField] private LayerMask belowCollisionLayer;
    
    [Header("������")] 
    [SerializeField] private float runSpeed; // �޸��� �ӵ�
    [SerializeField] private float crawlSpeed; // ���� �ӵ�
    [SerializeField] private float climbSpeed; // ��ٸ� �ӵ�
    [SerializeField] private float jumpForce; // ������
    [SerializeField] private float lowGravityScale; // ���� �߷� (���� ������)
    [SerializeField] private float highGravityScale; // ���� �߷� (�Ϲ� ������)
    [SerializeField] private bool movementLocked = false; // ������ ���

    private float moveSpeed; // ���� �����̴� �ӵ�
    private float initialSpeed; // �ʱ� �ӵ�
    private bool weightlessness = false;

    public float MoveSpeed => moveSpeed;
    public float RunSpeed => runSpeed;


    private Vector2 collisionSize; // �ٴ� �˻� size
    private Vector2 footPos; // �� ��ġ

    private Rigidbody2D rigid;
    private Collider2D collider;
    public Collider2D HitBelowObject { private set; get; }

    public bool IsLongJump { set; get; } = false;
    public bool IsGrounded { private set; get; } = false;

    public LayerMask GroundCheckLayer => groundCheckLayer;
    public float InteractSpeed
    {
        set => runSpeed = initialSpeed * (1 / value);
    }
    public Vector2 Velocity => rigid.linearVelocity;

    private void Awake()
    {
        initialSpeed = runSpeed;
        moveSpeed = runSpeed;
        rigid = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        
    }

    private void Update()
    {
        UpdateCollision();
        JumpHeight();
    }
    
    private void UpdateCollision()
    {
        Bounds bounds = collider.bounds;

        collisionSize = new Vector2((bounds.max.x - bounds.min.x) * 0.95f, 0.1f);
        footPos = new Vector2(bounds.center.x, bounds.min.y);

        IsGrounded = Physics2D.OverlapBox(footPos, collisionSize, 0, groundCheckLayer);

        HitBelowObject = Physics2D.OverlapBox(footPos, collisionSize, 0, belowCollisionLayer);
    }
    
    public void MoveTo(float x)
    {
        moveSpeed = runSpeed;
        if (x != 0) x = Mathf.Sign(x);
        rigid.linearVelocity = new Vector2(x * moveSpeed, rigid.linearVelocity.y);
    }

    public void MoveToFast(float x)
    {
        moveSpeed = runSpeed * 1.2f;
        if (x != 0) x = Mathf.Sign(x);
        rigid.linearVelocity = new Vector2(x * moveSpeed, rigid.linearVelocity.y);
    }
    
    public void Jump() //����
    {
        if (IsGrounded)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpForce);
            IsGrounded = false;
        }
    }

    private void JumpHeight() // ������ ����
    {
        if (IsLongJump && rigid.linearVelocity.y > 0)
        {
            rigid.gravityScale = lowGravityScale;
        }
        else
        {
            if (!weightlessness)
            {
                rigid.gravityScale = highGravityScale;
            }
        }
    }

    public void Crawl(float x) // ����
    {
        if (x != 0) x = Mathf.Sign(x);
        rigid.linearVelocity = new Vector2(x * crawlSpeed, rigid.linearVelocity.y);
    }

    public void Roll(float direction, float speed)
    {
        Vector2 rollVelocity = new Vector2(direction * speed, rigid.linearVelocity.y);
        rigid.linearVelocity = rollVelocity;
    }
    
    public void Climb(float y)
    {
        rigid.linearVelocity = new Vector2(0, y * climbSpeed);
    }

    public void LadderJump(float x)
    {
        if(x != 0) x = Mathf.Sign(x);
        rigid.linearVelocity = new Vector2(x * runSpeed, jumpForce / 2);
    }
        
    public void DisableGravity()
    {
        rigid.gravityScale = 0;
        weightlessness = true;
    }

    public void EnableGravity()
    {
        rigid.gravityScale = highGravityScale;
        weightlessness = false;
    }

    public void DisableRigidbody()
    {
        rigid.linearVelocity = Vector2.zero;
    }

    public void EnableRigidbody()
    {
        if (rigid != null)
        {
            rigid.isKinematic = false;
            rigid.simulated = true;
            rigid.linearVelocity = Vector2.zero;
            rigid.angularVelocity = 0f;
            rigid.gravityScale = highGravityScale;
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public void SetVelocity(Vector2 velocity)
    {
        rigid.linearVelocity = velocity;
    }

    public void AddForce(Vector2 force)
    {
        rigid.AddForce(force, ForceMode2D.Impulse);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        
        Gizmos.DrawWireCube(footPos, collisionSize);
    }
}
