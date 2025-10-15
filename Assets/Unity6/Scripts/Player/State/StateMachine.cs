using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerAnimator))]
public class StateMachine : MonoBehaviour
{
    // 현재 활성화된 상태 (읽기 전용)
    public IPlayerState CurrentState { get; private set; }

    // 필요한 컴포넌트들에 대한 참조
    public PlayerMovement Movement { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerAnimator Animator { get; private set; }
    public PlayerCombatSystem Combat { get; private set; }
    public PlayerInteractionSystem Interaction { get; private set; }

    // 상태 인스턴스 캐싱 (메모리 최적화)
    private Dictionary<Type, IPlayerState> stateInstances;

    private void Awake()
    {
        // 필수 컴포넌트 참조 가져오기
        Movement = GetComponent<PlayerMovement>();
        InputHandler = GetComponent<PlayerInputHandler>();
        Health = GetComponent<PlayerHealth>();
        Animator = GetComponent<PlayerAnimator>();
        Combat = GetComponent<PlayerCombatSystem>();
        Interaction = GetComponent<PlayerInteractionSystem>();

        stateInstances = new Dictionary<Type, IPlayerState>();

        // 컴포넌트 검증
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (Movement == null)
            Debug.LogError("[PlayerStateMachine] PlayerMovement component is missing!");

        if (InputHandler == null)
            Debug.LogError("[PlayerStateMachine] PlayerInputHandler component is missing!");

        if (Health == null)
            Debug.LogWarning("[PlayerStateMachine] PlayerHealth component is missing!");

        if (Combat == null)
            Debug.LogWarning("[PlayerStateMachine] PlayerCombatSystem component is missing! Combat features will be disabled.");
    }

    private void Start()
    {
        // 입력 핸들러와 시스템들 연결
        ConnectInputToSystems();

        // 초기 상태를 Idle로 설정
        ChangeState<PlayerIdleState>();
    }

    private void ConnectInputToSystems()
    {
        // 이동 입력이 발생할 때마다 Movement에 전달
        InputHandler.OnMoveInput += Movement.SetMoveInput;

        // 전투 입력 이벤트 연결 (Combat이 있는 경우)
        if (Combat != null)
        {
            InputHandler.OnAttackPressed += OnAttackInput;
            InputHandler.OnThrowWeaponPressed += OnThrowWeaponInput;
            InputHandler.OnTeleportPressed += OnTeleportInput;
            InputHandler.OnRollPressed += OnRollInput;
        }

        // 상호작용 입력 이벤트 연결
        if (Interaction != null)
        {
            InputHandler.OnInteractPressed += OnInteractInput;
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위한 이벤트 구독 해제
        if (InputHandler != null)
        {
            InputHandler.OnMoveInput -= Movement.SetMoveInput;

            if (Combat != null)
            {
                InputHandler.OnAttackPressed -= OnAttackInput;
                InputHandler.OnThrowWeaponPressed -= OnThrowWeaponInput;
                InputHandler.OnTeleportPressed -= OnTeleportInput;
                InputHandler.OnRollPressed -= OnRollInput;
            }

            if (Interaction != null)
            {
                InputHandler.OnInteractPressed -= OnInteractInput;
            }
        }
    }

    private void Update()
    {
        // 현재 상태의 Update 호출
        CurrentState?.Update();
    }

    private void FixedUpdate()
    {
        // 현재 상태의 FixedUpdate 호출
        CurrentState?.FixedUpdate();
    }

    public void ChangeState(Type stateType)
    {
        // 이미 해당 타입의 상태이면 전환하지 않음
        if (CurrentState != null && CurrentState.GetType() == stateType)
            return;

        // 캐시에서 상태 인스턴스 가져오기 (없으면 생성)
        if (!stateInstances.TryGetValue(stateType, out var newState))
        {
            newState = CreateState(stateType);
            stateInstances[stateType] = newState;
        }

        // 이전 상태 종료
        CurrentState?.Exit();

        // 새로운 상태로 전환
        CurrentState = newState;

        // 새로운 상태 시작
        CurrentState.Enter();

        // 디버그 로그 (개발 중에만 활성화)
        #if UNITY_EDITOR
        Debug.Log($"[StateMachine] State changed to: {stateType.Name}");
        #endif
    }

    public void ChangeState<TState>() where TState : IPlayerState
    {
        ChangeState(typeof(TState));
    }

    private IPlayerState CreateState(Type stateType)
    {
        return (IPlayerState)Activator.CreateInstance(stateType, new object[] { this });
    }

    public bool IsCurrentState<TState>() where TState : IPlayerState
    {
        return CurrentState != null && CurrentState.GetType() == typeof(TState);
    }

    public string GetCurrentStateName()
    {
        return CurrentState?.GetType().Name ?? "None";
    }

    // ========== 입력 핸들러 메서드들 ==========

    /// <summary>
    /// 공격 입력 처리
    /// </summary>
    private void OnAttackInput()
    {
        // 공격 가능한 상태인지 확인
        if (Combat == null || !Combat.CanPerformMeleeAttack()) return;

        // 지상에서 공격
        if (CurrentState is PlayerIdleState or PlayerRunState)
        {
            ChangeState<PlayerMeleeAttackState>();
        }
        // 공중에서 공격 (점프 또는 낙하 중)
        else if (CurrentState is PlayerJumpState or PlayerFallState)
        {
            ChangeState<PlayerAirMeleeAttackState>();
        }
    }

    /// <summary>
    /// 무기 던지기 입력 처리
    /// </summary>
    private void OnThrowWeaponInput()
    {
        // 던지기 가능한 상태인지 확인
        if (Combat == null || !Combat.CanPerformRangedAttack()) return;

        // 던지기 가능한 상태에서만 던지기 상태로 전환
        if (CurrentState is PlayerIdleState or PlayerRunState or PlayerJumpState or PlayerFallState)
        {
            ChangeState<PlayerThrowWeaponState>();
        }
    }

    /// <summary>
    /// 텔레포트 입력 처리
    /// </summary>
    private void OnTeleportInput()
    {
        // 텔레포트 가능한 상태인지 확인
        if (Combat == null || !Combat.CanPerformTeleport()) return;

        // 텔레포트 가능한 상태에서만 텔레포트 상태로 전환
        if (CurrentState is PlayerIdleState or PlayerRunState or PlayerJumpState or PlayerFallState)
        {
            bool success = Combat.TeleportAttack.StartTeleport();
            if (success)
            {
                ChangeState<PlayerTeleportStartState>();
            }
        }
    }

    /// <summary>
    /// 구르기 입력 처리
    /// </summary>
    private void OnRollInput()
    {
        // 지면에 있을 때만 구르기 가능
        if (!Movement.IsGrounded()) return;

        // 구르기 가능한 상태에서만 구르기 상태로 전환 (Crouch 상태 제외)
        if (CurrentState is PlayerIdleState or PlayerRunState)
        {
            ChangeState<PlayerRollState>();
        }
    }

    /// <summary>
    /// 상호작용 입력 처리
    /// </summary>
    private void OnInteractInput()
    {
        // 상호작용 가능한 대상이 있으면 실행
        if (Interaction != null && Interaction.HasInteractable)
        {
            Interaction.PerformInteraction();
        }
    }
}
