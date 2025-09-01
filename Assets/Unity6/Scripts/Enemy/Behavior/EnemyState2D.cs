using System;
using Unity.Behavior;

[BlackboardEnum]
public enum EnemyState2D
{
    Idle,
	Patrol,
	Chase,
	Stun,
	Attack,
	Death
}
