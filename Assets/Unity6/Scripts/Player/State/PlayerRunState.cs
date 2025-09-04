public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        
    }

    public override void OnUpdate()
    {
        if (stateMachine.Movement.MoveInput.sqrMagnitude < 0.1f)
        {
            stateMachine.ChangeState(new PlayerIdleState(stateMachine));
        }
    }

    public override void OnFixedUpdate()
    {
        // 움직임 로직 호출
        stateMachine.Movement.Move();
    }

    public override void OnExit() { }
}
