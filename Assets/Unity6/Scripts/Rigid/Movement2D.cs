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


    Rigidbody2D rigid2D;

    protected void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();

    }


    public virtual void MoveTo(float x) // 기본 움직임 코드
    {

    }
}
