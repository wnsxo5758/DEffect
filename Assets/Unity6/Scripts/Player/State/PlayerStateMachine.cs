using UnityEngine;
using UnityEngine.XR;

public class PlayerStateMachine : MonoBehaviour
{
    // 상태를 저장. 외부에서 읽기만 가능
    public PlayerBaseState CurrentState { get; private set; }
    
    // 필요한 컴포넌트들에 대한 참조
    public PlayerMovement Movement { get; private set; }
    // TODO: 다른 컴포넌트 참조를 여기에 추가

    private void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        ChangeState(new PlayerIdleState(this));
    }

    private void Update()
    {
        CurrentState?.OnUpdate();   // '?' (null-conditional operator)
    }

    private void FixedUpdate()
    {
        CurrentState?.OnFixedUpdate();
    }

    // 상태 변경 메소드
    public void ChangeState(PlayerBaseState newState)
    {
        // 이전 상태 OnExit 호출
        CurrentState.OnExit();

        // 현재 상태를 새로운 상태로 교체
        CurrentState = newState;
        
        // 새로운 상태의 OnEnter 호출
        CurrentState.OnEnter();
    }
}
