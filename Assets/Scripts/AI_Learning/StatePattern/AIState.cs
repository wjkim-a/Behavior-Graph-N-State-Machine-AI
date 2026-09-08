namespace AILearning
{
    /// <summary>
    /// State Pattern 의 핵심: 하나의 상태(행동 모드)를 나타내는 클래스.
    ///
    /// 왜 클래스로 나누는가?
    ///   - PATROL 일 때 할 일과 CHASE 일 때 할 일은 완전히 다른 코드다.
    ///   - 그것을 하나의 if / switch 안에 다 넣으면 점점 읽기 어려워진다.
    ///   - 그래서 "상태 하나 = 클래스 하나" 로 분리한다.
    ///
    /// 각 상태는 세 개의 시점을 가진다.
    ///   Enter()   : 이 상태에 처음 들어올 때 딱 1번
    ///   Execute() : 이 상태가 유지되는 동안 매 프레임 반복
    ///   Exit()    : 이 상태를 빠져나갈 때 딱 1번
    ///
    /// 그리고 각 상태는 "다음에 어떤 상태로 가야 하는가" 도 스스로 판단한다.
    /// => 그래서 State Pattern 의 질문은 항상 이것이다.
    ///    "나는 지금 어떤 상태인가? 그리고 언제 다른 상태로 넘어가야 하는가?"
    /// </summary>
    public abstract class AIState
    {
        /// <summary>이 상태를 소유한 상태 기계.</summary>
        protected readonly EnemyStateMachine Machine;

        protected AIState(EnemyStateMachine machine)
        {
            Machine = machine;
        }

        /// <summary>상태가 사용하는 감각 / 몸. 모든 상태가 같은 AIContext 를 공유한다.</summary>
        protected AIContext Ctx => Machine.Context;

        /// <summary>UI 에 표시되는 상태 이름. 예: "PATROL".</summary>
        public abstract string Name { get; }

        /// <summary>이 상태를 한 줄로 설명한 문장. 학습 UI 에서 보여준다.</summary>
        public abstract string Description { get; }

        /// <summary>상태에 처음 들어올 때 1번 호출된다.</summary>
        public virtual void Enter() { }

        /// <summary>상태가 유지되는 동안 매 프레임 호출된다.</summary>
        public virtual void Execute() { }

        /// <summary>상태를 빠져나갈 때 1번 호출된다.</summary>
        public virtual void Exit() { }

        /// <summary>
        /// 모든 상태에서 공통으로 먼저 확인하는 전환.
        /// (죽음과 도망은 어떤 상태에 있든 최우선으로 처리해야 한다.)
        /// </summary>
        /// <returns>상태를 바꿨으면 true. 이 경우 Execute 는 더 진행하지 않는다.</returns>
        protected bool CheckCommonTransitions()
        {
            if (Ctx.IsDead)
            {
                Machine.ChangeState(Machine.Dead, "HP 가 0 이 되었다");
                return true;
            }

            if (Ctx.IsLowHealth && !(this is FleeState))
            {
                Machine.ChangeState(Machine.Flee, "HP 가 30% 이하로 떨어졌다");
                return true;
            }

            return false;
        }
    }
}
