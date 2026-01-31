using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.EditorFramework;
using CFramework.Core.Editor.EditorFramework.Interfaces;
using CFramework.Core.Editor.Utilities;
using CFramework.Systems.UnityContainerSystem;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor.UnityContainerSystem
{
    /// <summary>
    ///     Unity Container 系统编辑器模块
    ///     负责在首次初始化框架时创建 UnityContainerConfig 配置文件
    /// </summary>
    [AutoEditorModule("UnityContainerSystemEditorModule", 30)]
    public class UnityContainerSystemEditorModule : IEditorModule, IEditorFrameworkInitialize
    {
        private static readonly string ConfigAssetPath = CFDirectoryKey.FrameworkConfig + "/UnityContainerConfig.asset";

        public void OnEditorFrameworkInitialize()
        {
            EnsureUnityContainerConfig();
        }

        private void EnsureUnityContainerConfig()
        {
            if(AssetDatabase.LoadAssetAtPath<UnityContainerConfig>(ConfigAssetPath))
            {
                return;
            }

            CreateUnityContainerConfig();
        }

        /// <summary>
        ///     创建 UnityContainerConfig
        /// </summary>
        internal static void CreateUnityContainerConfig()
        {
            CFDirectoryUtility.EnsureFolder(CFDirectoryKey.FrameworkConfig);

            UnityContainerConfig config = ScriptableObject.CreateInstance<UnityContainerConfig>();
            config.tag = "UnityContainer";
            config.globalDuplicatePolicy = DuplicatePolicy.Warn;

            AssetDatabase.CreateAsset(config, ConfigAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorLogUtility.LogInfo($"创建 UnityContainerConfig: {ConfigAssetPath}", "CFramework.UnityContainer");
        }

        /// <summary>
        ///     检查 UnityContainerConfig 是否存在
        /// </summary>
        public static bool IsUnityContainerConfigExists()
        {
            return AssetDatabase.LoadAssetAtPath<UnityContainerConfig>(ConfigAssetPath) != null;
        }

        /// <summary>
        ///     获取 UnityContainerConfig
        /// </summary>
        public static UnityContainerConfig GetUnityContainerConfig()
        {
            return AssetDatabase.LoadAssetAtPath<UnityContainerConfig>(ConfigAssetPath);
        }
    }
}