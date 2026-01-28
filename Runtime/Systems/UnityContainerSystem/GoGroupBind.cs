using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CFramework.Core;
using UnityEngine;

namespace CFramework.Systems.UnityContainerSystem
{

    /// <summary>
    ///     管理一组 GameObject，需主动调用 RegisterAll/UnregisterAll 方法。
    ///     使用 WeakReference 缓存 GameObject 引用，允许 GC 回收已销毁对象。
    /// </summary>
    [Serializable]
    public class GoGroupBind : IDisposable
    {
        [SerializeField, Tooltip("命名空间，用于避免 key 冲突")]
        private string _groupKey;

        [Tooltip("管理的 GameObject 列表"), SerializeField]
        private List<GameObject> gos = new List<GameObject>();
        private Dictionary<WeakReference<GameObject>, Dictionary<Type, WeakReference<Component>>> _gocompMap;

        // 使用 WeakReference 缓存
        private Dictionary<string, WeakReference<GameObject>> _goMap;
        private bool _isDisposed;

        private bool _isMapInitialized;

        public string GroupKey => _groupKey;

        public void Dispose()
        {
            if(_isDisposed) return;

            gos.Clear();
            _goMap?.Clear();
            _gocompMap?.Clear();
            _isMapInitialized = false;
            _isDisposed = true;
        }

        /// <summary>
        ///     初始化内部字典缓存
        /// </summary>
        private void EnsureMapInitialized()
        {
            if(_isMapInitialized) return;

            if(_goMap == null)
                _goMap = new Dictionary<string, WeakReference<GameObject>>();

            if(_gocompMap == null)
                _gocompMap = new Dictionary<WeakReference<GameObject>, Dictionary<Type, WeakReference<Component>>>();

            // 预热缓存
            foreach (GameObject go in gos)
            {
                if(go != null && !string.IsNullOrEmpty(go.name))
                {
                    _goMap[go.name] = new WeakReference<GameObject>(go);
                }
            }

            _isMapInitialized = true;
        }

        /// <summary>
        ///     注册所有 GameObject
        /// </summary>
        public void Register()
        {
            if(_isDisposed)
            {
                throw new ObjectDisposedException(nameof(GoGroupBind), "GoGroupBind 已被释放，无法注册。");
            }

            if(string.IsNullOrEmpty(_groupKey))
            {
                CF.LogError("[GoGroupBind] GroupKey 为空，无法注册");
                throw new ArgumentException("GroupKey 不能为空");
            }

            EnsureMapInitialized();

            // 检测是否有空的 GameObject
            var nullCount = 0;
            foreach (GameObject go in gos)
            {
                if(go == null) nullCount++;
                else if(string.IsNullOrEmpty(go.name))
                {
                    CF.LogError($"[GoGroupBind] GameObject 名称为空，将被跳过 (GroupKey: {_groupKey})");
                    nullCount++;
                }
            }

            if(nullCount > 0)
            {
                CF.LogWarning($"[GoGroupBind] 发现 {nullCount} 个无效的 GameObject，已被跳过 (GroupKey: {_groupKey})");
            }

            if(_goMap.Count == 0)
            {
                CF.LogWarning($"[GoGroupBind] 没有有效的 GameObject 可注册 (GroupKey: {_groupKey})");
            }

            try
            {
                UnityGoCommands.RegisterGosBind cmd = new UnityGoCommands.RegisterGosBind(this);
                CF.Execute(cmd);
            }
            catch (Exception ex)
            {
                CF.LogError($"[GoGroupBind] 注册失败 (GroupKey: {_groupKey}), 错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     注销
        /// </summary>
        public void Unregister()
        {
            if(string.IsNullOrEmpty(_groupKey))
            {
                CF.LogWarning("[GoGroupBind] GroupKey 为空，无法注销");
                return;
            }

            try
            {
                UnityGoCommands.UnregisterGosBind cmd = new UnityGoCommands.UnregisterGosBind(_groupKey);
                CF.Execute(cmd);
            }
            catch (Exception ex)
            {
                CF.LogError($"[GoGroupBind] 注销失败 (GroupKey: {_groupKey}), 错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     获取组件（带销毁检测和 WeakReference 缓存）
        /// </summary>
        public Component Get([NotNull] string goKey, Type type)
        {
            GameObject go = GetGameObject(goKey);
            if(go == null) return null;

            // 查找对应的 WeakReference
            WeakReference<GameObject> goWeakRef = null;
            foreach (KeyValuePair<string, WeakReference<GameObject>> kvp in _goMap)
            {
                if(kvp.Value.TryGetTarget(out GameObject target) && target == go)
                {
                    goWeakRef = kvp.Value;
                    break;
                }
            }

            if(goWeakRef == null) return null;

            if(!_gocompMap.TryGetValue(goWeakRef, out Dictionary<Type, WeakReference<Component>> compDict))
            {
                compDict = new Dictionary<Type, WeakReference<Component>>();
                _gocompMap[goWeakRef] = compDict;
            }

            // 从 WeakReference 缓存获取
            if(compDict.TryGetValue(type, out WeakReference<Component> weakComp) && weakComp.TryGetTarget(out Component cachedComp))
            {
                if(cachedComp != null && cachedComp.gameObject == go)
                {
                    return cachedComp;
                }
            }

            // 获取新组件并使用 WeakReference 缓存
            Component comp = go.GetComponent(type);
            if(comp != null)
            {
                compDict[type] = new WeakReference<Component>(comp);
            }

            return comp;
        }

        /// <summary>
        ///     获取泛型组件（类型安全的封装）
        /// </summary>
        public T GetComponent<T>([NotNull] string goKey) where T : Component
        {
            return Get(goKey, typeof(T)) as T;
        }

        public GameObject GetGameObject([NotNull] string goKey)
        {
            if(_isDisposed)
            {
                CF.LogWarning($"[GoGroupBind] GoGroupBind 已释放 (GroupKey: {_groupKey})");
                return null;
            }

            if(string.IsNullOrEmpty(goKey))
            {
                CF.LogError($"[GoGroupBind] goKey 为空 (GroupKey: {_groupKey})");
                return null;
            }

            EnsureMapInitialized();

            if(!_goMap.TryGetValue(goKey, out WeakReference<GameObject> weakRef))
            {
                CF.LogWarning($"[GoGroupBind] 未找到 GameObject (Key: {goKey}, GroupKey: {_groupKey})");
                return null;
            }

            if(!weakRef.TryGetTarget(out GameObject go))
            {
                CF.LogWarning($"[GoGroupBind] GameObject 已被销毁 (Key: {goKey}, GroupKey: {_groupKey})");
                return null;
            }

            return go;
        }

        public Transform GetTransform([NotNull] string goKey)
        {
            GameObject go = GetGameObject(goKey);
            return go?.transform;
        }

        public RectTransform GetRectTransform([NotNull] string goKey)
        {
            GameObject go = GetGameObject(goKey);
            return go?.GetComponent<RectTransform>();
        }
    }
}