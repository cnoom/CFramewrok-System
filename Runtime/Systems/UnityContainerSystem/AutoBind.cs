using System;
using CFramework.Core;
using UnityEngine;

namespace CFramework.Systems.UnityContainerSystem
{
    /// <summary>
    ///     自动绑定 GameObject 到容器系统，在 Awake 或 Start 时自动注册，Destroy 时自动注销。
    /// </summary>
    public class AutoBind : MonoBehaviour
    {
        [SerializeField, Tooltip("手动指定的 GoBind")]
        private GoBind goBind;

        [SerializeField, Tooltip("注册时机")]
        private AutoRegisterTiming registerTiming = AutoRegisterTiming.Awake;

        private bool _isRegistered;

        private void Awake()
        {
            if(registerTiming == AutoRegisterTiming.Awake)
            {
                RegisterGoBind();
            }
        }

        private void Start()
        {
            if(registerTiming == AutoRegisterTiming.Start)
            {
                RegisterGoBind();
            }
        }

        private void OnDestroy()
        {
            if(goBind == null)
            {
                return;
            }

            if(!_isRegistered)
            {
                return;
            }

            try
            {
                goBind.Unregister();
                _isRegistered = false;
            }
            catch (Exception ex)
            {
                CF.LogError($"[AutoBind] 注销失败 (GameObject: {name}), 错误: {ex.Message}");
            }
        }

        /// <summary>
        ///     注册 GoBind（带错误处理）
        /// </summary>
        private void RegisterGoBind()
        {
            if(_isRegistered)
            {
                CF.LogWarning($"[AutoBind] 已经注册过 (GameObject: {name})");
                return;
            }

            if(goBind == null)
            {
                CF.LogError($"[AutoBind] GoBind 为空，无法注册 (GameObject: {name})");
                return;
            }

            try
            {
                goBind.Register();
                _isRegistered = true;
            }
            catch (Exception ex)
            {
                CF.LogError($"[AutoBind] 注册失败 (GameObject: {name}), 错误: {ex.Message}");
            }
        }
    }
}