using Unity.Behavior;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Transform target;// Å¸°Ù
    private BehaviorGraphAgent behaviorAgent;

    public void SetUp(Transform target)
    {
        this.target = target;
        behaviorAgent = GetComponent<BehaviorGraphAgent>();
    }
}
