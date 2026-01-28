using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.EditorFramework;
using CFramework.Core.Editor.EditorFramework.Interfaces;
using CFramework.Core.Editor.Utilities;
using CFramework.Systems.AudioSystem;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor.AudioSystem
{
    /// <summary>
    /// Audio 系统编辑器模块
    /// 负责在首次初始化框架时创建 AudioConfig 配置文件
    /// </summary>
    [AutoEditorModule("AudioSystemEditorModule", 20)]
    public class AudioSystemEditorModule : IEditorModule, IEditorFrameworkInitialize
    {
        private static readonly string ConfigAssetPath = CFDirectoryKey.FrameworkConfig + "/AudioConfig.asset";

        public void OnEditorFrameworkInitialize()
        {
            EnsureAudioConfig();
        }

        private void EnsureAudioConfig()
        {
            if (AssetDatabase.LoadAssetAtPath<AudioConfig>(ConfigAssetPath))
            {
                return;
            }

            CreateAudioConfig();
        }

        /// <summary>
        /// 创建 AudioConfig
        /// </summary>
        internal static void CreateAudioConfig()
        {
            CFDirectoryUtility.EnsureFolder(CFDirectoryKey.FrameworkConfig);

            var config = ScriptableObject.CreateInstance<AudioConfig>();
            AssetDatabase.CreateAsset(config, ConfigAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorLogUtility.LogInfo($"创建 AudioConfig: {ConfigAssetPath}", "CFramework.Audio");
        }

        /// <summary>
        /// 检查 AudioConfig 是否存在
        /// </summary>
        public static bool IsAudioConfigExists()
        {
            return AssetDatabase.LoadAssetAtPath<AudioConfig>(ConfigAssetPath) != null;
        }

        /// <summary>
        /// 获取 AudioConfig
        /// </summary>
        public static AudioConfig GetAudioConfig()
        {
            return AssetDatabase.LoadAssetAtPath<AudioConfig>(ConfigAssetPath);
        }
    }
}
