using UnityEngine;
using UnityEngine.UI;

namespace AILearning
{
    /// <summary>
    /// Enemy 머리 위에 따라다니는 상태 라벨.
    ///
    /// 학습에서 가장 중요한 표시다.
    /// "지금 이 AI 가 어떤 상태 / 어떤 노드를 실행 중인가" 를 Game View 에서 바로 보여준다.
    /// </summary>
    public class EnemyWorldLabel
    {
        private readonly AIContext _ctx;
        private readonly RectTransform _root;
        private readonly Image _edge;
        private readonly Image _bg;
        private readonly Text _name;
        private readonly Text _label;
        private readonly Image _hpFill;
        private readonly Text _system;

        // Game View 세로 공간이 한정되어 있다. (지면 - Enemy - 이 라벨 - 구역 표지판)
        // Height 를 키우면 표지판이 상단바에 가려지므로 96px 을 유지하고 글자만 키웠다.
        private const float Width = 260f;
        private const float Height = 96f;

        public EnemyWorldLabel(Transform parent, AIContext ctx)
        {
            _ctx = ctx;

            _root = LabUIKit.NewRect(parent, "Label_" + ctx.gameObject.name);
            _root.sizeDelta = new Vector2(Width, Height);
            _root.pivot = new Vector2(0.5f, 0f);
            _root.anchorMin = new Vector2(0f, 0f);
            _root.anchorMax = new Vector2(0f, 0f);

            RectTransform edgeRt = LabUIKit.NewRect(_root, "Edge");
            LabUIKit.Stretch(edgeRt, 0f, 0f, 0f, 0f);
            _edge = edgeRt.gameObject.AddComponent<Image>();
            _edge.sprite = LabUIKit.Rounded;
            _edge.type = Image.Type.Sliced;
            _edge.raycastTarget = false;

            RectTransform bgRt = LabUIKit.NewRect(_root, "Bg");
            LabUIKit.Stretch(bgRt, 2f, 2f, 2f, 2f);
            _bg = bgRt.gameObject.AddComponent<Image>();
            _bg.sprite = LabUIKit.Rounded;
            _bg.type = Image.Type.Sliced;
            _bg.color = new Color(0.05f, 0.07f, 0.11f, 0.92f);
            _bg.raycastTarget = false;

            _system = LabUIKit.Label(_root, "System", "", LabUIKit.FsSmall, LabUIKit.TextDim, TextAnchor.UpperCenter);
            LabUIKit.TopLeft(_system.rectTransform, 6f, -3f, Width - 12f, 22f);

            _name = LabUIKit.Label(_root, "Name", ctx.displayName, LabUIKit.FsSmall, LabUIKit.TextMain,
                TextAnchor.UpperCenter);
            LabUIKit.TopLeft(_name.rectTransform, 6f, -25f, Width - 12f, 22f);

            _label = LabUIKit.Label(_root, "Label", "-", LabUIKit.FsHuge, Color.white,
                TextAnchor.UpperCenter, FontStyle.Bold);
            LabUIKit.TopLeft(_label.rectTransform, 6f, -45f, Width - 12f, 42f);

            _hpFill = LabUIKit.Bar(_root, "Hp", 12f, -88f, Width - 24f, 8f,
                new Color(0.18f, 0.20f, 0.26f, 1f), LabUIKit.Good);
        }

        public void Refresh(Camera cam, bool show, AIContext selected)
        {
            if (_ctx == null || cam == null)
            {
                _root.gameObject.SetActive(false);
                return;
            }

            Vector3 world = _ctx.transform.position + new Vector3(0f, 0.9f, 0f);
            Vector3 sp = cam.WorldToScreenPoint(world);

            bool onScreen = sp.z > 0f && sp.x > -Width && sp.x < Screen.width + Width &&
                            sp.y > -Height && sp.y < Screen.height + Height;

            _root.gameObject.SetActive(show && onScreen);
            if (!show || !onScreen)
                return;

            // Screen Space Overlay 캔버스는 화면 픽셀이 곧 좌표이므로 그대로 놓으면 된다.
            _root.position = new Vector3(sp.x, sp.y, 0f);

            IAIBrain brain = _ctx.Brain;
            string label = brain != null ? brain.CurrentLabel : "-";
            Color c = LabUIKit.LabelColor(label);

            _label.text = label;
            _label.color = c;
            _edge.color = selected == _ctx ? LabUIKit.Accent : c * 0.65f;
            _system.text = brain != null ? brain.SystemName : "";
            _name.text = _ctx.displayName;

            _hpFill.fillAmount = _ctx.HealthRatio;
            _hpFill.color = _ctx.HealthRatio <= _ctx.lowHealthRatio ? LabUIKit.Bad : LabUIKit.Good;
        }
    }

    /// <summary>학습 구역 표지. 어느 STEP 의 구역인지 Game View 에 떠 있게 한다.</summary>
    public class WorldSignLabel
    {
        private readonly LabZoneSign _sign;
        private readonly RectTransform _root;

        public WorldSignLabel(Transform parent, LabZoneSign sign)
        {
            _sign = sign;

            _root = LabUIKit.NewRect(parent, "Sign_" + sign.gameObject.name);
            _root.sizeDelta = new Vector2(420f, 58f);
            _root.pivot = new Vector2(0.5f, 0f);
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.zero;

            RectTransform bar = LabUIKit.Panel(_root, "Bar", new Color(0.05f, 0.07f, 0.11f, 0.75f));
            LabUIKit.Stretch(bar, 0f, 0f, 0f, 0f);

            Text title = LabUIKit.Label(_root, "Title", sign.title, LabUIKit.FsHead, sign.color,
                TextAnchor.UpperCenter, FontStyle.Bold);
            LabUIKit.TopLeft(title.rectTransform, 6f, -4f, 408f, 30f);

            Text sub = LabUIKit.Label(_root, "Sub", sign.subtitle, LabUIKit.FsSmall, LabUIKit.TextDim,
                TextAnchor.UpperCenter);
            LabUIKit.TopLeft(sub.rectTransform, 6f, -33f, 408f, 24f);
        }

        public void Refresh(Camera cam, bool show)
        {
            if (_sign == null || cam == null)
            {
                _root.gameObject.SetActive(false);
                return;
            }

            Vector3 sp = cam.WorldToScreenPoint(_sign.transform.position);
            bool onScreen = sp.z > 0f && sp.x > -420f && sp.x < Screen.width + 420f;

            _root.gameObject.SetActive(show && onScreen);
            if (show && onScreen)
                _root.position = new Vector3(sp.x, sp.y, 0f);
        }
    }
}
