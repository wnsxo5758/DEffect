using UnityEngine;

/// <summary>
/// 공중에서 무기 뽑기 상태
/// SRP: 공중에서의 무기 뽑기 동작만 담당
/// </summary>
public class PlayerWeaponPullAirState : PlayerAirborneStateBase
{
    public override bool ShouldLockDirection => true;

    private PlayerRangedAttack rangedAttack;
    private WeaponPullContext pullContext;

    // 위치 고정
    private Vector3 frozenPosition;
    private Rigidbody2D rb;

    public PlayerWeaponPullAirState(StateMachine stateMachine) : base(stateMachine)
    {
        PlayerCombatSystem combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        rangedAttack = combatSystem?.RangedAttack;
        rb = stateMachine.GetComponent<Rigidbody2D>();
    }

    protected override void SetupTransitions()
    {
        // 착지 전환 비활성화 (무기 뽑기 중에는 상태 유지)
        // 애니메이션 이벤트로만 전환
        // base.SetupTransitions()를 호출하지 않음으로써 자동 전환 차단
    }

    public override void Enter()
    {
        base.Enter();

        if (rangedAttack == null)
        {
            Debug.LogError("[PlayerWeaponPullAirState] RangedAttack component is missing!");
            stateMachine.ChangeState<PlayerFallState>();
            return;
        }

        // 무기 뽑기 컨텍스트 가져오기
        pullContext = rangedAttack.PendingPullContext;

        if (pullContext == null)
        {
            Debug.LogWarning("[PlayerWeaponPullAirState] No pending weapon pull context!");
            stateMachine.ChangeState<PlayerFallState>();
            return;
        }

        // 현재 위치 고정
        frozenPosition = stateMachine.transform.position;

        // 속도를 0으로 만들어 정지
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f; // 중력 비활성화
        }

        // 수평 이동 정지
        stateMachine.Movement.SetMoveInput(0);

        // 무기 방향으로 스프라이트 고정
        if (pullContext.targetWeapon != null && stateMachine.Animator != null)
        {
            Vector2 weaponPosition = pullContext.weaponPosition;
            Vector2 playerPosition = stateMachine.transform.position;

            // 무기가 플레이어의 왼쪽에 있으면 왼쪽을 보도록
            bool shouldFaceLeft = weaponPosition.x < playerPosition.x;
            stateMachine.Animator.SetSpriteDirection(shouldFaceLeft);
        }

        // 무기 뽑기 애니메이션
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.TriggerWeaponPullAir();
        }
    }

    protected override void UpdateState()
    {
        // 애니메이션이 끝나면 자동으로 전환
    }

    public override void FixedUpdate()
    {
        // 위치 고정 유지
        if (rb != null)
        {
            stateMachine.transform.position = frozenPosition;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public override void Exit()
    {
        base.Exit();

        // 중력 복원
        if (rb != null)
        {
            rb.gravityScale = stateMachine.Movement.GetDefaultGravityScale();
        }
    }

    /// <summary>
    /// 무기 뽑기 애니메이션 완료 시 호출 (애니메이션 이벤트)
    /// </summary>
    public void OnAnimationFinished()
    {
        // 무기 뽑기 완료 (무기 장착 처리)
        if (rangedAttack != null)
        {
            rangedAttack.CompleteWeaponPull();
        }

        // AfterPull 상태로 전환 (회전 낙하)
        stateMachine.ChangeState<PlayerWeaponPullAfterAirState>();
    }
}
