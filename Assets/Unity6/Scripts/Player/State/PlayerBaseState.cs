public abstract class PlayerBaseState
{
    // 모든 상태 클래스가 상속받을 추상 클래스.
    // 각 상태가 가져야 할 필수 기능을 정의
    
    protected PlayerStateMachine stateMachine;
    
    // 생성자
    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    
    public abstract void OnEnter();
    public abstract void OnUpdate();        // 매 프레임 호출
    public abstract void OnFixedUpdate();   // 물리 처리용
    public abstract void OnExit();
}
