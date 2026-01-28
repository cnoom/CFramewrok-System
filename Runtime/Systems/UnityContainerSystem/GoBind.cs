using System;
using System.Collections.Generic;
using CFramework.Core;
using UnityEngine;

namespace CFramework.Systems.UnityContainerSystem
{
    /// <summary>
    ///     手动绑定 GameObject 到容器系统，需主动调用 Register/Unregister 方法。
    ///     使用 WeakReference 缓存 GameObject 引用，允许 GC 回收已销毁对象。
    /// </summary>
    [Serializable]
    public class GoBind : IDisposable
    {
        [SerializeField, Tooltip("绑定的 GameObject")]
        private GameObject go;

        [SerializeField, Tooltip("用于查询该对象的命名空间")]
        public string scope = string.Empty;
        private Dictionary<Type, WeakReference<Component>> _cachedComponents = new Dictionary<Type, WeakReference<Component>>();

        // 使用 WeakReference 缓存 GameObject，允许 GC 回收
        private WeakReference<GameObject> _goWeakRef;

        private bool _isDisposed;

        public GameObject GameObject
        {
            get
            {
                if(_goWeakRef == null || !_goWeakRef.TryGetTarget(out GameObject target))
                {
                    return null;
                }

                // 对象销毁检测
                if(target == null)
                {
                    CF.LogWarning($"[GoBind] 尝试访问已销毁的 GameObject (Scope: {scope}, Key: {CachedName})");
                    return null;
                }
                return target;
            }
        }

        public Transform Transform => GameObject?.transform;

        public RectTransform RectTransform => GameObject?.GetComponent<RectTransform>();

        /// <summary>
        ///     获取缓存的名称（用于注销时使用）
        /// </summary>
        public string CachedName { get; private set; }

        public void Dispose()
        {
            if(_isDisposed) return;

            _goWeakRef = null;
            _cachedComponents.Clear();
            _isDisposed = true;
        }

        private void Awake()
        {
            // 初始化 WeakReference
            if(go != null)
            {
                _goWeakRef = new WeakReference<GameObject>(go);
                CachedName = go.name;
            }
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public void Register()
        {
            if(_isDisposed)
            {
                throw new ObjectDisposedException(nameof(GoBind), "GoBind 已被释放，无法注册。");
            }

            if(go == null)
            {
                CF.LogError($"[GoBind] 尝试注册 null GameObject (Scope: {scope})");
                throw new ArgumentException("GameObject 不能为 null");
            }

            if(string.IsNullOrEmpty(go.name))
            {
                CF.LogError($"[GoBind] GameObject 名称为空，无法注册 (Scope: {scope})");
                throw new ArgumentException("GameObject 必须有有效的名称");
            }

            // 确保 WeakReference 已初始化
            if(_goWeakRef == null)
            {
                _goWeakRef = new WeakReference<GameObject>(go);
            }
            CachedName = go.name;

            try
            {
                UnityGoCommands.RegisterGoBind cmd = new UnityGoCommands.RegisterGoBind(this);
                CF.Execute(cmd);
            }
            catch (Exception ex)
            {
                CF.LogError($"[GoBind] 注册失败 (Scope: {scope}, GameObject: {go.name}), 错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     手动注销
        /// </summary>
        public void Unregister()
        {
            if(string.IsNullOrEmpty(CachedName))
            {
                CF.LogWarning($"[GoBind] 尝试注销，但缓存的名称为空 (Scope: {scope})");
                return;
            }

            try
            {
                UnityGoCommands.UnregisterGoBind cmd = new UnityGoCommands.UnregisterGoBind(CachedName, scope);
                CF.Execute(cmd);
            }
            catch (Exception ex)
            {
                CF.LogError($"[GoBind] 注销失败 (Scope: {scope}, GameObject: {CachedName}), 错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     获取组件（带销毁检测和 WeakReference 缓存）
        /// </summary>
        public Component Get(Type type)
        {
            if(_isDisposed)
            {
                CF.LogWarning($"[GoBind] GoBind 已释放，无法获取组件 (Type: {type?.Name}, Scope: {scope})");
                return null;
            }

            GameObject targetGo = GameObject;
            if(targetGo == null)
            {
                CF.LogWarning($"[GoBind] GameObject 已销毁，无法获取组件 (Type: {type?.Name}, Scope: {scope})");
                return null;
            }

            if(type == null)
            {
                CF.LogError($"[GoBind] 组件类型为 null (Scope: {scope})");
                throw new ArgumentNullException(nameof(type));
            }

            // 从 WeakReference 缓存获取
            if(_cachedComponents.TryGetValue(type, out WeakReference<Component> weakComp) && weakComp.TryGetTarget(out Component cachedComp))
            {
                // 验证缓存的组件是否仍然有效
                if(cachedComp != null && cachedComp.gameObject == targetGo)
                {
                    return cachedComp;
                }
            }

            // 获取新组件并使用 WeakReference 缓存
            Component comp = targetGo.GetComponent(type);
            if(comp != null)
            {
                _cachedComponents[type] = new WeakReference<Component>(comp);
            }

            return comp;
        }

        /// <summary>
        ///     获取泛型组件（类型安全的封装）
        /// </summary>
        public T GetComponent<T>() where T : Component
        {
            return Get(typeof(T)) as T;
        }
    }
}