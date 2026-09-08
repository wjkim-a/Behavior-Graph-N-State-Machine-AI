using Unity.Behavior;
using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// Sequence 실험 그래프의 Blackboard 변수(Seq_PointA / Seq_PointB)에
    /// 실제 씬의 이동 지점을 넣어준다.
    ///
    /// 학습 포인트:
    ///   노드는 "어디로 갈지" 를 직접 알지 못한다.
    ///   Blackboard 에 적힌 값을 읽을 뿐이다.
    ///   그래서 그래프를 고치지 않고 Blackboard 값만 바꿔도 행동이 달라진다.
    /// </summary>
    [RequireComponent(typeof(BehaviorGraphAgent))]
    public class LabSequencePoints : MonoBehaviour
    {
        public GameObject pointA;
        public GameObject pointB;

        [Tooltip("이동 속도. Blackboard 의 MoveSpeed 에 들어간다.")]
        public float moveSpeed = 1.6f;

        [Tooltip("대기 시간(초). Blackboard 의 WaitSeconds 에 들어간다.")]
        public float waitSeconds = 1.0f;

        private BehaviorGraphAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<BehaviorGraphAgent>();
        }

        private void Start()
        {
            Apply();
        }

        private void Update()
        {
            // 그래프 초기화 순서에 따라 Start 시점의 값 설정이 늦게 반영될 수 있다.
            // 값이 아직 비어 있으면 다시 넣어준다.
            if (_agent == null || _agent.Graph == null)
                return;

            BlackboardVariable variable;
            if (_agent.GetVariable("Seq_PointA", out variable) && variable.ObjectValue == null)
                Apply();
        }

        public void Apply()
        {
            if (_agent == null || _agent.Graph == null)
                return;

            if (pointA != null) _agent.SetVariableValue("Seq_PointA", pointA);
            if (pointB != null) _agent.SetVariableValue("Seq_PointB", pointB);
            _agent.SetVariableValue("MoveSpeed", moveSpeed);
            _agent.SetVariableValue("WaitSeconds", waitSeconds);
        }
    }
}
