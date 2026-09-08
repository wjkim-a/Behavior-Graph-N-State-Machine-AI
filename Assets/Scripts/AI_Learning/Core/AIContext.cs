using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// AI 한 마리가 "세상에 대해 아는 것" 과 "쓸 수 있는 몸" 을 모아둔 곳.
    ///
    /// ===== 이 프로젝트에서 가장 중요한 클래스 =====
    /// State Pattern 버전 Enemy 와 Behavior Graph 버전 Enemy 는
    /// 판단 구조만 다르고, 감각(조건 판정)과 몸(이동/공격)은 완전히 같은 것을 쓴다.
    /// 그 "같은 부분" 이 바로 이 AIContext 다.
    ///
    /// 덕분에 학습자는 이렇게 비교할 수 있다.
    ///   - 결과(행동)는 같다.
    ///   - 그런데 구조(누가 무엇을 결정하는가)는 다르다.
    /// </summary>
    public class AIContext : MonoBehaviour
    {
        [Header("표시 정보")]
        [Tooltip("UI 에 표시할 이름.")]
        public string displayName = "Enemy";

        [Tooltip("이 AI 를 어떤 방식으로 구현했는가. UI 표시용.")]
        public AISystemKind systemKind = AISystemKind.StatePattern;

        [Header("추적 대상")]
        [Tooltip("비어 있으면 Player 태그 / 이름으로 자동 검색한다.")]
        public Transform player;

        [Header("감각(Sensor) - 조건 판정 기준")]
        [Tooltip("이 거리 안이면 Player 를 발견한다.")]
        public float detectRange = 5.5f;

        [Tooltip("한 번 발견한 뒤에는 이 거리까지는 놓치지 않는다. (깜빡임 방지)")]
        public float loseSightRange = 7.5f;

        [Tooltip("이 거리 안이면 공격 사거리에 들어온 것으로 본다.")]
        public float attackRange = 1.5f;

        [Header("이동 속도")]
        public float patrolSpeed = 1.6f;
        public float chaseSpeed = 3.1f;
        public float fleeSpeed = 3.6f;

        [Header("상태 전환 임계값")]
        [Range(0.05f, 0.95f)]
        [Tooltip("체력이 이 비율 이하로 떨어지면 도망친다.")]
        public float lowHealthRatio = 0.3f;

        [Header("도망 중 회복")]
        [Tooltip("도망치는 동안 초당 회복량. 0 이면 회복하지 않는다.")]
        public float fleeRegenPerSecond = 6f;

        [Header("공유 Blackboard")]
        [Tooltip("공유 칠판의 경보(AlarmActive)를 발견 신호로 취급한다. Blackboard 실험에서 켠다.")]
        public bool reactToSharedAlarm = false;

        // 캐시된 참조 -------------------------------------------------
        public Health Health { get; private set; }
        public AIMotor Motor { get; private set; }
        public AIAttack Attack { get; private set; }
        public PatrolRoute Route { get; private set; }
        public AIEventLog EventLog { get; private set; }

        /// <summary>이 AI 를 구동하는 두뇌(State Machine 또는 Behavior Graph 추적기).</summary>
        public IAIBrain Brain { get; private set; }

        /// <summary>실험 리셋용 시작 위치.</summary>
        public Vector3 StartPosition { get; private set; }

        private bool _sightLatched;

        private void Awake()
        {
            Health = GetComponent<Health>();
            Motor = GetComponent<AIMotor>();
            Attack = GetComponent<AIAttack>();
            Route = GetComponent<PatrolRoute>();
            EventLog = GetComponent<AIEventLog>();
            StartPosition = transform.position;
        }

        private void Start()
        {
            ResolvePlayer();

            // Brain 은 State Machine 또는 BT 추적기 중 실제로 붙어 있는 쪽이 잡힌다.
            Brain = GetComponent<IAIBrain>();

            // 공유 칠판에는 "칠판을 함께 보기로 한" AI 만 등록한다.
            // (그러지 않으면 학습장 반대편의 Enemy 가 Player 를 봤다는 이유로
            //  STEP 06 의 3인조가 갑자기 반응해서 실험이 헷갈리게 된다.)
            if (reactToSharedAlarm && SquadBlackboard.Instance != null)
                SquadBlackboard.Instance.Register(this);
        }

        private void OnDestroy()
        {
            if (SquadBlackboard.Instance != null)
                SquadBlackboard.Instance.Unregister(this);
        }

        private void ResolvePlayer()
        {
            if (player != null)
                return;

            GameObject found = null;
            try { found = GameObject.FindGameObjectWithTag("Player"); }
            catch (UnityException) { /* Player 태그가 없는 프로젝트도 있으므로 무시한다. */ }

            if (found == null)
            {
                PlayerController controller = FindFirstObjectByType<PlayerController>();
                if (controller != null)
                    found = controller.gameObject;
            }

            if (found == null)
                found = GameObject.Find("Player");

            if (found != null)
                player = found.transform;
        }

        // 조건 판정 (State 와 Behavior Graph 가 공통으로 사용) ----------

        public bool HasPlayer => player != null;

        public Vector3 PlayerPosition => player != null ? player.position : transform.position;

        /// <summary>Player 까지의 거리.</summary>
        public float DistanceToPlayer
        {
            get
            {
                if (player == null) return float.MaxValue;
                return Vector2.Distance(transform.position, player.position);
            }
        }

        /// <summary>
        /// 경보와 무관하게, 이 AI 가 직접 Player 를 보고 있는가.
        /// 공유 칠판은 이 값을 모아서 경보를 결정한다.
        /// </summary>
        public bool HasDirectSightOfPlayer
        {
            get
            {
                if (player == null || IsDead) return false;
                return DistanceToPlayer <= detectRange;
            }
        }

        /// <summary>
        /// AI 가 Player 를 발견했다고 판단하는 조건.
        /// - 처음에는 detectRange 안에 들어와야 발견된다.
        /// - 한 번 발견하면 loseSightRange 까지는 계속 발견 상태를 유지한다. (깜빡임 방지)
        /// - 공유 경보에 반응하도록 설정했다면, 경보만으로도 발견 상태가 된다.
        /// </summary>
        public bool PlayerDetected
        {
            get
            {
                if (IsDead || player == null)
                    return false;

                if (reactToSharedAlarm && AlarmActive)
                    return true;

                float d = DistanceToPlayer;
                if (_sightLatched)
                    _sightLatched = d <= loseSightRange;
                else
                    _sightLatched = d <= detectRange;

                return _sightLatched;
            }
        }

        /// <summary>공격 사거리 안인가.</summary>
        public bool InAttackRange => !IsDead && player != null && DistanceToPlayer <= attackRange;

        /// <summary>체력이 낮은가. (도망 조건)</summary>
        public bool IsLowHealth => Health != null && !IsDead && Health.HealthRatio <= lowHealthRatio;

        /// <summary>죽었는가.</summary>
        public bool IsDead => Health != null && Health.IsDead;

        /// <summary>체력 비율 0~1.</summary>
        public float HealthRatio => Health != null ? Health.HealthRatio : 1f;

        /// <summary>공유 칠판의 경보 상태.</summary>
        public bool AlarmActive => SquadBlackboard.Instance != null && SquadBlackboard.Instance.AlarmActive;

        // 행동 실행 헬퍼 (State / BT 노드가 그대로 호출한다) -------------

        /// <summary>순찰: 다음 지점으로 걸어가고, 도착하면 지점을 바꾼다.</summary>
        public void TickPatrol()
        {
            if (Motor == null) return;

            if (Route == null)
            {
                Motor.Stop();
                return;
            }

            if (Route.HasArrived(transform.position))
                Route.Advance();

            Motor.MoveTowardsX(Route.CurrentTarget.x, patrolSpeed);
        }

        /// <summary>추적: Player 쪽으로 걸어간다.</summary>
        public void TickChase()
        {
            if (Motor == null || player == null) return;
            Motor.MoveTowardsX(player.position.x, chaseSpeed);
        }

        /// <summary>공격: 멈춰서 Player 를 보고 공격을 시도한다.</summary>
        public bool TickAttack()
        {
            if (Motor == null) return false;

            Motor.Stop();
            if (player != null)
                Motor.FaceTowardsX(player.position.x);

            if (Attack == null || player == null)
                return false;

            Health targetHealth = player.GetComponent<Health>();
            return Attack.TryAttack(targetHealth);
        }

        /// <summary>도망: Player 반대 방향으로 달리고, 조금씩 회복한다.</summary>
        public void TickFlee()
        {
            if (Motor == null) return;

            if (player != null)
                Motor.MoveAwayFromX(player.position.x, fleeSpeed);
            else
                Motor.Stop();

            if (Health != null && fleeRegenPerSecond > 0f)
                Health.Heal(fleeRegenPerSecond * Time.deltaTime);
        }

        /// <summary>죽음: 더 이상 움직이지 않는다.</summary>
        public void TickDead()
        {
            if (Motor != null)
                Motor.Stop();
        }

        /// <summary>제자리 대기.</summary>
        public void TickIdle()
        {
            if (Motor != null)
                Motor.Stop();
        }

        /// <summary>Player 를 바라보기만 한다. (STEP 01 에서 사용)</summary>
        public void TickLookAtPlayer()
        {
            if (Motor == null) return;
            Motor.Stop();
            if (player != null)
                Motor.FaceTowardsX(player.position.x);
        }

        // 실험 리셋 ---------------------------------------------------

        /// <summary>실험을 처음부터 다시 하기 위해 초기 상태로 되돌린다.</summary>
        public void ResetAI()
        {
            _sightLatched = false;

            if (Health != null) Health.ResetHealth();
            if (Attack != null) Attack.ResetAttack();
            if (Route != null) Route.ResetRoute();
            if (Motor != null) Motor.TeleportTo(StartPosition);
            if (EventLog != null) EventLog.Clear();

            transform.rotation = Quaternion.identity;

            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }
    }
}
