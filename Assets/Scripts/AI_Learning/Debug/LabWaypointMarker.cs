using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// 순찰 지점을 Game View 에 작은 점으로 표시한다.
    /// "AI 가 저 두 점 사이를 왕복하는 것" 을 눈으로 확인하기 위한 표시다.
    /// </summary>
    public class LabWaypointMarker : MonoBehaviour
    {
        public Color color = new Color(0.42f, 0.82f, 0.55f, 0.75f);
        public float size = 0.22f;

        private static Sprite _dotSprite;

        private void Start()
        {
            GameObject go = new GameObject("Dot");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one * size;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = DotSprite();
            sr.color = color;
            sr.sortingOrder = -2;
        }

        private static Sprite DotSprite()
        {
            if (_dotSprite != null)
                return _dotSprite;

            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            Color[] px = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float a = d <= size * 0.5f - 1f ? 1f : Mathf.Clamp01(size * 0.5f - d);
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            _dotSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            _dotSprite.hideFlags = HideFlags.HideAndDontSave;
            return _dotSprite;
        }
    }
}
