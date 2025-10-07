using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement2D : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed; // 움직임 속도
    [SerializeField]
    private float gravityScale; // 적용되는 중력
    [SerializeField]
    private bool isFacingRight; // 우측을 보고 있는가?

    public Rigidbody2D Rigid2D { get; private set; }

    public bool IsGrounded { private set; get; } = false; // 바닥체크용

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
    public void SetMoveInput(float dir) // 입력에 따른 방향
    {
        moveInput = dir;
    }

    public void MoveTo(float dir) // 좌우 움직임 함수
    {
        Rigid2D.linearVelocityX = dir * moveSpeed;
    }
    protected void CheckGrounded() //하단 바닥 확인
    {
        Bounds bounds = _collider2D.bounds;
        collisionSize = new Vector2((bounds.max.x - bounds.min.x) * 0.5f, 0.1f);
        footPos = new Vector2(bounds.center.x, bounds.min.y);
        IsGrounded = Physics2D.OverlapBox(footPos, collisionSize, 0, groundLayer);
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
