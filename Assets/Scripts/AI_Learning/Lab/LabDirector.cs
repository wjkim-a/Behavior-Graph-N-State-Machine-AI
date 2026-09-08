using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AILearning
{
    /// <summary>
    /// AI Learning Lab 의 진행 담당.
    ///
    /// 하는 일
    ///   - 6개 학습 단계의 내용을 들고 있는다.
    ///   - 학습자의 키 입력을 받아 단계 이동 / 실험 조작을 처리한다.
    ///   - AI 들이 실제로 어떤 상태 / 노드를 실행했는지 기록해서 미션을 자동 채점한다.
    ///
    /// 학습자는 코드를 쓰지 않는다. Play 후 키만 누르면 된다.
    /// </summary>
    public class LabDirector : MonoBehaviour
    {
        [Header("참조")]
        public Transform playerRoot;

        [Header("옵션")]
        [Tooltip("F 키 한 번에 주는 피해량.")]
        public float manualDamage = 15f;

        private readonly List<LabStep> _steps = new List<LabStep>();
        private readonly List<AIContext> _enemies = new List<AIContext>();
        private readonly Dictionary<string, AIContext> _byName = new Dictionary<string, AIContext>();

        /// <summary>관찰된 사건 기록. 미션 채점의 근거가 된다.</summary>
        private readonly HashSet<string> _observed = new HashSet<string>();

        private readonly Dictionary<AIContext, string> _prevLabel = new Dictionary<AIContext, string>();

        private int _stepIndex;
        private LabPanel _panels = LabPanel.None;
        private AIContext _selected;
        private int _selectionCursor;

        public IReadOnlyList<LabStep> Steps => _steps;
        public IReadOnlyList<AIContext> Enemies => _enemies;
        public LabStep CurrentStep => _steps[Mathf.Clamp(_stepIndex, 0, _steps.Count - 1)];
        public int StepIndex => _stepIndex;
        public int StepCount => _steps.Count;
        public AIContext Selected => _selected;
        public bool UiHidden { get; private set; }

        /// <summary>마지막으로 눌린 조작에 대한 안내 문구. 화면 하단에 잠깐 표시된다.</summary>
        public string Toast { get; private set; } = string.Empty;
        private float _toastUntil;

        public bool IsOpen(LabPanel panel) => (_panels & panel) != 0;

        public bool Has(string key) => _observed.Contains(key);

        // 초기화 ------------------------------------------------------

        private void Awake()
        {
            // 학습자가 다른 창을 클릭해도 실험이 멈추지 않게 한다.
            // (에디터가 포커스를 잃으면 기본값으로는 게임이 정지한다.)
            Application.runInBackground = true;

            BuildSteps();
        }

        private void Start()
        {
            CollectEnemies();

            if (playerRoot == null)
            {
                PlayerController pc = FindFirstObjectByType<PlayerController>();
                if (pc != null) playerRoot = pc.transform;
            }

            GoToStep(0, movePlayer: true);
        }

        private void CollectEnemies()
        {
            _enemies.Clear();
            _byName.Clear();

            AIContext[] found = FindObjectsByType<AIContext>(FindObjectsSortMode.None);
            foreach (AIContext c in found)
            {
                _enemies.Add(c);
                _byName[c.gameObject.name] = c;
            }

            // 이름 순서가 아니라 X 좌표 순서로 정렬해두면 Tab 순환이 자연스럽다.
            _enemies.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        }

        public AIContext Find(string goName)
        {
            return _byName.TryGetValue(goName, out AIContext c) ? c : null;
        }

        // 매 프레임 ---------------------------------------------------

        private void Update()
        {
            ReadInput();
            RecordObservations();
            EvaluateMissions();

            if (Time.time > _toastUntil)
                Toast = string.Empty;
        }

        private void RecordObservations()
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                AIContext c = _enemies[i];
                if (c == null || c.Brain == null)
                    continue;

                string label = c.Brain.CurrentLabel;
                if (string.IsNullOrEmpty(label) || label == "-")
                    continue;

                string key = c.gameObject.name;
                _observed.Add(key + ":" + label);

                if (_prevLabel.TryGetValue(c, out string prev))
                {
                    if (prev != label)
                        _observed.Add(key + ":" + prev + ">" + label);
                }
                _prevLabel[c] = label;
            }

            if (SquadBlackboard.Instance != null && SquadBlackboard.Instance.AlarmActive)
            {
                _observed.Add("ALARM");

                int chasing = 0;
                foreach (AIContext c in _enemies)
                {
                    if (c == null || c.Brain == null || !c.reactToSharedAlarm) continue;
                    if (c.Brain.CurrentLabel == "CHASE" || c.Brain.CurrentLabel == "ATTACK")
                        chasing++;
                }
                if (chasing >= 2) _observed.Add("ALARM_GROUP_REACT");
                if (chasing >= 3) _observed.Add("ALARM_ALL_REACT");
            }
        }

        private void EvaluateMissions()
        {
            // 지금 보고 있는 단계의 미션만 채점한다.
            // (다른 구역의 AI 가 알아서 움직여서 미션이 저절로 완료되는 것을 막는다.)
            List<LabMission> missions = CurrentStep.Missions;
            for (int m = 0; m < missions.Count; m++)
            {
                LabMission mission = missions[m];
                if (mission.Done || mission.Check == null)
                    continue;

                if (mission.Check(this))
                    mission.Done = true;
            }
        }

        // 입력 --------------------------------------------------------

        private void ReadInput()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null)
                return;

            if (kb.digit1Key.wasPressedThisFrame) GoToStep(0, true);
            if (kb.digit2Key.wasPressedThisFrame) GoToStep(1, true);
            if (kb.digit3Key.wasPressedThisFrame) GoToStep(2, true);
            if (kb.digit4Key.wasPressedThisFrame) GoToStep(3, true);
            if (kb.digit5Key.wasPressedThisFrame) GoToStep(4, true);
            if (kb.digit6Key.wasPressedThisFrame) GoToStep(5, true);

            if (kb.tabKey.wasPressedThisFrame) SelectNext();

            if (kb.fKey.wasPressedThisFrame) DamageSelected(manualDamage);
            if (kb.gKey.wasPressedThisFrame) KillSelected();
            if (kb.tKey.wasPressedThisFrame) HealSelected();
            if (kb.rKey.wasPressedThisFrame) ResetExperiment();
            if (kb.zKey.wasPressedThisFrame) ToggleAlarm();

            // 구조 다이어그램은 E 키다. D 는 Player 이동(A / D)에 이미 쓰이고 있어서
            // 오른쪽으로 걸어가려는 순간마다 화면이 바뀌어 버렸다.
            if (kb.eKey.wasPressedThisFrame) TogglePanel(LabPanel.Diagram);
            if (kb.cKey.wasPressedThisFrame) TogglePanel(LabPanel.Compare);
            if (kb.bKey.wasPressedThisFrame) TogglePanel(LabPanel.Blackboard);
            if (kb.mKey.wasPressedThisFrame) TogglePanel(LabPanel.Mission);
            if (kb.qKey.wasPressedThisFrame) TogglePanel(LabPanel.Quiz);
            if (kb.vKey.wasPressedThisFrame) TogglePanel(LabPanel.Ranges);

            if (kb.hKey.wasPressedThisFrame)
            {
                UiHidden = !UiHidden;
                ShowToast(UiHidden ? "UI 숨김 (H 로 다시 표시)" : "UI 표시");
            }
        }

        // 조작 --------------------------------------------------------

        public void GoToStep(int index, bool movePlayer)
        {
            _stepIndex = Mathf.Clamp(index, 0, _steps.Count - 1);
            LabStep step = CurrentStep;

            // 이 단계에서 자동으로 열리는 페이지를 반영한다.
            // 페이지는 한 번에 하나만 열리므로, 여러 개가 지정되어 있으면 우선순위대로 하나만 남긴다.
            LabPanel wanted = step.AutoPanels & PageMask;
            if (wanted != LabPanel.None)
            {
                if ((wanted & LabPanel.Diagram) != 0) wanted = LabPanel.Diagram;
                else if ((wanted & LabPanel.Compare) != 0) wanted = LabPanel.Compare;
                else if ((wanted & LabPanel.Quiz) != 0) wanted = LabPanel.Quiz;
                else wanted = LabPanel.Blackboard;
            }

            _panels = wanted | (_panels & LabPanel.Ranges);

            if (movePlayer && playerRoot != null)
            {
                Vector3 p = playerRoot.position;
                p.x = step.PlayerAnchorX;
                playerRoot.position = p;

                Rigidbody2D rb = playerRoot.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }

            _selectionCursor = 0;
            _selected = FirstFocusEnemy(step);

            ShowToast("STEP " + step.Number.ToString("00") + " - " + step.Title);
        }

        private AIContext FirstFocusEnemy(LabStep step)
        {
            for (int i = 0; i < step.FocusEnemies.Length; i++)
            {
                AIContext c = Find(step.FocusEnemies[i]);
                if (c != null) return c;
            }
            return _enemies.Count > 0 ? _enemies[0] : null;
        }

        public void SelectNext()
        {
            LabStep step = CurrentStep;

            // 먼저 현재 단계의 관찰 대상들 사이를 돌고, 그 다음 전체를 돈다.
            List<AIContext> pool = new List<AIContext>();
            for (int i = 0; i < step.FocusEnemies.Length; i++)
            {
                AIContext c = Find(step.FocusEnemies[i]);
                if (c != null) pool.Add(c);
            }
            foreach (AIContext c in _enemies)
            {
                if (!pool.Contains(c)) pool.Add(c);
            }

            if (pool.Count == 0) return;

            _selectionCursor = (_selectionCursor + 1) % pool.Count;
            _selected = pool[_selectionCursor];
            ShowToast("선택: " + _selected.displayName);
        }

        /// <summary>가운데 큰 영역을 차지하는 페이지들. 한 번에 하나만 열린다.</summary>
        private const LabPanel PageMask = LabPanel.Diagram | LabPanel.Compare |
                                          LabPanel.Quiz | LabPanel.Blackboard;

        public void TogglePanel(LabPanel panel)
        {
            // 범위 표시는 다른 것과 상관없이 켜고 끈다.
            if (panel == LabPanel.Ranges)
            {
                if (IsOpen(panel)) _panels &= ~panel;
                else _panels |= panel;
                return;
            }

            // 나머지는 모두 같은 자리를 쓰는 페이지다.
            // 글자를 크게 보여주기 위해 동시에 띄우지 않는다.
            bool wasOpen = IsOpen(panel);
            _panels &= ~PageMask;

            if (!wasOpen && panel != LabPanel.Mission)
                _panels |= panel;

            if (panel == LabPanel.Diagram && IsOpen(panel)) _observed.Add("OPEN_DIAGRAM");
            if (panel == LabPanel.Compare && IsOpen(panel)) _observed.Add("OPEN_COMPARE");
            if (panel == LabPanel.Blackboard && IsOpen(panel)) _observed.Add("OPEN_BLACKBOARD");
            if (panel == LabPanel.Quiz && IsOpen(panel)) _observed.Add("OPEN_QUIZ");
        }

        public void DamageSelected(float amount)
        {
            if (_selected == null || _selected.Health == null) return;
            _selected.Health.TakeDamage(amount);
            _observed.Add("DAMAGE");
            ShowToast(_selected.displayName + " 에게 " + amount.ToString("0") + " 피해 (HP " +
                      Mathf.RoundToInt(_selected.HealthRatio * 100f) + "%)");
        }

        public void KillSelected()
        {
            if (_selected == null || _selected.Health == null) return;
            _selected.Health.TakeDamage(_selected.Health.MaxHealth * 2f);
            _observed.Add("DAMAGE");
            ShowToast(_selected.displayName + " 사망 처리");
        }

        public void HealSelected()
        {
            if (_selected == null || _selected.Health == null) return;
            _selected.Health.ResetHealth();
            ShowToast(_selected.displayName + " 체력 회복");
        }

        public void ResetExperiment()
        {
            foreach (AIContext c in _enemies)
            {
                if (c == null) continue;
                c.ResetAI();

                EnemyStateMachine sm = c.GetComponent<EnemyStateMachine>();
                if (sm != null) sm.ResetBrain();

                BTNodeTrace trace = c.GetComponent<BTNodeTrace>();
                if (trace != null) trace.ResetTrace();
            }

            if (SquadBlackboard.Instance != null)
                SquadBlackboard.Instance.ResetBoard();

            _prevLabel.Clear();
            ShowToast("실험 리셋 - 모든 Enemy 를 처음 상태로 되돌렸다");
        }

        public void ToggleAlarm()
        {
            if (SquadBlackboard.Instance == null) return;
            SquadBlackboard.Instance.ToggleAlarm();
            ShowToast(SquadBlackboard.Instance.AlarmActive
                ? "공유 Blackboard: AlarmActive = TRUE"
                : "공유 Blackboard: AlarmActive = FALSE");
        }

        public void ShowToast(string message)
        {
            Toast = message;
            _toastUntil = Time.time + 3.2f;
        }

        // 미션 진행률 ------------------------------------------------

        public int CompletedCount(LabStep step)
        {
            int n = 0;
            foreach (LabMission m in step.Missions)
                if (m.Done) n++;
            return n;
        }

        public int TotalCompleted()
        {
            int n = 0;
            foreach (LabStep s in _steps)
                n += CompletedCount(s);
            return n;
        }

        public int TotalMissions()
        {
            int n = 0;
            foreach (LabStep s in _steps)
                n += s.Missions.Count;
            return n;
        }

        // 미션 판정 도우미 --------------------------------------------

        private bool LabelSeen(string enemy, string label) => Has(enemy + ":" + label);
        private bool TransitionSeen(string enemy, string from, string to) => Has(enemy + ":" + from + ">" + to);

        private bool BothLabel(string label)
        {
            AIContext a = Find(LabNames.CompareState);
            AIContext b = Find(LabNames.CompareBt);
            if (a == null || b == null || a.Brain == null || b.Brain == null) return false;
            return a.Brain.CurrentLabel == label && b.Brain.CurrentLabel == label;
        }

        // 학습 단계 정의 ----------------------------------------------

        private void BuildSteps()
        {
            _steps.Clear();

            // ── STEP 01 ────────────────────────────────────────────
            LabStep s1 = new LabStep
            {
                Number = 1,
                Title = "AI 란 무엇인가?",
                Headline = "AI 는 지금 무엇을 판단하고 있을까?",
                Body = new[]
                {
                    "앞에 있는 Enemy 는 아주 단순한 AI 다.",
                    "이 AI 가 보는 것은 단 하나, <b>Player 와의 거리</b> 뿐이다.",
                    "",
                    "멀다        →  가만히 있는다   (IDLE)",
                    "보인다      →  Player 를 바라본다  (LOOK)",
                    "가깝다      →  쫓아온다   (CHASE)",
                    "아주 가깝다 →  공격한다   (ATTACK)",
                    "",
                    "A / D 로 직접 다가가면서 행동이 바뀌는 순간을 관찰하자.",
                    "V 키를 누르면 감지 범위가 화면에 표시된다."
                },
                Questions = new[]
                {
                    "AI 는 왜 가만히 있다가 Player 를 발견하면 움직일까?",
                    "AI 의 행동을 바꾸는 것은 무엇일까?",
                    "조건이 20개, 행동이 15개가 되면 이 if 문은 어떻게 될까?"
                },
                PlayerAnchorX = LabLayout.Anchor01,
                FocusEnemies = new[] { LabNames.Simple },
                AutoPanels = LabPanel.Mission | LabPanel.Ranges,
                HasDiagram = false,
                DiagramKind = AISystemKind.Reactive
            };
            s1.Missions.Add(new LabMission("멀리 떨어져서 IDLE 을 관찰한다.", d => d.LabelSeen(LabNames.Simple, "IDLE")));
            s1.Missions.Add(new LabMission("천천히 접근해서 LOOK 을 관찰한다.", d => d.LabelSeen(LabNames.Simple, "LOOK")));
            s1.Missions.Add(new LabMission("더 접근해서 CHASE 를 관찰한다.", d => d.LabelSeen(LabNames.Simple, "CHASE")));
            s1.Missions.Add(new LabMission("붙어서 ATTACK 을 관찰한다.", d => d.LabelSeen(LabNames.Simple, "ATTACK")));
            _steps.Add(s1);

            // ── STEP 02 ────────────────────────────────────────────
            LabStep s2 = new LabStep
            {
                Number = 2,
                Title = "State 란 무엇인가?",
                Headline = "STATE = 지금 AI 가 어떤 행동 모드에 있는가",
                Body = new[]
                {
                    "AI 의 행동을 이름 붙은 <b>모드</b>로 나눈 것이 State 다.",
                    "",
                    "PATROL   순찰 중",
                    "CHASE    추적 중",
                    "ATTACK   공격 중",
                    "FLEE     도망 중",
                    "DEAD     사망",
                    "",
                    "Enemy 머리 위에 지금의 STATE 가 표시된다.",
                    "",
                    "State 만큼 중요한 것이 <b>State Transition</b> 이다.",
                    "상태는 아무 때나 바뀌지 않는다.",
                    "반드시 <b>조건</b>이 있어야 바뀐다.",
                    "전환 규칙은 아래 다이어그램에 정리되어 있다.",
                    "",
                    "F 키로 Enemy 를 때려 HP 를 낮춰보자.",
                    "G 키는 즉시 사망이다."
                },
                Questions = new[]
                {
                    "State 와 행동은 어떻게 다를까?",
                    "상태를 바꾸는 것은 무엇일까?",
                    "DEAD 상태에서 나가는 화살표는 왜 없을까?"
                },
                PlayerAnchorX = LabLayout.Anchor02,
                FocusEnemies = new[] { LabNames.State },
                AutoPanels = LabPanel.Mission | LabPanel.Diagram,
                DiagramKind = AISystemKind.StatePattern
            };
            s2.Missions.Add(new LabMission("PATROL 상태를 관찰한다.", d => d.LabelSeen(LabNames.State, "PATROL")));
            s2.Missions.Add(new LabMission("PATROL → CHASE 전환을 발생시킨다.", d => d.TransitionSeen(LabNames.State, "PATROL", "CHASE")));
            s2.Missions.Add(new LabMission("CHASE → ATTACK 전환을 발생시킨다.", d => d.TransitionSeen(LabNames.State, "CHASE", "ATTACK")));
            s2.Missions.Add(new LabMission("F 키로 HP 를 낮춰 FLEE 를 확인한다.", d => d.LabelSeen(LabNames.State, "FLEE")));
            s2.Missions.Add(new LabMission("G 키로 DEAD 를 확인한다.", d => d.LabelSeen(LabNames.State, "DEAD")));
            _steps.Add(s2);

            // ── STEP 03 ────────────────────────────────────────────
            LabStep s3 = new LabStep
            {
                Number = 3,
                Title = "State Pattern 이란 무엇인가?",
                Headline = "상태마다 담당을 따로 두고, 현재 상태를 교체한다",
                Body = new[]
                {
                    "<b>State Pattern</b>",
                    "상태에 따라 행동이 달라질 때, 각 상태의 행동을",
                    "<b>따로 분리</b>하고 현재 상태를 <b>교체</b>하는 설계 방식이다.",
                    "",
                    "게임 AI 로 말하면 이렇다.",
                    "  PATROL 은 순찰만, CHASE 는 추적만 담당한다.",
                    "  자기 일만 하고, 조건이 맞으면 다음 상태에 넘긴다.",
                    "",
                    "<b>State Machine 은 상태가 아니다.</b>",
                    "지금 어떤 State 를 쓸지 <b>관리하는 역할</b>이다.",
                    "",
                    "각 상태에는 세 개의 시점이 있다.",
                    "  ENTER    상태에 처음 들어옴 (1회)",
                    "  EXECUTE  상태가 유지되는 동안 반복",
                    "  EXIT     상태를 빠져나감 (1회)",
                    "",
                    "오른쪽 AI MONITOR 아래 로그에서 이 흐름을 확인하자."
                },
                Questions = new[]
                {
                    "State Pattern 은 어떤 문제를 해결하려는 것일까?",
                    "Enter / Execute / Exit 는 각각 무엇을 위한 시점일까?",
                    "상태가 20개가 되면 전환 관계는 몇 개가 될 수 있을까?"
                },
                PlayerAnchorX = LabLayout.Anchor02,
                FocusEnemies = new[] { LabNames.State },
                AutoPanels = LabPanel.Mission | LabPanel.Diagram,
                DiagramKind = AISystemKind.StatePattern
            };
            s3.Missions.Add(new LabMission("E 키로 State Machine 구조를 확인한다.", d => d.Has("OPEN_DIAGRAM")));
            s3.Missions.Add(new LabMission("ENTER → EXECUTE → EXIT 로그를 확인한다.", d => d.TransitionSeen(LabNames.State, "PATROL", "CHASE")));
            s3.Missions.Add(new LabMission("ATTACK 에서 CHASE 로 돌아가게 만든다.", d => d.TransitionSeen(LabNames.State, "ATTACK", "CHASE")));
            s3.Missions.Add(new LabMission("CHASE 에서 PATROL 로 돌아가게 만든다.", d => d.TransitionSeen(LabNames.State, "CHASE", "PATROL")));
            s3.Missions.Add(new LabMission("FLEE 에서 회복해 PATROL 로 돌아가게 만든다.", d => d.TransitionSeen(LabNames.State, "FLEE", "PATROL")));
            _steps.Add(s3);

            // ── STEP 04 ────────────────────────────────────────────
            LabStep s4 = new LabStep
            {
                Number = 4,
                Title = "Behavior Tree 란 무엇인가?",
                Headline = "지금 어떤 행동을 선택해야 하는가?",
                Body = new[]
                {
                    "<b>Behavior Tree</b>",
                    "AI 의 행동을 <b>트리 모양의 노드</b>로 구성하고,",
                    "조건에 따라 어떤 행동을 실행할지 결정하는 방식이다.",
                    "",
                    "State Pattern 이 <b>나는 지금 어떤 상태인가</b>를 묻는다면,",
                    "Behavior Tree 는 <b>지금 어떤 행동을 고를까</b>를 묻는다.",
                    "",
                    "ROOT        트리의 시작점 (On Start)",
                    "SEQUENCE    여러 행동을 <b>순서대로</b> 실행",
                    "SELECTOR    조건에 맞는 것을 <b>위에서부터</b> 고름",
                    "CONDITION   판단만 한다. 참 / 거짓만 돌려준다",
                    "ACTION      실제로 행동한다. 이동 / 공격",
                    "REPEAT      하위 노드를 계속 반복한다",
                    "",
                    "왼쪽  Sequence 실험 : A → 대기 → B 를 Repeat 로 반복",
                    "        (이 AI 는 Player 를 아예 보지 않는다)",
                    "오른쪽 Selector 실험 : 사거리? / 발견? / 아니면 순찰",
                    "",
                    "오른쪽 Enemy 에게 걸어가면 Selector 의 선택이 바뀐다.",
                    "Tab 으로 AI 를 바꾸면 두 트리 구조를 비교할 수 있다."
                },
                Questions = new[]
                {
                    "Sequence 와 Selector 는 무엇이 다를까?",
                    "Condition 과 Action 의 차이는 무엇일까?",
                    "Selector 에서 노드의 순서가 왜 중요할까?"
                },
                PlayerAnchorX = LabLayout.Anchor04,
                FocusEnemies = new[] { LabNames.BtSequence, LabNames.BtSelector },
                AutoPanels = LabPanel.Mission | LabPanel.Diagram,
                DiagramKind = AISystemKind.BehaviorGraph
            };
            s4.Missions.Add(new LabMission("Sequence 실험 Enemy 의 MOVE 단계를 관찰한다.",
                d => d.Has(LabNames.BtSequence + ":MOVE Seq_PointA") || d.Has(LabNames.BtSequence + ":MOVE Seq_PointB")));
            s4.Missions.Add(new LabMission("Sequence 실험 Enemy 의 WAIT 단계를 관찰한다.",
                d => d.LabelSeen(LabNames.BtSequence, "WAIT")));
            s4.Missions.Add(new LabMission("Repeat 로 같은 순서가 다시 반복되는 것을 확인한다.",
                d => d.Has(LabNames.BtSequence + ":MOVE Seq_PointB>MOVE Seq_PointA")));
            s4.Missions.Add(new LabMission("Selector 실험 Enemy 가 PATROL 을 고르는 것을 확인한다.",
                d => d.LabelSeen(LabNames.BtSelector, "PATROL")));
            s4.Missions.Add(new LabMission("접근해서 Selector 의 선택이 CHASE 로 바뀌게 한다.",
                d => d.TransitionSeen(LabNames.BtSelector, "PATROL", "CHASE")));
            s4.Missions.Add(new LabMission("더 접근해서 Selector 가 ATTACK 을 고르게 한다.",
                d => d.LabelSeen(LabNames.BtSelector, "ATTACK")));
            _steps.Add(s4);

            // ── STEP 05 ────────────────────────────────────────────
            LabStep s5 = new LabStep
            {
                Number = 5,
                Title = "같은 AI 를 두 방식으로 구현",
                Headline = "같은 요구사항, 다른 구조",
                Body = new[]
                {
                    "여기 있는 두 Enemy 는 <b>완전히 같은 요구사항</b>으로 만들어졌다.",
                    "",
                    "  평소             →  Patrol",
                    "  Player 발견      →  Chase",
                    "  공격 사거리 진입 →  Attack",
                    "  HP 30% 이하      →  Flee",
                    "  HP 0             →  Dead",
                    "",
                    "왼쪽  A : <b>State Pattern</b> 으로 구현",
                    "오른쪽 B : <b>Behavior Graph</b> 로 구현",
                    "",
                    "감지 거리 / 이동 속도 / 공격력이 모두 같고,",
                    "조건을 판정하는 코드(AIContext)도 <b>똑같은 것</b>을 쓴다.",
                    "다른 것은 오직 <b>판단 구조</b>뿐이다.",
                    "",
                    "가운데에 서면 두 Enemy 가 동시에 반응한다.",
                    "E 키로 번갈아 보고, C 키로 나란히 비교하자."
                },
                Questions = new[]
                {
                    "두 Enemy 의 행동에 차이가 있는가?",
                    "그렇다면 무엇이 다른가?",
                    "같은 결과를 만드는 방법이 여러 개라는 것은 무엇을 의미할까?"
                },
                PlayerAnchorX = LabLayout.Anchor05,
                FocusEnemies = new[] { LabNames.CompareState, LabNames.CompareBt },
                AutoPanels = LabPanel.Mission | LabPanel.Diagram,
                DiagramKind = AISystemKind.StatePattern
            };
            s5.Missions.Add(new LabMission("두 Enemy 가 동시에 CHASE 하게 만든다.", d => d.BothLabel("CHASE")));
            s5.Missions.Add(new LabMission("두 Enemy 가 동시에 ATTACK 하게 만든다.", d => d.BothLabel("ATTACK")));
            s5.Missions.Add(new LabMission("Tab 으로 AI 를 바꿔가며 두 구조를 확인한다.",
                d => d.Has(LabNames.CompareState + ":CHASE") && d.Has(LabNames.CompareBt + ":CHASE")));
            s5.Missions.Add(new LabMission("두 Enemy 모두 FLEE 로 만든다.",
                d => d.LabelSeen(LabNames.CompareState, "FLEE") && d.LabelSeen(LabNames.CompareBt, "FLEE")));
            s5.Missions.Add(new LabMission("두 Enemy 모두 DEAD 로 만든다.",
                d => d.LabelSeen(LabNames.CompareState, "DEAD") && d.LabelSeen(LabNames.CompareBt, "DEAD")));
            _steps.Add(s5);

            // ── STEP 06 ────────────────────────────────────────────
            LabStep s6 = new LabStep
            {
                Number = 6,
                Title = "비교하고 직접 실험",
                Headline = "어떤 상황에서 어떤 방식을 선택하는가?",
                Body = new[]
                {
                    "<b>C 키</b> : 두 구조를 나란히 놓고 비교한다.",
                    "",
                    "STATE PATTERN 의 사고방식",
                    "  \"나는 지금 어떤 상태인가?\"",
                    "  현재 상태를 중심으로 행동을 관리한다.",
                    "",
                    "BEHAVIOR TREE 의 사고방식",
                    "  \"지금 어떤 행동을 선택해야 하는가?\"",
                    "  조건과 우선순위를 평가해 행동을 결정한다.",
                    "",
                    "<b>B 키</b> : Blackboard 를 본다.",
                    "  노드들이 함께 보는 데이터 칠판이다.",
                    "  오른쪽 3인조는 <b>같은 칠판</b>을 본다.",
                    "",
                    "<b>Z 키</b> : 경보(AlarmActive)를 켜고 끈다.",
                    "  한 마리만 자극해도 세 마리가 반응하는 이유를 보자.",
                    "",
                    "<b>Q 키</b> : 자기진단.    <b>R 키</b> : 처음부터 다시."
                },
                Questions = new[]
                {
                    "Player 가 Enemy 하나만 자극했는데 왜 다른 Enemy 들도 반응했을까?",
                    "공유 데이터는 AI 시스템에서 어떤 역할을 할까?",
                    "간단한 상태 중심 AI 와 복잡한 행동 조합 AI 중 각각 어떤 구조가 더 적합할까?"
                },
                PlayerAnchorX = LabLayout.Anchor06,
                FocusEnemies = new[] { LabNames.SquadA, LabNames.SquadB, LabNames.SquadC, LabNames.CompareState, LabNames.CompareBt },
                AutoPanels = LabPanel.Compare,
                DiagramKind = AISystemKind.BehaviorGraph
            };
            s6.Missions.Add(new LabMission("C 키로 두 구조를 나란히 비교한다.", d => d.Has("OPEN_COMPARE")));
            s6.Missions.Add(new LabMission("B 키로 Blackboard 값을 관찰한다.", d => d.Has("OPEN_BLACKBOARD")));
            s6.Missions.Add(new LabMission("3인조 중 한 마리에게 접근해 경보를 발생시킨다.", d => d.Has("ALARM")));
            s6.Missions.Add(new LabMission("경보로 두 마리 이상이 동시에 반응하는 것을 확인한다.", d => d.Has("ALARM_GROUP_REACT")));
            s6.Missions.Add(new LabMission("세 마리가 모두 동시에 반응하게 만든다.", d => d.Has("ALARM_ALL_REACT")));
            s6.Missions.Add(new LabMission("Q 키로 자기진단 질문을 확인한다.", d => d.Has("OPEN_QUIZ")));
            _steps.Add(s6);
        }
    }
}
