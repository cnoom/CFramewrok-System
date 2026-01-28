using System;
using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.EditorFramework;
using CFramework.Core.Editor.EditorFramework.Interfaces;
using CFramework.Core.Editor.Utilities;
using CFramework.Systems.UISystem;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor.UISystem
{
    /// <summary>
    ///     UISystem 编辑器模块，使用 EditorModule 框架管理 UI 系统相关功能
    /// </summary>
    [AutoEditorModule("UISystemEditorModule")]
    public class UISystemEditorModule : IEditorModule, IEditorInitialize, IEditorFrameworkInitialize
    {
        private bool _isInitialized;

        public void OnEditorFrameworkInitialize()
        {
            EnsureUIConfigFileExists();
        }

        public void OnEditorInitialize()
        {
            if(_isInitialized) return;

            EditorLogUtility.LogDebug("[UISystemEditorModule] 初始化中...");

            // 初始化UIConfigEditor的过渡类型缓存
            UIConfigEditor.InitializeTransitionTypes();

            _isInitialized = true;
            EditorLogUtility.LogDebug("[UISystemEditorModule] 初始化完成");
        }

        /// <summary>
        ///     生成 UI 层级常量
        /// </summary>
        public void GenerateLayerConstants()
        {
            UIConfig config = LoadUIConfig();
            if(!config)
            {
                EditorUtility.DisplayDialog("生成 UI 层级常量", "未找到 UIConfig 资产，请先在 CFramework/Config 下创建 config_UIConfig.asset。", "确定");
                return;
            }

            UILayerCodeGen.GenerateCodeInternal(config);
            AssetDatabase.Refresh();
            Debug.Log("[CFramework][UI][LayerGenerator] 已生成层级常量");
        }

        /// <summary>
        ///     生成 UI 动画常量
        /// </summary>
        public void GenerateTransitionConstants()
        {
            UITransitionCodeGen.GenerateCodeInternal();
            AssetDatabase.Refresh();
            Debug.Log("[CFramework][UI][TransitionGenerator] 已生成动画常量");
        }

        private static UIConfig LoadUIConfig()
        {
            var defaultPath = $"{CFDirectoryKey.FrameworkConfig}/config_UIConfig.asset";
            UIConfig cfg = AssetDatabase.LoadAssetAtPath<UIConfig>(defaultPath);
            if(cfg) return cfg;

            string[] guids = AssetDatabase.FindAssets("t:UIConfig");
            if(guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                cfg = AssetDatabase.LoadAssetAtPath<UIConfig>(path);
            }

            return cfg;
        }

        private static void EnsureUIConfigFileExists()
        {
            var configPath = $"{CFDirectoryKey.FrameworkConfig}/UIConfig.asset";
            UIConfig config = AssetDatabase.LoadAssetAtPath<UIConfig>(configPath);

            if (config != null) return;
            UIConfig uiConfig = ScriptableObject.CreateInstance<UIConfig>();

            CFDirectoryUtility.EnsureFolder(CFDirectoryKey.FrameworkConfig);
            AssetDatabase.CreateAsset(uiConfig, configPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorLogUtility.LogInfo($"创建 UIConfig 文件: {configPath}");
        }
    }
}