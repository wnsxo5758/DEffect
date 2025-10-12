using UnityEngine;

/// <summary>
/// 텔레포트 시작 상태
/// SRP: 텔레포트 시작 애니메이션과 이동 실행만 담당
/// </summary>
public class PlayerTeleportStartState : PlayerStateBase
{
    private PlayerTeleportAttack teleportAttack;
    private bool teleportExecuted = false;

    public PlayerTeleportStartState(StateMachine stateMachine) : base(stateMachine)
    {
        PlayerCombatSystem combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        teleportAttack = combatSystem?.TeleportAttack;
    }

    protected override void SetupTransitions()
    {
        // 애니메이션 이벤트로 전환 처리
    }

    public override void Enter()
    {
        base.Enter();

        teleportExecuted = false;

        if (teleportAttack == null)
        {
            Debug.LogError("[PlayerTeleportStartState] TeleportAttack component is missing!");
            stateMachine.ChangeState<PlayerIdleState>();
            return;
        }

        // 이동 입력 무시
        stateMachine.Movement.StopMovement();

        // 텔레포트 이벤트 구독
        teleportAttack.OnTeleportCompleted += OnTeleportCompleted;
        teleportAttack.OnTeleportPullRequired += OnTeleportPullRequired;

        // 텔레포트 시작 애니메이션
        // TODO: PlayerAnimator에 TriggerTeleportStart() 메서드 추가 필요
        if (stateMachine.Animator != null)
        {
            // stateMachine.Animator.TriggerTeleportStart();
        }
    }

    protected override void UpdateState()
    {
        // 애니메이션이 끝나면 자동으로 텔레포트 실행
    }

    public override void FixedUpdate()
    {
        // 텔레포트 중에는 물리 업데이트 없음
    }

    public override void Exit()
    {
        // 이벤트 구독 해제
        if (teleportAttack != null)
        {
            teleportAttack.OnTeleportCompleted -= OnTeleportCompleted;
            teleportAttack.OnTeleportPullRequired -= OnTeleportPullRequired;
        }
    }

    /// <summary>
    /// 텔레포트 시작 애니메이션 완료 시 호출 (애니메이션 이벤트)
    /// </summary>
    public void OnTeleportStartAnimationFinished()
    {
        if (teleportAttack != null && !teleportExecuted)
        {
            teleportExecuted = true;
            teleportAttack.ExecuteTeleport();
        }
    }

    /// <summary>
    /// 텔레포트 완료 시 호출 (적에게 박히지 않은 무기로 텔레포트한 경우)
    /// </summary>
    private void OnTeleportCompleted()
    {
        stateMachine.ChangeState<PlayerTeleportEndState>();
    }

    /// <summary>
    /// 무기 뽑기가 필요한 경우 호출 (적에게 박힌 무기로 텔레포트한 경우)
    /// </summary>
    private void OnTeleportPullRequired(WeaponPullContext pullContext)
    {
        stateMachine.ChangeState<PlayerTeleportEndState>();
    }
}
