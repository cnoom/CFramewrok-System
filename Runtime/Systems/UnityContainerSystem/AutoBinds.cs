using UnityEngine;

namespace CFramework.Systems.UnityContainerSystem
{
    /// <summary>
    ///     批量自动绑定一组 GameObject 到容器系统，在 Awake 或 Start 时自动注册，Destroy 时自动注销。
    /// </summary>
    public class AutoBinds : MonoBehaviour
    {
        [SerializeField]
        private GoGroupBind goGroupBind;

        [SerializeField, Tooltip("自动注册时机")]
        private AutoRegisterTiming registerTiming = AutoRegisterTiming.Awake;

        private void Awake()
        {
            if(registerTiming == AutoRegisterTiming.Awake) goGroupBind?.Register();
        }

        private void Start()
        {
            if(registerTiming == AutoRegisterTiming.Start) goGroupBind?.Register();
        }

        private void OnDestroy()
        {
            goGroupBind?.Unregister();
        }
    }
}