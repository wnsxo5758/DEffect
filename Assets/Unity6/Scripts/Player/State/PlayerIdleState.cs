public class PlayerIdleState : PlayerBaseState
{
    // 생성자
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        
    }

    public override void OnUpdate()
    {
        if (stateMachine.Movement.MoveInput.sqrMagnitude > 0.1f)
        {
            stateMachine.ChangeState(new PlayerRunState(stateMachine));
        }
    }
    
    public override void OnFixedUpdate() {}
    public override void OnExit() {}
}
