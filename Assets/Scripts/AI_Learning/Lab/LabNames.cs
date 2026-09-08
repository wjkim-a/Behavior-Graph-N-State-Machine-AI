namespace AILearning
{
    /// <summary>
    /// 씬에 배치되는 Enemy 들의 이름.
    /// 미션 판정과 UI 가 이 이름으로 대상을 찾으므로 한 곳에 모아둔다.
    /// </summary>
    public static class LabNames
    {
        // STEP 01 : 구조 없는 가장 단순한 AI
        public const string Simple = "Enemy_01_Simple";

        // STEP 02 / 03 : State Pattern 학습용
        public const string State = "Enemy_02_StatePattern";

        // STEP 04 : Behavior Tree 구성요소 학습용
        public const string BtSequence = "Enemy_04_BT_Sequence";
        public const string BtSelector = "Enemy_04_BT_Selector";

        // STEP 05 / 06 : 같은 요구사항을 두 방식으로 구현한 한 쌍
        public const string CompareState = "Enemy_05_A_StatePattern";
        public const string CompareBt = "Enemy_05_B_BehaviorGraph";

        // STEP 06 : 공유 Blackboard 실험용 3인조
        public const string SquadA = "Enemy_06_Squad_A";
        public const string SquadB = "Enemy_06_Squad_B";
        public const string SquadC = "Enemy_06_Squad_C";

        public const string Player = "LabPlayer";
    }
}
