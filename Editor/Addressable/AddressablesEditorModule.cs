using System;
using System.Collections.Generic;
using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.EditorFramework;
using CFramework.Core.Editor.EditorFramework.Interfaces;
using CFramework.Core.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor.AddressablesTools
{
    /// <summary>
    ///     Addressables 编辑器模块，使用 EditorModule 框架管理 Addressables 相关功能
    /// </summary>
    [AutoEditorModule("AddressablesEditorModule")]
    public class AddressablesEditorModule : IEditorModule, IEditorInitialize, IEditorProjectChange
    {
        private bool _isInitialized;

        public void OnEditorInitialize()
        {
            if(_isInitialized) return;

            EditorLogUtility.LogDebug("[AddressablesEditorModule] 初始化中...");

            // 初始化快捷键
            RegisterShortcuts();

            _isInitialized = true;
            EditorLogUtility.LogDebug("[AddressablesEditorModule] 初始化完成");
        }

        public void OnEditorProjectChanged()
        {
            AddressablesFolderRegistry reg = ConfigUtility.GetEditorConfig<AddressablesFolderRegistry>();
            List<FolderRecord> autoRecords = reg.records.FindAll(r => r.autoSync);

            if(autoRecords.Count == 0)
                return;

            // 注意：这里只是简化处理，实际应该追踪具体变化的文件
            // 完整实现需要更细致的变化追踪机制
            try
            {
                AddressablesSyncPipeline.SyncAll(reg);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AddressablesEditorModule] 同步失败: {ex.Message}");
            }
        }

        private void RegisterShortcuts()
        {
            // 快捷键已在 AddressablesShortcuts.cs 中通过特性注册
            // 这里可以添加需要动态注册的快捷键（如果有的话）
            EditorLogUtility.LogDebug("[AddressablesEditorModule] 快捷键已注册");
        }

        /// <summary>
        ///     打开管理器窗口
        /// </summary>
        public void OpenManagerWindow()
        {
            EditorApplication.ExecuteMenuItem(CFMenuKey.Systems + "/Addressables/地址管理器");
        }

        /// <summary>
        ///     同步所有记录的文件夹
        /// </summary>
        public void SyncAll()
        {
            AddressablesFolderRegistry registry = ConfigUtility.GetEditorConfig<AddressablesFolderRegistry>();
            AddressablesSyncPipeline.SyncAll(registry);
            Debug.Log("[CFramework][Addressables] 已同步所有记录文件夹");
        }

        /// <summary>
        ///     记录当前选中的文件夹
        /// </summary>
        public void RecordSelectedFolder()
        {
            AddressablesContextMenu.RecordSelectedFolders();
        }

        /// <summary>
        ///     生成地址常量代码
        /// </summary>
        public void GenerateAddressKeys()
        {
            AddressablesFolderRegistry registry = ConfigUtility.GetEditorConfig<AddressablesFolderRegistry>();
            AddressablesCodeGen.Generate(registry);
            Debug.Log("[CFramework][Addressables] 已生成地址常量");
        }
    }
}