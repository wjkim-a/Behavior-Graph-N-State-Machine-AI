using System;
using System.Collections.Generic;
using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// State Machine.
    ///
    /// 중요한 오해 하나를 먼저 정리한다.
    ///   State Machine 은 "상태" 가 아니다.
    ///   State Machine 은 "지금 어떤 State 를 쓸지 관리하는 역할" 이다.
    ///
    /// 구조:
    ///
    ///                 StateMachine
    ///                      |
    ///                 Current State
    ///                      |
    ///     +--------+-------+-------+--------+
    ///     |        |       |       |        |
    ///   Patrol   Chase   Attack   Flee    Dead
    ///
    /// 하는 일은 딱 세 가지다.
    ///   1) 현재 State 를 하나 들고 있는다.
    ///   2) 매 프레임 현재 State 의 Execute() 를 호출한다.
    ///   3) State 를 바꿀 때 이전 State 의 Exit() 과 새 State 의 Enter() 를 호출한다.
    /// </summary>
    [RequireComponent(typeof(AIContext))]
    public class EnemyStateMachine : MonoBehaviour, IAIBrain
    {
        /// <summary>감각과 몸. 모든 State 가 이것을 공유한다.</summary>
        public AIContext Context { get; private set; }

        // 상태 객체는 미리 하나씩 만들어두고 계속 재사용한다.
        // (매번 new 로 만들면 상태가 기억해야 할 값이 초기화되어 버린다.)
        public PatrolState Patrol { get; private set; }
        public ChaseState Chase { get; private set; }
        public AttackState Attack { get; private set; }
        public FleeState Flee { get; private set; }
        public DeadState Dead { get; private set; }

        /// <summary>지금 실행 중인 State. 이것을 교체하는 것이 곧 행동을 바꾸는 것이다.</summary>
        public AIState CurrentState { get; private set; }

        /// <summary>직전 State.</summary>
        public AIState PreviousState { get; private set; }

        /// <summary>가장 최근 상태 전환의 이유. 학습 UI 가 이 문장을 보여준다.</summary>
        public string LastTransitionReason { get; private set; } = "시작";

        /// <summary>상태가 바뀔 때 알림. (from, to, reason)</summary>
        public event Action<string, string, string> OnStateChanged;

        private float _stateEnterTime;

        // IAIBrain 구현 ------------------------------------------------
        public AISystemKind Kind => AISystemKind.StatePattern;
        public string SystemName => "STATE MACHINE";
        public string CurrentLabel => CurrentState != null ? CurrentState.Name : "-";
        public string PreviousLabel => PreviousState != null ? PreviousState.Name : "-";
        public float TimeInCurrent => Time.time - _stateEnterTime;

        public IReadOnlyList<string> RecentEvents =>
            Context != null && Context.EventLog != null ? Context.EventLog.Lines : EmptyLines;

        private static readonly string[] EmptyLines = new string[0];

        /// <summary>다이어그램 UI 가 그릴 상태 목록 (표시 순서).</summary>
        public static readonly string[] StateNames = { "PATROL", "CHASE", "ATTACK", "FLEE", "DEAD" };

        private void Awake()
        {
            Context = GetComponent<AIContext>();

            Patrol = new PatrolState(this);
            Chase = new ChaseState(this);
            Attack = new AttackState(this);
            Flee = new FleeState(this);
            Dead = new DeadState(this);
        }

        private void Start()
        {
            // 처음 상태는 PATROL.
            ChangeState(Patrol, "시작 상태");
        }

        private void Update()
        {
            if (CurrentState == null)
                return;

            // State Machine 이 하는 일은 이 한 줄이다.
            // "현재 상태가 자기 일을 하게 한다."
            CurrentState.Execute();

            if (Context != null && Context.EventLog != null)
                Context.EventLog.LogExecute(CurrentState.Name);
        }

        /// <summary>
        /// 상태를 교체한다. State Pattern 의 심장.
        ///   1) 기존 상태 Exit()
        ///   2) 현재 상태 교체
        ///   3) 새 상태 Enter()
        /// </summary>
        public void ChangeState(AIState next, string reason = null)
        {
            if (next == null || next == CurrentState)
                return;

            string fromName = CurrentState != null ? CurrentState.Name : "-";

            if (CurrentState != null)
            {
                CurrentState.Exit();
                if (Context != null && Context.EventLog != null)
                    Context.EventLog.LogExit(CurrentState.Name);
            }

            PreviousState = CurrentState;
            CurrentState = next;
            _stateEnterTime = Time.time;

            if (!string.IsNullOrEmpty(reason))
                LastTransitionReason = reason;

            CurrentState.Enter();
            if (Context != null && Context.EventLog != null)
                Context.EventLog.LogEnter(CurrentState.Name);

            OnStateChanged?.Invoke(fromName, CurrentState.Name, LastTransitionReason);
        }

        /// <summary>실험을 다시 하기 위해 PATROL 부터 시작한다.</summary>
        public void ResetBrain()
        {
            CurrentState = null;
            PreviousState = null;
            LastTransitionReason = "리셋";
            Patrol = new PatrolState(this);
            Chase = new ChaseState(this);
            Attack = new AttackState(this);
            Flee = new FleeState(this);
            Dead = new DeadState(this);
            ChangeState(Patrol, "리셋 후 시작 상태");
        }

        /// <summary>학습 UI 가 조건표를 그릴 때 사용한다.</summary>
        public string DescribeCurrentState()
        {
            return CurrentState != null ? CurrentState.Description : string.Empty;
        }
    }
}
