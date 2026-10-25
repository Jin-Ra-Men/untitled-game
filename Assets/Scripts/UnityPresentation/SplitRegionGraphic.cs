#if UNITY_5_3_OR_NEWER
using UnityEngine;
using UnityEngine.UI;

namespace UntitledGame.UnityPresentation
{
    /// <summary>
    /// One side of the diagonal split, as real geometry. Used as a stencil Mask graphic,
    /// so moving the cut only re-uploads 4 vertices (no texture rewrite, no alpha fringe).
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SplitRegionGraphic : MaskableGraphic
    {
        private float _topX = 0.58f;
        private float _botX = 0.42f;
        private bool _keepLeft = true;

        public void SetCut(float topX, float botX, bool keepLeft)
        {
            if (Mathf.Approximately(topX, _topX) && Mathf.Approximately(botX, _botX) && keepLeft == _keepLeft)
                return;
            _topX = topX;
            _botX = botX;
            _keepLeft = keepLeft;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            var bl = new Vector2(r.xMin, r.yMin);
            var tl = new Vector2(r.xMin, r.yMax);
            var br = new Vector2(r.xMax, r.yMin);
            var tr = new Vector2(r.xMax, r.yMax);
            var cutBot = new Vector2(Mathf.Lerp(r.xMin, r.xMax, _botX), r.yMin);
            var cutTop = new Vector2(Mathf.Lerp(r.xMin, r.xMax, _topX), r.yMax);

            if (_keepLeft)
                AddQuad(vh, bl, tl, cutTop, cutBot);
            else
                AddQuad(vh, cutBot, cutTop, tr, br);
        }

        private void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var start = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddVert(d, color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
#endif
