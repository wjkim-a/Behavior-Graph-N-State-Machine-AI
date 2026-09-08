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
        // 글자 크기 --------------------------------------------------
        //
        // 다섯 단계만 쓴다. 새 크기가 필요해 보이면 이 중 하나를 고른다. 늘리지 않는다.
        //
        // 왜 제한하는가
        //   OS 폰트로 만든 다이나믹 폰트는 (문자 x 크기 x 스타일) 조합마다 글리프를
        //   아틀라스 텍스처에 구워 넣는다. 한글은 쓰는 문자가 많아서, 크기 종류가
        //   늘어나면 아틀라스가 텍스처 상한에 부딪히고 계속 다시 구워진다.
        //   그러면 이미 화면에 그려진 글자의 UV 가 어긋나 글자가 깨져 보인다.
        //
        //   이 학습장의 전체 문자(409자)를 기준으로 실측한 값:
        //     크기 13종  ->  2048 x 4096   (상한. 계속 다시 구워지고 깨진다)
        //     크기  6종  ->  2048 x 2048
        //     크기  5종  ->  1024 x 2048   (여유 있음. 지금 이 사다리)
        public const int FsSmall = 16;   // 각주 / 조작 안내 / 로그 / 상자 부제
        public const int FsBody = 18;    // 본문 / 보조 본문
        public const int FsHead = 21;    // 문단 제목 / 트리 Action 제목
        public const int FsTitle = 24;   // 페이지 제목 / 다이어그램 제목 / 상태 상자
        public const int FsHuge = 30;    // Enemy 머리 위 상태 라벨

        /// <summary>위의 다섯 단계. 아틀라스를 미리 구울 때 쓴다.</summary>
        public static readonly int[] Ladder = { FsSmall, FsBody, FsHead, FsTitle, FsHuge };

        // 색 팔레트 --------------------------------------------------
        // 학습 패널은 완전히 불투명해야 한다.
        // 패널 위쪽 164px 이 지면 타일과 겹치기 때문이다. (카메라를 내려 Game View 를
        // 넓게 쓰면서 겹치는 구간이 32px -> 164px 로 늘어났다)
        // 조금이라도 투명하면 타일 무늬가 글자 뒤로 비쳐서 읽기 어려워진다.
        public static readonly Color PanelBg = new Color(0.07f, 0.09f, 0.13f, 1f);
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

        /// <summary>
        /// 텍스트 한 줄이 차지하는 높이의 배수.
        ///
        /// Unity UI 의 Text 는 (폰트 line height) x (lineSpacing) 만큼 줄을 내린다.
        /// 맑은 고딕은 em 당 약 1.33, 여기서 쓰는 lineSpacing 은 1.15 이므로 1.53 이다.
        /// </summary>
        private const float LineFactor = 1.53f;

        /// <summary>
        /// 텍스트 lines 줄이 차지하는 높이(px).
        ///
        /// 문단 상자의 높이를 눈대중으로 정하면 안 된다. Text 는 상자를 넘겨서라도
        /// 다 그리기 때문에(VerticalWrapMode.Overflow), 상자가 작으면 글자가 아래로
        /// 흘러내려 다음 문단 위에 겹쳐 찍힌다. 문단 높이는 항상 이 함수로 잡는다.
        /// </summary>
        public static float LineBlock(int lines, int fontSize)
        {
            return Mathf.Ceil(lines * fontSize * LineFactor);
        }

        /// <summary>
        /// 상자를 넘어가는 부분을 그리지 않게 한다.
        ///
        /// 내용 길이가 실행 중에 변하는 문단(모니터 / Blackboard / 미션 목록)에 쓴다.
        /// 길이를 미리 셀 수 없으니, 넘칠 때는 잘리는 편이 아래 문단과 겹치는 것보다 낫다.
        /// </summary>
        public static Text Clip(Text t)
        {
            t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }

        /// <summary>
        /// 앞으로 화면에 나올 글자를 폰트 아틀라스에 미리 다 구워 둔다.
        ///
        /// 이렇게 하지 않으면 글자가 처음 나올 때마다 아틀라스가 조금씩 커지고,
        /// 커질 때마다 이미 그려진 글자의 좌표가 어긋나 한동안 깨져 보인다.
        /// 시작할 때 최종 크기까지 한 번에 키워 두면 실행 중에는 다시 구울 일이 없다.
        /// </summary>
        public static void Warm(string characters)
        {
            if (string.IsNullOrEmpty(characters))
                return;

            Font f = Font;
            for (int i = 0; i < Ladder.Length; i++)
            {
                f.RequestCharactersInTexture(characters, Ladder[i], FontStyle.Normal);
                f.RequestCharactersInTexture(characters, Ladder[i], FontStyle.Bold);
            }
        }

        public static Text Label(Transform parent, string name, string content, int size, Color color,
            TextAnchor anchor = TextAnchor.UpperLeft, FontStyle style = FontStyle.Normal)
        {
#if UNITY_EDITOR
            // 사다리에 없는 크기를 쓰면 아틀라스가 커져 글자가 깨질 수 있다.
            if (System.Array.IndexOf(Ladder, size) < 0)
            {
                Debug.LogWarning("[Lab UI] 글자 크기 " + size + " 는 사다리에 없다. " +
                                 "LabUIKit.Fs* 중에서 골라 쓰자. (" + name + ")");
            }
#endif
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
