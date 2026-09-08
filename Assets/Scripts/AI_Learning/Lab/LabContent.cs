namespace AILearning
{
    /// <summary>
    /// 학습 화면에 표시되는 고정 텍스트 모음.
    /// (설명 문구를 한 곳에서 관리하면 교육자가 문장만 고쳐 쓸 수 있다.)
    /// </summary>
    public static class LabContent
    {
        // 비교 화면(C 키)의 문구.
        //
        // 줄 수를 세어 두었다. 화면 높이가 정해져 있으므로 여기서 줄이 늘어나면
        // LabUI.BuildComparePage 의 CompareMindsetLines / ComparePodLines 도 같이 올려야 한다.
        // 그러지 않으면 텍스트가 패널을 뚫고 내려가 아래 문단과 겹친다.

        /// <summary>STEP 06 의 비교 화면 좌측: State Pattern 사고방식. (8줄)</summary>
        public static readonly string[] StatePatternMindset =
        {
            "<b>\"나는 지금 어떤 상태인가?\"</b>",
            "현재 상태를 중심으로 행동을 관리한다.",
            "",
            "PATROL   ·   CHASE   ·   ATTACK   ·   FLEE   ·   DEAD",
            "다섯 상태 중 언제나 하나만 활성된다.",
            "",
            "각 상태가 자기 행동을 담당하고,",
            "조건이 만족되면 다른 상태로 이동한다."
        };

        /// <summary>STEP 06 의 비교 화면 우측: Behavior Tree 사고방식. (10줄)</summary>
        public static readonly string[] BehaviorTreeMindset =
        {
            "<b>\"지금 어떤 행동을 선택해야 하는가?\"</b>",
            "조건과 우선순위를 평가해 실행할 행동을 정한다.",
            "",
            "Is Dead?       →  Die",
            "Low HP?        →  Flee",
            "Attack Range?  →  Attack",
            "Detected?      →  Chase",
            "(그 외)        →  Patrol",
            "",
            "위에서부터 확인하고, 처음 성립한 가지를 실행한다."
        };

        // 둘째 / 넷째 줄의 앞 공백은 앞 줄 라벨("장점" / "주의점")의 폭에 맞춘 것이다.
        // 비례 폭 글꼴이라 공백 하나는 한글 한 자의 약 0.28배다.

        /// <summary>(4줄)</summary>
        public static readonly string[] StatePatternPros =
        {
            "<b>장점</b>      상태가 명확하고, 상태별 코드 분리가 쉽다.",
            "           간단한 AI 에서 이해하기 쉽고 흐름이 보인다.",
            "<b>주의점</b>   상태가 많아지면 전환 관계가 복잡해지고,",
            "            상태 간 연결이 급격히 늘어난다."
        };

        /// <summary>(4줄)</summary>
        public static readonly string[] BehaviorTreePros =
        {
            "<b>장점</b>      복잡한 행동 조합에 유리하고, 구조가 눈에 보인다.",
            "           기획자 · 디자이너도 읽을 수 있고 조합이 자유롭다.",
            "<b>주의점</b>   트리가 커지면 복잡해진다.",
            "            단순한 AI 에서는 오히려 과할 수 있다."
        };

        public static readonly string[] WhenToUse =
        {
            "<b>언제 무엇을 고르는가</b>",
            "",
            "상태가 뚜렷하고 개수가 적다  →  State Pattern",
            "  예: 문(열림/닫힘/잠김), 간단한 순찰 몬스터, 캐릭터 이동 상태",
            "",
            "조건이 많고 우선순위가 중요하다  →  Behavior Tree",
            "  예: 은신 / 엄폐 / 회피 / 재장전 / 지원 요청이 섞인 전투 AI",
            "",
            "정답은 하나가 아니다. 두 방식을 함께 쓰는 게임도 많다.",
            "  예: 큰 흐름은 State Machine, 전투 내부 판단은 Behavior Tree"
        };

        /// <summary>자기진단 질문 (Q 키).</summary>
        public static readonly string[] SelfCheck =
        {
            "1.  State 란 무엇인가?",
            "2.  State Transition 이란 무엇인가?",
            "3.  State Pattern 은 어떤 문제를 해결하기 위한 패턴인가?",
            "4.  Enter / Execute / Exit 의 역할은 무엇인가?",
            "5.  Behavior Tree 란 무엇인가?",
            "6.  Sequence 는 무엇을 하는가?",
            "7.  Selector 는 무엇을 하는가?",
            "8.  Condition 과 Action 의 차이는 무엇인가?",
            "9.  Blackboard 는 왜 필요한가?",
            "10. 같은 Enemy 를 State Pattern 과 Behavior Tree 로 만들었을 때",
            "     구조적으로 어떤 차이가 있는가?",
            "11. 간단한 상태 중심 AI 와 복잡한 행동 조합 AI 중",
            "     각각 어떤 구조가 더 적합할까?"
        };

        public static readonly string[] SelfCheckHint =
        {
            "정답을 외우는 것이 목적이 아니다.",
            "직접 본 장면을 근거로 자기 말로 설명할 수 있으면 충분하다.",
            "",
            "예) \"HP 를 30% 아래로 낮췄더니 어떤 상태에 있든 FLEE 로 바뀌었다.\"",
            "     \"그래서 State Pattern 에서는 이런 전환이 여러 상태에 중복으로 필요했다.\"",
            "     \"Behavior Tree 에서는 Low HP 가지를 위쪽에 한 번만 두면 되었다.\""
        };

        /// <summary>화면 하단 조작 안내.</summary>
        public static readonly string ControlsLine1 =
            "이동 A/D  ·  점프 Space  ·  STEP 이동 1~6  ·  AI 선택 Tab  ·  피해 F  ·  즉시사망 G  ·  회복 T  ·  리셋 R";

        public static readonly string ControlsLine2 =
            "가운데 화면 전환 :  설명 M  ·  구조 다이어그램 E  ·  두 구조 비교 C  ·  Blackboard B  ·  자기진단 Q        범위표시 V  ·  경보 Z  ·  UI 숨기기 H";
    }
}
