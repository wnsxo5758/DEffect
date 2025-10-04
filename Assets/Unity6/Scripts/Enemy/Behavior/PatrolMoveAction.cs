using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using PlayerStates;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PatrolMoveAction", story: "[Self] is Patroling", category: "Action", id: "e0d11e5a4192ac202141e394ea3a554f")]
public partial class PatrolMoveAction : Action
{
    [SerializeReference] 
    public BlackboardVariable<GameObject> Self;
    [SerializeReference] 
    public BlackboardVariable<MovementRigidbody2D> movement2D;
    [SerializeReference] 
    public BlackboardVariable<Animator> animator;

    [SerializeField] private float obstacleCheckDistance = 0.5f; // 장애물 감지 거리
    [SerializeField] private LayerMask obstacleLayer;

    private float currentDirection = 0f;
    protected override Status OnStart()
    {
        if (Self.Value == null) return Status.Failure; // null 방지
        if (animator != null) animator.Value.Play("Idle");
        if (movement2D == null) return Status.Failure;


        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        //장애물 발견시
        if (IsObstacleAhead())
        {
            currentDirection *= -1f; // 장애물 반대방향으로 설정
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }


    private bool IsObstacleAhead()
    {

        Vector2 origin = Self.Value.transform.position;
        Vector2 dir = new Vector2(currentDirection, 0f);

        if (dir == Vector2.zero) return false; // 정지 상태에선 감지 안 함

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, obstacleCheckDistance, obstacleLayer);
        return hit.collider != null;
    }
}

