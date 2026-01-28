using UnityEngine;

namespace CFramework.Extensions.VectorExtensions
{
    /// <summary>
    ///     向量数学便捷拓展：加减乘除（向量/标量/分量）、分量替换、Clamp、Abs/Floor/Ceil/Round、
    ///     近似判断、逐分量 Min/Max、归一化安全版本等。所有方法均返回新值，不修改原向量。
    /// </summary>
    public static class VectorMathExtensions
    {
        #region Vector2

        // 加法
        public static Vector2 Add(this Vector2 v, Vector2 o)
        {
            return v + o;
        }
        public static Vector2 Add(this Vector2 v, float scalar)
        {
            return new Vector2(v.x + scalar, v.y + scalar);
        }
        public static Vector2 Add(this Vector2 v, float x, float y)
        {
            return new Vector2(v.x + x, v.y + y);
        }
        public static Vector2 AddX(this Vector2 v, float x)
        {
            return new Vector2(v.x + x, v.y);
        }
        public static Vector2 AddY(this Vector2 v, float y)
        {
            return new Vector2(v.x, v.y + y);
        }

        // 减法
        public static Vector2 Sub(this Vector2 v, Vector2 o)
        {
            return v - o;
        }
        public static Vector2 Sub(this Vector2 v, float scalar)
        {
            return new Vector2(v.x - scalar, v.y - scalar);
        }
        public static Vector2 Sub(this Vector2 v, float x, float y)
        {
            return new Vector2(v.x - x, v.y - y);
        }
        public static Vector2 SubX(this Vector2 v, float x)
        {
            return new Vector2(v.x - x, v.y);
        }
        public static Vector2 SubY(this Vector2 v, float y)
        {
            return new Vector2(v.x, v.y - y);
        }

        // 乘法（逐分量/标量）
        public static Vector2 Mul(this Vector2 v, Vector2 o)
        {
            return new Vector2(v.x * o.x, v.y * o.y);
        }
        public static Vector2 Mul(this Vector2 v, float scalar)
        {
            return v * scalar;
        }
        public static Vector2 Mul(this Vector2 v, float x, float y)
        {
            return new Vector2(v.x * x, v.y * y);
        }
        public static Vector2 MulX(this Vector2 v, float x)
        {
            return new Vector2(v.x * x, v.y);
        }
        public static Vector2 MulY(this Vector2 v, float y)
        {
            return new Vector2(v.x, v.y * y);
        }

        // 除法（逐分量/标量）
        public static Vector2 Div(this Vector2 v, Vector2 o)
        {
            return new Vector2(v.x / o.x, v.y / o.y);
        }
        public static Vector2 Div(this Vector2 v, float scalar)
        {
            return v / scalar;
        }
        public static Vector2 Div(this Vector2 v, float x, float y)
        {
            return new Vector2(v.x / x, v.y / y);
        }
        public static Vector2 DivX(this Vector2 v, float x)
        {
            return new Vector2(v.x / x, v.y);
        }
        public static Vector2 DivY(this Vector2 v, float y)
        {
            return new Vector2(v.x, v.y / y);
        }

        // 分量替换
        public static Vector2 WithX(this Vector2 v, float x)
        {
            return new Vector2(x, v.y);
        }
        public static Vector2 WithY(this Vector2 v, float y)
        {
            return new Vector2(v.x, y);
        }
        public static Vector2 With(this Vector2 v, float? x = null, float? y = null)
        {
            return new Vector2(x ?? v.x, y ?? v.y);
        }
        public static Vector2 SwapXY(this Vector2 v)
        {
            return new Vector2(v.y, v.x);
        }

        // Clamp（逐分量）
        public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max)
        {
            return new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));
        }
        public static Vector2 Clamp01Comp(this Vector2 v)
        {
            return new Vector2(Mathf.Clamp01(v.x), Mathf.Clamp01(v.y));
        }

        // 取绝对值/取整
        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }
        public static Vector2 Floor(this Vector2 v)
        {
            return new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));
        }
        public static Vector2 Ceil(this Vector2 v)
        {
            return new Vector2(Mathf.Ceil(v.x), Mathf.Ceil(v.y));
        }
        public static Vector2 Round(this Vector2 v)
        {
            return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
        }

        // 近似判断（基于距离）
        public static bool Approximately(this Vector2 a, Vector2 b, float epsilon = 1e-5f)
        {
            return (a - b).sqrMagnitude <= epsilon * epsilon;
        }

        // 逐分量最小/最大
        public static Vector2 ComponentMin(this Vector2 a, Vector2 b)
        {
            return new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        }
        public static Vector2 ComponentMax(this Vector2 a, Vector2 b)
        {
            return new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        // 归一化安全（零向量时返回原值或 zero）
        public static Vector2 NormalizedSafe(this Vector2 v, float epsilon = 1e-6f)
        {
            return v.sqrMagnitude > epsilon * epsilon ? v.normalized : Vector2.zero;
        }

        // 点积/投影便捷封装
        public static float Dot(this Vector2 a, Vector2 b)
        {
            return Vector2.Dot(a, b);
        }
        public static Vector2 ProjectOn(this Vector2 v, Vector2 onNormal)
        {
            float denom = onNormal.sqrMagnitude;
            if(denom < Mathf.Epsilon) return Vector2.zero;
            return Vector2.Dot(v, onNormal) / denom * onNormal;
        }
        public static Vector2 LerpTo(this Vector2 from, Vector2 to, float t)
        {
            return Vector2.Lerp(from, to, t);
        }

        #endregion

        #region Vector2Int

        public static Vector2Int Add(this Vector2Int v, Vector2Int o)
        {
            return v + o;
        }
        public static Vector2Int Add(this Vector2Int v, int scalar)
        {
            return new Vector2Int(v.x + scalar, v.y + scalar);
        }
        public static Vector2Int Add(this Vector2Int v, int x, int y)
        {
            return new Vector2Int(v.x + x, v.y + y);
        }
        public static Vector2Int AddX(this Vector2Int v, int x)
        {
            return new Vector2Int(v.x + x, v.y);
        }
        public static Vector2Int AddY(this Vector2Int v, int y)
        {
            return new Vector2Int(v.x, v.y + y);
        }

        public static Vector2Int Sub(this Vector2Int v, Vector2Int o)
        {
            return v - o;
        }
        public static Vector2Int Sub(this Vector2Int v, int scalar)
        {
            return new Vector2Int(v.x - scalar, v.y - scalar);
        }
        public static Vector2Int Sub(this Vector2Int v, int x, int y)
        {
            return new Vector2Int(v.x - x, v.y - y);
        }
        public static Vector2Int SubX(this Vector2Int v, int x)
        {
            return new Vector2Int(v.x - x, v.y);
        }
        public static Vector2Int SubY(this Vector2Int v, int y)
        {
            return new Vector2Int(v.x, v.y - y);
        }

        public static Vector2Int Mul(this Vector2Int v, Vector2Int o)
        {
            return new Vector2Int(v.x * o.x, v.y * o.y);
        }
        public static Vector2Int Mul(this Vector2Int v, int scalar)
        {
            return v * scalar;
        }
        public static Vector2Int Mul(this Vector2Int v, int x, int y)
        {
            return new Vector2Int(v.x * x, v.y * y);
        }
        public static Vector2Int MulX(this Vector2Int v, int x)
        {
            return new Vector2Int(v.x * x, v.y);
        }
        public static Vector2Int MulY(this Vector2Int v, int y)
        {
            return new Vector2Int(v.x, v.y * y);
        }

        public static Vector2Int Div(this Vector2Int v, Vector2Int o)
        {
            return new Vector2Int(v.x / o.x, v.y / o.y);
        }
        public static Vector2Int Div(this Vector2Int v, int scalar)
        {
            return v / scalar;
        }
        public static Vector2Int Div(this Vector2Int v, int x, int y)
        {
            return new Vector2Int(v.x / x, v.y / y);
        }
        public static Vector2Int DivX(this Vector2Int v, int x)
        {
            return new Vector2Int(v.x / x, v.y);
        }
        public static Vector2Int DivY(this Vector2Int v, int y)
        {
            return new Vector2Int(v.x, v.y / y);
        }

        public static Vector2Int WithX(this Vector2Int v, int x)
        {
            return new Vector2Int(x, v.y);
        }
        public static Vector2Int WithY(this Vector2Int v, int y)
        {
            return new Vector2Int(v.x, y);
        }
        public static Vector2Int With(this Vector2Int v, int? x = null, int? y = null)
        {
            return new Vector2Int(x ?? v.x, y ?? v.y);
        }
        public static Vector2Int SwapXY(this Vector2Int v)
        {
            return new Vector2Int(v.y, v.x);
        }

        public static Vector2Int Clamp(this Vector2Int v, Vector2Int min, Vector2Int max)
        {
            return new Vector2Int(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));
        }

        public static Vector2Int Abs(this Vector2Int v)
        {
            return new Vector2Int(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }
        public static Vector2Int ComponentMin(this Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        }
        public static Vector2Int ComponentMax(this Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        #endregion

        #region Vector3

        public static Vector3 Add(this Vector3 v, Vector3 o)
        {
            return v + o;
        }
        public static Vector3 Add(this Vector3 v, float scalar)
        {
            return new Vector3(v.x + scalar, v.y + scalar, v.z + scalar);
        }
        public static Vector3 Add(this Vector3 v, float x, float y, float z)
        {
            return new Vector3(v.x + x, v.y + y, v.z + z);
        }
        public static Vector3 AddX(this Vector3 v, float x)
        {
            return new Vector3(v.x + x, v.y, v.z);
        }
        public static Vector3 AddY(this Vector3 v, float y)
        {
            return new Vector3(v.x, v.y + y, v.z);
        }
        public static Vector3 AddZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, v.z + z);
        }

        public static Vector3 Sub(this Vector3 v, Vector3 o)
        {
            return v - o;
        }
        public static Vector3 Sub(this Vector3 v, float scalar)
        {
            return new Vector3(v.x - scalar, v.y - scalar, v.z - scalar);
        }
        public static Vector3 Sub(this Vector3 v, float x, float y, float z)
        {
            return new Vector3(v.x - x, v.y - y, v.z - z);
        }
        public static Vector3 SubX(this Vector3 v, float x)
        {
            return new Vector3(v.x - x, v.y, v.z);
        }
        public static Vector3 SubY(this Vector3 v, float y)
        {
            return new Vector3(v.x, v.y - y, v.z);
        }
        public static Vector3 SubZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, v.z - z);
        }

        public static Vector3 Mul(this Vector3 v, Vector3 o)
        {
            return new Vector3(v.x * o.x, v.y * o.y, v.z * o.z);
        }
        public static Vector3 Mul(this Vector3 v, float scalar)
        {
            return v * scalar;
        }
        public static Vector3 Mul(this Vector3 v, float x, float y, float z)
        {
            return new Vector3(v.x * x, v.y * y, v.z * z);
        }
        public static Vector3 MulX(this Vector3 v, float x)
        {
            return new Vector3(v.x * x, v.y, v.z);
        }
        public static Vector3 MulY(this Vector3 v, float y)
        {
            return new Vector3(v.x, v.y * y, v.z);
        }
        public static Vector3 MulZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, v.z * z);
        }

        public static Vector3 Div(this Vector3 v, Vector3 o)
        {
            return new Vector3(v.x / o.x, v.y / o.y, v.z / o.z);
        }
        public static Vector3 Div(this Vector3 v, float scalar)
        {
            return v / scalar;
        }
        public static Vector3 Div(this Vector3 v, float x, float y, float z)
        {
            return new Vector3(v.x / x, v.y / y, v.z / z);
        }
        public static Vector3 DivX(this Vector3 v, float x)
        {
            return new Vector3(v.x / x, v.y, v.z);
        }
        public static Vector3 DivY(this Vector3 v, float y)
        {
            return new Vector3(v.x, v.y / y, v.z);
        }
        public static Vector3 DivZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, v.z / z);
        }

        public static Vector3 WithX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }
        public static Vector3 WithY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }
        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }
        public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
        }

        public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z));
        }
        public static Vector3 Clamp01Comp(this Vector3 v)
        {
            return new Vector3(Mathf.Clamp01(v.x), Mathf.Clamp01(v.y), Mathf.Clamp01(v.z));
        }

        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
        public static Vector3 Floor(this Vector3 v)
        {
            return new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
        }
        public static Vector3 Ceil(this Vector3 v)
        {
            return new Vector3(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z));
        }
        public static Vector3 Round(this Vector3 v)
        {
            return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
        }

        public static bool Approximately(this Vector3 a, Vector3 b, float epsilon = 1e-5f)
        {
            return (a - b).sqrMagnitude <= epsilon * epsilon;
        }

        public static Vector3 ComponentMin(this Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        }
        public static Vector3 ComponentMax(this Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }

        public static Vector3 NormalizedSafe(this Vector3 v, float epsilon = 1e-6f)
        {
            return v.sqrMagnitude > epsilon * epsilon ? v.normalized : Vector3.zero;
        }

        // 经典运算便捷封装
        public static float Dot(this Vector3 a, Vector3 b)
        {
            return Vector3.Dot(a, b);
        }
        public static Vector3 Cross(this Vector3 a, Vector3 b)
        {
            return Vector3.Cross(a, b);
        }
        public static Vector3 ProjectOn(this Vector3 v, Vector3 onNormal)
        {
            return Vector3.Project(v, onNormal);
        }
        public static Vector3 LerpTo(this Vector3 from, Vector3 to, float t)
        {
            return Vector3.Lerp(from, to, t);
        }

        #endregion

        #region Vector4

        public static Vector4 Add(this Vector4 v, Vector4 o)
        {
            return v + o;
        }
        public static Vector4 Add(this Vector4 v, float scalar)
        {
            return new Vector4(v.x + scalar, v.y + scalar, v.z + scalar, v.w + scalar);
        }
        public static Vector4 Add(this Vector4 v, float x, float y, float z, float w)
        {
            return new Vector4(v.x + x, v.y + y, v.z + z, v.w + w);
        }
        public static Vector4 AddX(this Vector4 v, float x)
        {
            return new Vector4(v.x + x, v.y, v.z, v.w);
        }
        public static Vector4 AddY(this Vector4 v, float y)
        {
            return new Vector4(v.x, v.y + y, v.z, v.w);
        }
        public static Vector4 AddZ(this Vector4 v, float z)
        {
            return new Vector4(v.x, v.y, v.z + z, v.w);
        }
        public static Vector4 AddW(this Vector4 v, float w)
        {
            return new Vector4(v.x, v.y, v.z, v.w + w);
        }

        public static Vector4 Sub(this Vector4 v, Vector4 o)
        {
            return v - o;
        }
        public static Vector4 Sub(this Vector4 v, float scalar)
        {
            return new Vector4(v.x - scalar, v.y - scalar, v.z - scalar, v.w - scalar);
        }
        public static Vector4 Sub(this Vector4 v, float x, float y, float z, float w)
        {
            return new Vector4(v.x - x, v.y - y, v.z - z, v.w - w);
        }
        public static Vector4 SubX(this Vector4 v, float x)
        {
            return new Vector4(v.x - x, v.y, v.z, v.w);
        }
        public static Vector4 SubY(this Vector4 v, float y)
        {
            return new Vector4(v.x, v.y - y, v.z, v.w);
        }
        public static Vector4 SubZ(this Vector4 v, float z)
        {
            return new Vector4(v.x, v.y, v.z - z, v.w);
        }
        public static Vector4 SubW(this Vector4 v, float w)
        {
            return new Vector4(v.x, v.y, v.z, v.w - w);
        }

        public static Vector4 Mul(this Vector4 v, Vector4 o)
        {
            return new Vector4(v.x * o.x, v.y * o.y, v.z * o.z, v.w * o.w);
        }
        public static Vector4 Mul(this Vector4 v, float scalar)
        {
            return v * scalar;
        }
        public static Vector4 Mul(this Vector4 v, float x, float y, float z, float w)
        {
            return new Vector4(v.x * x, v.y * y, v.z * z, v.w * w);
        }
        public static Vector4 MulX(this Vector4 v, float x)
        {
            return new Vector4(v.x * x, v.y, v.z, v.w);
        }
        public static Vector4 MulY(this Vector4 v, float y)
        {
            return new Vector4(v.x, v.y * y, v.z, v.w);
        }
        public static Vector4 MulZ(this Vector4 v, float z)
        {
            return new Vector4(v.x, v.y, v.z * z, v.w);
        }
        public static Vector4 MulW(this Vector4 v, float w)
        {
            return new Vector4(v.x, v.y, v.z, v.w * w);
        }

        public static Vector4 Div(this Vector4 v, Vector4 o)
        {
            return new Vector4(v.x / o.x, v.y / o.y, v.z / o.z, v.w / o.w);
        }
        public static Vector4 Div(this Vector4 v, float scalar)
        {
            return v / scalar;
        }
        public static Vector4 Div(this Vector4 v, float x, float y, float z, float w)
        {
            return new Vector4(v.x / x, v.y / y, v.z / z, v.w / w);
        }
        public static Vector4 DivX(this Vector4 v, float x)
        {
            return new Vector4(v.x / x, v.y, v.z, v.w);
        }
        public static Vector4 DivY(this Vector4 v, float y)
        {
            return new Vector4(v.x, v.y / y, v.z, v.w);
        }
        public static Vector4 DivZ(this Vector4 v, float z)
        {
            return new Vector4(v.x, v.y, v.z / z, v.w);
        }
        public static Vector4 DivW(this Vector4 v, float w)
        {
            return new Vector4(v.x, v.y, v.z, v.w / w);
        }

        // 分量替换
        public static Vector4 WithX(this Vector4 v, float x)
        {
            return new Vector4(x, v.y, v.z, v.w);
        }
        public static Vector4 WithY(this Vector4 v, float y)
        {
            return new Vector4(v.x, y, v.z, v.w);
        }
        public static Vector4 WithZ(this Vector4 v, float z)
        {
            return new Vector4(v.x, v.y, z, v.w);
        }
        public static Vector4 WithW(this Vector4 v, float w)
        {
            return new Vector4(v.x, v.y, v.z, w);
        }
        public static Vector4 With(this Vector4 v, float? x = null, float? y = null, float? z = null, float? w = null)
        {
            return new Vector4(x ?? v.x, y ?? v.y, z ?? v.z, w ?? v.w);
        }

        // Clamp（逐分量）
        public static Vector4 Clamp(this Vector4 v, Vector4 min, Vector4 max)
        {
            return new Vector4(
                Mathf.Clamp(v.x, min.x, max.x),
                Mathf.Clamp(v.y, min.y, max.y),
                Mathf.Clamp(v.z, min.z, max.z),
                Mathf.Clamp(v.w, min.w, max.w)
            );
        }
        public static Vector4 Clamp01Comp(this Vector4 v)
        {
            return new Vector4(Mathf.Clamp01(v.x), Mathf.Clamp01(v.y), Mathf.Clamp01(v.z), Mathf.Clamp01(v.w));
        }

        // 取绝对值/取整
        public static Vector4 Abs(this Vector4 v)
        {
            return new Vector4(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z), Mathf.Abs(v.w));
        }
        public static Vector4 Floor(this Vector4 v)
        {
            return new Vector4(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z), Mathf.Floor(v.w));
        }
        public static Vector4 Ceil(this Vector4 v)
        {
            return new Vector4(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z), Mathf.Ceil(v.w));
        }
        public static Vector4 Round(this Vector4 v)
        {
            return new Vector4(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z), Mathf.Round(v.w));
        }

        // 近似判断（基于距离）
        public static bool Approximately(this Vector4 a, Vector4 b, float epsilon = 1e-5f)
        {
            return (a - b).sqrMagnitude <= epsilon * epsilon;
        }

        // 逐分量最小/最大
        public static Vector4 ComponentMin(this Vector4 a, Vector4 b)
        {
            return new Vector4(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z), Mathf.Min(a.w, b.w));
        }
        public static Vector4 ComponentMax(this Vector4 a, Vector4 b)
        {
            return new Vector4(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z), Mathf.Max(a.w, b.w));
        }

        // 归一化安全
        public static Vector4 NormalizedSafe(this Vector4 v, float epsilon = 1e-6f)
        {
            return v.sqrMagnitude > epsilon * epsilon ? v.normalized : Vector4.zero;
        }

        // 点积/插值
        public static float Dot(this Vector4 a, Vector4 b)
        {
            return Vector4.Dot(a, b);
        }
        public static Vector4 LerpTo(this Vector4 from, Vector4 to, float t)
        {
            return Vector4.Lerp(from, to, t);
        }

        #endregion

        #region Vector3Int

        public static Vector3Int Add(this Vector3Int v, Vector3Int o)
        {
            return v + o;
        }
        public static Vector3Int Add(this Vector3Int v, int scalar)
        {
            return new Vector3Int(v.x + scalar, v.y + scalar, v.z + scalar);
        }
        public static Vector3Int Add(this Vector3Int v, int x, int y, int z)
        {
            return new Vector3Int(v.x + x, v.y + y, v.z + z);
        }
        public static Vector3Int AddX(this Vector3Int v, int x)
        {
            return new Vector3Int(v.x + x, v.y, v.z);
        }
        public static Vector3Int AddY(this Vector3Int v, int y)
        {
            return new Vector3Int(v.x, v.y + y, v.z);
        }
        public static Vector3Int AddZ(this Vector3Int v, int z)
        {
            return new Vector3Int(v.x, v.y, v.z + z);
        }

        public static Vector3Int Sub(this Vector3Int v, Vector3Int o)
        {
            return v - o;
        }
        public static Vector3Int Sub(this Vector3Int v, int scalar)
        {
            return new Vector3Int(v.x - scalar, v.y - scalar, v.z - scalar);
        }
        public static Vector3Int Sub(this Vector3Int v, int x, int y, int z)
        {
            return new Vector3Int(v.x - x, v.y - y, v.z - z);
        }
        public static Vector3Int SubX(this Vector3Int v, int x)
        {
            return new Vector3Int(v.x - x, v.y, v.z);
        }
        public static Vector3Int SubY(this Vector3Int v, int y)
        {
            return new Vector3Int(v.x, v.y - y, v.z);
        }
        public static Vector3Int SubZ(this Vector3Int v, int z)
        {
            return new Vector3Int(v.x, v.y, v.z - z);
        }

        public static Vector3Int Mul(this Vector3Int v, Vector3Int o)
        {
            return new Vector3Int(v.x * o.x, v.y * o.y, v.z * o.z);
        }
        public static Vector3Int Mul(this Vector3Int v, int scalar)
        {
            return v * scalar;
        }
        public static Vector3Int Mul(this Vector3Int v, int x, int y, int z)
        {
            return new Vector3Int(v.x * x, v.y * y, v.z * z);
        }
        public static Vector3Int MulX(this Vector3Int v, int x)
        {
            return new Vector3Int(v.x * x, v.y, v.z);
        }
        public static Vector3Int MulY(this Vector3Int v, int y)
        {
            return new Vector3Int(v.x, v.y * y, v.z);
        }
        public static Vector3Int MulZ(this Vector3Int v, int z)
        {
            return new Vector3Int(v.x, v.y, v.z * z);
        }

        public static Vector3Int Div(this Vector3Int v, Vector3Int o)
        {
            return new Vector3Int(v.x / o.x, v.y / o.y, v.z / o.z);
        }
        public static Vector3Int Div(this Vector3Int v, int scalar)
        {
            return v / scalar;
        }
        public static Vector3Int Div(this Vector3Int v, int x, int y, int z)
        {
            return new Vector3Int(v.x / x, v.y / y, v.z / z);
        }
        public static Vector3Int DivX(this Vector3Int v, int x)
        {
            return new Vector3Int(v.x / x, v.y, v.z);
        }
        public static Vector3Int DivY(this Vector3Int v, int y)
        {
            return new Vector3Int(v.x, v.y / y, v.z);
        }
        public static Vector3Int DivZ(this Vector3Int v, int z)
        {
            return new Vector3Int(v.x, v.y, v.z / z);
        }

        public static Vector3Int WithX(this Vector3Int v, int x)
        {
            return new Vector3Int(x, v.y, v.z);
        }
        public static Vector3Int WithY(this Vector3Int v, int y)
        {
            return new Vector3Int(v.x, y, v.z);
        }
        public static Vector3Int WithZ(this Vector3Int v, int z)
        {
            return new Vector3Int(v.x, v.y, z);
        }
        public static Vector3Int With(this Vector3Int v, int? x = null, int? y = null, int? z = null)
        {
            return new Vector3Int(x ?? v.x, y ?? v.y, z ?? v.z);
        }

        public static Vector3Int Clamp(this Vector3Int v, Vector3Int min, Vector3Int max)
        {
            return new Vector3Int(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z));
        }

        public static Vector3Int Abs(this Vector3Int v)
        {
            return new Vector3Int(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
        public static Vector3Int ComponentMin(this Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        }
        public static Vector3Int ComponentMax(this Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }

        #endregion
    }
}