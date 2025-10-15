using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement2D : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed; // 움직임 속도
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float gravityScale; // 적용되는 중력

    private bool isFacingRight = true; // 우측을 보고 있는가?


    [SerializeField]
    private float knockBackForce;

    public Rigidbody2D Rigid2D { get; private set; }

    public bool IsGrounded { private set; get; } = false; // 바닥체크용

    [Header("벽 감지 설정")]
    [SerializeField] protected float wallCheckDistance = 0.5f;
    [SerializeField] protected Vector2 wallCheckOffset = new Vector2(0.5f, 0);
    [SerializeField] protected Vector2 groundCheckOffset = new Vector2(0.5f, -0.5f);
    [SerializeField] protected LayerMask wallLayer; // 벽 레이어

    private float moveInput;
    public Vector2 Velocity => Rigid2D.linearVelocity;

    private Vector2 collisionSize;
    private Vector2 footPos;
    Collider2D _collider2D;
    [SerializeField]
    protected LayerMask groundLayer; //바닥 취급 레이어

    protected void Awake()
    {
        Rigid2D = GetComponent<Rigidbody2D>();

    }

    protected void SetUp()
    {

    }

    public bool CheckGround(float direction)
    {
        Vector2 originPos = (Vector2)transform.position +
                            new Vector2(groundCheckOffset.x * direction, groundCheckOffset.y);

        RaycastHit2D hit = Physics2D.Raycast(originPos, Vector2.down, wallCheckDistance, groundLayer);
        Debug.DrawRay(originPos, Vector2.down * wallCheckDistance, hit ? Color.green : Color.red);

        return hit;
    }
    public bool CheckWall(float direction)
    {
        Vector2 originPos = (Vector2)transform.position +
                            new Vector2(wallCheckOffset.x * direction, wallCheckOffset.y);

        RaycastHit2D hit = Physics2D.Raycast(originPos, new Vector2(direction, 0), wallCheckDistance, wallLayer);
        Debug.DrawRay(originPos, new Vector2(direction, 0) * wallCheckDistance, hit ? Color.red : Color.green);

        return hit;
    }
    public void FreezeMovement()
    {
        if (Rigid2D != null)
        {
            Rigid2D.linearVelocity = Vector2.zero;
            Rigid2D.angularVelocity = 0f;
            Rigid2D.Sleep();
        }

        // 이동 관련 동작 중지
        MoveTo(0f);
    }

    public void KnockBack(Vector2 dir)
    {
        Rigid2D.linearVelocity = Vector2.zero;
        Rigid2D.AddForce(dir * knockBackForce, ForceMode2D.Impulse);
    }

    public void UnfreezeTime()
    {
        if (Rigid2D != null)
        {
            Rigid2D.WakeUp();
        }
    }
    public void SetMoveInput(float dir) // 입력에 따른 방향
    {
        moveInput = dir;
    }

    public void MoveTo(float dir) // 좌우 움직임 함수
    {
        Rigid2D.linearVelocityX = dir * walkSpeed;
    }
    public void RunTo(float dir)
    {
        Rigid2D.linearVelocityX = dir * runSpeed;
    }
    protected void CheckGrounded() //하단 바닥 확인
    {
        Bounds bounds = _collider2D.bounds;
        collisionSize = new Vector2((bounds.max.x - bounds.min.x) * 0.5f, 0.1f);
        footPos = new Vector2(bounds.center.x, bounds.min.y);
        IsGrounded = Physics2D.OverlapBox(footPos, collisionSize, 0, groundLayer);
    }

    public void DeathAction()
    {
        MoveTo(0);
        Rigid2D.bodyType = RigidbodyType2D.Static;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (_collider2D == null) return;

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(footPos, collisionSize);
    }
#endif

}
