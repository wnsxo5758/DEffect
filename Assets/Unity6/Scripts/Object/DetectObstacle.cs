using UnityEngine;

public class DetectObstacle : MonoBehaviour
{
    [Header("벽 감지 설정")]
    [SerializeField] protected float wallCheckDistance = 0.5f;
    [SerializeField] protected Vector2 wallCheckOffset = new Vector2(0.5f, 0);
    [SerializeField] protected Vector2 groundCheckOffset = new Vector2(0.5f, -0.5f);
    [SerializeField] protected LayerMask wallLayer; // 벽 레이어
    [SerializeField] protected LayerMask groundLayer; // 지면 레이어

    // 벽 체크 메서드
    public bool CheckWall(float direction)
    {
        Vector2 originPos = (Vector2)transform.position +
                    new Vector2(wallCheckOffset.x * direction, wallCheckOffset.y);

        RaycastHit2D hit = Physics2D.Raycast(originPos, new Vector2(direction, 0), wallCheckDistance, wallLayer);
        Debug.DrawRay(originPos, new Vector2(direction, 0) * wallCheckDistance, hit ? Color.red : Color.green);

        return hit;
    }

    // 땅 체크 메서드 (낭떠러지 감지)
    public bool CheckGround(float direction)
    {
        Vector2 originPos = (Vector2)transform.position +
                            new Vector2(groundCheckOffset.x * direction, groundCheckOffset.y);

        RaycastHit2D hit = Physics2D.Raycast(originPos, Vector2.down, wallCheckDistance, groundLayer);
        Debug.DrawRay(originPos, Vector2.down * wallCheckDistance, hit ? Color.green : Color.red);

        return hit;
    }
}
