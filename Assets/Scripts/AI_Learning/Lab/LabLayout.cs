namespace AILearning
{
    /// <summary>
    /// 학습장의 배치 좌표.
    ///
    /// 씬을 만드는 빌더와, 단계 이동 시 Player 를 옮기는 LabDirector 가
    /// 같은 값을 봐야 하므로 여기 한 곳에 모아둔다.
    ///
    /// 배치 원칙 — 단계마다 독립된 플랫폼을 쓴다
    ///   예전에는 -46 ~ 50 을 한 줄로 이어 깔고 단계별 구역만 나눠 두었다.
    ///   그러면 Enemy 의 이동 범위(순찰 구간 ± 여유)가 옆 단계까지 넘어가서
    ///   서로 밀고 겹쳐 버린다. STEP 05 의 두 마리, STEP 06 의 세 마리가 특히 심했다.
    ///
    ///   그래서 지금은
    ///     1) 단계별로 지면 플랫폼을 따로 깔고 사이를 빈 공간으로 띄운다.
    ///     2) 플랫폼 양 끝에 벽을 세워 Player 가 빈 공간으로 떨어지지 않게 한다.
    ///     3) Enemy 마다 자기 "레인(Lane)" 을 주고 AIMotor 를 그 안으로 묶는다.
    ///        레인은 절대로 서로 겹치지 않는다. → 두 Enemy 가 같은 자리를 다투지 않는다.
    ///
    ///   ┌──── STEP 01 ────┐   ┌──── STEP 02·03 ────┐   ┌── STEP 04 ──┐  ...
    ///   │  A              │   │   State            │   │ Seq    Sel  │
    ///   └─────────────────┘   └────────────────────┘   └─────────────┘
    ///            ↑ 빈 공간(벽으로 막힘)이 각 단계를 눈으로도 갈라준다
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

        /// <summary>표지판 높이. Enemy 라벨 바로 위에 오도록 맞춰 둔 값이다.</summary>
        public const float SignY = -0.6f;

        // =============================================================
        //  카메라
        // =============================================================

        /// <summary>
        /// 카메라 Y. 화면 아래쪽을 학습 패널이 차지하므로 지면보다 아래로 내린다.
        /// 이 값과 LabUI 의 BandTop 은 같이 움직여야 한다.
        /// (지면 y=-3 이 화면 740px 쯤에 오고, 패널 위쪽 끝이 724px 이다.)
        /// </summary>
        public const float CameraY = -5.22f;

        /// <summary>카메라 orthographicSize.</summary>
        public const float CameraSize = 6f;

        /// <summary>카메라가 이동할 수 있는 X 범위.</summary>
        public const float CameraMinX = -38f;
        public const float CameraMaxX = 118f;

        // =============================================================
        //  플랫폼 — 단계별 지면
        // =============================================================

        /// <summary>플랫폼 하나. 타일이 깔리는 X 구간이다. (양 끝 바깥에 벽이 선다)</summary>
        public struct Platform
        {
            public readonly int MinX;
            public readonly int MaxX;

            public Platform(int minX, int maxX)
            {
                MinX = minX;
                MaxX = maxX;
            }
        }

        /// <summary>벽의 높이(타일 칸 수). 점프로 넘어갈 수 없을 만큼 높게 둔다.</summary>
        public const int WallHeight = 6;

        /// <summary>단계별 플랫폼. 사이의 빈 칸이 곧 "맵 이격" 이다.</summary>
        public static readonly Platform[] Platforms =
        {
            new Platform(-46, -26),   // STEP 01
            new Platform(-16, 6),     // STEP 02 · 03
            new Platform(16, 50),     // STEP 04  (Sequence 실험 + Selector 실험)
            new Platform(60, 86),     // STEP 05
            new Platform(96, 126)     // STEP 06
        };

        // =============================================================
        //  STEP 01 : 구조 없는 AI
        // =============================================================
        public const float Anchor01 = -42f;
        public const float SimpleX = -34f;

        /// <summary>제자리에서 판단만 하는 AI 지만, 추적할 때 움직일 여유를 준다.</summary>
        public const float SimpleLaneL = -40f;
        public const float SimpleLaneR = -28f;

        // =============================================================
        //  STEP 02 / 03 : State Pattern
        // =============================================================
        public const float Anchor02 = -12f;
        public const float StateX = -4f;
        public const float StatePatrolLeft = -6.5f;
        public const float StatePatrolRight = -1.5f;
        public const float StateLaneL = -9.5f;
        public const float StateLaneR = 1.5f;

        // =============================================================
        //  STEP 04-1 : Sequence + Repeat
        // =============================================================
        public const float Anchor04 = 18f;
        public const float SeqEnemyX = 24f;
        public const float SeqPointAX = 21f;
        public const float SeqPointBX = 27f;
        public const float SeqLaneL = 19f;
        public const float SeqLaneR = 29f;

        // =============================================================
        //  STEP 04-2 : Selector + Condition
        //  Sequence 실험과 같은 플랫폼에 있지만 레인이 겹치지 않는다. (29 < 35.5)
        // =============================================================
        public const float SelEnemyX = 40f;
        public const float SelPatrolLeft = 38.5f;
        public const float SelPatrolRight = 42f;
        public const float SelLaneL = 35.5f;
        public const float SelLaneR = 45f;

        // =============================================================
        //  STEP 05 : 같은 AI 두 방식 비교
        //  두 마리가 "동시에" 반응해야 하므로 가까이 두되(5칸), 레인은 갈라 둔다.
        //  Player 가 74 에 서면 양쪽 모두 감지 범위(5) 안에 들어온다.
        // =============================================================
        public const float Anchor05 = 64f;

        public const float CompareStateX = 71f;
        public const float CompareStateLeft = 69.8f;
        public const float CompareStateRight = 72.2f;
        public const float CompareStateLaneL = 69f;
        public const float CompareStateLaneR = 73.2f;

        public const float CompareBtX = 76f;
        public const float CompareBtLeft = 74.8f;
        public const float CompareBtRight = 77.2f;
        public const float CompareBtLaneL = 74.8f;
        public const float CompareBtLaneR = 79f;

        /// <summary>두 구조를 한눈에 보는 자리. 표지판도 여기 세운다.</summary>
        public const float CompareCenterX = 74f;

        // 레인 사이가 1.6칸 벌어져 있다. 캡슐 지름이 0.7칸이므로 서로 닿지 않는다.
        // 동시에 두 마리를 ATTACK 으로 만들려면 74 근처(73.4 ~ 74.6)에 서면 된다.
        // (공격 사거리 1.4 기준. 레인을 더 벌리면 이 구간이 사라진다.)

        // =============================================================
        //  STEP 06 : 공유 Blackboard 3인조
        //  감지 범위 3.2 보다 넓게(4.5) 벌려 둔다.
        //  → 한 마리 앞에 서면 그 한 마리만 직접 발견한다. 나머지는 "경보" 로 반응한다.
        // =============================================================
        public const float Anchor06 = 99f;
        public const float SquadAX = 106f;
        public const float SquadBX = 110.5f;
        public const float SquadCX = 115f;
        public const float SquadPatrolSpan = 1.2f;

        /// <summary>3인조 각자의 레인 절반 폭. 서로 겹치지 않는 최대치다.</summary>
        public const float SquadLaneSpan = 1.7f;
    }
}
