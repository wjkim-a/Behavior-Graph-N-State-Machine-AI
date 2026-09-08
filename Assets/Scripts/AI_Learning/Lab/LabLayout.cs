namespace AILearning
{
    /// <summary>
    /// 학습장의 배치 좌표.
    ///
    /// 씬을 만드는 빌더와, 단계 이동 시 Player 를 옮기는 LabDirector 가
    /// 같은 값을 봐야 하므로 여기 한 곳에 모아둔다.
    ///
    /// 지면(발이 닿는 높이)은 y = -3 이다.
    /// </summary>
    public static class LabLayout
    {
        /// <summary>지면 높이. 타일은 이 아래 칸에 채운다.</summary>
        public const float GroundY = -3f;

        /// <summary>Enemy 를 놓는 높이. 스프라이트 절반 높이만큼 위로 올린다.</summary>
        public const float EnemyY = -2.55f;

        /// <summary>Player 를 놓는 높이. 중력으로 지면에 내려앉는다.</summary>
        public const float PlayerY = -2.2f;

        /// <summary>표지판 높이.</summary>
        public const float SignY = -0.6f;

        /// <summary>지면 타일을 채우는 X 범위.</summary>
        public const int GroundMinX = -46;
        public const int GroundMaxX = 50;

        /// <summary>카메라가 이동할 수 있는 X 범위.</summary>
        public const float CameraMinX = -40f;
        public const float CameraMaxX = 44f;

        // STEP 01 : 구조 없는 AI
        public const float SimpleX = -30f;
        public const float Anchor01 = -38f;

        // STEP 02 / 03 : State Pattern
        public const float StateX = -12f;
        public const float StatePatrolLeft = -14.5f;
        public const float StatePatrolRight = -9.5f;
        public const float Anchor02 = -20f;

        // STEP 04-1 : Sequence + Repeat
        public const float SeqEnemyX = -1f;
        public const float SeqPointAX = -4f;
        public const float SeqPointBX = 2f;

        // STEP 04-2 : Selector + Condition
        public const float SelEnemyX = 9f;
        public const float SelPatrolLeft = 8f;
        public const float SelPatrolRight = 11.5f;
        public const float Anchor04 = 3f;

        // STEP 05 : 같은 AI 두 방식 비교
        public const float CompareStateX = 25f;
        public const float CompareStateLeft = 23.8f;
        public const float CompareStateRight = 26.2f;

        public const float CompareBtX = 28f;
        public const float CompareBtLeft = 26.8f;
        public const float CompareBtRight = 29.2f;

        public const float Anchor05 = 19f;

        // STEP 06 : 공유 Blackboard 3인조
        public const float SquadAX = 36f;
        public const float SquadBX = 39f;
        public const float SquadCX = 42f;
        public const float SquadPatrolSpan = 1.2f;
        public const float Anchor06 = 32f;
    }
}
