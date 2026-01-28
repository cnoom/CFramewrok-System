using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.EditorFramework;
using UnityEditor;

namespace CFramework.Editor.SaveSystem
{
    /// <summary>
    ///     存档系统相关的编辑器工具。
    ///     提供一键清理所有存档的功能。
    /// </summary>
    public static class SaveSystemEditorTools
    {
        private static SaveSystemEditorModule Module => EditorCF.GetModule<SaveSystemEditorModule>();

        /// <summary>
        ///     清理所有存档：删除 persistentDataPath 下 cframework/saves 目录。
        ///     建议在非播放状态下执行，避免运行中的存档系统仍持有内存缓存。
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/Save/清理所有存档", priority = 5100)]
        private static void ClearAllSavesMenu()
        {
            Module?.ClearAllSaves();
        }
    }
}