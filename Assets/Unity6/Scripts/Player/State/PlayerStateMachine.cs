using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    // 현재 활성화된 상태 (읽기 전용)
    public IPlayerState CurrentState { get; private set; }

    // 필요한 컴포넌트들에 대한 참조
    public PlayerMovement Movement { get; private set; }
    // TODO: 다른 컴포넌트 참조를 여기에 추가
    // public PlayerAnimator Animator { get; private set; }
    // public PlayerHealth Health { get; private set; }

    // 상태 인스턴스 캐싱 (메모리 최적화)
    private Dictionary<Type, IPlayerState> stateInstances;

    private void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        stateInstances = new Dictionary<Type, IPlayerState>();
    }

    private void Start()
    {
        // 초기 상태를 Idle로 설정
        ChangeState<PlayerIdleState>();
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
        try
        {
            // 생성자에 this(PlayerStateMachine)를 전달하여 인스턴스 생성
            return (IPlayerState)Activator.CreateInstance(stateType, this);
        }
        catch (Exception e)
        {
            Debug.LogError($"[StateMachine] Failed to create state: {stateType.Name}\n{e.Message}");
            return null;
        }
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
