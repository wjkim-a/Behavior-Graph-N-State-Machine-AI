namespace AILearning
{
    /// <summary>
    /// ATTACK 상태 - 멈춰서 공격한다.
    ///
    /// Enter   : 멈춘다.
    /// Execute : 일정 간격으로 공격을 시도한다.
    /// 전환    : Player 가 사거리에서 벗어나면 CHASE.
    /// </summary>
    public class AttackState : AIState
    {
        public AttackState(EnemyStateMachine machine) : base(machine) { }

        public override string Name => "ATTACK";
        public override string Description => "제자리에서 Player 를 공격한다. 사거리를 벗어나면 CHASE 로 돌아간다.";

        /// <summary>이 상태에 들어온 뒤 성공한 공격 횟수. 상태가 값을 기억한다는 예시.</summary>
        public int HitsInThisState { get; private set; }

        public override void Enter()
        {
            // ENTER 는 딱 한 번만 실행된다. 그래서 초기화에 쓴다.
            HitsInThisState = 0;
            if (Ctx.Motor != null)
                Ctx.Motor.Stop();
        }

        public override void Execute()
        {
            if (CheckCommonTransitions())
                return;

            if (Ctx.TickAttack())
                HitsInThisState++;

            if (!Ctx.InAttackRange)
                Machine.ChangeState(Machine.Chase, "Player 가 공격 사거리에서 벗어났다");
        }
    }
}
