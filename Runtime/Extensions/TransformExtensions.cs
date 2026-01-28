using System.Collections.Generic;
using UnityEngine;

namespace CFramework.Extensions
{
    /// <summary>
    ///     对 <see cref="Transform" /> 的常用拓展：快速设置/叠加位置旋转缩放、重置、
    ///     层级遍历、深度查找、子节点清理等。
    /// </summary>
    public static class TransformExtensions
    {
        /* --------------------------------------------------------------------
         * 1️⃣ 快速设置 / 叠加
         * ------------------------------------------------------------------ */

        public static void SetPositionX(this Transform t, float x)
        {
            if(t == null) return;
            Vector3 p = t.position;
            p.x = x;
            t.position = p;
        }

        public static void SetPositionY(this Transform t, float y)
        {
            if(t == null) return;
            Vector3 p = t.position;
            p.y = y;
            t.position = p;
        }

        public static void SetPositionZ(this Transform t, float z)
        {
            if(t == null) return;
            Vector3 p = t.position;
            p.z = z;
            t.position = p;
        }

        public static void SetLocalPositionX(this Transform t, float x)
        {
            if(t == null) return;
            Vector3 p = t.localPosition;
            p.x = x;
            t.localPosition = p;
        }

        public static void SetLocalPositionY(this Transform t, float y)
        {
            if(t == null) return;
            Vector3 p = t.localPosition;
            p.y = y;
            t.localPosition = p;
        }

        public static void SetLocalPositionZ(this Transform t, float z)
        {
            if(t == null) return;
            Vector3 p = t.localPosition;
            p.z = z;
            t.localPosition = p;
        }

        public static void SetLocalScaleX(this Transform t, float x)
        {
            if(t == null) return;
            Vector3 s = t.localScale;
            s.x = x;
            t.localScale = s;
        }

        public static void SetLocalScaleY(this Transform t, float y)
        {
            if(t == null) return;
            Vector3 s = t.localScale;
            s.y = y;
            t.localScale = s;
        }

        public static void SetLocalScaleZ(this Transform t, float z)
        {
            if(t == null) return;
            Vector3 s = t.localScale;
            s.z = z;
            t.localScale = s;
        }

        public static void AddPosition(this Transform t, Vector3 delta)
        {
            if(t == null) return;
            t.position += delta;
        }

        public static void AddLocalPosition(this Transform t, Vector3 delta)
        {
            if(t == null) return;
            t.localPosition += delta;
        }

        public static void AddLocalScale(this Transform t, Vector3 delta)
        {
            if(t == null) return;
            t.localScale += delta;
        }

        /* --------------------------------------------------------------------
         * 2️⃣ 重置 / 复制
         * ------------------------------------------------------------------ */

        /// <summary>
        ///     重置本地坐标、旋转、缩放为默认值。
        /// </summary>
        public static void ResetLocal(this Transform t)
        {
            if(t == null) return;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        ///     重置世界坐标与旋转为零/单位，缩放为一。
        /// </summary>
        public static void ResetWorld(this Transform t)
        {
            if(t == null) return;
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        ///     将当前位置、旋转、缩放拷贝到目标 Transform。
        /// </summary>
        /// <param name="sourceSpace">true: 复制世界空间，false: 复制本地空间。</param>
        /// <param name="includeScale">是否包含缩放。</param>
        public static void CopyTo(this Transform from, Transform to, bool sourceSpace = true, bool includeScale = true)
        {
            if(from == null || to == null) return;
            if(sourceSpace)
            {
                to.position = from.position;
                to.rotation = from.rotation;
            }
            else
            {
                to.localPosition = from.localPosition;
                to.localRotation = from.localRotation;
            }

            if(includeScale)
                to.localScale = from.localScale;
        }

        /// <summary>
        ///     先 SetParent，再将本地坐标/旋转/缩放重置为默认。
        /// </summary>
        public static void SetParentAndResetLocal(this Transform t, Transform parent, bool worldPositionStays = false)
        {
            if(t == null) return;
            t.SetParent(parent, worldPositionStays);
            t.ResetLocal();
        }

        /* --------------------------------------------------------------------
         * 3️⃣ 子节点操作 / 搜索
         * ------------------------------------------------------------------ */

        /// <summary>
        ///     遍历直接子节点（安全返回空序列）。
        /// </summary>
        public static IEnumerable<Transform> GetChildren(this Transform t)
        {
            if(t == null) yield break;
            for(var i = 0; i < t.childCount; i++)
                yield return t.GetChild(i);
        }

        /// <summary>
        ///     深度优先遍历所有后代（含子孙）。
        /// </summary>
        public static IEnumerable<Transform> GetDescendants(this Transform t)
        {
            if(t == null) yield break;
            for(var i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                yield return child;
                foreach (Transform sub in child.GetDescendants())
                    yield return sub;
            }
        }

        /// <summary>
        ///     深度优先查找后代，返回第一个名字匹配的子节点。
        /// </summary>
        public static Transform FindDeepChild(this Transform t, string childName)
        {
            if(t == null || string.IsNullOrEmpty(childName)) return null;
            for(var i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                if(child.name == childName) return child;
                Transform result = child.FindDeepChild(childName);
                if(result != null) return result;
            }

            return null;
        }

        /// <summary>
        ///     删除所有子节点。
        /// </summary>
        public static void ClearChildren(this Transform t, bool immediate = false)
        {
            if(t == null) return;
            for(int i = t.childCount - 1; i >= 0; i--)
            {
                Transform child = t.GetChild(i);
                if(immediate)
                    Object.DestroyImmediate(child.gameObject);
                else
                    Object.Destroy(child.gameObject);
            }
        }

        /* --------------------------------------------------------------------
         * 4️⃣ 辅助信息
         * ------------------------------------------------------------------ */

        /// <summary>
        ///     获取从根节点到当前节点的层级路径，例如 "Root/Child/Leaf"。
        /// </summary>
        public static string GetHierarchyPath(this Transform t)
        {
            if(t == null) return string.Empty;
            Stack<string> stack = new Stack<string>();
            Transform cur = t;
            while (cur != null)
            {
                stack.Push(cur.name);
                cur = cur.parent;
            }

            return string.Join("/", stack);
        }
    }
}