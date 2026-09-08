using System.Collections.Generic;
using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// STEP 01 전용, 가장 단순한 AI.
    ///
    /// 여기에는 State 도 없고 Behavior Tree 도 없다.
    /// 그냥 거리 조건을 순서대로 확인해서 행동을 고른다.
    ///
    ///   아주 가까움  -> 공격
    ///   가까움       -> 추적
    ///   보이는 거리  -> 바라보기
    ///   그 외        -> 가만히 있기
    ///
    /// 이 스크립트의 목적은 "AI 는 조건을 보고 행동을 고르는 것" 이라는
    /// 가장 기본적인 사실을 먼저 체험하게 하는 것이다.
    ///
    /// 그 다음 단계에서 이렇게 묻는다.
    ///   "조건이 20개, 행동이 15개가 되면 이 if 문은 어떻게 될까?"
    /// 그 질문의 답이 State Pattern 과 Behavior Tree 다.
    /// </summary>
    [RequireComponent(typeof(AIContext))]
    public class SimpleReactiveAI : MonoBehaviour, IAIBrain
    {
        [Tooltip("이 거리 안에 들어오면 Player 를 바라본다.")]
        public float noticeRange = 7.5f;

        [Tooltip("이 거리 안에 들어오면 추적한다.")]
        public float chaseRange = 4.5f;

        private AIContext _ctx;
        private string _label = "IDLE";
        private string _previous = "-";
        private float _enterTime;

        public AISystemKind Kind => AISystemKind.Reactive;
        public string SystemName => "IF / ELSE (구조 없음)";
        public string CurrentLabel => _label;
        public string PreviousLabel => _previous;
        public float TimeInCurrent => Time.time - _enterTime;

        public IReadOnlyList<string> RecentEvents =>
            _ctx != null && _ctx.EventLog != null ? _ctx.EventLog.Lines : System.Array.Empty<string>();

        /// <summary>지금 어떤 조건이 만족되어 이 행동이 선택되었는가. UI 표시용.</summary>
        public string SelectedBecause { get; private set; } = "Player 가 멀리 있다";

        /// <summary>다이어그램에 표시할 행동 목록.</summary>
        public static readonly string[] BehaviourNames = { "IDLE", "LOOK", "CHASE", "ATTACK" };

        private void Awake()
        {
            _ctx = GetComponent<AIContext>();
        }

        private void Update()
        {
            if (_ctx == null || !_ctx.HasPlayer)
                return;

            float d = _ctx.DistanceToPlayer;

            // 조건을 위에서부터 확인한다. 먼저 맞는 것이 실행된다.
            if (d <= _ctx.attackRange)
            {
                SetLabel("ATTACK", "Player 가 아주 가깝다 (공격 사거리)");
                _ctx.TickAttack();
            }
            else if (d <= chaseRange)
            {
                SetLabel("CHASE", "Player 가 가깝다 (추적 거리)");
                _ctx.TickChase();
            }
            else if (d <= noticeRange)
            {
                SetLabel("LOOK", "Player 가 보인다 (인식 거리)");
                _ctx.TickLookAtPlayer();
            }
            else
            {
                SetLabel("IDLE", "Player 가 멀리 있다");
                _ctx.TickIdle();
            }

            if (_ctx.EventLog != null)
                _ctx.EventLog.LogExecute(_label);
        }

        private void SetLabel(string label, string because)
        {
            SelectedBecause = because;

            if (_label == label)
                return;

            _previous = _label;
            _label = label;
            _enterTime = Time.time;

            if (_ctx != null && _ctx.EventLog != null)
                _ctx.EventLog.LogRaw(label + "  <= " + because);
        }
    }
}
