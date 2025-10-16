using UnityEngine;

/// <summary>
/// 텔레포트 시작 상태
/// SRP: 텔레포트 대시 이동 전체 과정 담당 (시작 → 이동 → 도착 → 상황별 전환)
/// </summary>
public class PlayerTeleportStartState : PlayerStateBase
{
    public override bool ShouldLockDirection => true;

    private readonly PlayerTeleportAttack teleportAttack;
    private bool teleportExecuted = false;

    public PlayerTeleportStartState(StateMachine stateMachine) : base(stateMachine)
    {
        PlayerCombatSystem combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        teleportAttack = combatSystem != null ? combatSystem.TeleportAttack : null;
    }

    protected override void SetupTransitions()
    {
        // 대시 중 애니메이션 전환 대기
        // 도착 후 이벤트 기반으로 상태 전환
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

        // 무기 방향으로 플레이어 스프라이트 방향 전환
        SetFacingDirectionToWeapon();

        // 텔레포트 이벤트 구독
        teleportAttack.OnTeleportCompleted += OnTeleportCompleted;
        teleportAttack.OnTeleportPullRequired += OnTeleportPullRequired;

        // 텔레포트 시작 애니메이션 (손 뻗기)
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.TriggerTeleportStart();
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
    /// 무기 방향으로 플레이어 스프라이트 방향 전환
    /// </summary>
    private void SetFacingDirectionToWeapon()
    {
        PlayerCombatSystem combatSystem = stateMachine.GetComponent<PlayerCombatSystem>();
        if (combatSystem == null || combatSystem.RangedAttack == null) return;

        ThrownWeapon weapon = combatSystem.RangedAttack.LastThrownWeapon;
        if (weapon == null) return;

        Vector2 weaponPosition = weapon.transform.position;
        Vector2 playerPosition = stateMachine.transform.position;

        // 무기가 플레이어의 왼쪽에 있으면 왼쪽을 보도록
        bool shouldFaceLeft = weaponPosition.x < playerPosition.x;

        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.SetSpriteDirection(shouldFaceLeft);
        }

        // FacingDirection도 업데이트
        stateMachine.Movement.UpdateFacingDirection(shouldFaceLeft ? -1f : 1f);
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
    /// 텔레포트 완료 시 호출 (바닥에 떨어진 무기로 텔레포트한 경우)
    /// </summary>
    private void OnTeleportCompleted()
    {
        // 바닥 무기 (날아가는 중 또는 땅에 떨어짐) → TeleportEnd 애니메이션 재생 후 Idle/Fall
        stateMachine.ChangeState<PlayerTeleportEndState>();
    }

    /// <summary>
    /// 무기 뽑기가 필요한 경우 호출 (무기가 박힌 경우)
    /// </summary>
    private void OnTeleportPullRequired(WeaponPullContext pullContext)
    {
        if (pullContext == null || pullContext.targetWeapon == null)
        {
            // 컨텍스트가 없으면 기본 종료
            stateMachine.ChangeState<PlayerTeleportEndState>();
            return;
        }

        ThrownWeapon weapon = pullContext.targetWeapon;

        // 1. 지면에 박힌 경우 (서 있을 수 있는 위치) → PullGround
        if (weapon.IsStuckOnGround())
        {
            stateMachine.ChangeState<PlayerWeaponPullGroundState>();
        }
        // 2. 적에게 박힌 경우 → PullAir
        else if (weapon.IsStuckToEnemy())
        {
            stateMachine.ChangeState<PlayerWeaponPullAirState>();
        }
        // 3. 벽이나 오브젝트 옆면에 박힌 경우 (법선 벡터가 옆/아래 방향) → PullAir
        else if (weapon.IsStuck())
        {
            stateMachine.ChangeState<PlayerWeaponPullAirState>();
        }
        // 4. 그 외 (박히지 않은 경우) → TeleportEnd
        else
        {
            stateMachine.ChangeState<PlayerTeleportEndState>();
        }
    }
}
