namespace AILearning
{
    /// <summary>
    /// PATROL 상태 - 평소 행동.
    ///
    /// Execute : 순찰 지점 사이를 왕복한다.
    /// 전환    : Player 를 발견하면 CHASE 로 넘어간다.
    /// </summary>
    public class PatrolState : AIState
    {
        public PatrolState(EnemyStateMachine machine) : base(machine) { }

        public override string Name => "PATROL";
        public override string Description => "순찰 지점 사이를 왕복한다. Player 를 발견하면 CHASE 로 간다.";

        public override void Enter()
        {
            // 상태에 들어올 때 준비할 것이 있으면 여기에 쓴다.
            // 순찰은 특별한 준비가 없으므로 비어 있다.
        }

        public override void Execute()
        {
            // 1) 어떤 상태에 있든 먼저 확인해야 하는 것 (죽음 / 도망)
            if (CheckCommonTransitions())
                return;

            // 2) 이 상태가 하는 행동
            Ctx.TickPatrol();

            // 3) 이 상태에서만 확인하는 전환 조건
            if (Ctx.PlayerDetected)
                Machine.ChangeState(Machine.Chase, "Player 를 발견했다 (감지 범위 진입)");
        }

        public override void Exit()
        {
            // 순찰을 멈추고 나갈 때 몸을 정지시켜 둔다.
            if (Ctx.Motor != null)
                Ctx.Motor.Stop();
        }
    }
}
