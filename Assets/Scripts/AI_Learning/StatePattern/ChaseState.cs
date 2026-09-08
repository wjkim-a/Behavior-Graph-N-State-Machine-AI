namespace AILearning
{
    /// <summary>
    /// CHASE 상태 - Player 를 쫓아간다.
    ///
    /// Execute : Player 방향으로 이동한다.
    /// 전환    : 공격 사거리에 들어오면 ATTACK,
    ///           Player 를 놓치면 PATROL.
    /// </summary>
    public class ChaseState : AIState
    {
        public ChaseState(EnemyStateMachine machine) : base(machine) { }

        public override string Name => "CHASE";
        public override string Description => "Player 쪽으로 이동한다. 사거리에 들어오면 ATTACK, 놓치면 PATROL 로 간다.";

        public override void Execute()
        {
            if (CheckCommonTransitions())
                return;

            Ctx.TickChase();

            // 우선순위가 중요하다. 공격 사거리 확인이 추적 종료 확인보다 앞에 온다.
            if (Ctx.InAttackRange)
            {
                Machine.ChangeState(Machine.Attack, "공격 사거리 안으로 들어왔다");
                return;
            }

            if (!Ctx.PlayerDetected)
                Machine.ChangeState(Machine.Patrol, "Player 를 놓쳤다 (감지 범위 이탈)");
        }

        public override void Exit()
        {
            if (Ctx.Motor != null)
                Ctx.Motor.Stop();
        }
    }
}
