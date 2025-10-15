using UnityEngine;

//적 애니메이션 코드
[RequireComponent (typeof(Animator))]
public class EnemyAnimation2D : MonoBehaviour
{
    private readonly int moveSpeed = Animator.StringToHash("Speed");
    private readonly int attack = Animator.StringToHash("Attack");
    private readonly int hit = Animator.StringToHash("Hit");
    private readonly int death = Animator.StringToHash("Death");
    private readonly int isChasing = Animator.StringToHash("IsChasing");

    private Animator animator;
    private MovementRigidbody2D movement; // 움직임

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<MovementRigidbody2D>();
    }

    // 새로운 animation 메소드

    public void SetMovementAnim(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat(moveSpeed, Mathf.Abs(speed));
        }
    }

    public void SetChasingState(bool _isChasing)
    {
        if (animator != null)
        {
            animator.SetBool(isChasing, _isChasing);
        }
    }

    public void TriggerAttackAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(attack);
        }
    }

    public void TriggerHitAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(hit);
        }
    }

    public void TriggerDeathAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(death);
        }
    }

    private void OnAttackEvent()
    {
        EnemyBTBase enemy = transform.GetComponentInParent<EnemyBTBase>();
        enemy.OnAttackAnimationEvent();
    }

    private void OnAttackFinished()
    {
        EnemyBTBase enemy = transform.GetComponentInParent<EnemyBTBase>();
        enemy.OnAttackAnimationFinished();
    }
}
