namespace AILearning
{
    /// <summary>
    /// FLEE 상태 - 체력이 낮아 도망친다.
    ///
    /// Execute : Player 반대 방향으로 달리면서 조금씩 회복한다.
    /// 전환    : HP 가 0 이면 DEAD,
    ///           체력이 회복되고 Player 도 멀어졌으면 PATROL.
    ///
    /// 학습 포인트:
    ///   FLEE 는 PATROL / CHASE / ATTACK 어디에서든 들어올 수 있다.
    ///   State Pattern 에서 상태가 늘어나면 이런 "어디서든 들어오는 전환" 이 늘어나고,
    ///   그것이 State Pattern 의 주의점(전환 관계가 복잡해진다)으로 이어진다.
    /// </summary>
    public class FleeState : AIState
    {
        public FleeState(EnemyStateMachine machine) : base(machine) { }

        public override string Name => "FLEE";
        public override string Description => "HP 가 낮아 Player 에게서 멀어진다. 회복되고 안전해지면 PATROL 로 돌아간다.";

        public override void Execute()
        {
            // DEAD 는 여기서도 최우선으로 확인한다.
            if (Ctx.IsDead)
            {
                Machine.ChangeState(Machine.Dead, "도망 중 HP 가 0 이 되었다");
                return;
            }

            Ctx.TickFlee();

            // 회복이 충분하고 Player 도 멀어졌으면 다시 순찰로 돌아간다.
            bool recovered = !Ctx.IsLowHealth;
            bool safe = Ctx.DistanceToPlayer > Ctx.loseSightRange;

            if (recovered && safe)
                Machine.ChangeState(Machine.Patrol, "체력을 회복하고 Player 도 멀어졌다");
        }

        public override void Exit()
        {
            if (Ctx.Motor != null)
                Ctx.Motor.Stop();
        }
    }
}
