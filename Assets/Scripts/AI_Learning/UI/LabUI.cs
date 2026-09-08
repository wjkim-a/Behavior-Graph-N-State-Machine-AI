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
    ///   |  또는 구조 다이어그램         (E)    |   AI MONITOR    |
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
        //
        // BandTop 을 올리면 패널이 커지는 대신 Game View 가 좁아진다.
        // Game View 에는 [지면 - Enemy - 상태 라벨 - 구역 표지판] 이 위로 쌓이므로
        // 이 값은 LabLayout.CameraY 와 짝으로 맞춰야 한다. 지금 값에서는
        // 지면(y=-3)이 화면 740px, 표지판 위쪽 끝이 1014px 에 온다. (상단바 아래 1018px)
        private const float BandBottom = 100f;
        private const float BandTop = 724f;
        private const float BandHeight = BandTop - BandBottom;        // 624
        private const float MonitorWidth = 520f;
        private const float MainWidth = 1352f;                        // x 16 ~ 1368
        private const float PageInset = 26f;
        private const float PageWidth = MainWidth - PageInset * 2f;   // 1300
        private const float PageHeight = BandHeight - 46f;            // 578

        // 글자 크기는 LabUIKit 의 사다리(5단계)만 쓴다.
        // 종류를 늘리면 다이나믹 폰트 아틀라스가 상한에 부딪혀 글자가 깨진다.
        // 자세한 내용은 LabUIKit 의 글자 크기 주석에 적어 두었다.
        //
        // 문단 높이는 반드시 LabUIKit.LineBlock 으로 계산한다. (겹침 방지)
        private const int FsSmall = LabUIKit.FsSmall;   // 16
        private const int FsBody = LabUIKit.FsBody;     // 18
        private const int FsHead = LabUIKit.FsHead;     // 21
        private const int FsTitle = LabUIKit.FsTitle;   // 24

        // 비교 화면(C)의 문단 줄 수. LabContent 의 배열 길이와 맞춰 둔다.
        private const int CompareMindsetLines = 10;
        private const int ComparePodLines = 4;

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

            // 글자를 굽기 전에 먼저 구독한다.
            // OnEnable 은 Awake 뒤에 돌기 때문에, 여기서 굽는 동안 일어난
            // 아틀라스 재구성을 OnEnable 로는 놓친다.
            Font.textureRebuilt += OnFontTextureRebuilt;

            WarmFontAtlas();
        }

        private void Start()
        {
            TryBuildWorldLabels();
        }

        private void OnDestroy()
        {
            Font.textureRebuilt -= OnFontTextureRebuilt;
        }

        // 폰트 --------------------------------------------------------

        /// <summary>
        /// 앞으로 화면에 나올 글자를 폰트 아틀라스에 미리 다 구워 둔다.
        ///
        /// 다이나믹 폰트는 글자가 처음 나올 때 아틀라스에 굽고, 자리가 없으면
        /// 더 큰 텍스처로 전부 다시 굽는다. 다시 구울 때 글리프 위치가 바뀌므로
        /// 그 순간 화면에 있던 글자는 엉뚱한 자리를 가리켜 깨져 보인다.
        /// 시작할 때 최종 크기까지 한 번에 키워 두면 실행 중에 다시 구울 일이 없다.
        ///
        /// Awake 와 Start 에서 각각 한 번씩 부른다.
        /// Awake 때는 아직 LabDirector 가 Enemy 목록을 못 채웠기 때문이다.
        /// </summary>
        private void WarmFontAtlas()
        {
            StringBuilder sb = new StringBuilder();

            // 1) 이미 만들어 둔 Text. 꺼져 있는 페이지까지 포함해야 한다.
            Text[] built = _canvas.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < built.Length; i++)
                sb.Append(built[i].text);

            // 2) 실행 중에 내용이 바뀌는 문단들. 나올 수 있는 글자를 미리 모은다.
            if (director != null)
            {
                foreach (LabStep step in director.Steps)
                {
                    sb.Append(step.Title).Append(step.Headline);
                    foreach (string line in step.Body) sb.Append(line);
                    foreach (string line in step.Questions) sb.Append(line);
                    foreach (LabMission m in step.Missions) sb.Append(m.Text);
                }

                foreach (AIContext ctx in director.Enemies)
                    if (ctx != null) sb.Append(ctx.displayName);
            }

            sb.Append(RuntimeVocabulary);

            LabUIKit.Warm(DistinctChars(sb.ToString()));

            // 굽는 동안 아틀라스가 커졌다면, 이미 메시를 만들어 둔 Text 는
            // 옛 글리프 좌표를 그대로 들고 있다. 그대로 두면 그 글자만 뭉개져 보인다.
            // ("AI MONITOR" 는 깨지고 매 프레임 값이 바뀌는 "Squad A" 는 멀쩡한 이유였다.
            //  값이 바뀌는 Text 는 저절로 다시 만들어져서 티가 나지 않았을 뿐이다.)
            DirtyAllTexts();
        }

        /// <summary>
        /// 모든 Text 를 다시 만들게 한다. 꺼져 있는 페이지까지 포함한다.
        ///
        /// Unity 는 켜져 있고 값이 바뀐 Text 만 알아서 다시 만든다.
        /// 이 화면은 값이 안 바뀌는 제목이 많고 페이지를 껐다 켜서 쓰므로 직접 해준다.
        /// 201개를 한 번 훑는 정도라 시작할 때 한두 번 부르는 비용은 무시할 수 있다.
        /// </summary>
        private void DirtyAllTexts()
        {
            if (_canvas == null)
                return;

            Text[] all = _canvas.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < all.Length; i++)
                all[i].SetAllDirty();
        }

        /// <summary>
        /// 실행 중에만 나타나는 짧은 문구들. 미리 구워 둘 글자에 포함시킨다.
        /// (전환 이유 문장은 단계 설명과 전환 규칙에 쓰인 글자를 거의 그대로 쓴다)
        /// </summary>
        private const string RuntimeVocabulary =
            "0123456789.,%()[]<>/:+-· " +
            "Current State Node Action EXECUTE 중 실행 대기 TRUE FALSE " +
            "PATROL CHASE ATTACK FLEE DEAD IDLE LOOK MOVE WAIT " +
            "초 유지 직전 이유 거리 감지 공격 " +
            "선택된 AI 가 없다 지금 미션 전체 진행 리셋 리셋됨 시작 아직 평가 전 " +
            "이 없어서 가 아니어서 는 상태 객체가 직접 들고 있다 " +
            "씬에 공유 칠판 이 없다 노드들이 함께 보는 값 " +
            "숨김 으로 다시 표시 체력 회복 사망 경보 켜기 끄기";

        /// <summary>중복 글자를 걸러낸다. 폰트에 같은 글자를 여러 번 요청할 필요는 없다.</summary>
        private static string DistinctChars(string source)
        {
            HashSet<char> seen = new HashSet<char>();
            StringBuilder sb = new StringBuilder(source.Length);
            foreach (char c in source)
            {
                if (c == '\n' || c == '\r' || c == '\t')
                    continue;
                if (seen.Add(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 아틀라스가 그래도 다시 구워졌을 때의 복구.
        ///
        /// Unity 는 켜져 있는 Text 만 자동으로 다시 만들어 준다.
        /// 이 화면은 페이지를 껐다 켜서 쓰므로, 꺼져 있는 Text 까지 직접 무효화해 둔다.
        /// 그러지 않으면 다시 켰을 때 예전 글리프 좌표를 그대로 써서 깨진 글자가 남는다.
        /// </summary>
        private void OnFontTextureRebuilt(Font font)
        {
            if (font != LabUIKit.Font)
                return;

            DirtyAllTexts();
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

            // 방금 만든 라벨에는 아직 굽지 않은 글자가 있을 수 있다.
            // (Enemy 이름, 표지판 문구는 Awake 때는 존재하지 않았다)
            WarmFontAtlas();
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
            // 상단바를 50px 로 얇게 둔다. Game View 를 조금이라도 넓게 쓰기 위해서다.
            RectTransform bar = LabUIKit.Panel(_hudRoot, "TopBar", LabUIKit.PanelBgSolid);
            LabUIKit.StretchTop(bar, 16f, 12f, 50f);

            Text title = LabUIKit.Label(bar, "Title", "AI PROGRAMMING LAB", FsTitle, LabUIKit.Accent,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            LabUIKit.TopLeft(title.rectTransform, 22f, -9f, 350f, 32f);

            _stepLabel = LabUIKit.Label(bar, "StepLabel", "STEP 01 / 06", FsHead, LabUIKit.AccentWarm,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_stepLabel.rectTransform, 390f, -9f, 190f, 32f);

            _stepTitle = LabUIKit.Label(bar, "StepTitle", "", FsHead, LabUIKit.TextMain,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_stepTitle.rectTransform, 590f, -9f, 560f, 32f);

            // 오른쪽 끝에서 왼쪽으로 자란다. StepTitle 이 끝나는 1150px 앞에서 멈추도록 폭을 잡았다.
            _progress = LabUIKit.Label(bar, "Progress", "", FsBody, LabUIKit.TextDim, TextAnchor.MiddleRight);
            RectTransform pr = _progress.rectTransform;
            pr.anchorMin = new Vector2(1f, 0.5f);
            pr.anchorMax = new Vector2(1f, 0.5f);
            pr.pivot = new Vector2(1f, 0.5f);
            pr.anchoredPosition = new Vector2(-22f, 0f);
            pr.sizeDelta = new Vector2(720f, 30f);
        }

        /// <summary>가운데 큰 영역. 페이지들이 이 자리를 번갈아 쓴다.</summary>
        private void BuildMainArea()
        {
            RectTransform main = LabUIKit.Panel(_hudRoot, "MainArea", LabUIKit.PanelBg);
            LabUIKit.BottomLeft(main, 16f, BandBottom, MainWidth, BandHeight);

            _pageHint = LabUIKit.Label(main, "PageHint", "", FsSmall, LabUIKit.TextDim, TextAnchor.UpperRight);
            LabUIKit.TopLeft(_pageHint.rectTransform, PageInset, -10f, PageWidth, 24f);

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

            const float leftW = 700f;
            const float rightX = 740f;
            const float rightW = PageWidth - rightX;   // 560

            _headline = LabUIKit.Label(_teachPage, "Headline", "", FsTitle, LabUIKit.AccentWarm,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_headline.rectTransform, 0f, 0f, PageWidth, 38f);

            // 가장 긴 단계(STEP 04 / 06)가 19줄이다. 그만큼을 잡아 둔다.
            _body = LabUIKit.Label(_teachPage, "Body", "", FsBody, LabUIKit.TextMain);
            LabUIKit.TopLeft(_body.rectTransform, 0f, -46f, leftW, LabUIKit.LineBlock(19, FsBody));

            Text qHeader = LabUIKit.Label(_teachPage, "QHeader", "생각해 보기", FsHead, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(qHeader.rectTransform, rightX, -46f, rightW, 34f);

            _questions = LabUIKit.Clip(LabUIKit.Label(_teachPage, "Questions", "", FsBody, LabUIKit.TextDim));
            LabUIKit.TopLeft(_questions.rectTransform, rightX, -84f, rightW, LabUIKit.LineBlock(6, FsBody));

            _missionHeader = LabUIKit.Label(_teachPage, "MissionHeader", "MISSION", FsHead, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_missionHeader.rectTransform, rightX, -258f, rightW, 34f);

            _missionBody = LabUIKit.Clip(LabUIKit.Label(_teachPage, "MissionBody", "", FsBody, LabUIKit.TextMain));
            LabUIKit.TopLeft(_missionBody.rectTransform, rightX, -296f, rightW, LabUIKit.LineBlock(9, FsBody));
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

        /// <summary>
        /// 두 구조 비교 페이지 (C 키).
        ///
        /// 예전에는 사고방식 문단의 상자 높이를 210px 로 고정해 두었는데,
        /// 실제 텍스트는 14줄(약 294px)이라 상자를 뚫고 내려와 아래의
        /// 장점 / 주의점 문단 위에 그대로 겹쳐 찍혔다.
        ///
        /// 그래서 지금은
        ///   - 모든 문단 높이를 LabUIKit.LineBlock 으로 계산한다.
        ///   - 위에서 아래로 y 를 누적해서 쌓는다. 겹칠 수 있는 여지가 없다.
        ///   - 문단 줄 수는 상수(CompareMindsetLines / ComparePodLines)로 못박아 두고
        ///     LabContent 쪽에도 줄 수를 주석으로 적어 두었다.
        /// </summary>
        private void BuildComparePage(Transform parent)
        {
            _comparePage = NewPage(parent, "ComparePage");

            Text head = LabUIKit.Label(_comparePage, "Head",
                "같은 요구사항을 두 방식으로 만들면 구조가 어떻게 달라지는가", FsTitle, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 0f, 0f, PageWidth, 38f);

            const float colGap = 28f;
            float colW = (PageWidth - colGap) * 0.5f;      // 636
            float innerW = colW - 40f;                     // 596
            float rightX = colW + colGap;

            // 윗줄 : 사고방식 --------------------------------------------
            const float mindTop = -46f;
            const float padTop = 14f;
            const float headH = 34f;
            const float headGap = 6f;
            float mindBodyH = LabUIKit.LineBlock(CompareMindsetLines, FsBody);
            float mindPanelH = padTop + headH + headGap + mindBodyH + 12f;
            float mindBodyY = -(padTop + headH + headGap);

            _compareStateLabel = BuildComparePod(
                _comparePage, "Left", 0f, mindTop, colW, mindPanelH,
                "STATE PATTERN", LabUIKit.LabelColor("PATROL"),
                LabContent.StatePatternMindset, FsBody, LabUIKit.TextMain,
                innerW, mindBodyY, mindBodyH, headH);

            _compareBtLabel = BuildComparePod(
                _comparePage, "Right", rightX, mindTop, colW, mindPanelH,
                "BEHAVIOR TREE", LabUIKit.LabelColor("CHASE"),
                LabContent.BehaviorTreeMindset, FsBody, LabUIKit.TextMain,
                innerW, mindBodyY, mindBodyH, headH);

            // 아랫줄 : 장점 / 주의점 -------------------------------------
            float prosTop = mindTop - mindPanelH - 12f;
            float prosBodyH = LabUIKit.LineBlock(ComparePodLines, FsBody);
            float prosPanelH = 12f + prosBodyH + 10f;

            BuildComparePod(
                _comparePage, "LeftPros", 0f, prosTop, colW, prosPanelH,
                null, default, LabContent.StatePatternPros, FsBody, LabUIKit.TextDim,
                innerW, -12f, prosBodyH, 0f);

            BuildComparePod(
                _comparePage, "RightPros", rightX, prosTop, colW, prosPanelH,
                null, default, LabContent.BehaviorTreePros, FsBody, LabUIKit.TextDim,
                innerW, -12f, prosBodyH, 0f);
        }

        /// <summary>
        /// 비교 페이지의 문단 하나(패널 + 제목 + 본문)를 만든다.
        /// 제목이 있으면 제목 오른쪽에 "지금 : XXX" 를 붙일 Text 를 돌려준다.
        /// </summary>
        private static Text BuildComparePod(Transform parent, string name, float x, float y,
            float panelW, float panelH, string title, Color titleColor,
            string[] lines, int fontSize, Color bodyColor,
            float innerW, float bodyY, float bodyH, float headH)
        {
            RectTransform panel = LabUIKit.Panel(parent, name, new Color(0.10f, 0.13f, 0.19f, 1f));
            LabUIKit.TopLeft(panel, x, y, panelW, panelH);

            Text live = null;
            if (!string.IsNullOrEmpty(title))
            {
                Text h = LabUIKit.Label(panel, "H", title, LabUIKit.FsHead, titleColor,
                    TextAnchor.UpperLeft, FontStyle.Bold);
                LabUIKit.TopLeft(h.rectTransform, 20f, -14f, innerW, headH);

                // 같은 줄의 오른쪽 끝에 붙는다. 정렬이 반대라 제목과 부딪히지 않는다.
                live = LabUIKit.Label(panel, "Live", "", LabUIKit.FsBody, LabUIKit.TextDim, TextAnchor.UpperRight);
                LabUIKit.TopLeft(live.rectTransform, 20f, -14f, innerW, headH);
            }

            Text body = LabUIKit.Label(panel, "Body", string.Join("\n", lines), fontSize, bodyColor);
            LabUIKit.TopLeft(body.rectTransform, 20f, bodyY, innerW, bodyH);

            return live;
        }

        private void BuildBlackboardPage(Transform parent)
        {
            _blackboardPage = NewPage(parent, "BlackboardPage");

            Text head = LabUIKit.Label(_blackboardPage, "Head",
                "BLACKBOARD  -  노드들이 함께 보는 데이터 칠판", FsTitle, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 0f, 0f, PageWidth, 38f);

            // 칠판 값의 줄 수는 실행 중에 바뀐다. 넘치면 겹치지 않도록 잘라낸다.
            _blackboardBody = LabUIKit.Clip(
                LabUIKit.Label(_blackboardPage, "Body", "", FsBody, LabUIKit.TextMain));
            LabUIKit.TopLeft(_blackboardBody.rectTransform, 10f, -46f, 700f,
                LabUIKit.LineBlock(19, FsBody));

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
            LabUIKit.TopLeft(notePanel, 740f, -46f, PageWidth - 740f,
                28f + LabUIKit.LineBlock(note.Length, FsBody));

            Text noteText = LabUIKit.Label(notePanel, "T", string.Join("\n", note), FsBody, LabUIKit.TextDim);
            LabUIKit.Stretch(noteText.rectTransform, 20f, 14f, 20f, 14f);
        }

        private void BuildQuizPage(Transform parent)
        {
            _quizPage = NewPage(parent, "QuizPage");

            Text head = LabUIKit.Label(_quizPage, "Head",
                "SELF CHECK  -  스스로 설명할 수 있는지 확인하기", FsTitle,
                LabUIKit.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 0f, 0f, PageWidth, 38f);

            const float rightX = 740f;
            const float rightW = PageWidth - rightX;   // 560

            Text body = LabUIKit.Label(_quizPage, "Body", string.Join("\n", LabContent.SelfCheck), FsBody,
                LabUIKit.TextMain);
            LabUIKit.TopLeft(body.rectTransform, 10f, -46f, 700f,
                LabUIKit.LineBlock(LabContent.SelfCheck.Length, FsBody));

            // 오른쪽 두 문단은 줄바꿈이 생길 수 있어 줄 수를 넉넉히 잡는다.
            float hintH = 26f + LabUIKit.LineBlock(7, FsSmall);
            RectTransform hintPanel = LabUIKit.Panel(_quizPage, "HintPanel", new Color(0.10f, 0.13f, 0.19f, 1f));
            LabUIKit.TopLeft(hintPanel, rightX, -46f, rightW, hintH);

            Text hint = LabUIKit.Label(hintPanel, "Hint", string.Join("\n", LabContent.SelfCheckHint), FsSmall,
                LabUIKit.TextDim);
            LabUIKit.Stretch(hint.rectTransform, 18f, 12f, 18f, 12f);

            Text when = LabUIKit.Label(_quizPage, "When", string.Join("\n", LabContent.WhenToUse), FsSmall,
                LabUIKit.TextDim);
            LabUIKit.TopLeft(when.rectTransform, rightX, -46f - hintH - 14f, rightW,
                LabUIKit.LineBlock(12, FsSmall));
        }

        private void BuildMonitorPanel()
        {
            RectTransform panel = LabUIKit.Panel(_hudRoot, "MonitorPanel", LabUIKit.PanelBg);
            LabUIKit.BottomRight(panel, 16f, BandBottom, MonitorWidth, BandHeight);

            float w = MonitorWidth - 40f;

            Text head = LabUIKit.Label(panel, "Head", "AI MONITOR", FsHead, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 20f, -8f, 260f, 34f);

            Text hint = LabUIKit.Label(panel, "Hint", "Tab : 다른 AI", FsSmall, LabUIKit.TextDim,
                TextAnchor.UpperRight);
            LabUIKit.TopLeft(hint.rectTransform, 20f, -8f, w, 34f);

            _monitorName = LabUIKit.Label(panel, "Name", "", FsHead, LabUIKit.TextMain,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_monitorName.rectTransform, 20f, -42f, w, 34f);

            _monitorSystem = LabUIKit.Label(panel, "System", "", FsBody, LabUIKit.Accent);
            LabUIKit.TopLeft(_monitorSystem.rectTransform, 20f, -78f, w, 28f);

            _hpFill = LabUIKit.Bar(panel, "HpBar", 20f, -108f, w, 18f,
                new Color(0.16f, 0.18f, 0.24f, 1f), LabUIKit.Good);

            _hpText = LabUIKit.Label(panel, "HpText", "", FsSmall, LabUIKit.TextDim);
            LabUIKit.TopLeft(_hpText.rectTransform, 20f, -128f, w, 26f);

            // 상태 이름 한 줄만 <size=24> 로 크게 찍고, 나머지는 본문 크기다.
            // 전환 이유가 길어 두 줄이 되는 경우가 있어 잘라낸다. (아래 로그를 밀지 않게)
            _monitorBody = LabUIKit.Clip(LabUIKit.Label(panel, "Body", "", FsBody, LabUIKit.TextMain));
            LabUIKit.TopLeft(_monitorBody.rectTransform, 20f, -156f, w, 320f);

            Text logHead = LabUIKit.Label(panel, "LogHead", "ENTER / EXECUTE / EXIT 로그", FsBody,
                LabUIKit.AccentWarm, TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(logHead.rectTransform, 20f, -484f, w, 28f);

            _logText = LabUIKit.Clip(LabUIKit.Label(panel, "Log", "", FsSmall, LabUIKit.TextDim));
            LabUIKit.TopLeft(_logText.rectTransform, 20f, -516f, w, LabUIKit.LineBlock(4, FsSmall));
        }

        private void BuildBottomBar()
        {
            RectTransform bar = LabUIKit.Panel(_hudRoot, "BottomBar", LabUIKit.PanelBgSolid);
            LabUIKit.StretchBottom(bar, 16f, 12f, 84f);

            _toast = LabUIKit.Label(bar, "Toast", "", FsHead, LabUIKit.AccentWarm,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            LabUIKit.TopLeft(_toast.rectTransform, 22f, -4f, 1500f, 32f);

            Text c1 = LabUIKit.Label(bar, "C1", LabContent.ControlsLine1, FsSmall, LabUIKit.TextMain);
            LabUIKit.TopLeft(c1.rectTransform, 22f, -34f, 1856f, 26f);

            Text c2 = LabUIKit.Label(bar, "C2", LabContent.ControlsLine2, FsSmall, LabUIKit.TextDim);
            LabUIKit.TopLeft(c2.rectTransform, 22f, -58f, 1856f, 26f);
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
            _pageHint.text = "E : 구조 보기      C : 비교      B : Blackboard      Q : 자기진단";

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
            _pageHint.text = "E 또는 M : 설명으로 돌아가기      Tab : 다른 AI 의 구조 보기";

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
            sb.Append("<size=24><b><color=#")
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
            int start = Mathf.Max(0, lines.Count - 4);
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
