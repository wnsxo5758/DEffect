using UnityEngine;

//利 局聪皋捞记 内靛
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


}
