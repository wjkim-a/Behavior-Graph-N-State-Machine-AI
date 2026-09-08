using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

// =====================================================================
//  AI Lab - Behavior Graph 의 Action 노드 모음 (전투 AI 용)
// =====================================================================
//
//  Action 노드란?
//    "실제로 행동하는 노드" 다. 이동한다 / 공격한다 / 도망친다.
//    (반대로 Condition 은 판단만 하고 행동하지 않는다.)
//
//  이 노드들의 공통 규칙
//  --------------------------------------------------------------
//  1) 노드는 스스로 감각을 갖지 않는다. AIContext 를 통해 세상을 본다.
//     => State Pattern 버전과 똑같은 감각을 쓴다는 뜻이다.
//
//  2) 노드는 한 프레임 분량의 행동만 하고 Success 를 돌려준다.
//     왜 그렇게 하는가?
//       Behavior Tree 는 매 프레임 Root 부터 다시 평가한다.
//       그래야 "더 우선순위가 높은 조건이 방금 참이 되었는가?" 를
//       매 프레임 다시 확인할 수 있다.
//       만약 Chase 가 Running 을 계속 돌려주면 트리는 그 가지에 갇혀서
//       공격 사거리에 들어온 것을 알아채지 못한다.
//
//  3) 노드는 실행될 때 자기 이름을 BTNodeTrace 에 보고한다.
//     학습용 UI 가 트리 그림에서 그 노드를 강조하기 위해서다.
// =====================================================================

namespace AILearning
{
    /// <summary>순찰 행동. State Pattern 의 PatrolState.Execute 와 완전히 같은 일을 한다.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Lab Patrol",
        description: "순찰 지점 사이를 왕복한다. (AIContext.TickPatrol)",
        story: "순찰한다",
        category: "AI Lab/Action",
        id: "1a2b3c4d5e6f708192a3b4c5d6e7f801")]
    public partial class LabPatrolAction : Action
    {
        protected override Status OnStart()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            if (ctx == null)
                return Status.Failure;

            LabNodeHelper.ReportAction(GameObject, "PATROL", "위쪽 조건이 모두 거짓이므로 기본 행동이 선택되었다");
            ctx.TickPatrol();
            return Status.Success;
        }
    }

    /// <summary>추적 행동.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Lab Chase",
        description: "Player 쪽으로 이동한다. (AIContext.TickChase)",
        story: "Player 를 추적한다",
        category: "AI Lab/Action",
        id: "1a2b3c4d5e6f708192a3b4c5d6e7f802")]
    public partial class LabChaseAction : Action
    {
        protected override Status OnStart()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            if (ctx == null)
                return Status.Failure;

            LabNodeHelper.ReportAction(GameObject, "CHASE", "Player Detected 조건이 참이다");
            ctx.TickChase();
            return Status.Success;
        }
    }

    /// <summary>공격 행동.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Lab Attack",
        description: "제자리에서 Player 를 공격한다. (AIContext.TickAttack)",
        story: "Player 를 공격한다",
        category: "AI Lab/Action",
        id: "1a2b3c4d5e6f708192a3b4c5d6e7f803")]
    public partial class LabAttackAction : Action
    {
        protected override Status OnStart()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            if (ctx == null)
                return Status.Failure;

            LabNodeHelper.ReportAction(GameObject, "ATTACK", "Attack Range 조건이 참이다");
            ctx.TickAttack();
            return Status.Success;
        }
    }

    /// <summary>도망 행동.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Lab Flee",
        description: "Player 반대 방향으로 도망치며 조금씩 회복한다. (AIContext.TickFlee)",
        story: "Player 에게서 도망친다",
        category: "AI Lab/Action",
        id: "1a2b3c4d5e6f708192a3b4c5d6e7f804")]
    public partial class LabFleeAction : Action
    {
        protected override Status OnStart()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            if (ctx == null)
                return Status.Failure;

            LabNodeHelper.ReportAction(GameObject, "FLEE", "Low HP 조건이 참이다");
            ctx.TickFlee();
            return Status.Success;
        }
    }

    /// <summary>사망 처리. 최우선 가지에서 실행된다.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Lab Die",
        description: "쓰러진 상태를 유지한다. (AIContext.TickDead)",
        story: "쓰러진다",
        category: "AI Lab/Action",
        id: "1a2b3c4d5e6f708192a3b4c5d6e7f805")]
    public partial class LabDieAction : Action
    {
        protected override Status OnStart()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            if (ctx == null)
                return Status.Failure;

            LabNodeHelper.ReportAction(GameObject, "DEAD", "Is Dead 조건이 참이다");
            ctx.TickDead();

            // 같은 값을 다시 써도 문제가 없으므로 매 tick 그대로 적용한다.
            // (실험 리셋으로 원래 모습이 복구된 뒤에도 다시 죽으면 정상 동작한다.)
            ApplyDeathVisual(ctx);

            return Status.Success;
        }

        private static void ApplyDeathVisual(AIContext ctx)
        {
            ctx.transform.rotation = Quaternion.Euler(0f, 0f, -80f);

            SpriteRenderer sr = ctx.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0.45f;
                sr.color = c;
            }

            Collider2D col = ctx.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }
    }
}
