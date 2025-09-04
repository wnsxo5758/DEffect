public class PlayerFallState : PlayerAirborneState
{
    public PlayerFallState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void OnEnter()
    {
        
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }
    
    public override void OnFixedUpdate() { base.OnFixedUpdate(); }
    public override void OnExit() { }
}
