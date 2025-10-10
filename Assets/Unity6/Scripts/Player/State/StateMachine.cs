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
    // TODO: 추가 컴포넌트 참조
    // public PlayerCombatSystem Combat { get; private set; }

    // 상태 인스턴스 캐싱 (메모리 최적화)
    private Dictionary<Type, IPlayerState> stateInstances;

    private void Awake()
    {
        // 필수 컴포넌트 참조 가져오기
        Movement = GetComponent<PlayerMovement>();
        InputHandler = GetComponent<PlayerInputHandler>();
        Health = GetComponent<PlayerHealth>();
        Animator = GetComponent<PlayerAnimator>();

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
    }

    private void Start()
    {
        // 입력 핸들러와 이동 시스템 연결
        ConnectInputToMovement();

        // 초기 상태를 Idle로 설정
        ChangeState<PlayerIdleState>();
    }

    private void ConnectInputToMovement()
    {
        // 이동 입력이 발생할 때마다 Movement에 전달
        InputHandler.OnMoveInput += Movement.SetMoveInput;
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위한 이벤트 구독 해제
        if (InputHandler != null)
        {
            InputHandler.OnMoveInput -= Movement.SetMoveInput;
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
}
