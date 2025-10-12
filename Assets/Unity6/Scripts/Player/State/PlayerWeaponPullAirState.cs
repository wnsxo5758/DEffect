using UnityEngine;

/// <summary>
/// 공중에서 무기 뽑기 상태
/// SRP: 공중에서의 무기 뽑기 동작만 담당
/// </summary>
public class PlayerWeaponPullAirState : PlayerAirborneStateBase
{
    private PlayerRangedAttack rangedAttack;
    private WeaponPullContext pullContext;

    public PlayerWeaponPullAirState(StateMachine stateMachine) : base(stateMachine)
    {
        PlayerCombatSystem combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        rangedAttack = combatSystem?.RangedAttack;
    }

    protected override void SetupTransitions()
    {
        // 착지 전환은 비활성화 (무기 뽑기 중에는 상태 유지)
        // 애니메이션 이벤트로만 전환
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

        // 수평 이동 정지
        stateMachine.Movement.SetMoveInput(0);

        // 무기 뽑기 애니메이션
        // TODO: PlayerAnimator에 TriggerWeaponPullAir() 메서드 추가 필요
        if (stateMachine.Animator != null)
        {
            // stateMachine.Animator.TriggerWeaponPullAir(pullContext);
        }
    }

    protected override void UpdateState()
    {
        // 애니메이션이 끝나면 자동으로 전환
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 무기 뽑기 중에는 수평 이동 불가 (중력은 영향받음)
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

        // Fall 상태로 전환 (착지는 공통 전환 조건에서 처리)
        if (stateMachine.Movement.IsGrounded())
        {
            stateMachine.ChangeState<PlayerIdleState>();
        }
        else
        {
            stateMachine.ChangeState<PlayerFallState>();
        }
    }
}
