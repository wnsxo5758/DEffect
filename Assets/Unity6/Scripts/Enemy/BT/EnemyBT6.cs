using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBT6 : MonoBehaviour
{
    protected BehaviorTree behaviorTree;
    protected Blackboard blackboard;

    [SerializeField] protected float normalStunDuration = 0.5f; // 일반 공격 스턴 시간
    [SerializeField] protected float throwStunDuration = 3f; // 던지기 공격 스턴 시간
    [SerializeField] protected float knockBackForce = 3f; // 넉백 힘
    [SerializeField] protected float deathDelay = 2f;
    
    [Header("기본 AI 설정")]
    [SerializeField] protected float detectionRange; // 플레이어 인지 거리
    [SerializeField] protected float loseTargetRange; // 추적 최대 거리
    [SerializeField] protected Transform target; // 플레이어 타깃
    [SerializeField] protected LayerMask targetLayer; // 플레이어 레이어

    [Header("패트롤 설정")] 
    [SerializeField] protected float patrolDirection = 1f; // 초기 패트롤 방향


    protected DetectObstacle obstacleDetect;

    protected ObjectSound objSound;
    protected Rigidbody2D rb;
    protected MovementRigidbody2D movement;
    protected EnemyAnimator animator;
    protected Collider2D enemyCollider;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected Coroutine flashCoroutine;
    protected CheckDistance distanceCheck;
    protected EnemyHp enemyHp;

    // 상태 변수
    protected bool isDeathProcessed = false;
    protected float stunTimer = 0f;
    protected bool isTimeFrozen = false; // 시간 정지 관련 변수
    
    protected virtual void Awake()
    {
        enemyHp = GetComponent<EnemyHp>(); 
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<MovementRigidbody2D>();
        animator = GetComponentInChildren<EnemyAnimator>();
        enemyCollider = GetComponent<Collider2D>();
        obstacleDetect = GetComponent<DetectObstacle>();


        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        objSound = GetComponentInChildren<ObjectSound>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        blackboard = new Blackboard();
        
        // 블랙보드에 초기 데이터 설정
        blackboard.SetValue("Target", target);
        blackboard.SetValue("PlayerDetected", false);
        blackboard.SetValue("PatrolDirection", patrolDirection);

        blackboard.SetValue("StunTimer", 0f);

        blackboard.SetValue("CurrentHp", enemyHp.CurrentHp);
        blackboard.SetValue("MaxHp", enemyHp.MaxHp);
        blackboard.SetValue("IsHit", enemyHp.IsHit);
        blackboard.SetValue("IsDead", enemyHp.IsDeath);

        // 초기 방향 설정
        SetDirection(patrolDirection);
    }

    protected virtual void Start()
    {
        // 기본 행동 트리 설정
        SetupBaseBehaviorTree();
    }

    protected virtual void Update()
    {
        // 시간 정지 상태라면 아무 행동도 하지 않음
        if (isTimeFrozen)
            return;

        if (isDeathProcessed) 
            return;
        Hit();
    }
    
    protected virtual void Hit()
    {
        // 피격 상태 처리
        if (enemyHp.IsHit)
        {
            stunTimer -= Time.deltaTime;
            blackboard.SetValue("StunTimer", stunTimer);

            if (stunTimer <= 0)
            {
                //enemyHp.IsHit = false;
                blackboard.SetValue("IsHit", false);

                if (flashCoroutine != null)
                {
                    StopCoroutine(flashCoroutine);
                    flashCoroutine = null;
                }

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }

        // 타깃 감지

        if (!enemyHp.IsHit && !enemyHp.IsDeath)
        {
            DetectTarget();
        }

        // Behavior Tree 평가
        if (behaviorTree != null)
        {
            behaviorTree.Evaluate();
        }
    }

    // 기본 행동 트리 설정
    protected virtual void SetupBaseBehaviorTree()
    {
        // 루트 노드 (셀렉터)
        Selector rootSelector = new Selector();
        
        // 사망 시퀀스 (최우선)
        Sequence deathSequence = new Sequence();
        ConditionNode isDeadCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsDead"));
        ActionNode deadAction = new ActionNode(HandleDeath);
        deathSequence.AddChild(isDeadCondition);
        deathSequence.AddChild(deadAction);
        
        // 피격 시퀀스 (다음 우선순위)
        Sequence hitSequence = new Sequence();
        ConditionNode isHitCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsHit"));
        ActionNode hitAction = new ActionNode(HandleHit);
        hitSequence.AddChild(isHitCondition);
        hitSequence.AddChild(hitAction);
        
        // 공격 시퀀스 (하위 클래스에서 구현)
        Node attackSequence = CreateAttackSequence();
        
        // 공격 중 시퀀스
        Sequence attackingSequence = new Sequence();
        ConditionNode isAttackingCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsAttacking"));
        ActionNode stayInAttackAction = new ActionNode(MaintainAttackState);
        attackingSequence.AddChild(isAttackingCondition);
        attackingSequence.AddChild(stayInAttackAction);
        
        // 추적 시퀀스
        Sequence chaseSequence = new Sequence();
        ConditionNode isPlayerDetected = new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected"));
        ActionNode chaseAction = new ActionNode(ChaseTarget);
        chaseSequence.AddChild(isPlayerDetected);
        chaseSequence.AddChild(chaseAction);
        
        // 패트롤 시퀀스
        Sequence patrolSequence = new Sequence();
        ActionNode patrolAction = new ActionNode(Patrol);
        patrolSequence.AddChild(patrolAction);
        
        // 트리 구성 (우선순위 순)
        rootSelector.AddChild(deathSequence);   // 사망 상태 (최우선)
        rootSelector.AddChild(hitSequence);     // 피격 상태 (다음 우선순위)
        rootSelector.AddChild(attackingSequence); // 공격 중 상태
        rootSelector.AddChild(attackSequence);  // 공격 가능하면 공격
        rootSelector.AddChild(chaseSequence);   // 공격 불가능하면 추적
        rootSelector.AddChild(patrolSequence);  // 추적 불가능하면 패트롤
        
        // Behavior Tree 생성
        behaviorTree = new BehaviorTree(rootSelector)
        {
            Blackboard = blackboard
        };
    }
    
    // 하위 클래스에서 오버라이드할 공격 시퀀스 생성 메서드
    protected virtual Node CreateAttackSequence()
    {
        return new ConditionNode(() => false);
    }
    // 타겟 감지 메서드
    protected virtual void DetectTarget()
    {
        // 피격 중이거나 사망 상태면 타겟 감지하지 않음
        if (enemyHp.IsHit || enemyHp.IsDeath) return;
        
        bool previouslyDetected = blackboard.GetValue<bool>("PlayerDetected");
        bool currentlyDetected = previouslyDetected;
        
        if (target == null)
        {
            // 플레이어 자동 탐색
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, 
                                                    detectionRange, targetLayer);
            if (playerCollider != null)
            {
                target = playerCollider.transform;
                blackboard.SetValue("Target", target);
                currentlyDetected = true;
            }
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
                }
                else
                {
                    currentlyDetected = true;
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
                    }
                }
            }
        }
        
        // 블랙보드 업데이트
        blackboard.SetValue("PlayerDetected", currentlyDetected);
        
        // 플레이어 감지 상태가 변경되었으면 애니메이션 업데이트 및 패트롤 방향 설정
        if (previouslyDetected != currentlyDetected)
        {
            if (animator != null)
            {
                animator.SetChasingState(currentlyDetected);
            }
                
            // 추적을 포기할 때 현재 방향에 맞게 패트롤 방향 초기화
            if (!currentlyDetected && previouslyDetected)
            {
                // 추적 모드에서 패트롤 모드로 전환될 때 방향 설정
                HandleLostTarget();
            }
        }
    }

    // 추적 포기 시 방향 설정 처리
    protected virtual void HandleLostTarget()
    {
        // 현재 방향 가져오기
        float currentDirection = GetDirection();
        
        // 현재 방향으로 패트롤 방향 설정
        blackboard.SetValue("PatrolDirection", currentDirection);

        // 벽 체크 - 만약 현재 방향에 벽 있다면 방향 반전
        if (obstacleDetect.CheckWall(currentDirection))
        {
            currentDirection *= -1;
            blackboard.SetValue("PatrolDirection", currentDirection);
            SetDirection(currentDirection);
        }
    }

    protected virtual NodeState MaintainAttackState()
    {
        if (movement != null)
        {
            movement.MoveTo(0);
        }

        return NodeState.Running;
    }
    
    // 타겟 추적 메서드
    protected virtual NodeState ChaseTarget()
    {
        // 피격 중이거나 사망 상태면 추적하지 않음
        if (enemyHp.IsHit || enemyHp.IsDeath) return NodeState.Failure;
        
        Transform currentTarget = blackboard.GetValue<Transform>("Target");

        if (currentTarget == null)
        {
            if (animator != null)
            {
                animator.SetChasingState(false);
            }
            return NodeState.Failure;
        }

        // 추적 애니메이션 활성화
        if (animator != null)
        {
            animator.SetChasingState(true);
        }
        
        // 타겟 방향으로 이동
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        float directionToTarget = Mathf.Sign(direction.x);
        
        // 방향 설정
        if (directionToTarget != 0)
        {
            SetDirection(directionToTarget);
        }

        // 공격 범위 내에 있는지 확인
        bool inAttackRange = IsTargetInAttackRange();

        // 공격 범위 내에 있으면 이동하지 않고 방향만 설정
        if (inAttackRange)
        {
            if (movement != null)
            {
                movement.MoveTo(0);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        // 공격 범위 밖에 있으면 추적
        else
        {
            // 이동
            if (movement != null)
            {
                movement.MoveToFast(directionToTarget);
            }
        }
        
        return NodeState.Running;
    }

    // 패트롤 메서드
    protected virtual NodeState Patrol()
    {
        // 피격 중이거나 사망 상태면 패트롤하지 않음
        if (enemyHp.IsHit || enemyHp.IsDeath) return NodeState.Failure;

        if (animator != null)
        {
            animator.SetChasingState(false);
        }
        
        float direction = blackboard.GetValue<float>("PatrolDirection");

        // 방향 값 검증
        if (float.IsNaN(direction) || direction == 0)
        {
            direction = 1f;
            blackboard.SetValue("PatrolDirection", direction);
        }
        
        // 벽 체크
        bool isWallAhead = obstacleDetect.CheckWall(direction);

        // 벽이 있으면 방향 전환
        if (isWallAhead)
        {
            direction *= -1;
            blackboard.SetValue("PatrolDirection", direction);
            SetDirection(direction);
        }
        else
        {
            SetDirection(direction);
        }
        
        // 이동
        if (movement != null)
        {
            movement.MoveTo(direction);
        }

        if (animator != null)
        {
            animator.SetMovementAnim(Mathf.Abs(direction));
        }

        return NodeState.Running;
    }
    
    // 피격 처리 메서드
    protected virtual NodeState HandleHit()
    {
        // 공격 중이면 공격 애니메이션 종료
        bool wasAttacking = blackboard.GetValue<bool>("IsAttacking");
        if (wasAttacking)
        {
            OnAttackAnimationFinished();
        }
        
        // 움직임 멈춤
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        else if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        
        // 피격 애니메이션 재생
        if (animator != null)
        {
            animator.SetMovementAnim(0);
            animator.TriggerHitAnim();
            objSound.HitSound();
        }

        return NodeState.Running;
    }
    
    //사망 처리 메서드
    protected virtual NodeState HandleDeath()
    {
        if (isDeathProcessed)
        {
            return NodeState.Success;
        }
        
        isDeathProcessed = true;
        
        // 움직임 멈춤
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        else if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        enemyCollider.enabled = false;
        
        Debug.Log("적 사망 애니메이션 시작");
        // 사망 애니메이션 재생
        if (animator != null)
        {
            animator.SetMovementAnim(0);
            animator.SetChasingState(false);
            animator.TriggerDeathAnim();
        }
        objSound.DeadSound();

        // 오브젝트 제거 (딜레이 적용)
        StartCoroutine(DestroyAfterDelay(deathDelay));
        
        return NodeState.Success;
    }

    // 딜레이 후 오브젝트 제거
    protected IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ThrownWeapon attachedWeapon = GetComponentInChildren<ThrownWeapon>();
        if (attachedWeapon != null)
        {
            attachedWeapon.DetachFromEnemy(transform.position);
        }
        // 오브젝트 풀링 비활성화
        gameObject.SetActive(false);
    }
    
    // 데미지 적용 메서드
    public virtual void DecreaseHp(int damage, bool isThrownWeapon = false)
    {

        blackboard.SetValue("CurrentHp", enemyHp.CurrentHp);

        // 사망 체크
        blackboard.SetValue("IsDead", enemyHp.IsDeath);
        if (enemyHp.IsDeath == true) return;

        // 피격 상태 설정
        blackboard.SetValue("IsHit", enemyHp.IsHit);
        
        // 스턴 타이머 설정 (던진 무기인 경우 더 긴 스턴)
        stunTimer = isThrownWeapon ? throwStunDuration : normalStunDuration;
        blackboard.SetValue("StunTimer", stunTimer);
        
        // 피격 이펙트
        // 넉백 적용 (피격 방향의 반대로)
        if (target != null && rb != null && !isThrownWeapon)
        {
            Vector2 knockBackDirection = ((Vector2)transform.position - (Vector2)target.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockBackDirection * knockBackForce, ForceMode2D.Impulse);
        }
    }


    
    // 방향 설정 메서드
    protected virtual void SetDirection(float direction)
    {
        if (direction != 0)
        {
            Vector3 scale = transform.localScale;
            float previousX = scale.x;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);
            
            transform.localScale = scale;
        }
    }

    protected float GetDirection()
    {
        return Mathf.Sign(transform.localScale.x);
    }

    protected virtual bool IsTargetInAttackRange()
    {
        // 하위 클래스에서 공격 범위를 설정
        return false;
    }

    protected virtual float GetAttackRange()
    {
        // 하위 클래스에서 오버라이`드
        return 0f;
    }
    
    // 거리 계산 메서드
    protected float DistanceToTarget()
    {
        Transform currentTarget = blackboard.GetValue<Transform>("Target");

        return distanceCheck.DistanceToTarget(currentTarget);
    }
    
    public virtual void FreezeTime()
    {
        isTimeFrozen = true;
        
        // 물리 객체 정지 
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.Sleep();
        }
        
        // 이동 관련 동작 중지
        if (movement != null)
        {
            movement.MoveTo(0f);
        }

        if (behaviorTree != null)
        {
            behaviorTree.Pause();
        }
    }

    public virtual void UnfreezeTime()
    {
        isTimeFrozen = false;
        
        // 물리 객체 깨우기
        if (rb != null)
        {
            rb.WakeUp();
        }

        if (behaviorTree != null)
        {
            behaviorTree.Resume();
        }
    }
    
    public virtual void OnAttackAnimationEvent()
    {
        
    }

    public virtual void OnAttackAnimationFinished()
    { 
    
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
    }
}
