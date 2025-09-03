using Unity.Behavior;
using UnityEngine;

[BlackboardEnum]
public enum EnemyState 
{
    Idle,
    Patrol,
    Chase,
    Stun,
    Attack,
    Death,
}
