using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.EditorFramework;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor.AudioSystem
{
    /// <summary>
    /// Audio 系统相关的编辑器工具
    /// 提供生成配置、选择配置等菜单功能
    /// </summary>
    public static class AudioSystemEditorTools
    {
        private static AudioSystemEditorModule Module => EditorCF.GetModule<AudioSystemEditorModule>();

        /// <summary>
        /// 生成音频配置
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/Audio/生成音频配置", priority = 5100)]
        public static void CreateAudioConfig()
        {
            // 模块会在初始化时自动创建，这里只是提供菜单入口
            if (AudioSystemEditorModule.IsAudioConfigExists())
            {
                EditorUtility.DisplayDialog("配置已存在", "AudioConfig 已存在，无需重复创建。", "确定");
                return;
            }

            // 调用模块的内部方法创建配置
            AudioSystemEditorModule.CreateAudioConfig();

            // 提示用户可以手动同步到 Addressables
            var configPath = CFDirectoryKey.FrameworkConfig + "/AudioConfig.asset";
            if (EditorUtility.DisplayDialog("配置创建完成",
                $"AudioConfig 已创建于：\n{configPath}\n\n是否现在同步到 Addressables？",
                "是", "否"))
            {
                // 调用 SingleAddressAssetProcessor 来同步
                var processorType = System.Type.GetType("CFramework.Editor.AddressablesTools.SingleAddressAssetProcessor, CFramework.Attachment.Editor");
                if (processorType != null)
                {
                    var processAllMethod = processorType.GetMethod("ProcessAll", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (processAllMethod != null)
                    {
                        processAllMethod.Invoke(null, null);
                    }
                    else
                    {
                        Debug.LogWarning("[AudioSystemEditorTools] 未找到 ProcessAll 方法，请手动执行：Assets > CFramework > Addressables > Sync Single Address Assets");
                    }
                }
                else
                {
                    Debug.LogWarning("[AudioSystemEditorTools] 未找到 SingleAddressAssetProcessor，请手动执行：Assets > CFramework > Addressables > Sync Single Address Assets");
                }
            }
        }

        /// <summary>
        /// 选择 AudioConfig
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/Audio/选择音频配置", priority = 5101)]
        public static void SelectAudioConfig()
        {
            var config = AudioSystemEditorModule.GetAudioConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("配置不存在", "AudioConfig 尚未创建，请先生成配置。", "确定");
                return;
            }

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        /// <summary>
        /// 检查配置状态
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/Audio/检查配置状态", priority = 5102)]
        public static void CheckConfigStatus()
        {
            var config = AudioSystemEditorModule.GetAudioConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("配置状态", 
                    "AudioConfig 尚未创建。\n\n请点击菜单：CFramework > Systems > Audio > 生成音频配置", 
                    "确定");
                return;
            }

            var message = $"AudioConfig 配置状态：\n\n" +
                         $"路径：{AssetDatabase.GetAssetPath(config)}\n" +
                         $"日志标签：{config.logTag}\n" +
                         $"启用日志：{config.enableLogs}\n" +
                         $"主音量：{config.masterVolume}\n" +
                         $"音乐音量：{config.musicVolume}\n" +
                         $"音效音量：{config.sfxVolume}\n" +
                         $"SFX 池初始大小：{config.sfxPoolInitialSize}\n" +
                         $"SFX 池最大大小：{config.sfxPoolMaxSize}";

            EditorUtility.DisplayDialog("配置状态", message, "确定");
        }

        /// <summary>
        /// 验证"选择音频配置"菜单是否可用
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/Audio/选择音频配置", true)]
        private static bool ValidateSelectAudioConfig()
        {
            return AudioSystemEditorModule.GetAudioConfig() != null;
        }

        /// <summary>
        /// 验证"检查配置状态"菜单是否可用
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/Audio/检查配置状态", true)]
        private static bool ValidateCheckConfigStatus()
        {
            return AudioSystemEditorModule.GetAudioConfig() != null;
        }
    }
}
