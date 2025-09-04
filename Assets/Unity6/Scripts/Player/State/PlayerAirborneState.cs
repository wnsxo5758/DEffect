public abstract class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void OnUpdate()
    {
        // '공중 상태'의 공통 로직
        if (stateMachine.Movement.IsGrounded())
        {
            stateMachine.ChangeState(new PlayerIdleState(stateMachine));
        }
    }

    public override void OnFixedUpdate()
    {
        // 공중에 떠 있는 동안에 좌우 이동 가능
        stateMachine.Movement.Move();
    }
}
