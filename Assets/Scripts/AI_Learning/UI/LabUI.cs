using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AILearning
{
    /// <summary>
    /// 학습용 화면 전체를 실행 시점에 만들고 갱신한다.
    ///
    /// 화면 배치 원칙
    ///   1) 화면 위쪽은 Game View 전용으로 비워둔다. 어떤 패널도 AI 를 가리지 않는다.
    ///   2) 가운데 큰 영역에는 한 번에 한 가지만 크게 보여준다.
    ///      설명 / 구조 다이어그램 / 비교 / Blackboard / 자기진단 이 같은 자리를 번갈아 쓴다.
    ///      (여러 개를 동시에 띄우면 각 영역이 좁아져 글자를 읽을 수 없다.)
    ///   3) 오른쪽 AI MONITOR 는 항상 보인다. 지금 무엇이 실행 중인지는 늘 필요하다.
    ///
    ///   +--------------- 상단바 : STEP / 진행률 ------------------+
    ///   |                                                        |
    ///   |              Game View (가려지지 않는 영역)             |
    ///   |                                                        |
    ///   +--------------------------------------+-----------------+
    ///   |  설명 + 질문 + 미션           (기본) |                 |
    ///   |  또는 구조 다이어그램         (D)    |   AI MONITOR    |
    ///   |  또는 두 구조 비교            (C)    |                 |
    ///   |  또는 Blackboard              (B)    |                 |
    ///   |  또는 자기진단                (Q)    |                 |
    ///   +--------------------------------------+-----------------+
    ///   +--------------- 하단바 : 조작 안내 ----------------------+
    /// </summary>
    public class LabUI : MonoBehaviour
    {
        public LabDirector director;

        private Canvas _canvas;
        private RectTransform _hudRoot;

        private Text _stepLabel;
        private Text _stepTitle;
        private Text _progress;

        // 가운데 큰 영역의 페이지들
        private RectTransform _teachPage;
        private Text _headline;
        private Text _body;
        private Text _questions;
        private Text _missionHeader;
        private Text _missionBody;

        private RectTransform _diagramPage;
        private RectTransform _comparePage;
        private RectTransform _quizPage;
        private RectTransform _blackboardPage;
        private Text _blackboardBody;
        private Text _pageHint;

        // 오른쪽 모니터
        private Text _monitorName;
        private Text _monitorSystem;
        private Image _hpFill;
        private Text _hpText;
        private Text _monitorBody;
        private Text _logText;

        private Text _toast;

        private StateDiagramWidget _stateDiagram;
        private ReactiveDiagramWidget _reactiveDiagram;
        private TreeDiagramWidget _treeFull;
        private TreeDiagramWidget _treeSelector;
        private TreeDiagramWidget _treeSequence;

        private Text _compareStateLabel;
        private Text _compareBtLabel;

        private readonly List<EnemyWorldLabel> _worldLabels = new List<EnemyWorldLabel>();
        private readonly List<WorldSignLabel> _signLabels = new List<WorldSignLabel>();
        private RectTransform _worldLabelHost;
        private bool _worldLabelsBuilt;

        private string _lastFrom = "-";
        private string _lastTo = "-";

        // 화면 배치 상수 (1920 x 1080 기준)
        private const float BandBottom = 96f;
        private const float BandTop = 664f;
        private const float BandHeight = BandTop - BandBottom;        // 568
        private const float MonitorWidth = 520f;
        private const float MainWidth = 1352f;                        // x 16 ~ 1368
        private const float PageInset = 26f;
        private const float PageWidth = MainWidth - PageInset * 2f;   // 1300
        private const float PageHeight = BandHeight - 44f;            // 524

        private void Awake()
        {
            if (director == null)
                director = FindFirstObjectByType<LabDirector>();

            BuildCanvas();

            _hudRoot = LabUIKit.NewRect(_canvas.transform, "HudRoot");
            LabUIKit.Stretch(_hudRoot, 0f, 0f, 0f, 0f);

            BuildTopBar();
            BuildMainArea();
            BuildMonitorPanel();
            BuildBottomBar();

            _worldLabelHost = LabUIKit.NewRect(_canvas.transform, "WorldLabels");
            LabUIKit.Stretch(_worldLabelHost, 0f, 0f, 0f, 0f);
        }

        private void Start()
        {
            TryBuildWorldLabels();
        }

        /// <summary>
        /// LabDirector 가 Enemy 목록을 채우는 시점(Start)과 이 스크립트의 Start 순서는
        /// Unity 가 보장하지 않는다. 그래서 목록이 준비된 뒤에 한 번만 만든다.
        /// </summary>
        private void TryBuildWorldLabels()
        {
            if (_worldLabelsBuilt || director == null || director.Enemies.Count == 0)
                return;

            foreach (AIContext ctx in director.Enemies)
            {
                if (ctx == null) continue;
                _worldLabels.Add(new EnemyWorldLabel(_worldLabelHost, ctx));

                EnemyStateMachine sm = ctx.GetComponent<EnemyStateMachine>();
                if (sm != null)
                    sm.OnStateChanged += OnAnyStateChanged;
            }

            LabZoneSign[] signs = FindObjectsByType<LabZoneSign>(FindObjectsSortMode.None);
            foreach (LabZoneSign sign in signs)
                _signLabels.Add(new WorldSignLabel(_worldLabelHost, sign));

            _worldLabelsBuilt = true;
        }

        private void OnAnyStateChanged(string from, string to, string reason)
        {
            _lastFrom = from;
            _lastTo = to;
        }

        // 화면 구성 ---------------------------------------------------

        private void BuildCanvas()
        {
            GameObject go = new GameObject("LabCanvas");
            go.transform.SetParent(transform, false);

            _canvas = go.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 200;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f; // 높이 기준. 화면이 넓어지면 좌우가 넓어진다.
        }

        private void BuildTopBar()
        {
            RectTransform bar = LabUIKit.Panel(_hudRoot, "TopBar", LabUIKit.PanelBgSolid);
            LabUIKit.StretchTop(bar, 16f, 12f, 60f);

            Text title = LabUIKit.Label(bar, "Title", "AI PROGRAMMING LAB", 24, LabUIKit.Accent,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            LabUIKit.TopLeft(title.rectTransform, 22f, -16f, 340f, 30f);

            _stepLabel = LabUIKit.Label(bar, "StepLabel", "STEP 01 / 06", 21, LabUIKit.AccentWarm,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_stepLabel.rectTransform, 380f, -16f, 180f, 30f);

            _stepTitle = LabUIKit.Label(bar, "StepTitle", "", 22, LabUIKit.TextMain,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_stepTitle.rectTransform, 560f, -16f, 620f, 30f);

            _progress = LabUIKit.Label(bar, "Progress", "", 18, LabUIKit.TextDim, TextAnchor.MiddleRight);
            RectTransform pr = _progress.rectTransform;
            pr.anchorMin = new Vector2(1f, 0.5f);
            pr.anchorMax = new Vector2(1f, 0.5f);
            pr.pivot = new Vector2(1f, 0.5f);
            pr.anchoredPosition = new Vector2(-22f, 0f);
            pr.sizeDelta = new Vector2(600f, 30f);
        }

        /// <summary>가운데 큰 영역. 페이지들이 이 자리를 번갈아 쓴다.</summary>
        private void BuildMainArea()
        {
            RectTransform main = LabUIKit.Panel(_hudRoot, "MainArea", LabUIKit.PanelBg);
            LabUIKit.BottomLeft(main, 16f, BandBottom, MainWidth, BandHeight);

            _pageHint = LabUIKit.Label(main, "PageHint", "", 15, LabUIKit.TextDim, TextAnchor.UpperRight);
            LabUIKit.TopLeft(_pageHint.rectTransform, PageInset, -12f, PageWidth, 22f);

            BuildTeachPage(main);
            BuildDiagramPage(main);
            BuildComparePage(main);
            BuildBlackboardPage(main);
            BuildQuizPage(main);
        }

        private RectTransform NewPage(Transform parent, string name)
        {
            RectTransform rt = LabUIKit.NewRect(parent, name);
            LabUIKit.TopLeft(rt, PageInset, -34f, PageWidth, PageHeight);
            return rt;
        }

        private void BuildTeachPage(Transform parent)
        {
            _teachPage = NewPage(parent, "TeachPage");

            const float leftW = 690f;
            const float rightX = 730f;
            const float rightW = PageWidth - rightX;

            _headline = LabUIKit.Label(_teachPage, "Headline", "", 21, LabUIKit.AccentWarm,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_headline.rectTransform, 0f, 0f, PageWidth, 34f);

            _body = LabUIKit.Label(_teachPage, "Body", "", 16, LabUIKit.TextMain);
            LabUIKit.TopLeft(_body.rectTransform, 0f, -44f, leftW, 470f);

            Text qHeader = LabUIKit.Label(_teachPage, "QHeader", "생각해 보기", 18, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(qHeader.rectTransform, rightX, -44f, rightW, 24f);

            _questions = LabUIKit.Label(_teachPage, "Questions", "", 16, LabUIKit.TextDim);
            LabUIKit.TopLeft(_questions.rectTransform, rightX, -78f, rightW, 150f);

            _missionHeader = LabUIKit.Label(_teachPage, "MissionHeader", "MISSION", 18, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_missionHeader.rectTransform, rightX, -244f, rightW, 24f);

            _missionBody = LabUIKit.Label(_teachPage, "MissionBody", "", 16, LabUIKit.TextMain);
            LabUIKit.TopLeft(_missionBody.rectTransform, rightX, -278f, rightW, 236f);
        }

        private void BuildDiagramPage(Transform parent)
        {
            _diagramPage = NewPage(parent, "DiagramPage");

            _stateDiagram = new StateDiagramWidget(_diagramPage, PageWidth, PageHeight);
            _reactiveDiagram = new ReactiveDiagramWidget(_diagramPage, PageWidth, PageHeight);
            _treeFull = new TreeDiagramWidget(_diagramPage, PageWidth, PageHeight,
                LabDiagramSpecs.Full(), "BEHAVIOR GRAPH  -  구조");
            _treeSelector = new TreeDiagramWidget(_diagramPage, PageWidth, PageHeight,
                LabDiagramSpecs.SelectorDemo(), "BEHAVIOR GRAPH  -  SELECTOR 실험");
            _treeSequence = new TreeDiagramWidget(_diagramPage, PageWidth, PageHeight,
                LabDiagramSpecs.SequenceDemo(), "BEHAVIOR GRAPH  -  SEQUENCE + REPEAT 실험");
        }

        private void BuildComparePage(Transform parent)
        {
            _comparePage = NewPage(parent, "ComparePage");

            Text head = LabUIKit.Label(_comparePage, "Head",
                "같은 요구사항을 두 방식으로 만들면 구조가 어떻게 달라지는가", 23, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 0f, 0f, PageWidth, 32f);

            float colW = (PageWidth - 30f) * 0.5f;

            RectTransform left = LabUIKit.Panel(_comparePage, "Left", new Color(0.10f, 0.13f, 0.19f, 1f));
            LabUIKit.TopLeft(left, 0f, -40f, colW, 268f);

            Text lh = LabUIKit.Label(left, "H", "STATE PATTERN", 20, LabUIKit.LabelColor("PATROL"),
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(lh.rectTransform, 20f, -14f, colW - 40f, 26f);

            _compareStateLabel = LabUIKit.Label(left, "Live", "", 18, LabUIKit.TextDim, TextAnchor.UpperRight);
            LabUIKit.TopLeft(_compareStateLabel.rectTransform, 20f, -14f, colW - 40f, 26f);

            Text lb = LabUIKit.Label(left, "Body", string.Join("\n", LabContent.StatePatternMindset), 16,
                LabUIKit.TextMain);
            LabUIKit.TopLeft(lb.rectTransform, 20f, -48f, colW - 40f, 210f);

            RectTransform right = LabUIKit.Panel(_comparePage, "Right", new Color(0.10f, 0.13f, 0.19f, 1f));
            LabUIKit.TopLeft(right, colW + 30f, -40f, colW, 268f);

            Text rh = LabUIKit.Label(right, "H", "BEHAVIOR TREE", 20, LabUIKit.LabelColor("CHASE"),
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(rh.rectTransform, 20f, -14f, colW - 40f, 26f);

            _compareBtLabel = LabUIKit.Label(right, "Live", "", 18, LabUIKit.TextDim, TextAnchor.UpperRight);
            LabUIKit.TopLeft(_compareBtLabel.rectTransform, 20f, -14f, colW - 40f, 26f);

            Text rb = LabUIKit.Label(right, "Body", string.Join("\n", LabContent.BehaviorTreeMindset), 16,
                LabUIKit.TextMain);
            LabUIKit.TopLeft(rb.rectTransform, 20f, -48f, colW - 40f, 210f);

            Text lp = LabUIKit.Label(_comparePage, "LeftPros", string.Join("\n", LabContent.StatePatternPros), 15,
                LabUIKit.TextDim);
            LabUIKit.TopLeft(lp.rectTransform, 10f, -320f, colW - 20f, 200f);

            Text rp = LabUIKit.Label(_comparePage, "RightPros", string.Join("\n", LabContent.BehaviorTreePros), 15,
                LabUIKit.TextDim);
            LabUIKit.TopLeft(rp.rectTransform, colW + 40f, -320f, colW - 20f, 200f);
        }

        private void BuildBlackboardPage(Transform parent)
        {
            _blackboardPage = NewPage(parent, "BlackboardPage");

            Text head = LabUIKit.Label(_blackboardPage, "Head",
                "BLACKBOARD  -  노드들이 함께 보는 데이터 칠판", 23, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 0f, 0f, PageWidth, 32f);

            _blackboardBody = LabUIKit.Label(_blackboardPage, "Body", "", 17, LabUIKit.TextMain);
            LabUIKit.TopLeft(_blackboardBody.rectTransform, 10f, -44f, 700f, 400f);

            string[] note =
            {
                "<b>Blackboard 는 왜 필요한가</b>",
                "",
                "노드는 서로를 직접 알지 못한다.",
                "칠판에 적힌 값을 읽고 쓸 뿐이다.",
                "",
                "그래서 그래프를 고치지 않고",
                "칠판 값만 바꿔도 행동이 달라진다.",
                "",
                "AI 하나하나가 각자 정보를 들고 있으면",
                "서로 협력할 수 없다.",
                "정보를 한 곳에 적어두면 모든 AI 가",
                "같은 정보를 보고 함께 반응할 수 있다.",
                "",
                "<b>Z 키</b> 로 경보를 직접 켜고 끄면서",
                "3인조가 함께 반응하는 것을 확인하자."
            };

            RectTransform notePanel = LabUIKit.Panel(_blackboardPage, "Note",
                new Color(0.10f, 0.13f, 0.19f, 1f));
            LabUIKit.TopLeft(notePanel, 730f, -44f, PageWidth - 730f, 420f);

            Text noteText = LabUIKit.Label(notePanel, "T", string.Join("\n", note), 16, LabUIKit.TextDim);
            LabUIKit.Stretch(noteText.rectTransform, 20f, 14f, 20f, 14f);
        }

        private void BuildQuizPage(Transform parent)
        {
            _quizPage = NewPage(parent, "QuizPage");

            Text head = LabUIKit.Label(_quizPage, "Head",
                "SELF CHECK  -  스스로 설명할 수 있는지 확인하기", 23,
                LabUIKit.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 0f, 0f, PageWidth, 32f);

            Text body = LabUIKit.Label(_quizPage, "Body", string.Join("\n", LabContent.SelfCheck), 18,
                LabUIKit.TextMain);
            LabUIKit.TopLeft(body.rectTransform, 10f, -44f, 760f, 420f);

            RectTransform hintPanel = LabUIKit.Panel(_quizPage, "HintPanel", new Color(0.10f, 0.13f, 0.19f, 1f));
            LabUIKit.TopLeft(hintPanel, 790f, -44f, PageWidth - 790f, 190f);

            Text hint = LabUIKit.Label(hintPanel, "Hint", string.Join("\n", LabContent.SelfCheckHint), 15,
                LabUIKit.TextDim);
            LabUIKit.Stretch(hint.rectTransform, 18f, 12f, 18f, 12f);

            Text when = LabUIKit.Label(_quizPage, "When", string.Join("\n", LabContent.WhenToUse), 15,
                LabUIKit.TextDim);
            LabUIKit.TopLeft(when.rectTransform, 790f, -250f, PageWidth - 790f, 250f);
        }

        private void BuildMonitorPanel()
        {
            RectTransform panel = LabUIKit.Panel(_hudRoot, "MonitorPanel", LabUIKit.PanelBg);
            LabUIKit.BottomRight(panel, 16f, BandBottom, MonitorWidth, BandHeight);

            float w = MonitorWidth - 40f;

            Text head = LabUIKit.Label(panel, "Head", "AI MONITOR", 20, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 20f, -12f, 260f, 26f);

            Text hint = LabUIKit.Label(panel, "Hint", "Tab : 다른 AI", 14, LabUIKit.TextDim,
                TextAnchor.UpperRight);
            LabUIKit.TopLeft(hint.rectTransform, 20f, -12f, w, 26f);

            _monitorName = LabUIKit.Label(panel, "Name", "", 19, LabUIKit.TextMain,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_monitorName.rectTransform, 20f, -46f, w, 26f);

            _monitorSystem = LabUIKit.Label(panel, "System", "", 16, LabUIKit.Accent);
            LabUIKit.TopLeft(_monitorSystem.rectTransform, 20f, -76f, w, 22f);

            _hpFill = LabUIKit.Bar(panel, "HpBar", 20f, -106f, w, 16f,
                new Color(0.16f, 0.18f, 0.24f, 1f), LabUIKit.Good);

            _hpText = LabUIKit.Label(panel, "HpText", "", 14, LabUIKit.TextDim);
            LabUIKit.TopLeft(_hpText.rectTransform, 20f, -128f, w, 20f);

            _monitorBody = LabUIKit.Label(panel, "Body", "", 16, LabUIKit.TextMain);
            LabUIKit.TopLeft(_monitorBody.rectTransform, 20f, -158f, w, 250f);

            Text logHead = LabUIKit.Label(panel, "LogHead", "ENTER / EXECUTE / EXIT 로그", 16,
                LabUIKit.AccentWarm, TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(logHead.rectTransform, 20f, -454f, w, 22f);

            _logText = LabUIKit.Label(panel, "Log", "", 15, LabUIKit.TextDim);
            LabUIKit.TopLeft(_logText.rectTransform, 20f, -482f, w, 100f);
        }

        private void BuildBottomBar()
        {
            RectTransform bar = LabUIKit.Panel(_hudRoot, "BottomBar", LabUIKit.PanelBgSolid);
            LabUIKit.StretchBottom(bar, 16f, 12f, 78f);

            _toast = LabUIKit.Label(bar, "Toast", "", 18, LabUIKit.AccentWarm,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_toast.rectTransform, 22f, -6f, 1500f, 26f);

            Text c1 = LabUIKit.Label(bar, "C1", LabContent.ControlsLine1, 15, LabUIKit.TextMain);
            LabUIKit.TopLeft(c1.rectTransform, 22f, -32f, 1856f, 22f);

            Text c2 = LabUIKit.Label(bar, "C2", LabContent.ControlsLine2, 15, LabUIKit.TextDim);
            LabUIKit.TopLeft(c2.rectTransform, 22f, -56f, 1856f, 22f);
        }

        // 갱신 --------------------------------------------------------

        private void LateUpdate()
        {
            if (director == null)
                return;

            bool show = !director.UiHidden;
            _hudRoot.gameObject.SetActive(show);

            if (show)
            {
                bool diagram = director.IsOpen(LabPanel.Diagram);
                bool compare = director.IsOpen(LabPanel.Compare);
                bool quiz = director.IsOpen(LabPanel.Quiz);
                bool blackboard = director.IsOpen(LabPanel.Blackboard);
                bool teach = !diagram && !compare && !quiz && !blackboard;

                _teachPage.gameObject.SetActive(teach);
                _diagramPage.gameObject.SetActive(diagram);
                _comparePage.gameObject.SetActive(compare);
                _quizPage.gameObject.SetActive(quiz);
                _blackboardPage.gameObject.SetActive(blackboard);

                UpdateTopBar();
                if (teach) UpdateTeachPage();
                if (diagram) UpdateDiagramPage();
                if (compare) UpdateComparePage();
                if (blackboard) UpdateBlackboardPage();
                if (quiz) _pageHint.text = "Q 또는 M : 설명으로 돌아가기";

                UpdateMonitor();
                _toast.text = director.Toast;
            }

            UpdateWorldLabels(show);
        }

        private void UpdateTopBar()
        {
            LabStep step = director.CurrentStep;
            _stepLabel.text = "STEP " + step.Number.ToString("00") + " / " + director.StepCount.ToString("00");
            _stepTitle.text = step.Title;

            Health playerHealth = director.playerRoot != null
                ? director.playerRoot.GetComponent<Health>()
                : null;

            string hp = playerHealth != null
                ? "     PLAYER HP " + Mathf.RoundToInt(playerHealth.HealthRatio * 100f) + " %"
                : string.Empty;

            _progress.text = "미션 " + director.CompletedCount(step) + " / " + step.Missions.Count +
                             "   (전체 " + director.TotalCompleted() + " / " + director.TotalMissions() + ")" + hp;
        }

        private void UpdateTeachPage()
        {
            LabStep step = director.CurrentStep;
            _pageHint.text = "D : 구조 보기      C : 비교      B : Blackboard      Q : 자기진단";

            _headline.text = step.Headline;
            _body.text = string.Join("\n", step.Body);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < step.Questions.Length; i++)
                sb.Append("Q").Append(i + 1).Append(".  ").Append(step.Questions[i]).Append('\n');
            _questions.text = sb.ToString();

            _missionHeader.text = "MISSION   (" + director.CompletedCount(step) + " / " +
                                  step.Missions.Count + ")";

            StringBuilder ms = new StringBuilder();
            foreach (LabMission m in step.Missions)
            {
                if (m.Done)
                    ms.Append("<color=#66D98A>[V]  ").Append(m.Text).Append("</color>\n");
                else
                    ms.Append("<color=#8A93A3>[  ]  ").Append(m.Text).Append("</color>\n");
            }
            _missionBody.text = ms.ToString();
        }

        private void UpdateDiagramPage()
        {
            _pageHint.text = "D 또는 M : 설명으로 돌아가기      Tab : 다른 AI 의 구조 보기";

            AIContext ctx = director.Selected;
            LabStructureKind kind = LabStructureKind.StateMachine;
            LabAIStructure structure = ctx != null ? ctx.GetComponent<LabAIStructure>() : null;
            if (structure != null) kind = structure.kind;

            _stateDiagram.Root.gameObject.SetActive(kind == LabStructureKind.StateMachine);
            _reactiveDiagram.Root.gameObject.SetActive(kind == LabStructureKind.Reactive);
            _treeFull.Root.gameObject.SetActive(kind == LabStructureKind.BtFull);
            _treeSelector.Root.gameObject.SetActive(kind == LabStructureKind.BtSelectorDemo);
            _treeSequence.Root.gameObject.SetActive(kind == LabStructureKind.BtSequenceDemo);

            switch (kind)
            {
                case LabStructureKind.StateMachine:
                    _stateDiagram.Refresh(ctx, _lastFrom, _lastTo);
                    break;
                case LabStructureKind.Reactive:
                    _reactiveDiagram.Refresh(ctx);
                    break;
                case LabStructureKind.BtFull:
                    _treeFull.Refresh(ctx);
                    break;
                case LabStructureKind.BtSelectorDemo:
                    _treeSelector.Refresh(ctx);
                    break;
                case LabStructureKind.BtSequenceDemo:
                    _treeSequence.Refresh(ctx);
                    break;
            }
        }

        private void UpdateComparePage()
        {
            _pageHint.text = "C 또는 M : 설명으로 돌아가기";

            AIContext a = director.Find(LabNames.CompareState);
            AIContext b = director.Find(LabNames.CompareBt);
            _compareStateLabel.text = a != null && a.Brain != null ? "지금 : " + a.Brain.CurrentLabel : "-";
            _compareBtLabel.text = b != null && b.Brain != null ? "지금 : " + b.Brain.CurrentLabel : "-";
        }

        private void UpdateBlackboardPage()
        {
            _pageHint.text = "B 또는 M : 설명으로 돌아가기      Z : 경보 켜기 / 끄기      Tab : 다른 AI";

            StringBuilder sb = new StringBuilder();

            SquadBlackboard board = SquadBlackboard.Instance;
            sb.Append("<b>공유 Blackboard   (STEP 06 의 3인조가 함께 본다)</b>\n\n");
            if (board != null)
            {
                sb.Append(BoolLine("AlarmActive", board.AlarmActive));
                sb.Append("  ").Append("DetectedCount".PadRight(20))
                  .Append(board.DetectedCount).Append("\n");
                sb.Append("  ").Append("LastPlayerPos".PadRight(20))
                  .Append(board.LastKnownPlayerPosition.x.ToString("0.0")).Append(" , ")
                  .Append(board.LastKnownPlayerPosition.y.ToString("0.0")).Append('\n');
            }
            else
            {
                sb.Append("  (씬에 공유 Blackboard 가 없다)\n");
            }

            AIContext ctx = director.Selected;
            BTBlackboardSync sync = ctx != null ? ctx.GetComponent<BTBlackboardSync>() : null;

            sb.Append('\n');
            if (sync != null)
            {
                sb.Append("<b>").Append(ctx.displayName)
                  .Append(" 의 Behavior Graph Blackboard</b>\n\n");
                foreach (KeyValuePair<string, string> kv in sync.Snapshot)
                    sb.Append("  ").Append(kv.Key.PadRight(20)).Append(kv.Value).Append('\n');

                sb.Append("\n  이 값들은 Behavior 그래프 창의 Blackboard 에서도\n");
                sb.Append("  똑같이 실시간으로 확인할 수 있다.\n");
            }
            else if (ctx != null)
            {
                sb.Append("<b>").Append(ctx.displayName).Append("</b>\n\n");
                sb.Append("  이 AI 는 Behavior Graph 가 아니어서 Blackboard 가 없다.\n");
                sb.Append("  State Pattern 은 필요한 값을 상태 객체가 직접 들고 있다.\n");
                sb.Append("\n  Tab 을 눌러 Behavior Graph AI 를 선택해 보자.\n");
            }

            _blackboardBody.text = sb.ToString();
        }

        private void UpdateMonitor()
        {
            AIContext ctx = director.Selected;
            if (ctx == null || ctx.Brain == null)
            {
                _monitorName.text = "선택된 AI 가 없다";
                _monitorSystem.text = string.Empty;
                _monitorBody.text = string.Empty;
                _logText.text = string.Empty;
                return;
            }

            IAIBrain brain = ctx.Brain;
            Color c = LabUIKit.LabelColor(brain.CurrentLabel);

            _monitorName.text = ctx.displayName;
            _monitorSystem.text = brain.SystemName;

            _hpFill.fillAmount = ctx.HealthRatio;
            _hpFill.color = ctx.HealthRatio <= ctx.lowHealthRatio ? LabUIKit.Bad : LabUIKit.Good;
            _hpText.text = "HP  " + Mathf.RoundToInt(ctx.HealthRatio * 100f) + " %      " +
                           "( " + Mathf.RoundToInt(ctx.lowHealthRatio * 100f) + " % 이하면 FLEE )";

            string labelWord = brain.Kind == AISystemKind.BehaviorGraph ? "Current Node" : "Current State";

            StringBuilder sb = new StringBuilder();
            sb.Append("<color=#8A93A3>").Append(labelWord).Append("</color>\n");
            sb.Append("<size=30><b><color=#")
              .Append(ColorUtility.ToHtmlStringRGB(c)).Append(">")
              .Append(brain.CurrentLabel).Append("</color></b></size>\n");
            sb.Append("<color=#8A93A3>").Append(brain.TimeInCurrent.ToString("0.0"))
              .Append("초 유지     직전 </color>").Append(brain.PreviousLabel).Append('\n');
            sb.Append("<color=#8A93A3>이유</color> ").Append(DescribeReason(ctx)).Append('\n');

            sb.Append("\nCONDITION\n");
            sb.Append(BoolLine("Player Detected", ctx.PlayerDetected));
            sb.Append(BoolLine("Attack Range", ctx.InAttackRange));
            sb.Append(BoolLine("Low HP", ctx.IsLowHealth));
            sb.Append(BoolLine("Is Dead", ctx.IsDead));
            sb.Append("  <color=#8A93A3>거리</color> ")
              .Append(ctx.DistanceToPlayer >= 999f ? "-" : ctx.DistanceToPlayer.ToString("0.0"))
              .Append("   <color=#8A93A3>감지</color> ").Append(ctx.detectRange.ToString("0.0"))
              .Append("   <color=#8A93A3>공격</color> ").Append(ctx.attackRange.ToString("0.0"));

            _monitorBody.text = sb.ToString();

            StringBuilder log = new StringBuilder();
            IReadOnlyList<string> lines = brain.RecentEvents;
            int start = Mathf.Max(0, lines.Count - 5);
            for (int i = start; i < lines.Count; i++)
                log.Append(lines[i]).Append('\n');
            _logText.text = log.ToString();
        }

        private static string BoolLine(string name, bool value)
        {
            string color = value ? "#66D98A" : "#5A6270";
            return "  " + name.PadRight(20) + "<color=" + color + ">" +
                   (value ? "TRUE" : "FALSE") + "</color>\n";
        }

        private static string DescribeReason(AIContext ctx)
        {
            EnemyStateMachine sm = ctx.GetComponent<EnemyStateMachine>();
            if (sm != null) return sm.LastTransitionReason;

            BTNodeTrace trace = ctx.GetComponent<BTNodeTrace>();
            if (trace != null) return trace.SelectedBecause;

            SimpleReactiveAI simple = ctx.GetComponent<SimpleReactiveAI>();
            if (simple != null) return simple.SelectedBecause;

            return "-";
        }

        private void UpdateWorldLabels(bool show)
        {
            TryBuildWorldLabels();

            Camera cam = Camera.main;
            for (int i = 0; i < _worldLabels.Count; i++)
                _worldLabels[i].Refresh(cam, show, director.Selected);

            for (int i = 0; i < _signLabels.Count; i++)
                _signLabels[i].Refresh(cam, show);
        }
    }
}
