using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

// =====================================================================
//  AI Lab - Behavior Graph 의 Condition 노드 모음
// =====================================================================
//
//  Condition 과 Action 의 차이
//  --------------------------------------------------------------
//    Condition : 판단만 한다. 참/거짓만 돌려준다. 세상을 바꾸지 않는다.
//    Action    : 실제로 행동한다. 이동하거나 공격해서 세상을 바꾼다.
//
//  Behavior Tree 는 이 둘을 조합해서 다음을 표현한다.
//    "조건이 참이면 그 행동을 한다."
//
//  이 조건들은 State Pattern 이 쓰는 것과 완전히 같은 AIContext 를 읽는다.
//  즉 두 AI 는 같은 감각을 갖고, 판단 구조만 다르다.
// =====================================================================

namespace AILearning
{
    /// <summary>Player 를 발견했는가.</summary>
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Lab Player Detected",
        description: "감지 범위 안에 Player 가 있으면 참. (AIContext.PlayerDetected)",
        story: "Player 를 발견했는가",
        category: "AI Lab/Condition",
        id: "2a2b3c4d5e6f708192a3b4c5d6e7f801")]
    public partial class LabPlayerDetectedCondition : Condition
    {
        public override bool IsTrue()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            bool result = ctx != null && ctx.PlayerDetected;
            return LabNodeHelper.ReportCondition(GameObject, "Player Detected", result);
        }
    }

    /// <summary>공격 사거리에 들어왔는가.</summary>
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Lab In Attack Range",
        description: "공격 사거리 안에 Player 가 있으면 참. (AIContext.InAttackRange)",
        story: "공격 사거리에 들어왔는가",
        category: "AI Lab/Condition",
        id: "2a2b3c4d5e6f708192a3b4c5d6e7f802")]
    public partial class LabInAttackRangeCondition : Condition
    {
        public override bool IsTrue()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            bool result = ctx != null && ctx.InAttackRange;
            return LabNodeHelper.ReportCondition(GameObject, "Attack Range", result);
        }
    }

    /// <summary>체력이 낮은가.</summary>
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Lab Low Health",
        description: "체력이 임계값(기본 30%) 이하이면 참. (AIContext.IsLowHealth)",
        story: "체력이 낮은가",
        category: "AI Lab/Condition",
        id: "2a2b3c4d5e6f708192a3b4c5d6e7f803")]
    public partial class LabLowHealthCondition : Condition
    {
        public override bool IsTrue()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            bool result = ctx != null && ctx.IsLowHealth;
            return LabNodeHelper.ReportCondition(GameObject, "Low HP", result);
        }
    }

    /// <summary>죽었는가.</summary>
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Lab Is Dead",
        description: "체력이 0 이면 참. (AIContext.IsDead)",
        story: "죽었는가",
        category: "AI Lab/Condition",
        id: "2a2b3c4d5e6f708192a3b4c5d6e7f804")]
    public partial class LabIsDeadCondition : Condition
    {
        public override bool IsTrue()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            bool result = ctx != null && ctx.IsDead;
            return LabNodeHelper.ReportCondition(GameObject, "Is Dead", result);
        }
    }

    /// <summary>공유 칠판의 경보가 켜져 있는가.</summary>
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Lab Alarm Active",
        description: "공유 Blackboard 의 경보가 켜져 있으면 참. (SquadBlackboard.AlarmActive)",
        story: "공유 경보가 켜졌는가",
        category: "AI Lab/Condition",
        id: "2a2b3c4d5e6f708192a3b4c5d6e7f805")]
    public partial class LabAlarmActiveCondition : Condition
    {
        public override bool IsTrue()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            bool result = ctx != null && ctx.AlarmActive;
            return LabNodeHelper.ReportCondition(GameObject, "Alarm Active", result);
        }
    }

    /// <summary>인식 범위 안에 있는가. (STEP 01 / Selector 실험용, 감지보다 넓은 범위)</summary>
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Lab Player Nearby",
        description: "감지 범위보다 넓은 범위(loseSightRange) 안에 Player 가 있으면 참.",
        story: "Player 가 근처에 있는가",
        category: "AI Lab/Condition",
        id: "2a2b3c4d5e6f708192a3b4c5d6e7f806")]
    public partial class LabPlayerNearbyCondition : Condition
    {
        public override bool IsTrue()
        {
            AIContext ctx = LabNodeHelper.Context(GameObject);
            bool result = ctx != null && ctx.HasPlayer && ctx.DistanceToPlayer <= ctx.loseSightRange;
            return LabNodeHelper.ReportCondition(GameObject, "Player Nearby", result);
        }
    }
}
