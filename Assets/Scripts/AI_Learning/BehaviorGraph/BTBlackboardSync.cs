using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// Behavior Graph 의 Blackboard 에 실제 값을 계속 써 넣는다.
    ///
    /// Blackboard 란?
    ///   Behavior Tree 의 노드들이 함께 보는 "데이터 칠판" 이다.
    ///   노드는 서로를 직접 알지 못한다. 대신 칠판에 적힌 값을 읽고 쓴다.
    ///
    /// 이 컴포넌트가 칠판에 적는 값:
    ///   Target          : 추적 대상 (Player)
    ///   PlayerDetected  : Player 를 발견했는가
    ///   InAttackRange   : 공격 사거리에 들어왔는가
    ///   HealthPercent   : 현재 체력 비율(0~100)
    ///   AlarmActive     : 공유 경보가 켜졌는가
    ///
    /// 학습자는 Play 중에 Behavior Graph 창의 Blackboard 를 열어
    /// 이 값들이 실시간으로 바뀌는 것을 볼 수 있다.
    /// 학습용 UI 의 BLACKBOARD 패널도 이 값을 그대로 읽어서 보여준다.
    /// </summary>
    [RequireComponent(typeof(AIContext))]
    public class BTBlackboardSync : MonoBehaviour
    {
        public const string VarTarget = "Target";
        public const string VarPlayerDetected = "PlayerDetected";
        public const string VarInAttackRange = "InAttackRange";
        public const string VarHealthPercent = "HealthPercent";
        public const string VarAlarmActive = "AlarmActive";

        private AIContext _ctx;
        private BehaviorGraphAgent _agent;

        /// <summary>UI 표시용으로 마지막에 칠판에 쓴 값들을 그대로 보관한다.</summary>
        private readonly List<KeyValuePair<string, string>> _snapshot = new List<KeyValuePair<string, string>>();

        public IReadOnlyList<KeyValuePair<string, string>> Snapshot => _snapshot;

        private void Awake()
        {
            _ctx = GetComponent<AIContext>();
            _agent = GetComponent<BehaviorGraphAgent>();
        }

        private void Update()
        {
            if (_ctx == null)
                return;

            bool detected = _ctx.PlayerDetected;
            bool inRange = _ctx.InAttackRange;
            float hp = _ctx.HealthRatio * 100f;
            bool alarm = _ctx.AlarmActive;

            if (_agent != null && _agent.Graph != null)
            {
                // Behavior Graph 의 Blackboard 에 실제로 값을 쓴다.
                if (_ctx.player != null)
                    _agent.SetVariableValue(VarTarget, _ctx.player.gameObject);

                _agent.SetVariableValue(VarPlayerDetected, detected);
                _agent.SetVariableValue(VarInAttackRange, inRange);
                _agent.SetVariableValue(VarHealthPercent, hp);
                _agent.SetVariableValue(VarAlarmActive, alarm);
            }

            _snapshot.Clear();
            _snapshot.Add(new KeyValuePair<string, string>(VarTarget,
                _ctx.player != null ? _ctx.player.name : "(없음)"));
            _snapshot.Add(new KeyValuePair<string, string>(VarPlayerDetected, detected ? "TRUE" : "FALSE"));
            _snapshot.Add(new KeyValuePair<string, string>(VarInAttackRange, inRange ? "TRUE" : "FALSE"));
            _snapshot.Add(new KeyValuePair<string, string>(VarHealthPercent, Mathf.RoundToInt(hp) + " %"));
            _snapshot.Add(new KeyValuePair<string, string>(VarAlarmActive, alarm ? "TRUE" : "FALSE"));
        }
    }
}
