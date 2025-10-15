using UnityEngine;

public class CheckDistance : MonoBehaviour
{

    [Header("기본 AI 설정")]
    [SerializeField] 
    protected float detectionRange; // 플레이어 인지 거리
    [SerializeField] 
    protected float loseTargetRange; // 추적 최대 거리
    [SerializeField] 
    protected Transform target; // 플레이어 타깃
    [SerializeField] 
    protected LayerMask targetLayer; // 플레이어 레이어

    public Transform Target => target;
    protected bool currentlyDetected = false;



    private void Awake()
    {
        if(target == null)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
    }

    public float DistanceToTarget(Transform target)
    {
        if (target == null)
        {
            return float.MaxValue;
        }

        return Vector2.Distance(transform.position, target.position);
    }

    public bool DetectTarget(bool previouslyDetected)
    {
        if (target == null)
        {
            // 플레이어 자동 탐색
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position,
                                                    detectionRange, targetLayer);
            if (playerCollider != null)
            {
                target = playerCollider.transform;
                //blackboard.SetValue("Target", target);
                currentlyDetected = true;
                return true;
            }
            else
            {
                // 타겟 거리 확인
                float distanceToTarget = Vector2.Distance(transform.position, target.position);

                if (previouslyDetected)
                {
                    // 이미 추적 중
                    if (distanceToTarget > loseTargetRange)
                    {
                        currentlyDetected = false;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    // 추적 중이 아니면 시야 체크
                    if (distanceToTarget <= detectionRange)
                    {
                        Vector2 directionToTarget = (target.position - transform.position).normalized;
                        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget,
                            detectionRange, targetLayer);
                        Debug.DrawRay(transform.position, directionToTarget * detectionRange, Color.red);

                        if (hit.collider != null && hit.collider.transform == target)
                        {
                            currentlyDetected = true;
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
    }

}
