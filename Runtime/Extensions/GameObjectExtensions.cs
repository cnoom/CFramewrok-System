using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CFramework.Extensions
{
    /// <summary>
    ///     对 UnityEngine.GameObject 的扩展方法集合。
    ///     包括常用的层级/组件/激活/层级递归操作等。
    /// </summary>
    public static class GameObjectExtensions
    {
        /* --------------------------------------------------------------------
         * 1️⃣ 生成 / 添加相关
         * ------------------------------------------------------------------ */

        /// <summary>
        ///     若该 GameObject 未包含指定组件，则添加，否则直接返回已有组件。
        ///     可在一行完成「Get-Or-Add」的常见需求。
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if(go == null) throw new ArgumentNullException(nameof(go));
            T comp = go.GetComponent<T>();
            if(comp == null) comp = go.AddComponent<T>();
            return comp;
        }

        /// <summary>
        ///     在父级 (root 或指定父) 上实例化 Prefab 并返回实例。
        /// </summary>
        public static GameObject InstantiateWithParent(this GameObject prefab, Transform parent,
            Vector3? position = null,
            Quaternion? rotation = null, bool worldPositionStays = true)
        {
            if(prefab == null) throw new ArgumentNullException(nameof(prefab));
            GameObject instance = Object.Instantiate(prefab, parent);
            if(position.HasValue) instance.transform.position = position.Value;
            if(rotation.HasValue) instance.transform.rotation = rotation.Value;
            instance.transform.SetParent(parent, worldPositionStays);
            return instance;
        }

        /* --------------------------------------------------------------------
         * 2️⃣ 递归层级 / 布局相关
         * ------------------------------------------------------------------ */

        /// <summary>
        ///     递归设置层级（Layer）。默认递归子物体不改变。
        /// </summary>
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            if(go == null) return;
            go.layer = layer;
            foreach (Transform child in go.transform)
                child.gameObject.SetLayerRecursively(layer);
        }

        /// <summary>
        ///     递归激活/停用物体（包括子级）。类似 transform.SetSiblingIndex。
        /// </summary>
        public static void SetActiveRecursively(this GameObject go, bool state)
        {
            if(go == null) return;
            go.SetActive(state);
            foreach (Transform child in go.transform)
                child.gameObject.SetActive(state);
        }

        /// <summary>
        ///     获取物体的所有子级（不递归）。返回空序列不会抛异常。
        /// </summary>
        public static IEnumerable<GameObject> GetChildren(this GameObject go)
        {
            return go?.transform.GetChildren().ToList().ConvertAll(t => t.gameObject);
        }

        /// <summary>
        ///     递归获取所有子级
        /// </summary>
        public static IEnumerable<GameObject> GetChildrenRecursion(this GameObject go)
        {
            if(go == null) yield break;

            foreach (Transform child in go.transform)
            {
                yield return child.gameObject;
                foreach (GameObject grandchild in child.gameObject.GetChildrenRecursion())
                {
                    yield return grandchild;
                }
            }
        }

        /* --------------------------------------------------------------------
         * 3️⃣ 组件获取相关
         * ------------------------------------------------------------------ */

        /// <summary>
        ///     从自身到根，按层级向上查找，返回第一个匹配的组件。
        /// </summary>
        public static T GetComponentInParents<T>(this GameObject go) where T : Component
        {
            if(go == null) return null;
            Transform current = go.transform;
            while (current != null)
            {
                T comp = current.GetComponent<T>();
                if(comp != null) return comp;
                current = current.parent;
            }

            return null;
        }

        /// <summary>
        ///     在当前物体及其所有后代中查找第一个匹配的组件。
        ///     类似 GetComponentInChildren，但更灵活，可直接遍历树。
        /// </summary>
        public static T GetComponentInDescendants<T>(this GameObject go) where T : Component
        {
            if(go == null) return null;
            T result = go.GetComponent<T>();
            if(result != null) return result;
            foreach (Transform child in go.transform)
            {
                result = child.gameObject.GetComponentInDescendants<T>();
                if(result != null) return result;
            }

            return null;
        }

        /// <summary>
        ///     在当前物体及其所有后代中查找所有匹配的组件。
        /// </summary>
        /// <typeparam name="T">要查找的组件类型。</typeparam>
        /// <param name="go">开始搜索的游戏对象。</param>
        /// <returns>返回一个包含所有匹配组件的列表。</returns>
        public static List<T> GetComponentsInDescendants<T>(this GameObject go) where T : Component
        {
            if(go == null) return new List<T>();

            List<T> components = new List<T>();
            // 首先检查当前游戏对象是否包含所需组件
            T currentComponent = go.GetComponent<T>();
            if(currentComponent != null)
            {
                components.Add(currentComponent);
            }

            // 递归地在其子节点中查找组件
            foreach (Transform child in go.transform)
            {
                components.AddRange(child.gameObject.GetComponentsInDescendants<T>());
            }

            return components;
        }


        /* --------------------------------------------------------------------
         * 4️⃣ 带安全检查的删除
         * ------------------------------------------------------------------ */

        /// <summary>
        ///     安全删除物体：先检查是否非 null，再销毁。
        /// </summary>
        public static void SafeDestroy(this GameObject go)
        {
            if(go == null) return;
            Object.Destroy(go);
        }

        /// <summary>
        ///     安全删除组件：先检查是否非 null，再销毁。
        /// </summary>
        public static void SafeDestroy<T>(this T component) where T : Component
        {
            if(component == null) return;
            Object.Destroy(component);
        }

        /* --------------------------------------------------------------------
         * 5️⃣ 搜索 / 过滤
         * ------------------------------------------------------------------ */

        /// <summary>
        ///     深度优先查找后代，返回第一个 name 匹配的 GameObject。
        ///     大家常用来替代 transform.Find 但支持多层级搜索。
        /// </summary>
        public static GameObject FindDeepChild(this GameObject go, string childName)
        {
            if(go == null) return null;
            foreach (Transform child in go.transform)
            {
                if(child.name == childName) return child.gameObject;
                GameObject result = child.gameObject.FindDeepChild(childName);
                if(result != null) return result;
            }

            return null;
        }

        /// <summary>
        ///     判断当前 GameObject 是否被某个 Tag 识别。<br />
        ///     和 GameObject.CompareTag 一样，但不会抛异常（若 tag 未定义）。
        /// </summary>
        public static bool IsTagged(this GameObject go, string tag)
        {
            if(go == null || string.IsNullOrEmpty(tag)) return false;
            return go.CompareTag(tag);
        }

        /// <summary>
        ///     判断 LayerMask 是否包含指定层。<br />
        ///     用法：`if (layerMask.ContainsLayer(myLayer)) { … }`
        /// </summary>
        public static bool ContainsLayer(this LayerMask mask, int layer)
        {
            return (mask.value & 1 << layer) != 0;
        }

        /* --------------------------------------------------------------------
         * 6️⃣ 常用快捷属性
         * ------------------------------------------------------------------ */

        /// <summary>
        ///     让物体保持世界坐标，快速地设置父物体。
        /// </summary>
        public static void SetParent(this GameObject go, Transform parent, bool worldPositionStays = true)
        {
            if(go == null) return;
            go.transform.SetParent(parent, worldPositionStays);
        }

        /// <summary>
        ///     通过一个矩阵一次性设定位置、旋转与缩放（适用于动画/复制等）。
        /// </summary>
        public static void SetLocalTransform(this GameObject go, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if(go == null) return;
            Transform t = go.transform;
            t.localPosition = position;
            t.localRotation = rotation;
            t.localScale = scale;
        }

        /// <summary>
        ///     将 Transform 的位置、旋转复制到目标 Transform（适用于“跟随/同步”）。
        /// </summary>
        public static void CopyTransformTo(this Transform from, Transform to)
        {
            to.position = from.position;
            to.rotation = from.rotation;
            to.localScale = from.localScale;
        }

        /* --------------------------------------------------------------------
         * 7️⃣ 其它 (可根据需要自行添加)
         * ------------------------------------------------------------------ */
    }
}