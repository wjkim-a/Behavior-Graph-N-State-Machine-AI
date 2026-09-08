using UnityEngine;
using UnityEngine.UI;

namespace AILearning
{
    /// <summary>
    /// 학습용 UI 를 코드로 만들기 위한 작은 도우미 모음.
    ///
    /// 왜 코드로 UI 를 만드는가?
    ///   학습자가 Unity UI 를 직접 조립하는 것은 이 수업의 목표가 아니다.
    ///   씬만 열면 항상 같은 화면이 나오도록, UI 는 실행 시점에 자동으로 구성한다.
    /// </summary>
    public static class LabUIKit
    {
        // 색 팔레트 --------------------------------------------------
        // 학습 패널 영역은 지면 타일과 겹치므로 거의 불투명하게 둔다.
        // (반투명하면 타일 무늬가 글자 뒤로 비쳐 읽기 어려워진다.)
        public static readonly Color PanelBg = new Color(0.07f, 0.09f, 0.13f, 0.985f);
        public static readonly Color PanelBgSolid = new Color(0.05f, 0.07f, 0.10f, 1f);
        public static readonly Color PanelEdge = new Color(0.25f, 0.32f, 0.45f, 1f);
        public static readonly Color TextMain = new Color(0.90f, 0.93f, 0.97f, 1f);
        public static readonly Color TextDim = new Color(0.58f, 0.64f, 0.74f, 1f);
        public static readonly Color Accent = new Color(0.35f, 0.75f, 1.00f, 1f);
        public static readonly Color AccentWarm = new Color(1.00f, 0.72f, 0.28f, 1f);
        public static readonly Color Good = new Color(0.40f, 0.85f, 0.50f, 1f);
        public static readonly Color Bad = new Color(0.95f, 0.42f, 0.42f, 1f);
        public static readonly Color Idle = new Color(0.35f, 0.40f, 0.50f, 1f);

        /// <summary>상태 / 노드 이름별 대표 색. Game View 라벨과 다이어그램이 같은 색을 쓴다.</summary>
        public static Color LabelColor(string label)
        {
            if (string.IsNullOrEmpty(label)) return Idle;

            if (label.StartsWith("PATROL")) return new Color(0.42f, 0.82f, 0.55f);
            if (label.StartsWith("CHASE")) return new Color(1.00f, 0.70f, 0.25f);
            if (label.StartsWith("ATTACK")) return new Color(0.98f, 0.38f, 0.38f);
            if (label.StartsWith("FLEE")) return new Color(0.45f, 0.70f, 1.00f);
            if (label.StartsWith("DEAD")) return new Color(0.55f, 0.55f, 0.60f);
            if (label.StartsWith("LOOK")) return new Color(0.70f, 0.80f, 0.95f);
            if (label.StartsWith("IDLE")) return new Color(0.60f, 0.65f, 0.72f);
            if (label.StartsWith("MOVE")) return new Color(0.50f, 0.85f, 0.85f);
            if (label.StartsWith("WAIT")) return new Color(0.80f, 0.78f, 0.45f);
            return Accent;
        }

        // 폰트 ------------------------------------------------------
        private static Font _font;

        /// <summary>
        /// 한글이 표시되는 OS 폰트를 동적으로 만든다.
        /// (프로젝트에 폰트 파일을 복사해 넣지 않아도 한글이 정상 출력된다.)
        /// </summary>
        public static Font Font
        {
            get
            {
                if (_font == null)
                {
                    _font = UnityEngine.Font.CreateDynamicFontFromOSFont(
                        new[] { "Malgun Gothic", "맑은 고딕", "Noto Sans KR", "Segoe UI", "Arial" }, 18);
                }
                return _font;
            }
        }

        // 스프라이트 -------------------------------------------------
        private static Sprite _roundedSprite;
        private static Sprite _flatSprite;

        /// <summary>모서리가 둥근 9-slice 스프라이트를 절차적으로 만든다.</summary>
        public static Sprite Rounded
        {
            get
            {
                if (_roundedSprite == null)
                    _roundedSprite = BuildRounded(24, 8);
                return _roundedSprite;
            }
        }

        /// <summary>1x1 흰색 스프라이트. 선이나 채움에 사용.</summary>
        public static Sprite Flat
        {
            get
            {
                if (_flatSprite == null)
                {
                    Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                    Color[] px = new Color[16];
                    for (int i = 0; i < px.Length; i++) px[i] = Color.white;
                    tex.SetPixels(px);
                    tex.Apply();
                    tex.hideFlags = HideFlags.HideAndDontSave;
                    _flatSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
                    _flatSprite.hideFlags = HideFlags.HideAndDontSave;
                }
                return _flatSprite;
            }
        }

        private static Sprite BuildRounded(int size, int radius)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float a = 1f;

                    // 네 모서리에서만 원형으로 잘라낸다.
                    float cx = x < radius ? radius - 0.5f : (x >= size - radius ? size - radius - 0.5f : x);
                    float cy = y < radius ? radius - 0.5f : (y >= size - radius ? size - radius - 0.5f : y);
                    float dx = x - cx;
                    float dy = y - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    if (d > radius)
                        a = Mathf.Clamp01(1f - (d - radius));

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }

            tex.Apply();
            Sprite s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            s.hideFlags = HideFlags.HideAndDontSave;
            return s;
        }

        // 생성 도우미 ------------------------------------------------

        public static RectTransform NewRect(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>사각 배경이 있는 패널을 만든다.</summary>
        public static RectTransform Panel(Transform parent, string name, Color bg, bool rounded = true)
        {
            RectTransform rt = NewRect(parent, name);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = rounded ? Rounded : Flat;
            img.type = rounded ? Image.Type.Sliced : Image.Type.Simple;
            img.color = bg;
            img.raycastTarget = false;
            return rt;
        }

        /// <summary>테두리만 있는 상자를 만든다. (다이어그램 노드 강조에 사용)</summary>
        public static Image Border(Transform parent, string name, Color color, float thickness = 2f)
        {
            RectTransform rt = NewRect(parent, name);
            Stretch(rt, -thickness, -thickness, thickness, thickness);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = Rounded;
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
            rt.SetAsFirstSibling();
            return img;
        }

        public static Text Label(Transform parent, string name, string content, int size, Color color,
            TextAnchor anchor = TextAnchor.UpperLeft, FontStyle style = FontStyle.Normal)
        {
            RectTransform rt = NewRect(parent, name);
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.text = content;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.lineSpacing = 1.15f;
            t.raycastTarget = false;
            t.supportRichText = true;
            return t;
        }

        // 배치 도우미 ------------------------------------------------

        /// <summary>부모 전체를 채우도록 늘린다. (여백 지정 가능)</summary>
        public static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>왼쪽 위를 기준으로 위치와 크기를 지정한다. (y 는 아래로 갈수록 음수)</summary>
        public static void TopLeft(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);
        }

        /// <summary>오른쪽 위 기준 배치. x 는 오른쪽 끝에서의 거리.</summary>
        public static void TopRight(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-x, y);
            rt.sizeDelta = new Vector2(width, height);
        }

        /// <summary>왼쪽 아래 기준 배치. y 는 아래쪽 끝에서의 거리.</summary>
        public static void BottomLeft(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);
        }

        /// <summary>오른쪽 아래 기준 배치. x 는 오른쪽 끝, y 는 아래쪽 끝에서의 거리.</summary>
        public static void BottomRight(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-x, y);
            rt.sizeDelta = new Vector2(width, height);
        }

        /// <summary>가로로 꽉 채우고 위쪽에 붙인다. topOffset 은 화면 위에서의 거리(양수).</summary>
        public static void StretchTop(RectTransform rt, float margin, float topOffset, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(margin, -topOffset - height);
            rt.offsetMax = new Vector2(-margin, -topOffset);
        }

        /// <summary>가로로 꽉 채우고 아래쪽에 붙인다. bottomOffset 은 화면 아래에서의 거리(양수).</summary>
        public static void StretchBottom(RectTransform rt, float margin, float bottomOffset, float height)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(margin, bottomOffset);
            rt.offsetMax = new Vector2(-margin, bottomOffset + height);
        }

        /// <summary>화면 중앙 기준 배치.</summary>
        public static void Center(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);
        }

        /// <summary>가로 선을 그린다.</summary>
        public static Image HLine(Transform parent, float x, float y, float width, Color color, float thickness = 2f)
        {
            RectTransform rt = NewRect(parent, "HLine");
            TopLeft(rt, x, y, width, thickness);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = Flat;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>세로 선을 그린다.</summary>
        public static Image VLine(Transform parent, float x, float y, float height, Color color, float thickness = 2f)
        {
            RectTransform rt = NewRect(parent, "VLine");
            TopLeft(rt, x, y, thickness, height);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = Flat;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>진행 막대(체력 등)를 만든다. 반환값의 fillAmount 를 조절한다.</summary>
        public static Image Bar(Transform parent, string name, float x, float y, float width, float height,
            Color backColor, Color fillColor)
        {
            RectTransform back = Panel(parent, name, backColor);
            TopLeft(back, x, y, width, height);

            RectTransform fillRt = NewRect(back, "Fill");
            Stretch(fillRt, 2f, 2f, 2f, 2f);
            Image fill = fillRt.gameObject.AddComponent<Image>();
            fill.sprite = Rounded;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.color = fillColor;
            fill.raycastTarget = false;
            return fill;
        }
    }
}
