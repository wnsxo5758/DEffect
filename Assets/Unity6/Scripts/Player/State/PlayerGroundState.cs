public abstract class PlayerGroundState : PlayerBaseState
{
    public PlayerGroundState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void OnUpdate()
    {
        // 점프 입력 확인
        // 점프 키가 눌리면 Jump 상태로 전환
        if (stateMachine.Movement.IsJumpPressed)
        {
            stateMachine.ChangeState(new PlayerJumpState(stateMachine));
        }
    }
}
