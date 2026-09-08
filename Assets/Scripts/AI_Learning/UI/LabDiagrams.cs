using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AILearning
{
    /// <summary>다이어그램에 쓰이는 상자 하나. 강조 표시를 켜고 끌 수 있다.</summary>
    public class DiagramBox
    {
        public RectTransform Rt;
        public Image Edge;
        public Image Bg;
        public Text Title;
        public Text Sub;
        public Text Badge;

        /// <summary>
        /// 상자 하나를 만든다.
        ///
        /// 높이 h 는 위 60% 를 제목, 아래 40% 를 부제에 쓴다.
        /// 그래서 부제가 있는 상자는 h 가 66 보다 작으면 글자가 상자를 넘친다.
        /// (제목 24 -> 37px, 부제 16 -> 25px 필요)
        /// </summary>
        public static DiagramBox Create(Transform parent, string name, float x, float y, float w, float h,
            string title, string sub, int titleSize = LabUIKit.FsHead, int subSize = LabUIKit.FsSmall)
        {
            DiagramBox box = new DiagramBox();
            box.Rt = LabUIKit.NewRect(parent, name);
            LabUIKit.TopLeft(box.Rt, x, y, w, h);

            RectTransform edgeRt = LabUIKit.NewRect(box.Rt, "Edge");
            LabUIKit.Stretch(edgeRt, 0f, 0f, 0f, 0f);
            box.Edge = edgeRt.gameObject.AddComponent<Image>();
            box.Edge.sprite = LabUIKit.Rounded;
            box.Edge.type = Image.Type.Sliced;
            box.Edge.color = new Color(0.22f, 0.27f, 0.36f, 1f);
            box.Edge.raycastTarget = false;

            RectTransform bgRt = LabUIKit.NewRect(box.Rt, "Bg");
            LabUIKit.Stretch(bgRt, 2f, 2f, 2f, 2f);
            box.Bg = bgRt.gameObject.AddComponent<Image>();
            box.Bg.sprite = LabUIKit.Rounded;
            box.Bg.type = Image.Type.Sliced;
            box.Bg.color = new Color(0.11f, 0.14f, 0.20f, 1f);
            box.Bg.raycastTarget = false;

            bool hasSub = !string.IsNullOrEmpty(sub);

            box.Title = LabUIKit.Label(box.Rt, "Title", title, titleSize, LabUIKit.TextMain,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            LabUIKit.Stretch(box.Title.rectTransform, 8f, hasSub ? h * 0.40f : 2f, 8f, 2f);

            if (hasSub)
            {
                box.Sub = LabUIKit.Label(box.Rt, "Sub", sub, subSize, LabUIKit.TextDim, TextAnchor.MiddleCenter);
                LabUIKit.Stretch(box.Sub.rectTransform, 8f, 4f, 8f, h * 0.60f);
            }

            return box;
        }

        /// <summary>왼쪽 위에 작은 순서 배지를 붙인다.</summary>
        public void AddBadge(string text, Color color)
        {
            RectTransform rt = LabUIKit.Panel(Rt, "Badge", color);
            LabUIKit.TopLeft(rt, -9f, 11f, 32f, 28f);
            Badge = LabUIKit.Label(rt, "T", text, LabUIKit.FsSmall, new Color(0.06f, 0.08f, 0.12f),
                TextAnchor.MiddleCenter, FontStyle.Bold);
            LabUIKit.Stretch(Badge.rectTransform, 0f, 0f, 0f, 0f);
        }

        public void SetHighlight(bool on, Color accent)
        {
            if (on)
            {
                Edge.color = accent;
                Bg.color = new Color(accent.r * 0.28f + 0.05f, accent.g * 0.28f + 0.06f, accent.b * 0.28f + 0.08f, 1f);
                Title.color = Color.white;
            }
            else
            {
                Edge.color = new Color(0.22f, 0.27f, 0.36f, 1f);
                Bg.color = new Color(0.11f, 0.14f, 0.20f, 1f);
                Title.color = LabUIKit.TextMain;
            }
        }

        public void SetBoolean(bool value)
        {
            Color c = value ? LabUIKit.Good : LabUIKit.Idle;
            Edge.color = c;
            Bg.color = value
                ? new Color(0.10f, 0.22f, 0.14f, 1f)
                : new Color(0.11f, 0.13f, 0.17f, 1f);
            if (Sub != null)
            {
                Sub.text = value ? "TRUE" : "FALSE";
                Sub.color = value ? LabUIKit.Good : LabUIKit.TextDim;
            }
        }
    }

    // =================================================================
    //  State Machine 구조 다이어그램
    // =================================================================
    public class StateDiagramWidget
    {
        private readonly Dictionary<string, DiagramBox> _stateBoxes = new Dictionary<string, DiagramBox>();
        private readonly List<Text> _ruleTexts = new List<Text>();
        private DiagramBox _currentBox;
        private Text _owner;

        public RectTransform Root { get; private set; }

        public StateDiagramWidget(Transform parent, float width, float height)
        {
            Root = LabUIKit.NewRect(parent, "StateDiagram");
            LabUIKit.Stretch(Root, 0f, 0f, 0f, 0f);

            Text headline = LabUIKit.Label(Root, "Headline", "STATE PATTERN  -  구조", LabUIKit.FsTitle,
                LabUIKit.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(headline.rectTransform, 0f, 0f, width, 38f);

            // 제목과 같은 줄. 제목은 왼쪽, 이것은 오른쪽 정렬이라 부딪히지 않는다.
            // (한 줄 위에 두면 페이지 상단의 안내 문구와 겹친다.)
            _owner = LabUIKit.Label(Root, "Owner", "", LabUIKit.FsBody, LabUIKit.TextDim, TextAnchor.UpperRight);
            LabUIKit.TopLeft(_owner.rectTransform, 0f, 0f, width, 38f);

            float cx = width * 0.5f;

            DiagramBox machine = DiagramBox.Create(Root, "Machine", cx - 200f, -44f, 400f, 66f,
                "StateMachine", "지금 어떤 State 를 쓸지 관리한다");
            machine.SetHighlight(true, LabUIKit.Accent);

            LabUIKit.VLine(Root, cx - 1f, -110f, 18f, LabUIKit.PanelEdge, 3f);

            _currentBox = DiagramBox.Create(Root, "Current", cx - 270f, -128f, 540f, 68f,
                "Current State : -", "이 값을 교체하는 것이 곧 행동을 바꾸는 것이다", LabUIKit.FsTitle);

            LabUIKit.VLine(Root, cx - 1f, -196f, 18f, LabUIKit.PanelEdge, 3f);

            // 5개 상태 상자
            string[] names = EnemyStateMachine.StateNames;
            int n = names.Length;
            float gap = 18f;
            float boxW = Mathf.Min(225f, (width - (n - 1) * gap) / n);
            float total = n * boxW + (n - 1) * gap;
            float startX = (width - total) * 0.5f;
            float boxY = -240f;
            float boxH = 68f;

            float firstCenter = startX + boxW * 0.5f;
            float lastCenter = startX + total - boxW * 0.5f;
            LabUIKit.HLine(Root, firstCenter, -214f, lastCenter - firstCenter, LabUIKit.PanelEdge, 3f);

            for (int i = 0; i < n; i++)
            {
                float x = startX + i * (boxW + gap);
                float center = x + boxW * 0.5f;
                LabUIKit.VLine(Root, center - 1f, -214f, 26f, LabUIKit.PanelEdge, 3f);

                DiagramBox box = DiagramBox.Create(Root, "State_" + names[i], x, boxY, boxW, boxH,
                    names[i], "대기", LabUIKit.FsTitle);
                _stateBoxes[names[i]] = box;
            }

            // 전환 규칙 목록. 7줄이 페이지 아래 끝(578px) 안에 들어간다.
            Text rulesTitle = LabUIKit.Label(Root, "RulesTitle",
                "STATE TRANSITION  -  상태는 조건이 있을 때만 바뀐다", LabUIKit.FsHead,
                LabUIKit.AccentWarm, TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(rulesTitle.rectTransform, 6f, -320f, width, 34f);

            int rules = LabDiagramSpecs.TransitionRules.GetLength(0);
            for (int i = 0; i < rules; i++)
            {
                string from = LabDiagramSpecs.TransitionRules[i, 0];
                string to = LabDiagramSpecs.TransitionRules[i, 1];
                string cond = LabDiagramSpecs.TransitionRules[i, 2];
                string fromLabel = from == "*" ? "모든 상태" : from;

                Text t = LabUIKit.Label(Root, "Rule" + i,
                    "    " + fromLabel.PadRight(10) + "  →  " + to.PadRight(8) + "      " + cond,
                    LabUIKit.FsBody, LabUIKit.TextDim);
                LabUIKit.TopLeft(t.rectTransform, 6f, -358f - i * 29f, width - 12f, 28f);
                _ruleTexts.Add(t);
            }
        }

        public void Refresh(AIContext ctx, string lastFrom, string lastTo)
        {
            if (ctx == null || ctx.Brain == null)
                return;

            string current = ctx.Brain.CurrentLabel;
            _owner.text = ctx.displayName + "   ·   " + ctx.Brain.SystemName;

            foreach (KeyValuePair<string, DiagramBox> kv in _stateBoxes)
            {
                bool on = kv.Key == current;
                kv.Value.SetHighlight(on, LabUIKit.LabelColor(kv.Key));
                if (kv.Value.Sub != null)
                    kv.Value.Sub.text = on ? "EXECUTE 중" : "대기";
            }

            _currentBox.Title.text = "Current State :  " + current;
            _currentBox.SetHighlight(true, LabUIKit.LabelColor(current));

            int rules = LabDiagramSpecs.TransitionRules.GetLength(0);
            for (int i = 0; i < rules && i < _ruleTexts.Count; i++)
            {
                string from = LabDiagramSpecs.TransitionRules[i, 0];
                string to = LabDiagramSpecs.TransitionRules[i, 1];
                bool fired = to == lastTo && (from == "*" || from == lastFrom);
                _ruleTexts[i].color = fired ? LabUIKit.AccentWarm : LabUIKit.TextDim;
                _ruleTexts[i].fontStyle = fired ? FontStyle.Bold : FontStyle.Normal;
            }
        }
    }

    // =================================================================
    //  Behavior Tree 구조 다이어그램
    // =================================================================
    public class TreeDiagramWidget
    {
        private readonly List<DiagramBox> _conditionBoxes = new List<DiagramBox>();
        private readonly List<DiagramBox> _actionBoxes = new List<DiagramBox>();
        private readonly TreeSpec _spec;
        private Text _owner;

        public RectTransform Root { get; private set; }

        public TreeDiagramWidget(Transform parent, float width, float height, TreeSpec spec, string headline)
        {
            _spec = spec;
            Root = LabUIKit.NewRect(parent, "TreeDiagram");
            LabUIKit.Stretch(Root, 0f, 0f, 0f, 0f);

            Text head = LabUIKit.Label(Root, "Headline", headline, LabUIKit.FsTitle, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 0f, 0f, width, 38f);

            // 제목과 같은 줄. 제목은 왼쪽, 이것은 오른쪽 정렬이라 부딪히지 않는다.
            // (한 줄 위에 두면 페이지 상단의 안내 문구와 겹친다.)
            _owner = LabUIKit.Label(Root, "Owner", "", LabUIKit.FsBody, LabUIKit.TextDim, TextAnchor.UpperRight);
            LabUIKit.TopLeft(_owner.rectTransform, 0f, 0f, width, 38f);

            float cx = width * 0.5f;

            DiagramBox root = DiagramBox.Create(Root, "Root", cx - 185f, -44f, 370f, 54f,
                spec.RootTitle, null, LabUIKit.FsTitle);
            root.SetHighlight(true, LabUIKit.Accent);

            LabUIKit.VLine(Root, cx - 1f, -98f, 18f, LabUIKit.PanelEdge, 3f);

            DiagramBox composite = DiagramBox.Create(Root, "Composite", cx - 300f, -116f, 600f, 66f,
                spec.CompositeTitle, spec.CompositeNote, LabUIKit.FsTitle);
            composite.SetHighlight(true, LabUIKit.AccentWarm);

            LabUIKit.VLine(Root, cx - 1f, -182f, 16f, LabUIKit.PanelEdge, 3f);

            int n = spec.Branches.Count;
            float gap = 18f;
            float colW = Mathf.Min(255f, (width - (n - 1) * gap) / n);
            float total = n * colW + (n - 1) * gap;
            float startX = (width - total) * 0.5f;

            float firstCenter = startX + colW * 0.5f;
            float lastCenter = startX + total - colW * 0.5f;
            LabUIKit.HLine(Root, firstCenter, -198f, lastCenter - firstCenter, LabUIKit.PanelEdge, 3f);

            for (int i = 0; i < n; i++)
            {
                TreeBranchSpec b = spec.Branches[i];
                float x = startX + i * (colW + gap);
                float center = x + colW * 0.5f;

                LabUIKit.VLine(Root, center - 1f, -198f, 24f, LabUIKit.PanelEdge, 3f);

                string condTitle = string.IsNullOrEmpty(b.ConditionName) ? "(조건 없음)" : b.ConditionName;
                string condSub = spec.ConditionsAreEvaluated
                    ? (string.IsNullOrEmpty(b.ConditionName) ? "기본 행동" : "FALSE")
                    : "Sequence 단계";

                DiagramBox cond = DiagramBox.Create(Root, "Cond" + i, x, -222f, colW, 68f,
                    condTitle, condSub, LabUIKit.FsBody);
                if (spec.ShowPriority)
                    cond.AddBadge((i + 1).ToString(), LabUIKit.AccentWarm);
                _conditionBoxes.Add(cond);

                LabUIKit.VLine(Root, center - 1f, -290f, 16f, LabUIKit.PanelEdge, 3f);

                DiagramBox act = DiagramBox.Create(Root, "Act" + i, x, -306f, colW, 68f,
                    b.ActionTitle, "Action", LabUIKit.FsHead);
                _actionBoxes.Add(act);
            }

            // 아래쪽 설명 : 두 열로 나눠서 넉넉하게 보여준다.
            // 두 열로 쪼개면 한 열이 630px 밖에 안 되므로 각주 크기를 쓴다.
            // 가장 긴 것이 SelectorDemo(왼쪽 3줄 + 오른쪽 6줄)다. 6줄이 578px 안에 들어간다.
            float colWidth = spec.Footer2.Length > 0 ? (width - 40f) * 0.5f : width - 12f;

            for (int i = 0; i < spec.Footer.Length; i++)
            {
                Text t = LabUIKit.Label(Root, "Foot" + i, "·  " + spec.Footer[i], LabUIKit.FsSmall, LabUIKit.TextDim);
                LabUIKit.TopLeft(t.rectTransform, 6f, -388f - i * 26f, colWidth, 26f);
            }

            for (int i = 0; i < spec.Footer2.Length; i++)
            {
                Text t = LabUIKit.Label(Root, "Foot2_" + i, spec.Footer2[i], LabUIKit.FsSmall, LabUIKit.TextDim);
                LabUIKit.TopLeft(t.rectTransform, colWidth + 40f, -388f - i * 26f, colWidth, 26f);
            }
        }

        public void Refresh(AIContext ctx)
        {
            if (ctx == null || ctx.Brain == null)
                return;

            BTNodeTrace trace = ctx.GetComponent<BTNodeTrace>();
            string current = ctx.Brain.CurrentLabel;
            _owner.text = ctx.displayName + "   ·   " + ctx.Brain.SystemName;

            for (int i = 0; i < _spec.Branches.Count; i++)
            {
                TreeBranchSpec b = _spec.Branches[i];
                bool active = b.ActionLabel == current;

                _actionBoxes[i].SetHighlight(active, LabUIKit.LabelColor(b.ActionLabel));
                if (_actionBoxes[i].Sub != null)
                    _actionBoxes[i].Sub.text = active ? "실행 중" : "Action";

                if (_spec.ConditionsAreEvaluated)
                {
                    if (string.IsNullOrEmpty(b.ConditionName))
                    {
                        _conditionBoxes[i].SetHighlight(active, LabUIKit.LabelColor(b.ActionLabel));
                    }
                    else
                    {
                        bool value = trace != null && trace.GetConditionResult(b.ConditionName);
                        _conditionBoxes[i].SetBoolean(value);
                    }
                }
                else
                {
                    _conditionBoxes[i].SetHighlight(active, LabUIKit.AccentWarm);
                }
            }
        }
    }

    // =================================================================
    //  STEP 01 : if / else 구조 다이어그램
    // =================================================================
    public class ReactiveDiagramWidget
    {
        private readonly List<DiagramBox> _rows = new List<DiagramBox>();
        private readonly TreeBranchSpec[] _specs;
        private Text _distance;

        public RectTransform Root { get; private set; }

        public ReactiveDiagramWidget(Transform parent, float width, float height)
        {
            _specs = LabDiagramSpecs.ReactiveRows();

            Root = LabUIKit.NewRect(parent, "ReactiveDiagram");
            LabUIKit.Stretch(Root, 0f, 0f, 0f, 0f);

            Text head = LabUIKit.Label(Root, "Headline",
                "구조 없는 AI  -  거리 조건을 위에서부터 확인한다", LabUIKit.FsTitle,
                LabUIKit.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 0f, 0f, width, 38f);

            _distance = LabUIKit.Label(Root, "Distance", "", LabUIKit.FsBody, LabUIKit.TextDim, TextAnchor.UpperRight);
            LabUIKit.TopLeft(_distance.rectTransform, 0f, 0f, width, 38f);

            float rowW = Mathf.Min(960f, width - 120f);
            float rowX = (width - rowW) * 0.5f;

            for (int i = 0; i < _specs.Length; i++)
            {
                DiagramBox row = DiagramBox.Create(Root, "Row" + i, rowX, -56f - i * 86f, rowW, 74f,
                    _specs[i].ConditionName + "        →        " + _specs[i].ActionLabel +
                    "   (" + _specs[i].ActionTitle + ")",
                    null, LabUIKit.FsHead);
                row.AddBadge((i + 1).ToString(), LabUIKit.AccentWarm);
                _rows.Add(row);
            }

            string[] footer =
            {
                "조건을 위에서부터 확인해서 처음 맞는 것을 실행한다. 아주 단순하고, 아주 잘 동작한다.",
                "그런데 상태마다 기억해야 할 값이 생기면 어떻게 될까?  (예: 공격 쿨다운, 순찰 지점 번호)",
                "행동이 늘어나고 조건이 겹치기 시작하면 이 목록은 빠르게 읽기 어려워진다.",
                "그 문제를 정리하는 두 가지 방법이 STATE PATTERN 과 BEHAVIOR TREE 다."
            };

            for (int i = 0; i < footer.Length; i++)
            {
                Text t = LabUIKit.Label(Root, "Foot" + i, "·  " + footer[i], LabUIKit.FsBody, LabUIKit.TextDim);
                LabUIKit.TopLeft(t.rectTransform, 10f, -402f - i * 28f, width - 20f, 28f);
            }
        }

        public void Refresh(AIContext ctx)
        {
            if (ctx == null || ctx.Brain == null)
                return;

            string current = ctx.Brain.CurrentLabel;
            _distance.text = "Player 와의 거리 : " + ctx.DistanceToPlayer.ToString("0.0");

            for (int i = 0; i < _rows.Count; i++)
            {
                bool active = _specs[i].ActionLabel == current;
                _rows[i].SetHighlight(active, LabUIKit.LabelColor(_specs[i].ActionLabel));
            }
        }
    }
}
