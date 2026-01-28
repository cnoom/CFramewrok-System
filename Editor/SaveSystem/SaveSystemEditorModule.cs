using System;
using System.IO;
using CFramework.Core.Editor.EditorFramework;
using CFramework.Core.Editor.EditorFramework.Interfaces;
using CFramework.Core.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor.SaveSystem
{
    /// <summary>
    ///     SaveSystem 编辑器模块，使用 EditorModule 框架管理存档系统相关功能
    /// </summary>
    [AutoEditorModule("SaveSystemEditorModule")]
    public class SaveSystemEditorModule : IEditorModule, IEditorInitialize, IEditorFrameworkInitialize
    {
        private bool _isInitialized;

        public void OnEditorFrameworkInitialize()
        {
            EnsureSaveDirectoryExists();
        }

        public void OnEditorInitialize()
        {
            if(_isInitialized) return;

            EditorLogUtility.LogDebug("[SaveSystemEditorModule] 初始化中...");

            _isInitialized = true;
            EditorLogUtility.LogDebug("[SaveSystemEditorModule] 初始化完成");
        }

        /// <summary>
        ///     清理所有存档
        /// </summary>
        public void ClearAllSaves()
        {
            string rootDir = Path.Combine(Application.persistentDataPath, "cframework", "saves");
            if(!Directory.Exists(rootDir))
            {
                Debug.Log($"[CFramework][SaveSystem] 清理所有存档：目录不存在，无需处理。\n路径: {rootDir}");
                EditorUtility.DisplayDialog("清理所有存档", "目录不存在，无需处理。", "确定");
                return;
            }

            if(!EditorUtility.DisplayDialog(
                   "清理所有存档",
                   "此操作将删除所有存档文件（persistentDataPath 下 cframework/saves 目录）。\n" +
                   "建议在停止播放状态下执行，确定继续？",
                   "确定",
                   "取消"))
            {
                return;
            }

            try
            {
                Directory.Delete(rootDir, true);
                Debug.Log($"[CFramework][SaveSystem] 已清理所有存档。\n路径: {rootDir}");
                EditorUtility.DisplayDialog("清理所有存档", "已成功清理所有存档。", "确定");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CFramework][SaveSystem] 清理所有存档失败: {e}\n路径: {rootDir}");
                EditorUtility.DisplayDialog("错误", $"清理失败: {e.Message}", "确定");
            }
        }

        private static void EnsureSaveDirectoryExists()
        {
            string saveDir = Path.Combine(Application.persistentDataPath, "cframework", "saves");

            if(!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
                EditorLogUtility.LogInfo($"[SaveSystemEditorModule] 已创建存档目录: {saveDir}");
            }
        }
    }
}