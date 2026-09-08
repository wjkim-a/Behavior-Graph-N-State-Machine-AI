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

        public static DiagramBox Create(Transform parent, string name, float x, float y, float w, float h,
            string title, string sub, int titleSize = 18, int subSize = 13)
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
            LabUIKit.TopLeft(rt, -8f, 10f, 30f, 26f);
            Badge = LabUIKit.Label(rt, "T", text, 16, new Color(0.06f, 0.08f, 0.12f),
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

            Text headline = LabUIKit.Label(Root, "Headline", "STATE PATTERN  -  구조", 24,
                LabUIKit.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(headline.rectTransform, 0f, 0f, width, 30f);

            _owner = LabUIKit.Label(Root, "Owner", "", 16, LabUIKit.TextDim, TextAnchor.UpperRight);
            LabUIKit.TopLeft(_owner.rectTransform, 0f, 4f, width, 24f);

            float cx = width * 0.5f;

            DiagramBox machine = DiagramBox.Create(Root, "Machine", cx - 190f, -40f, 380f, 50f,
                "StateMachine", "지금 어떤 State 를 쓸지 관리한다", 20, 14);
            machine.SetHighlight(true, LabUIKit.Accent);

            LabUIKit.VLine(Root, cx - 1f, -90f, 24f, LabUIKit.PanelEdge, 3f);

            _currentBox = DiagramBox.Create(Root, "Current", cx - 250f, -114f, 500f, 54f,
                "Current State : -", "이 값을 교체하는 것이 곧 행동을 바꾸는 것이다", 21, 14);

            LabUIKit.VLine(Root, cx - 1f, -168f, 22f, LabUIKit.PanelEdge, 3f);

            // 5개 상태 상자
            string[] names = EnemyStateMachine.StateNames;
            int n = names.Length;
            float gap = 18f;
            float boxW = Mathf.Min(210f, (width - (n - 1) * gap) / n);
            float total = n * boxW + (n - 1) * gap;
            float startX = (width - total) * 0.5f;
            float boxY = -214f;
            float boxH = 72f;

            float firstCenter = startX + boxW * 0.5f;
            float lastCenter = startX + total - boxW * 0.5f;
            LabUIKit.HLine(Root, firstCenter, -190f, lastCenter - firstCenter, LabUIKit.PanelEdge, 3f);

            for (int i = 0; i < n; i++)
            {
                float x = startX + i * (boxW + gap);
                float center = x + boxW * 0.5f;
                LabUIKit.VLine(Root, center - 1f, -190f, 26f, LabUIKit.PanelEdge, 3f);

                DiagramBox box = DiagramBox.Create(Root, "State_" + names[i], x, boxY, boxW, boxH,
                    names[i], "대기", 22, 14);
                _stateBoxes[names[i]] = box;
            }

            // 전환 규칙 목록
            Text rulesTitle = LabUIKit.Label(Root, "RulesTitle",
                "STATE TRANSITION  -  상태는 조건이 있을 때만 바뀐다", 19,
                LabUIKit.AccentWarm, TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(rulesTitle.rectTransform, 6f, -304f, width, 26f);

            int rules = LabDiagramSpecs.TransitionRules.GetLength(0);
            for (int i = 0; i < rules; i++)
            {
                string from = LabDiagramSpecs.TransitionRules[i, 0];
                string to = LabDiagramSpecs.TransitionRules[i, 1];
                string cond = LabDiagramSpecs.TransitionRules[i, 2];
                string fromLabel = from == "*" ? "모든 상태" : from;

                Text t = LabUIKit.Label(Root, "Rule" + i,
                    "    " + fromLabel.PadRight(10) + "  →  " + to.PadRight(8) + "      " + cond,
                    17, LabUIKit.TextDim);
                LabUIKit.TopLeft(t.rectTransform, 6f, -338f - i * 27f, width - 12f, 26f);
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

            Text head = LabUIKit.Label(Root, "Headline", headline, 24, LabUIKit.Accent,
                TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 0f, 0f, width, 30f);

            _owner = LabUIKit.Label(Root, "Owner", "", 16, LabUIKit.TextDim, TextAnchor.UpperRight);
            LabUIKit.TopLeft(_owner.rectTransform, 0f, 4f, width, 24f);

            float cx = width * 0.5f;

            DiagramBox root = DiagramBox.Create(Root, "Root", cx - 170f, -40f, 340f, 48f,
                spec.RootTitle, null, 21);
            root.SetHighlight(true, LabUIKit.Accent);

            LabUIKit.VLine(Root, cx - 1f, -88f, 24f, LabUIKit.PanelEdge, 3f);

            DiagramBox composite = DiagramBox.Create(Root, "Composite", cx - 280f, -112f, 560f, 58f,
                spec.CompositeTitle, spec.CompositeNote, 22, 14);
            composite.SetHighlight(true, LabUIKit.AccentWarm);

            LabUIKit.VLine(Root, cx - 1f, -170f, 22f, LabUIKit.PanelEdge, 3f);

            int n = spec.Branches.Count;
            float gap = 18f;
            float colW = Mathf.Min(250f, (width - (n - 1) * gap) / n);
            float total = n * colW + (n - 1) * gap;
            float startX = (width - total) * 0.5f;

            float firstCenter = startX + colW * 0.5f;
            float lastCenter = startX + total - colW * 0.5f;
            LabUIKit.HLine(Root, firstCenter, -192f, lastCenter - firstCenter, LabUIKit.PanelEdge, 3f);

            for (int i = 0; i < n; i++)
            {
                TreeBranchSpec b = spec.Branches[i];
                float x = startX + i * (colW + gap);
                float center = x + colW * 0.5f;

                LabUIKit.VLine(Root, center - 1f, -192f, 26f, LabUIKit.PanelEdge, 3f);

                string condTitle = string.IsNullOrEmpty(b.ConditionName) ? "(조건 없음)" : b.ConditionName;
                string condSub = spec.ConditionsAreEvaluated
                    ? (string.IsNullOrEmpty(b.ConditionName) ? "기본 행동" : "FALSE")
                    : "Sequence 단계";

                DiagramBox cond = DiagramBox.Create(Root, "Cond" + i, x, -218f, colW, 70f,
                    condTitle, condSub, 17, 15);
                if (spec.ShowPriority)
                    cond.AddBadge((i + 1).ToString(), LabUIKit.AccentWarm);
                _conditionBoxes.Add(cond);

                LabUIKit.VLine(Root, center - 1f, -288f, 22f, LabUIKit.PanelEdge, 3f);

                DiagramBox act = DiagramBox.Create(Root, "Act" + i, x, -310f, colW, 70f,
                    b.ActionTitle, "Action", 19, 14);
                _actionBoxes.Add(act);
            }

            // 아래쪽 설명 : 두 열로 나눠서 넉넉하게 보여준다.
            float colWidth = spec.Footer2.Length > 0 ? (width - 40f) * 0.5f : width - 12f;

            for (int i = 0; i < spec.Footer.Length; i++)
            {
                Text t = LabUIKit.Label(Root, "Foot" + i, "·  " + spec.Footer[i], 16, LabUIKit.TextDim);
                LabUIKit.TopLeft(t.rectTransform, 6f, -398f - i * 26f, colWidth, 24f);
            }

            for (int i = 0; i < spec.Footer2.Length; i++)
            {
                Text t = LabUIKit.Label(Root, "Foot2_" + i, spec.Footer2[i], 16, LabUIKit.TextDim);
                LabUIKit.TopLeft(t.rectTransform, colWidth + 40f, -398f - i * 26f, colWidth, 24f);
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
                "구조 없는 AI  -  거리 조건을 위에서부터 확인한다", 24,
                LabUIKit.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            LabUIKit.TopLeft(head.rectTransform, 0f, 0f, width, 30f);

            _distance = LabUIKit.Label(Root, "Distance", "", 17, LabUIKit.TextDim, TextAnchor.UpperRight);
            LabUIKit.TopLeft(_distance.rectTransform, 0f, 4f, width, 24f);

            float rowW = Mathf.Min(900f, width - 120f);
            float rowX = (width - rowW) * 0.5f;

            for (int i = 0; i < _specs.Length; i++)
            {
                DiagramBox row = DiagramBox.Create(Root, "Row" + i, rowX, -54f - i * 80f, rowW, 68f,
                    _specs[i].ConditionName + "        →        " + _specs[i].ActionLabel +
                    "   (" + _specs[i].ActionTitle + ")",
                    null, 21);
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
                Text t = LabUIKit.Label(Root, "Foot" + i, "·  " + footer[i], 17, LabUIKit.TextDim);
                LabUIKit.TopLeft(t.rectTransform, 10f, -390f - i * 28f, width - 20f, 26f);
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
