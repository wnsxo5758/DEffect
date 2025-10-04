using UnityEngine;

//적 애니메이션 코드
[RequireComponent (typeof(Animator))]
public class EnemyAnimation2D : MonoBehaviour
{
    Animator enemyAnimator;

    private void Awake()
    {
        enemyAnimator = GetComponent<Animator>();
    }

    public void UpdateAnimation(float x)
    {

    }

    private void FlipX(float x) // 좌우 반전
    {

    }

    public void OnAttackAnimation()
    {

    
    }

    public void OnHitAnimation()
    {

    }

    public void OnDeathAnimation()
    {

    }
    public void OnSkillAnimation()
    {

    }


}
