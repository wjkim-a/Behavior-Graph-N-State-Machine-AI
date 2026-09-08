using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// 감지 범위와 공격 사거리를 Game View 에 원으로 그린다.
    ///
    /// 왜 필요한가?
    ///   "왜 지금 상태가 바뀌었지?" 라는 질문에 가장 빠른 답은
    ///   조건의 경계선을 눈으로 보는 것이다.
    ///   V 키로 켜고 끌 수 있다.
    /// </summary>
    [RequireComponent(typeof(AIContext))]
    public class AIRangeRing : MonoBehaviour
    {
        private AIContext _ctx;
        private LabDirector _director;

        private SpriteRenderer _detectRing;
        private SpriteRenderer _attackRing;

        private static Sprite _ringSprite;

        private void Awake()
        {
            _ctx = GetComponent<AIContext>();

            _detectRing = CreateRing("DetectRing", new Color(0.35f, 0.75f, 1f, 0.42f), -1);
            _attackRing = CreateRing("AttackRing", new Color(0.98f, 0.38f, 0.38f, 0.55f), 0);
        }

        private void Start()
        {
            _director = FindFirstObjectByType<LabDirector>();
        }

        private SpriteRenderer CreateRing(string name, Color color, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RingSprite();
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        /// <summary>가운데가 비어 있는 원(고리) 스프라이트를 절차적으로 만든다.</summary>
        private static Sprite RingSprite()
        {
            if (_ringSprite != null)
                return _ringSprite;

            const int size = 256;
            const float outer = 127f;
            const float inner = 122f;

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;

            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float a = 0f;

                    if (d <= outer && d >= inner)
                        a = 1f;
                    else if (d < inner && d > inner - 1.5f)
                        a = Mathf.InverseLerp(inner - 1.5f, inner, d);
                    else if (d > outer && d < outer + 1.5f)
                        a = 1f - Mathf.InverseLerp(outer, outer + 1.5f, d);

                    pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            // pixelsPerUnit 을 지름과 같게 두면 localScale 1 이 곧 지름 1 이 된다.
            _ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            _ringSprite.hideFlags = HideFlags.HideAndDontSave;
            return _ringSprite;
        }

        private void LateUpdate()
        {
            bool show = _director != null && _director.IsOpen(LabPanel.Ranges) && !_ctx.IsDead;

            _detectRing.enabled = show;
            _attackRing.enabled = show;

            if (!show)
                return;

            // 지름 = 반지름 * 2
            _detectRing.transform.localScale = Vector3.one * (_ctx.detectRange * 2f);
            _attackRing.transform.localScale = Vector3.one * (_ctx.attackRange * 2f);

            bool detected = _ctx.PlayerDetected;
            _detectRing.color = detected
                ? new Color(1f, 0.72f, 0.28f, 0.75f)
                : new Color(0.35f, 0.75f, 1f, 0.40f);

            _attackRing.color = _ctx.InAttackRange
                ? new Color(1f, 0.30f, 0.30f, 0.90f)
                : new Color(0.98f, 0.38f, 0.38f, 0.45f);
        }
    }
}
