using UnityEngine;

/// <summary>
/// 무기 던지기 상태
/// SRP: 무기 던지기 동작과 상태 전환만 담당
/// </summary>
public class PlayerThrowWeaponState : PlayerStateBase
{
    private PlayerCombatSystem combatSystem;
    private PlayerRangedAttack rangedAttack;
    private PlayerTeleportAttack teleportAttack;

    public PlayerThrowWeaponState(StateMachine stateMachine) : base(stateMachine)
    {
        combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        rangedAttack = combatSystem?.RangedAttack;
        teleportAttack = combatSystem?.TeleportAttack;
    }

    protected override void SetupTransitions()
    {
        // 던지기가 끝나면 자동으로 전환 (애니메이션 이벤트로 처리)
    }

    public override void Enter()
    {
        base.Enter();

        if (rangedAttack == null)
        {
            Debug.LogError("[PlayerThrowWeaponState] RangedAttack component is missing!");
            stateMachine.ChangeState<PlayerIdleState>();
            return;
        }

        // 이동 정지
        stateMachine.Movement.StopMovement();

        // 던지기 시작
        rangedAttack.StartThrow();

        // 던지기 종료 이벤트 구독
        rangedAttack.OnThrowFinished += OnThrowFinished;

        // 애니메이션 트리거
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.TriggerThrow();
        }
    }

    protected override void UpdateState()
    {
        // 던지는 중에는 이동 불가
    }

    public override void FixedUpdate()
    {
        // 던지는 중에는 물리 업데이트 없음
    }

    public override void Exit()
    {
        // 이벤트 구독 해제
        if (rangedAttack != null)
        {
            rangedAttack.OnThrowFinished -= OnThrowFinished;
        }
    }

    /// <summary>
    /// 던지기 종료 시 호출
    /// </summary>
    private void OnThrowFinished()
    {
        // 텔레포트 활성화
        if (teleportAttack != null)
        {
            teleportAttack.EnableTeleport();
        }

        // 입력에 따라 적절한 상태로 전환
        if (!stateMachine.Movement.IsGrounded())
        {
            stateMachine.ChangeState<PlayerFallState>();
        }
        else if (Mathf.Abs(stateMachine.InputHandler.MoveInput) > 0.1f)
        {
            stateMachine.ChangeState<PlayerRunState>();
        }
        else
        {
            stateMachine.ChangeState<PlayerIdleState>();
        }
    }
}
