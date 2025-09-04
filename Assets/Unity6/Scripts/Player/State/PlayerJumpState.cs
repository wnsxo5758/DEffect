public class PlayerJumpState : PlayerAirborneState
{
    public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // 떨어지기 시작하면 Fall 상태로 전환
        if (stateMachine.Movement.Velocity.y < 0)
        {
            stateMachine.ChangeState(new PlayerFallState(stateMachine));
        }
    }
    
    public override void OnFixedUpdate() { base.OnFixedUpdate(); }

    public override void OnExit() { }
}
