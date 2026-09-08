using System.Collections.Generic;
using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// Behavior Graph 는 노드가 트리 안에서 실행되므로,
    /// 밖에서 "지금 어떤 노드가 실행 중인가" 를 알기 어렵다.
    ///
    /// 그래서 이 프로젝트의 Behavior Graph 노드들은 실행될 때
    /// 자기 이름을 이 컴포넌트에 보고한다.
    /// 학습용 UI 는 이 값을 읽어서 트리 다이어그램의 해당 노드를 강조한다.
    ///
    /// State Pattern 쪽의 EnemyStateMachine 과 같은 IAIBrain 을 구현하므로
    /// UI 는 두 방식을 똑같은 방법으로 표시할 수 있다.
    /// </summary>
    public class BTNodeTrace : MonoBehaviour, IAIBrain
    {
        private string _current = "-";
        private string _previous = "-";
        private float _enterTime;
        private int _lastReportFrame = -1;

        private readonly Dictionary<string, bool> _conditionResults = new Dictionary<string, bool>();
        private readonly List<string> _conditionOrder = new List<string>();

        public AISystemKind Kind => AISystemKind.BehaviorGraph;
        public string SystemName => "BEHAVIOR GRAPH";
        public string CurrentLabel => _current;
        public string PreviousLabel => _previous;
        public float TimeInCurrent => Time.time - _enterTime;

        private AIContext _ctx;

        public IReadOnlyList<string> RecentEvents =>
            _ctx != null && _ctx.EventLog != null ? _ctx.EventLog.Lines : System.Array.Empty<string>();

        /// <summary>가장 최근에 성공한 조건 노드 이름. "왜 이 행동이 선택되었는가" 표시용.</summary>
        public string SelectedBecause { get; private set; } = "아직 평가 전";

        /// <summary>조건 노드들의 최근 평가 결과. UI 가 TRUE / FALSE 로 보여준다.</summary>
        public IReadOnlyList<string> ConditionOrder => _conditionOrder;

        private void Awake()
        {
            _ctx = GetComponent<AIContext>();
        }

        /// <summary>Action 노드가 실행을 시작할 때 호출한다.</summary>
        public void ReportAction(string nodeName, string because = null)
        {
            // 같은 프레임에 여러 번 보고되면 마지막 것만 의미가 있다.
            _lastReportFrame = Time.frameCount;

            if (!string.IsNullOrEmpty(because))
                SelectedBecause = because;

            if (_current == nodeName)
                return;

            _previous = _current;
            _current = nodeName;
            _enterTime = Time.time;

            if (_ctx != null && _ctx.EventLog != null)
                _ctx.EventLog.LogRaw("NODE  " + nodeName + "  선택됨");
        }

        /// <summary>Action 노드가 매 tick 실행 중임을 알린다. (로그 표시용)</summary>
        public void ReportTick(string nodeName)
        {
            if (_ctx != null && _ctx.EventLog != null)
                _ctx.EventLog.LogExecute("NODE " + nodeName);
        }

        /// <summary>Condition 노드가 평가될 때 호출한다.</summary>
        public void ReportCondition(string conditionName, bool result)
        {
            if (!_conditionResults.ContainsKey(conditionName))
                _conditionOrder.Add(conditionName);

            _conditionResults[conditionName] = result;
        }

        public bool GetConditionResult(string conditionName)
        {
            return _conditionResults.TryGetValue(conditionName, out bool v) && v;
        }

        public bool HasConditionResult(string conditionName)
        {
            return _conditionResults.ContainsKey(conditionName);
        }

        public void ResetTrace()
        {
            _current = "-";
            _previous = "-";
            SelectedBecause = "리셋됨";
            _conditionResults.Clear();
            _conditionOrder.Clear();
        }
    }
}
