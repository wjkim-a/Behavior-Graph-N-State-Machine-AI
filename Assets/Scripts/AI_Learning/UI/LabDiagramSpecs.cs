using System.Collections.Generic;

namespace AILearning
{
    /// <summary>Behavior Tree 다이어그램의 한 가지(branch).</summary>
    public class TreeBranchSpec
    {
        /// <summary>이 가지의 조건 노드 이름. 조건이 없으면 null.</summary>
        public string ConditionName;

        /// <summary>BTNodeTrace 가 보고하는 라벨. 강조 판정에 사용한다.</summary>
        public string ActionLabel;

        /// <summary>화면에 표시할 Action 노드 이름.</summary>
        public string ActionTitle;

        public TreeBranchSpec(string conditionName, string actionLabel, string actionTitle)
        {
            ConditionName = conditionName;
            ActionLabel = actionLabel;
            ActionTitle = actionTitle;
        }
    }

    /// <summary>Behavior Tree 다이어그램 하나의 구조 정보.</summary>
    public class TreeSpec
    {
        public string RootTitle = "On Start (Repeat)";
        public string CompositeTitle = "Try In Order  (Selector)";
        public string CompositeNote = "위에서부터 확인, 처음 성립한 가지를 실행";
        public bool ShowPriority = true;

        /// <summary>
        /// 위쪽 칸이 실제 Condition 노드인가.
        /// true  : TRUE / FALSE 평가 결과를 표시한다.
        /// false : 단순 설명 문구로만 표시한다. (Sequence 실험처럼 조건이 없는 트리)
        /// </summary>
        public bool ConditionsAreEvaluated = true;
        public List<TreeBranchSpec> Branches = new List<TreeBranchSpec>();

        /// <summary>아래쪽 설명 문구 (왼쪽 열).</summary>
        public string[] Footer = new string[0];

        /// <summary>아래쪽 설명 문구 (오른쪽 열). 비어 있으면 왼쪽만 표시한다.</summary>
        public string[] Footer2 = new string[0];
    }

    /// <summary>
    /// 각 구조 종류별 다이어그램 정의.
    /// 실제 Behavior Graph 에셋의 구조와 1:1로 맞춰 두었다.
    /// </summary>
    public static class LabDiagramSpecs
    {
        /// <summary>STEP 05 / 06 의 완성형 전투 트리. (5개 가지)</summary>
        public static TreeSpec Full()
        {
            TreeSpec spec = new TreeSpec();
            spec.Branches.Add(new TreeBranchSpec("Is Dead", "DEAD", "Lab Die"));
            spec.Branches.Add(new TreeBranchSpec("Low HP", "FLEE", "Lab Flee"));
            spec.Branches.Add(new TreeBranchSpec("Attack Range", "ATTACK", "Lab Attack"));
            spec.Branches.Add(new TreeBranchSpec("Player Detected", "CHASE", "Lab Chase"));
            spec.Branches.Add(new TreeBranchSpec(null, "PATROL", "Lab Patrol"));
            spec.Footer = new[]
            {
                "Selector 는 왼쪽부터 순서대로 확인한다. 조건이 참인 첫 가지의 Action 을 실행한다.",
                "조건이 붙은 가지는 Sequence 로 묶여 있다. [Conditional Guard] 가 실패하면 그 가지 전체가 실패한다.",
                "마지막 Patrol 에는 조건이 없다. 그래서 앞의 조건이 모두 거짓일 때 실행되는 기본 행동이 된다."
            };
            return spec;
        }

        /// <summary>STEP 04 의 Selector 실험 트리. (3개 가지)</summary>
        public static TreeSpec SelectorDemo()
        {
            TreeSpec spec = new TreeSpec();
            spec.Branches.Add(new TreeBranchSpec("Attack Range", "ATTACK", "Lab Attack"));
            spec.Branches.Add(new TreeBranchSpec("Player Detected", "CHASE", "Lab Chase"));
            spec.Branches.Add(new TreeBranchSpec(null, "PATROL", "Lab Patrol"));
            spec.Footer = new[]
            {
                "SELECTOR = 여러 선택지 중 조건에 맞는 행동을 고른다.",
                "가까이 다가가면 위쪽 조건이 참이 되고, 선택되는 행동이 즉시 바뀐다.",
                "순서를 바꾸면 결과도 바뀐다. Selector 에서는 노드 순서가 곧 우선순위다."
            };
            spec.Footer2 = new[]
            {
                "<b>이 트리는 이렇게 조립했다</b>",
                "STEP A   ROOT 만 놓는다  (On Start)",
                "STEP B   ROOT 아래에 SELECTOR 를 붙인다",
                "STEP C   SELECTOR 아래에 ACTION 을 우선순위대로 붙인다",
                "STEP D   각 ACTION 앞에 CONDITION 을 붙인다",
                "             ← 지금 화면이 STEP D 상태다"
            };
            return spec;
        }

        /// <summary>STEP 04 의 Sequence + Repeat 실험 트리.</summary>
        public static TreeSpec SequenceDemo()
        {
            TreeSpec spec = new TreeSpec
            {
                RootTitle = "On Start",
                CompositeTitle = "Repeat  →  Sequence",
                CompositeNote = "순서대로 실행하고, 끝나면 처음부터 반복",
                ShowPriority = true,
                ConditionsAreEvaluated = false
            };
            spec.Branches.Add(new TreeBranchSpec("1번째로 실행", "MOVE Seq_PointA", "Move To A"));
            spec.Branches.Add(new TreeBranchSpec("2번째로 실행", "WAIT", "Wait 1.0s"));
            spec.Branches.Add(new TreeBranchSpec("3번째로 실행", "MOVE Seq_PointB", "Move To B"));
            spec.Footer = new[]
            {
                "SEQUENCE = 여러 행동을 순서대로 실행한다. 1번이 끝나야 2번으로 넘어간다.",
                "이동과 대기는 한 프레임에 끝나지 않으므로 Running 상태를 돌려준다.",
                "Running 인 동안 Sequence 는 다음 노드로 넘어가지 않고 기다린다.",
                "REPEAT 는 Sequence 가 모두 끝나면 다시 처음부터 실행시킨다."
            };
            return spec;
        }

        /// <summary>STEP 01 의 if / else AI 를 표현한 목록.</summary>
        public static TreeBranchSpec[] ReactiveRows()
        {
            return new[]
            {
                new TreeBranchSpec("거리 <= 공격 사거리", "ATTACK", "공격한다"),
                new TreeBranchSpec("거리 <= 추적 거리", "CHASE", "쫓아간다"),
                new TreeBranchSpec("거리 <= 인식 거리", "LOOK", "바라본다"),
                new TreeBranchSpec("그 외", "IDLE", "가만히 있는다")
            };
        }

        /// <summary>State Machine 다이어그램의 전환 규칙 목록. (from, to, 조건)</summary>
        public static readonly string[,] TransitionRules =
        {
            { "PATROL", "CHASE",  "Player 를 발견했다" },
            { "CHASE",  "ATTACK", "공격 사거리에 들어왔다" },
            { "ATTACK", "CHASE",  "공격 사거리에서 벗어났다" },
            { "CHASE",  "PATROL", "Player 를 놓쳤다" },
            { "*",      "FLEE",   "HP 30% 이하" },
            { "FLEE",   "PATROL", "회복하고 안전해졌다" },
            { "*",      "DEAD",   "HP 0" }
        };
    }
}
