using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileMovement : MonoBehaviour
{

    Rigidbody2D rigid2D;

    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
    }

    // ÃÑ¾Ë ¹ß»ç½Ã 
    public void Fire(Vector2 dir, float speed)
    {
        rigid2D.linearVelocity = dir.normalized * speed;
        transform.right = dir;
    }

    private void OnDisable()
    {
        rigid2D.linearVelocity = Vector3.zero;
    }
}
