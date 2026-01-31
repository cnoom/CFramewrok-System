using System;
using System.Collections.Generic;
using System.Reflection;
using CFramework.Attachment.com.cnoom.cframework.systems.Runtime.Attributes;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace CFramework.Editor.AddressablesTools
{
    /// <summary>
    ///     SingleAddressAssetAttribute 处理器
    ///     提供手动同步所有带有该特性的资源的功能
    /// </summary>
    public static class SingleAddressAssetProcessor
    {
        /// <summary>
        ///     处理所有带有 SingleAddressAssetAttribute 的类型
        /// </summary>
        public static void ProcessAll()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if(settings == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到 AddressableAssetSettings，请先在 Addressables 窗口创建设置。", "确定");
                return;
            }

            // 获取所有程序集中的所有类型
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var processedCount = 0;
            var errorCount = 0;

            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        SingleAddressAssetAttribute[] attributes = (SingleAddressAssetAttribute[])type.GetCustomAttributes(
                            typeof(SingleAddressAssetAttribute),
                            false);

                        if(attributes == null || attributes.Length == 0)
                            continue;

                        foreach (SingleAddressAssetAttribute attr in attributes)
                        {
                            try
                            {
                                ProcessType(type, attr, settings);
                                processedCount++;
                            }
                            catch (Exception ex)
                            {
                                errorCount++;
                                Debug.LogError($"[SingleAddressAsset] 处理类型失败: {type.FullName}\n{ex}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SingleAddressAsset] 无法访问程序集: {assembly.FullName}\n{ex}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SingleAddressAsset] 处理完成: {processedCount} 个类型成功, {errorCount} 个失败");
        }

        private static void ProcessType(Type type, SingleAddressAssetAttribute attr, AddressableAssetSettings settings)
        {
            // 只处理 ScriptableObject 或 MonoBehaviour 类型
            if(typeof(ScriptableObject).IsAssignableFrom(type) || typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                ProcessTypeAssets(type, attr, settings);
            }
        }

        private static void ProcessTypeAssets(Type type, SingleAddressAssetAttribute attr, AddressableAssetSettings settings)
        {
            // 查找该类型的所有资源实例
            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
            if(guids == null || guids.Length == 0)
                return;

            // 确定组名
            string groupName = string.IsNullOrWhiteSpace(attr.Group)
                ? "Default Local Group"
                : attr.Group;

            // 获取或创建组
            AddressableAssetGroup group = settings.FindGroup(groupName);
            if(group == null)
            {
                group = settings.CreateGroup(groupName, false, false, false, null);
                Debug.Log($"[SingleAddressAsset] 创建 Addressables 组: {groupName}");
            }

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // 跳过脚本文件本身
                if(assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
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

                        Debug.Log($"[SingleAddressAsset] 已添加资源: {assetPath} -> {attr.Path} (组: {groupName})");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SingleAddressAsset] 添加资源失败: {assetPath}\n{ex}");
                }
            }
        }

        /// <summary>
        ///     获取所有带有 SingleAddressAssetAttribute 的类型信息
        /// </summary>
        public static List<TypeInfo> GetAnnotatedTypes()
        {
            List<TypeInfo> result = new List<TypeInfo>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        SingleAddressAssetAttribute[] attributes = (SingleAddressAssetAttribute[])type.GetCustomAttributes(
                            typeof(SingleAddressAssetAttribute),
                            false);

                        if(attributes == null || attributes.Length == 0)
                            continue;

                        foreach (SingleAddressAssetAttribute attr in attributes)
                        {
                            result.Add(new TypeInfo
                            {
                                Type = type,
                                Path = attr.Path,
                                Group = attr.Group,
                                Label = attr.Label
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    // 忽略无法访问的程序集
                }
            }

            return result;
        }

        /// <summary>
        ///     类型信息
        /// </summary>
        public class TypeInfo
        {
            public Type Type { get; set; }
            public string Path { get; set; }
            public string Group { get; set; }
            public string Label { get; set; }

            public string TypeName => Type?.FullName ?? string.Empty;
            public string AssetCount => AssetDatabase.FindAssets($"t:{Type.Name}").Length.ToString();
        }
    }
}