using System;
using System.Collections.Generic;
using System.Text;
using CFramework.Core.Log;
using CFramework.Systems.UISystem.Internal;
using CFramework.Systems.UISystem.LifeScope;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace CFramework.Systems.UISystem
{
    /// <summary>
    ///     UI视图池，用于复用已实例化的视图对象。
    /// </summary>
    internal sealed class UIViewPool : IViewPool
    {
        private readonly bool _enabled;
        private readonly CFLogger _logger;
        private readonly int _maxSize;

        // key -> pooled instances (stack) - 存储具体类型
        private readonly Dictionary<string, Stack<UIViewInstance>> _pool = new Dictionary<string, Stack<UIViewInstance>>();

        public UIViewPool(CFLogger logger, bool enabled, int maxSize)
        {
            _logger = logger;
            _enabled = enabled;
            _maxSize = maxSize;
        }

        /// <summary>从池中获取视图实例（如果存在）</summary>
        public IViewInstance Get(string key)
        {
            if(!_enabled) return null;

            if(_pool.TryGetValue(key, out Stack<UIViewInstance> stack) && stack.Count > 0)
            {
                UIViewInstance instance = stack.Pop();
                _logger?.LogDebug($"从池中取出视图: {key}, 剩余: {stack.Count}");

                // 调用池取出钩子
                if(instance.Controller is IViewPoolRetrieve poolRetrieve)
                {
                    try
                    {
                        poolRetrieve.OnPoolRetrieve();
                    }
                    catch (Exception e)
                    {
                        _logger?.LogException(e);
                    }
                }

                return instance;
            }
            return null;
        }

        /// <summary>将视图实例归还到池中</summary>
        public async UniTask ReturnAsync(IViewInstance instance, int? maxPoolSize = null)
        {
            if(!_enabled) return;
            if(instance == null) return;
            if(!instance.Root) return;

            // 转换为具体类型
            UIViewInstance uiInstance = instance as UIViewInstance;
            if(uiInstance == null) return;

            string key = instance.Key;
            int actualMaxSize = maxPoolSize ?? _maxSize;

            // 如果池大小为0，直接销毁（禁用池化）
            if(actualMaxSize <= 0)
            {
                DestroyInstance(instance);
                return;
            }

            if(!_pool.TryGetValue(key, out Stack<UIViewInstance> stack))
            {
                stack = new Stack<UIViewInstance>();
                _pool[key] = stack;
            }

            // 检查容量
            if(stack.Count >= actualMaxSize)
            {
                _logger?.LogWarning($"视图池已满 ({actualMaxSize})，销毁实例: {key}");
                DestroyInstance(instance);
                return;
            }

            // 调用池归还钩子
            if(instance.Controller is IViewPoolReturn poolReturn)
            {
                try
                {
                    poolReturn.OnPoolReturn();
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }

            // 重置状态
            instance.Visible = false;
            instance.Root.SetActive(false);
            instance.Root.transform.SetParent(null);

            stack.Push(uiInstance);
            _logger?.LogDebug($"视图归还到池: {key}, 当前: {stack.Count}");
        }

        /// <summary>清空指定key的所有缓存</summary>
        public void Clear(string key = null)
        {
            if(string.IsNullOrEmpty(key))
            {
                // 清空所有
                foreach (KeyValuePair<string, Stack<UIViewInstance>> kv in _pool)
                {
                    while (kv.Value.Count > 0)
                    {
                        UIViewInstance inst = kv.Value.Pop();
                        DestroyInstance(inst);
                    }
                }
                _pool.Clear();
                _logger?.LogInfo("视图池已清空");
            }
            else
            {
                // 清空指定key
                if(_pool.TryGetValue(key, out Stack<UIViewInstance> stack))
                {
                    while (stack.Count > 0)
                    {
                        UIViewInstance inst = stack.Pop();
                        DestroyInstance(inst);
                    }
                    _pool.Remove(key);
                    _logger?.LogInfo($"视图池已清空: {key}");
                }
            }
        }

        /// <summary>同步版本Return（兼容旧代码，已废弃）</summary>
        [Obsolete("请使用 ReturnAsync 替代此方法", false)]
        public void Return(IViewInstance instance)
        {
            // 不再调用Close，仅重置状态
            if(!_enabled) return;
            if(instance == null) return;
            if(!instance.Root) return;

            UIViewInstance uiInstance = instance as UIViewInstance;
            if(uiInstance == null) return;

            string key = instance.Key;
            if(!_pool.TryGetValue(key, out Stack<UIViewInstance> stack))
            {
                stack = new Stack<UIViewInstance>();
                _pool[key] = stack;
            }

            if(stack.Count >= _maxSize)
            {
                DestroyInstance(instance);
                return;
            }

            instance.Visible = false;
            instance.Root.SetActive(false);
            instance.Root.transform.SetParent(null);

            stack.Push(uiInstance);
        }

        private void DestroyInstance(IViewInstance instance)
        {
            // 转换为具体类型
            UIViewInstance uiInstance = instance as UIViewInstance;
            if(uiInstance == null || uiInstance.Root == null) return;

            // 调用Destroy生命周期
            if(instance.Controller is IViewDestroy destroy)
                destroy.OnViewDestroy();

            if(instance.Controller is IViewDestroyAsync destroyAsync)
                destroyAsync.OnViewDestroyAsync(default).Forget();

            Object.Destroy(uiInstance.Root);
        }

        /// <summary>获取池状态信息</summary>
        public string GetStatus()
        {
            if(!_enabled) return "视图池未启用";

            StringBuilder info = new StringBuilder();
            info.AppendLine("视图池状态:");
            foreach (KeyValuePair<string, Stack<UIViewInstance>> kv in _pool)
            {
                info.AppendLine($"  {kv.Key}: {kv.Value.Count}/{_maxSize}");
            }
            return info.ToString();
        }
    }
}