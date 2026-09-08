using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

// =====================================================================
//  AI Lab - Behavior Tree 구성요소 학습용 Action 노드
//
//  STEP 04 에서 Sequence / Selector / Repeat 를 하나씩 실험할 때 쓰는
//  아주 단순한 행동들이다.
//
//  여기서 처음으로 Running 상태가 등장한다.
//    Success : 이 행동은 끝났다
//    Failure : 이 행동은 실패했다
//    Running : 아직 진행 중이다. 다음 프레임에 계속 호출해 달라
// =====================================================================

namespace AILearning
{
    /// <summary>지정한 지점까지 걸어간다. 도착하면 Success.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Lab Move To Point",
        description: "지정한 GameObject 위치까지 걸어간다. 도착하면 Success, 가는 중에는 Running.",
        story: "[Point] 까지 [Speed] 로 이동한다",
        category: "AI Lab/Basic",
        id: "1a2b3c4d5e6f708192a3b4c5d6e7f808")]
    public partial class LabMoveToPointAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Point;
        [SerializeReference] public BlackboardVariable<float> Speed;

        protected override Status OnStart()
        {
            if (Point == null || Point.Value == null)
                return Status.Failure;

            LabNodeHelper.ReportAction(GameObject, "MOVE " + Point.Value.name, "Sequence 의 현재 단계");
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            if (ctx == null || Point == null || Point.Value == null)
                return Status.Failure;

            LabNodeHelper.ReportAction(GameObject, "MOVE " + Point.Value.name, "Sequence 의 현재 단계");

            float targetX = Point.Value.transform.position.x;
            float speed = Speed != null && Speed.Value > 0f ? Speed.Value : ctx.patrolSpeed;

            if (ctx.Motor != null)
                ctx.Motor.MoveTowardsX(targetX, speed);

            if (Mathf.Abs(ctx.transform.position.x - targetX) <= 0.12f)
            {
                if (ctx.Motor != null) ctx.Motor.Stop();
                return Status.Success;
            }

            return Status.Running;
        }
    }

    /// <summary>지정한 시간만큼 제자리에서 기다린다.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Lab Wait",
        description: "제자리에서 지정한 시간(초)만큼 기다린다. 기다리는 동안 Running.",
        story: "[Seconds] 초 기다린다",
        category: "AI Lab/Basic",
        id: "1a2b3c4d5e6f708192a3b4c5d6e7f807")]
    public partial class LabWaitAction : Action
    {
        [SerializeReference] public BlackboardVariable<float> Seconds;

        private float _endTime;

        protected override Status OnStart()
        {
            float duration = Seconds != null ? Mathf.Max(0f, Seconds.Value) : 1f;
            _endTime = Time.time + duration;

            AIContext ctx = LabNodeHelper.Context(GameObject);
            if (ctx != null && ctx.Motor != null)
                ctx.Motor.Stop();

            LabNodeHelper.ReportAction(GameObject, "WAIT", "Sequence 의 현재 단계");
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            LabNodeHelper.ReportAction(GameObject, "WAIT", "Sequence 의 현재 단계");
            return Time.time >= _endTime ? Status.Success : Status.Running;
        }
    }

    /// <summary>제자리에서 Player 를 바라본다.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Lab Face Player",
        description: "제자리에서 Player 쪽으로 방향만 돌린다.",
        story: "Player 를 바라본다",
        category: "AI Lab/Basic",
        id: "1a2b3c4d5e6f708192a3b4c5d6e7f809")]
    public partial class LabFacePlayerAction : Action
    {
        protected override Status OnStart()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            if (ctx == null)
                return Status.Failure;

            LabNodeHelper.ReportAction(GameObject, "LOOK", "Player 가 인식 범위 안에 있다");
            ctx.TickLookAtPlayer();
            return Status.Success;
        }
    }

    /// <summary>아무것도 하지 않고 제자리에 선다.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Lab Idle",
        description: "제자리에 가만히 서 있는다.",
        story: "가만히 있는다",
        category: "AI Lab/Basic",
        id: "1a2b3c4d5e6f708192a3b4c5d6e7f806")]
    public partial class LabIdleAction : Action
    {
        protected override Status OnStart()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            if (ctx == null)
                return Status.Failure;

            LabNodeHelper.ReportAction(GameObject, "IDLE", "선택할 다른 행동이 없다");
            ctx.TickIdle();
            return Status.Success;
        }
    }
}
