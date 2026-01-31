using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.EditorFramework;
using UnityEditor;

namespace CFramework.Editor.UISystem
{
    /// <summary>
    ///     UI系统相关的编辑器工具。
    ///     提供生成层级常量和动画常量的菜单功能。
    /// </summary>
    public static class UISystemEditorTools
    {
        private static UISystemEditorModule Module => EditorCF.GetModule<UISystemEditorModule>();

        /// <summary>
        ///     生成 UI 层级常量
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/UI/生成层级常量", priority = 5200)]
        public static void GenerateLayerConstants()
        {
            Module?.GenerateLayerConstants();
        }

        /// <summary>
        ///     生成 UI 动画常量
        /// </summary>
        [MenuItem(CFMenuKey.Systems + "/UI/生成动画常量", priority = 5201)]
        public static void GenerateTransitionConstants()
        {
            Module?.GenerateTransitionConstants();
        }
    }
}