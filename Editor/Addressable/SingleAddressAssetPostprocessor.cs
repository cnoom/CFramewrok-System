using System;
using System.Collections.Generic;
using CFramework.Attachment.com.cnoom.cframework.systems.Runtime.Attributes;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace CFramework.Editor.AddressablesTools
{
    /// <summary>
    ///     SingleAddressAssetAttribute 后处理器
    ///     自动将带有 SingleAddressAssetAttribute 的资源添加到 Addressables 系统中
    /// </summary>
    public class SingleAddressAssetPostprocessor : AssetPostprocessor
    {
        private static readonly List<string> s_PendingImports = new List<string>();
        private static bool s_ProcessingPending;

        static SingleAddressAssetPostprocessor()
        {
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if(importedAssets == null || importedAssets.Length == 0)
                return;

            // 收集需要处理的资源
            foreach (string assetPath in importedAssets)
            {
                if(ShouldProcessAsset(assetPath))
                {
                    s_PendingImports.Add(assetPath);
                }
            }

            if(s_PendingImports.Count > 0)
            {
                ScheduleProcessing();
            }
        }

        private static bool ShouldProcessAsset(string assetPath)
        {
            // 只处理 ScriptableObject 或 MonoBehaviour 类型的资源
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if(assetType == null) return false;

            // 检查是否有 MonoScript（.cs 脚本文件）
            if(assetType == typeof(MonoScript))
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                if(script == null) return false;

                Type scriptClass = script.GetClass();
                if(scriptClass == null) return false;

                // 检查类是否有 SingleAddressAssetAttribute
                return HasSingleAddressAssetAttribute(scriptClass);
            }

            // 检查是否是 ScriptableObject 资产文件（.asset）
            if(typeof(ScriptableObject).IsAssignableFrom(assetType))
            {
                // 检查资产类型是否有 SingleAddressAssetAttribute
                return HasSingleAddressAssetAttribute(assetType);
            }

            return false;
        }

        private static bool HasSingleAddressAssetAttribute(Type type)
        {
            if(type == null) return false;

            object[] attributes = type.GetCustomAttributes(
                typeof(SingleAddressAssetAttribute),
                false);

            return attributes != null && attributes.Length > 0;
        }

        private static void ScheduleProcessing()
        {
            if(!s_ProcessingPending)
            {
                s_ProcessingPending = true;
                EditorApplication.delayCall += ProcessPendingAssets;
            }
        }

        private static void ProcessPendingAssets()
        {
            s_ProcessingPending = false;
            EditorApplication.delayCall -= ProcessPendingAssets;

            if(s_PendingImports.Count == 0)
                return;

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if(settings == null)
            {
                Debug.LogWarning("[SingleAddressAsset] 未找到 AddressableAssetSettings，无法自动添加资源。");
                s_PendingImports.Clear();
                return;
            }

            foreach (string assetPath in s_PendingImports)
            {
                ProcessAsset(assetPath, settings);
            }

            s_PendingImports.Clear();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ProcessAsset(string assetPath, AddressableAssetSettings settings)
        {
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if(assetType == null) return;

            SingleAddressAssetAttribute[] attributes = null;
            Type assetClassType = null;

            // 处理 .cs 脚本文件
            if(assetType == typeof(MonoScript))
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                if(script == null) return;

                assetClassType = script.GetClass();
                if(assetClassType == null) return;

                attributes = (SingleAddressAssetAttribute[])assetClassType.GetCustomAttributes(
                    typeof(SingleAddressAssetAttribute),
                    false);
            }
            // 处理 .asset 文件
            else if(typeof(ScriptableObject).IsAssignableFrom(assetType))
            {
                assetClassType = assetType;
                attributes = (SingleAddressAssetAttribute[])assetType.GetCustomAttributes(
                    typeof(SingleAddressAssetAttribute),
                    false);
            }

            if(attributes == null || attributes.Length == 0)
                return;

            foreach (SingleAddressAssetAttribute attr in attributes)
            {
                if(assetClassType != null)
                {
                    AddToAddressables(assetClassType, attr, settings);
                }
            }
        }

        private static void AddToAddressables(
            Type assetType,
            SingleAddressAssetAttribute attr,
            AddressableAssetSettings settings)
        {
            // 查找该类型的所有资源实例
            string[] guids = AssetDatabase.FindAssets($"t:{assetType.Name}");
            if(guids == null || guids.Length == 0)
                return;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // 跳过脚本文件本身
                if(assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    // 确定组名
                    string groupName = string.IsNullOrWhiteSpace(attr.Group)
                        ? "Default Local Group"
                        : attr.Group;

                    // 获取或创建组
                    AddressableAssetGroup group = settings.FindGroup(groupName);
                    if(group == null)
                    {
                        group = settings.CreateGroup(groupName, false, false, false, null);
                    }

                    // 创建或移动条目
                    AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
                    if(entry != null)
                    {
                        // 设置地址
                        entry.address = attr.Path;

                        // 设置标签
                        if(!string.IsNullOrWhiteSpace(attr.Label))
                        {
                            settings.AddLabel(attr.Label);
                            entry.SetLabel(attr.Label, true, true);
                        }

                        Debug.Log($"[SingleAddressAsset] 已添加资源到 Addressables: {assetPath} -> {attr.Path}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SingleAddressAsset] 添加资源失败: {assetPath}\n{ex}");
                }
            }
        }
    }
}