using UnityEngine;
using UnityEngine.UI;

namespace CFramework.Guide
{
    /// <summary>
    ///     基于 UI 的引导遮罩，可在遮罩中挖出指定 RectTransform 对应的高亮区域，
    ///     高亮区域点击可穿透到底层 UI，遮罩区域会拦截输入。
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class GuideMask : MaskableGraphic
    {


        private static readonly Vector3[] TargetCorners = new Vector3[4];
        [SerializeField]
        private RectTransform highlightTarget;

        [SerializeField]
        private Vector2 highlightPadding = new Vector2(16f, 16f);

        [SerializeField]
        private bool continuousUpdate = true;

        [SerializeField]
        private bool hideMaskWhenNoTarget;

        [SerializeField]
        private float cornerRadius;

        [SerializeField, Range(1, 32)]
        private int cornerSegments = 8;
        private float effectiveCornerRadius;
        private bool hasValidHighlight;
        private Rect highlightRect;


        public RectTransform HighlightTarget => highlightTarget;

        public Vector2 HighlightPadding
        {
            get => highlightPadding;
            set
            {
                highlightPadding = value;
                UpdateHighlightRect(true);
            }
        }

        public bool ContinuousUpdate
        {
            get => continuousUpdate;
            set => continuousUpdate = value;
        }

        public float CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Mathf.Max(0f, value);
                UpdateEffectiveCornerRadius();
                SetVerticesDirty();
            }
        }

        public int CornerSegments
        {
            get => cornerSegments;
            set
            {
                cornerSegments = Mathf.Clamp(value, 1, 32);
                SetVerticesDirty();
            }
        }

        private void LateUpdate()
        {
            if(continuousUpdate)
            {
                UpdateHighlightRect(false);
            }
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateHighlightRect(true);
        }

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            UpdateHighlightRect(true);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            UpdateHighlightRect(true);
        }


        public void SetHighlightTarget(RectTransform target, bool forceRefresh = true)
        {
            highlightTarget = target;
            UpdateHighlightRect(forceRefresh);
        }


        public void ClearHighlightTarget()
        {
            highlightTarget = null;
            hasValidHighlight = false;
            effectiveCornerRadius = 0f;
            SetVerticesDirty();
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            if(!raycastTarget)
            {
                return false;
            }

            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out Vector2 localPoint))
            {
                return false;
            }

            if(hasValidHighlight && IsPointInsideHighlight(localPoint))
            {
                return false;
            }

            return true;
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if(hideMaskWhenNoTarget && !hasValidHighlight)
            {
                return;
            }

            Rect rect = rectTransform.rect;

            if(!hasValidHighlight)
            {
                AddQuad(vh, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMax));
                return;
            }

            DrawRectangularMask(vh, rect, highlightRect);

            if(effectiveCornerRadius > 0f)
            {
                DrawRoundedCornerMasks(vh, highlightRect, effectiveCornerRadius);
            }
        }


        private void DrawRectangularMask(VertexHelper vh, Rect fullRect, Rect holeRect)
        {
            if(holeRect.yMax < fullRect.yMax)
            {
                AddQuad(vh,
                    new Vector2(fullRect.xMin, holeRect.yMax),
                    new Vector2(fullRect.xMax, fullRect.yMax));
            }

            if(holeRect.yMin > fullRect.yMin)
            {
                AddQuad(vh,
                    new Vector2(fullRect.xMin, fullRect.yMin),
                    new Vector2(fullRect.xMax, holeRect.yMin));
            }

            if(holeRect.xMin > fullRect.xMin)
            {
                AddQuad(vh,
                    new Vector2(fullRect.xMin, holeRect.yMin),
                    new Vector2(holeRect.xMin, holeRect.yMax));
            }

            if(holeRect.xMax < fullRect.xMax)
            {
                AddQuad(vh,
                    new Vector2(holeRect.xMax, holeRect.yMin),
                    new Vector2(fullRect.xMax, holeRect.yMax));
            }
        }


        private void AddQuad(VertexHelper vh, Vector2 min, Vector2 max)
        {
            if(min.x >= max.x || min.y >= max.y)
            {
                return;
            }

            int startIndex = vh.currentVertCount;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = new Vector3(min.x, min.y);
            vh.AddVert(vertex);

            vertex.position = new Vector3(min.x, max.y);
            vh.AddVert(vertex);

            vertex.position = new Vector3(max.x, max.y);
            vh.AddVert(vertex);

            vertex.position = new Vector3(max.x, min.y);
            vh.AddVert(vertex);

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

        private void DrawRoundedCornerMasks(VertexHelper vh, Rect holeRect, float radius)
        {
            int segments = Mathf.Max(1, cornerSegments);

            Vector2 topLeftCorner = new Vector2(holeRect.xMin, holeRect.yMax);
            Vector2 topRightCorner = new Vector2(holeRect.xMax, holeRect.yMax);
            Vector2 bottomRightCorner = new Vector2(holeRect.xMax, holeRect.yMin);
            Vector2 bottomLeftCorner = new Vector2(holeRect.xMin, holeRect.yMin);

            Vector2 topLeftCenter = new Vector2(holeRect.xMin + radius, holeRect.yMax - radius);
            Vector2 topRightCenter = new Vector2(holeRect.xMax - radius, holeRect.yMax - radius);
            Vector2 bottomRightCenter = new Vector2(holeRect.xMax - radius, holeRect.yMin + radius);
            Vector2 bottomLeftCenter = new Vector2(holeRect.xMin + radius, holeRect.yMin + radius);

            AddCornerFan(vh, topLeftCorner, topLeftCenter, Mathf.PI, Mathf.PI / 2f, segments, radius);
            AddCornerFan(vh, topRightCorner, topRightCenter, Mathf.PI / 2f, 0f, segments, radius);
            AddCornerFan(vh, bottomRightCorner, bottomRightCenter, 0f, -Mathf.PI / 2f, segments, radius);
            AddCornerFan(vh, bottomLeftCorner, bottomLeftCenter, -Mathf.PI / 2f, -Mathf.PI, segments, radius);
        }

        private void AddCornerFan(VertexHelper vh, Vector2 corner, Vector2 center, float startAngle, float endAngle, int segments, float radius)
        {
            int startIndex = vh.currentVertCount;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = corner;
            vh.AddVert(vertex);

            for(var i = 0; i <= segments; i++)
            {
                float t = segments == 0 ? 0f : (float)i / segments;
                float angle = Mathf.Lerp(startAngle, endAngle, t);
                vertex.position = new Vector2(
                    center.x + Mathf.Cos(angle) * radius,
                    center.y + Mathf.Sin(angle) * radius);
                vh.AddVert(vertex);

                if(i > 0)
                {
                    vh.AddTriangle(startIndex, startIndex + i, startIndex + i + 1);
                }
            }
        }

        private void UpdateHighlightRect(bool forceRefresh)
        {
            bool updated = TryCalculateHighlightRect();
            UpdateEffectiveCornerRadius();

            if(!updated && !forceRefresh)
            {
                return;
            }

            SetVerticesDirty();
        }

        private void UpdateEffectiveCornerRadius()
        {
            if(!hasValidHighlight)
            {
                effectiveCornerRadius = 0f;
                return;
            }

            float maxRadius = Mathf.Min(highlightRect.width, highlightRect.height) * 0.5f;
            effectiveCornerRadius = Mathf.Clamp(cornerRadius, 0f, maxRadius);
        }

        private bool TryCalculateHighlightRect()

        {
            if(highlightTarget == null || !highlightTarget.gameObject.activeInHierarchy)
            {
                SetHasValidHighlight(false);
                return false;
            }

            highlightTarget.GetWorldCorners(TargetCorners);

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            for(var i = 0; i < TargetCorners.Length; i++)
            {
                Vector3 localCorner = rectTransform.InverseTransformPoint(TargetCorners[i]);
                min = Vector2.Min(min, localCorner);
                max = Vector2.Max(max, localCorner);
            }

            min -= highlightPadding;
            max += highlightPadding;

            Rect rootRect = rectTransform.rect;
            min.x = Mathf.Max(rootRect.xMin, min.x);
            min.y = Mathf.Max(rootRect.yMin, min.y);
            max.x = Mathf.Min(rootRect.xMax, max.x);
            max.y = Mathf.Min(rootRect.yMax, max.y);

            if(max.x <= min.x || max.y <= min.y)
            {
                SetHasValidHighlight(false);
                return false;
            }

            Rect newRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

            if(hasValidHighlight && ApproximatelyRectEquals(highlightRect, newRect))
            {
                return false;
            }

            highlightRect = newRect;
            SetHasValidHighlight(true);
            return true;
        }

        private bool IsPointInsideHighlight(Vector2 localPoint)
        {
            if(!highlightRect.Contains(localPoint))
            {
                return false;
            }

            if(effectiveCornerRadius <= 0f)
            {
                return true;
            }

            return IsPointInsideRoundedRect(localPoint, highlightRect, effectiveCornerRadius);
        }

        private static bool IsPointInsideRoundedRect(Vector2 point, Rect rect, float radius)
        {
            if(radius <= 0f)
            {
                return rect.Contains(point);
            }

            float clampedRadius = Mathf.Min(radius, Mathf.Min(rect.width, rect.height) * 0.5f);
            float innerXMin = rect.xMin + clampedRadius;
            float innerXMax = rect.xMax - clampedRadius;
            float innerYMin = rect.yMin + clampedRadius;
            float innerYMax = rect.yMax - clampedRadius;

            if(point.x >= innerXMin && point.x <= innerXMax)
            {
                return true;
            }

            if(point.y >= innerYMin && point.y <= innerYMax)
            {
                return true;
            }

            float centerX = point.x < rect.center.x ? innerXMin : innerXMax;
            float centerY = point.y < rect.center.y ? innerYMin : innerYMax;
            float dx = point.x - centerX;
            float dy = point.y - centerY;
            return dx * dx + dy * dy <= clampedRadius * clampedRadius;
        }

        private void SetHasValidHighlight(bool isValid)
        {
            hasValidHighlight = isValid;

            if(!isValid)
            {
                effectiveCornerRadius = 0f;
            }

            canvasRenderer.cull = hideMaskWhenNoTarget && !isValid;
        }



        private static bool ApproximatelyRectEquals(Rect lhs, Rect rhs)
        {
            return Mathf.Approximately(lhs.xMin, rhs.xMin) &&
                   Mathf.Approximately(lhs.xMax, rhs.xMax) &&
                   Mathf.Approximately(lhs.yMin, rhs.yMin) &&
                   Mathf.Approximately(lhs.yMax, rhs.yMax);
        }
    }
}