using UnityEngine;

/// <summary>
/// 지면에서 무기 뽑기 상태
/// SRP: 지면에서의 무기 뽑기 동작만 담당
/// </summary>
public class PlayerWeaponPullGroundState : PlayerGroundStateBase
{
    private PlayerRangedAttack rangedAttack;
    private WeaponPullContext pullContext;

    public PlayerWeaponPullGroundState(StateMachine stateMachine) : base(stateMachine)
    {
        PlayerCombatSystem combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        rangedAttack = combatSystem?.RangedAttack;
    }

    protected override void SetupTransitions()
    {
        // 낙하 전환은 비활성화 (무기 뽑기 중에는 상태 유지)
        // 애니메이션 이벤트로만 전환
    }

    public override void Enter()
    {
        base.Enter();

        if (rangedAttack == null)
        {
            Debug.LogError("[PlayerWeaponPullGroundState] RangedAttack component is missing!");
            stateMachine.ChangeState<PlayerIdleState>();
            return;
        }

        // 무기 뽑기 컨텍스트 가져오기
        pullContext = rangedAttack.PendingPullContext;

        if (pullContext == null)
        {
            Debug.LogWarning("[PlayerWeaponPullGroundState] No pending weapon pull context!");
            stateMachine.ChangeState<PlayerIdleState>();
            return;
        }

        // 이동 정지
        stateMachine.Movement.StopMovement();

        // 무기 뽑기 애니메이션
        // TODO: PlayerAnimator에 TriggerWeaponPullGround() 메서드 추가 필요
        if (stateMachine.Animator != null)
        {
            // stateMachine.Animator.TriggerWeaponPullGround(pullContext);
        }
    }

    protected override void UpdateState()
    {
        // 애니메이션이 끝나면 자동으로 전환
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 무기 뽑기 중에는 움직이지 않음
        stateMachine.Movement.StopMovement();
    }

    public override void Exit()
    {
        base.Exit();
        // 정리 작업 없음
    }

    /// <summary>
    /// 무기 뽑기 애니메이션 완료 시 호출 (애니메이션 이벤트)
    /// </summary>
    public void OnAnimationFinished()
    {
        // 무기 뽑기 완료
        if (rangedAttack != null)
        {
            rangedAttack.CompleteWeaponPull();
        }

        // Idle 상태로 전환
        if (Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.1f)
        {
            stateMachine.ChangeState<PlayerRunState>();
        }
        else
        {
            stateMachine.ChangeState<PlayerIdleState>();
        }
    }
}
