#if UNITY_5_3_OR_NEWER
using UnityEngine;
using UnityEngine.UI;

namespace UntitledGame.UnityPresentation
{
    /// <summary>
    /// Jagged lightning-style seam drawn along the split line (Juusei look).
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SplitSeamGraphic : MaskableGraphic
    {
        private const int Segments = 18;

        private float _topX = 0.58f;
        private float _botX = 0.42f;
        private float _width = 6f;
        private float _zigzag = 7f;

        public void Configure(float width, float zigzag)
        {
            _width = width;
            _zigzag = zigzag;
            SetVerticesDirty();
        }

        public void SetCut(float topX, float botX)
        {
            if (Mathf.Approximately(topX, _topX) && Mathf.Approximately(botX, _botX))
                return;
            _topX = topX;
            _botX = botX;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            var p0 = new Vector2(Mathf.Lerp(r.xMin, r.xMax, _botX), r.yMin - 4f);
            var p1 = new Vector2(Mathf.Lerp(r.xMin, r.xMax, _topX), r.yMax + 4f);
            var dir = (p1 - p0).normalized;
            var normal = new Vector2(-dir.y, dir.x);
            var half = _width * 0.5f;

            for (var i = 0; i <= Segments; i++)
            {
                var t = i / (float)Segments;
                var offset = (i == 0 || i == Segments) ? 0f : (i % 2 == 0 ? _zigzag : -_zigzag);
                var center = Vector2.Lerp(p0, p1, t) + normal * offset;
                vh.AddVert(center - normal * half, color, Vector2.zero);
                vh.AddVert(center + normal * half, color, Vector2.zero);
            }

            for (var i = 0; i < Segments; i++)
            {
                var a = i * 2;
                vh.AddTriangle(a, a + 1, a + 3);
                vh.AddTriangle(a, a + 3, a + 2);
            }
        }
    }
}
#endif
