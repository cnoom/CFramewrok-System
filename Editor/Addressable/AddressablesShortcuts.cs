using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.Utilities;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace CFramework.Editor.AddressablesTools
{
    /// <summary>
    ///     Addressables 子系统的标准快捷键定义
    ///     快捷键通过特性注册，实际逻辑直接调用相关服务
    /// </summary>
    public static class AddressablesShortcuts
    {
        /// <summary>
        ///     打开 Addressables 管理器窗口
        ///     推荐组合：Action + Shift + 1
        /// </summary>
        [Shortcut(
            AddressableShortcutKey.OpenManagerWindow,
            KeyCode.Alpha1,
            ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        private static void OpenManagerWindow()
        {
            EditorApplication.ExecuteMenuItem(CFMenuKey.Systems + "/Addressables/地址配置");
        }

        /// <summary>
        ///     同步所有已记录的 Addressables 文件夹
        ///     推荐组合：Action + Shift + F12
        /// </summary>
        [Shortcut(
            AddressableShortcutKey.SyncAll,
            KeyCode.F12,
            ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        private static void SyncAll()
        {
            AddressableConfig registry = ConfigUtility.GetOrCreateEditorConfig<AddressableConfig>();
            AddressablesSyncPipeline.SyncAll(registry);
            Debug.Log("[CFramework][Addressables] 已同步所有记录文件夹");
        }

        /// <summary>
        ///     记录当前选中的文件夹
        ///     推荐组合：Action + Shift + F10
        /// </summary>
        [Shortcut(
            AddressableShortcutKey.RecordFolder,
            KeyCode.F10,
            ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        private static void RecordFolder()
        {
            AddressablesContextMenu.RecordSelectedFolders();
        }

        /// <summary>
        ///     生成 Addressables 常量代码
        ///     推荐组合：Action + Shift + F11
        /// </summary>
        [Shortcut(
            AddressableShortcutKey.GenerateConstants,
            KeyCode.F11,
            ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        private static void GenerateConstants()
        {
            AddressableConfig registry = ConfigUtility.GetOrCreateEditorConfig<AddressableConfig>();
            AddressablesCodeGen.Generate(registry);
            Debug.Log("[CFramework][Addressables] 已生成地址常量");
        }
    }
}