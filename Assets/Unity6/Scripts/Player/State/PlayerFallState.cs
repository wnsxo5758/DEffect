using UnityEngine;

public class PlayerFallState : PlayerAirborneStateBase
{
    public PlayerFallState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void SetupTransitions()
    {
        // 부모의 공통 전환 조건 설정 (착지)
        base.SetupTransitions();

        // Fall 상태는 착지 조건만 있으면 되므로 추가 전환 조건 없음
    }

    public override void Enter()
    {
        base.Enter();
        // Fall 애니메이션 재생 등
    }

    protected override void UpdateState()
    {
        // Fall 상태의 고유 로직
        // 예: 낙하 속도에 따른 효과, 낙사 체크 등
    }

    public override void FixedUpdate()
    {
        // 부모의 공중 이동 로직 사용
        base.FixedUpdate();
    }

    public override void Exit()
    {
        // Fall 상태 종료 시 정리 작업
    }
}
