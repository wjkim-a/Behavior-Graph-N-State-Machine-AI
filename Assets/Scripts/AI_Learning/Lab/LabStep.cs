using System;
using System.Collections.Generic;

namespace AILearning
{
    /// <summary>화면에 켜고 끌 수 있는 학습 패널.</summary>
    [Flags]
    public enum LabPanel
    {
        None = 0,
        Diagram = 1 << 0,      // 구조 다이어그램 (State Machine / Behavior Tree)
        Blackboard = 1 << 1,   // Blackboard 값 보기
        Compare = 1 << 2,      // 두 구조 비교
        Mission = 1 << 3,      // 미션 목록
        Quiz = 1 << 4,         // 자기진단
        Ranges = 1 << 5        // 감지 / 공격 범위 표시
    }

    /// <summary>학습 미션 하나. Check 가 참이 되면 자동으로 완료 표시된다.</summary>
    public class LabMission
    {
        public string Text;
        public Func<LabDirector, bool> Check;
        public bool Done;

        public LabMission(string text, Func<LabDirector, bool> check)
        {
            Text = text;
            Check = check;
        }
    }

    /// <summary>
    /// 학습 단계 하나.
    /// 설명(Body) → 질문(Questions) → 미션(Missions) 의 순서로 화면에 표시된다.
    /// </summary>
    public class LabStep
    {
        public int Number;
        public string Title;
        public string Headline;
        public string[] Body = Array.Empty<string>();
        public string[] Questions = Array.Empty<string>();
        public List<LabMission> Missions = new List<LabMission>();

        /// <summary>이 단계를 시작할 때 Player 를 옮겨줄 X 좌표.</summary>
        public float PlayerAnchorX;

        /// <summary>이 단계에서 관찰 대상이 되는 Enemy 들의 GameObject 이름.</summary>
        public string[] FocusEnemies = Array.Empty<string>();

        /// <summary>이 단계에 들어올 때 자동으로 열리는 패널.</summary>
        public LabPanel AutoPanels = LabPanel.None;

        /// <summary>다이어그램 패널이 이 단계에서 어떤 구조를 보여줄지.</summary>
        public AISystemKind DiagramKind = AISystemKind.StatePattern;

        /// <summary>다이어그램을 보여줄지 여부.</summary>
        public bool HasDiagram = true;
    }
}
