using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.EditorFramework;
using CFramework.Systems.UnityContainerSystem;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor.UnityContainerSystem
{
    /// <summary>
    /// Unity Container 系统相关的编辑器工具
    /// 提供生成配置、选择配置等菜单功能
    /// </summary>
    public static class UnityContainerSystemEditorTools
    {
        private static UnityContainerSystemEditorModule Module => EditorCF.GetModule<UnityContainerSystemEditorModule>();

        /// <summary>
        /// 生成 Unity Container 配置
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/UnityContainer/生成容器配置", priority = 5300)]
        public static void CreateUnityContainerConfig()
        {
            if (UnityContainerSystemEditorModule.IsUnityContainerConfigExists())
            {
                EditorUtility.DisplayDialog("配置已存在", "UnityContainerConfig 已存在，无需重复创建。", "确定");
                return;
            }

            UnityContainerSystemEditorModule.CreateUnityContainerConfig();

            var configPath = CFDirectoryKey.FrameworkConfig + "/UnityContainerConfig.asset";
            if (EditorUtility.DisplayDialog("配置创建完成",
                $"UnityContainerConfig 已创建于：\n{configPath}\n\n是否现在同步到 Addressables？",
                "是", "否"))
            {
                SyncToAddressables();
            }
        }

        /// <summary>
        /// 选择 UnityContainerConfig
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/UnityContainer/选择容器配置", priority = 5301)]
        public static void SelectUnityContainerConfig()
        {
            var config = UnityContainerSystemEditorModule.GetUnityContainerConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("配置不存在", "UnityContainerConfig 尚未创建，请先生成配置。", "确定");
                return;
            }

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        /// <summary>
        /// 检查配置状态
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/UnityContainer/检查配置状态", priority = 5302)]
        public static void CheckConfigStatus()
        {
            var config = UnityContainerSystemEditorModule.GetUnityContainerConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("配置状态", 
                    "UnityContainerConfig 尚未创建。\n\n请点击菜单：CFramework > Systems > UnityContainer > 生成容器配置", 
                    "确定");
                return;
            }

            var message = $"UnityContainerConfig 配置状态：\n\n" +
                         $"路径：{AssetDatabase.GetAssetPath(config)}\n" +
                         $"日志标签：{config.tag}\n" +
                         $"冲突策略：{config.globalDuplicatePolicy}\n";

            EditorUtility.DisplayDialog("配置状态", message, "确定");
        }

        /// <summary>
        /// 同步配置到 Addressables
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/UnityContainer/同步到Addressables", priority = 5303)]
        public static void SyncToAddressables()
        {
            var configPath = CFDirectoryKey.FrameworkConfig + "/UnityContainerConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(configPath);
            
            if (config == null)
            {
                EditorUtility.DisplayDialog("同步失败", "UnityContainerConfig 不存在，无法同步。", "确定");
                return;
            }

            var processorType = System.Type.GetType("CFramework.Editor.AddressablesTools.SingleAddressAssetProcessor, CFramework.Attachment.Editor");
            if (processorType != null)
            {
                var processAllMethod = processorType.GetMethod("ProcessAll", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (processAllMethod != null)
                {
                    processAllMethod.Invoke(null, null);
                    EditorUtility.DisplayDialog("同步完成", "UnityContainerConfig 已同步到 Addressables。", "确定");
                }
                else
                {
                    Debug.LogWarning("[UnityContainerSystemEditorTools] 未找到 ProcessAll 方法，请手动执行：Assets > CFramework > Addressables > Sync Single Address Assets");
                }
            }
            else
            {
                Debug.LogWarning("[UnityContainerSystemEditorTools] 未找到 SingleAddressAssetProcessor，请手动执行：Assets > CFramework > Addressables > Sync Single Address Assets");
            }
        }

        /// <summary>
        /// 验证"选择容器配置"菜单是否可用
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/UnityContainer/选择容器配置", true)]
        private static bool ValidateSelectUnityContainerConfig()
        {
            return UnityContainerSystemEditorModule.GetUnityContainerConfig() != null;
        }

        /// <summary>
        /// 验证"检查配置状态"菜单是否可用
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/UnityContainer/检查配置状态", true)]
        private static bool ValidateCheckConfigStatus()
        {
            return UnityContainerSystemEditorModule.GetUnityContainerConfig() != null;
        }

        /// <summary>
        /// 验证"同步到Addressables"菜单是否可用
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/UnityContainer/同步到Addressables", true)]
        private static bool ValidateSyncToAddressables()
        {
            return UnityContainerSystemEditorModule.GetUnityContainerConfig() != null;
        }
    }
}
