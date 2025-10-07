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
    public BlackboardVariable<Movement2D> movement2D;
    [SerializeReference] 
    public BlackboardVariable<EnemyAnimation2D> animator;

    [SerializeField] private float obstacleCheckDistance = 0.5f; // 장애물 감지 거리
    [SerializeField] private LayerMask obstacleLayer;

    private float currentDirection = 0f;
    protected override Status OnStart()
    {
        if (Self.Value == null) return Status.Failure; // null 방지
        if (animator != null) animator.Value.SetChase();
        if (movement2D == null) return Status.Failure;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {

        if (movement2D == null || Self.Value == null) return Status.Failure;
        
        //장애물 발견시
        if (IsObstacleAhead())
        {
            currentDirection *= -1f; // 장애물 반대방향으로 설정
        }

        //이동관련 코드
        if (currentDirection > 0f)
        {
            Vector3 scale = Self.Value.transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            Self.Value.transform.localScale = scale;
        }
        else
        {
            Vector3 scale = Self.Value.transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            Self.Value.transform.localScale = scale;
        }

        movement2D.Value.SetMoveInput(currentDirection);

        return Status.Success;
    }

    protected override void OnEnd()
    {
        if (movement2D != null) movement2D.Value.SetMoveInput(0);
    }


    private bool IsObstacleAhead() // 장애물 판정
    {

        Vector2 origin = Self.Value.transform.position;
        Vector2 dir = new Vector2(currentDirection, 0f);

        if (dir == Vector2.zero) return false; // 정지 상태에선 감지 안 함

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, obstacleCheckDistance, obstacleLayer);
        return hit.collider != null;
    }
}

