using System;
using CFramework.Attachment.com.cnoom.cframework.systems.Runtime.Attributes;
using CFramework.Core.Log;
using CFramework.Systems.UISystem.Transitions;
using UnityEngine;

namespace CFramework.Systems.UISystem
{
    [Serializable]
    public sealed class UiInfo
    {
        [Tooltip("UI名称")]
        public string uiName;
        [Tooltip("Addressables地址")]
        public string uiAddress;
        [Tooltip("对应的View类型全名（用于通过类型打开UI），如：MyGame.UISettingsView。由编辑器扫描自动填充，请勿手动修改")]
        public string viewTypeName;

        [Header("Pool Settings"), Tooltip("是否禁用此UI的池化（如需要多实例的弹窗）")]
        public bool disablePooling;

        [Tooltip("池化最大数量（覆盖全局配置）")]
        public int? maxPoolSize;
    }

    [SingleAddressAsset("UIConfig", "Configs")]
    public class UIConfig : ScriptableObject
    {
        [Header("Logging")]
        public string logTag = "UI";
        public bool enableLogs = true;
        public ICFLogger.Level logLevel = ICFLogger.Level.Info;

        [Header("Canvas")]
        public Canvas canvas;

        [Header("Layers Order (bottom->top)")]
        public string[] layerOrder =
        {
            "Screen",
            "Window",
            "Popup",
            "Overlay",
            "System"
        };

        [Header("Transitions")]
        public string defaultTransition = nameof(FadeTransition);
        public float defaultTransitionSeconds = 0.2f;

        [Header("Behavior"), Tooltip("是否尊重全局暂停")]
        public bool respectGlobalPause = true;
        [Tooltip("是否阻止重复打开相同界面")]
        public bool preventReplaySameView = true;
        [Tooltip("是否启用视图池")]
        public bool enableViewPool = true;
        [Tooltip("视图池最大容量（每个key），默认为1避免状态污染")]
        public int viewPoolMaxSize = 1;

        [Header("UIs")]
        public UiInfo[] uis = Array.Empty<UiInfo>();

        public string GetUiAddress(string uiName)
        {
            UiInfo info = Array.Find(uis, i => i.uiName == uiName);
            return info?.uiAddress;
        }

        /// <summary>根据View类型获取Addressables key</summary>
        public string GetUiAddressByType(Type viewType)
        {
            if(viewType == null) return null;
            UiInfo info = Array.Find(uis, i => !string.IsNullOrEmpty(i.viewTypeName) && i.viewTypeName == viewType.FullName);
            if(info != null && !string.IsNullOrEmpty(info.uiAddress))
            {
                return info.uiAddress;
            }

            // 未找到配置时记录警告
            Debug.LogWarning($"[UIConfig] 未找到类型 {viewType.FullName} 对应的UI配置，请运行 '扫描可寻址UI' 按钮");
            return null;
        }

        /// <summary>根据View类型获取UI配置</summary>
        public UiInfo GetUiInfoByType(Type viewType)
        {
            if(viewType == null) return null;
            return Array.Find(uis, i => !string.IsNullOrEmpty(i.viewTypeName) && i.viewTypeName == viewType.FullName);
        }

        /// <summary>根据UI地址获取UI配置</summary>
        public UiInfo GetUiInfoByKey(string key)
        {
            if(string.IsNullOrEmpty(key)) return null;
            return Array.Find(uis, i => i.uiAddress == key);
        }
    }
}