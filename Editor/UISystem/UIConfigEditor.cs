using System;
using System.Collections.Generic;
using System.Linq;
using CFramework.Systems.UISystem;
using CFramework.Systems.UISystem.Transitions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace CFramework.Editor.UISystem
{
    [CustomEditor(typeof(UIConfig))]
    public class UIConfigEditor : UnityEditor.Editor
    {
        private static Type[] cachedTransitionTypes;
        private static string[] cachedTypeDisplayNames;
        private static string[] cachedTypeNames;
        private SerializedProperty defaultTransitionProp;

        static UIConfigEditor()
        {
            InitializeTransitionTypes();
        }

        private void OnEnable()
        {
            defaultTransitionProp = serializedObject.FindProperty("defaultTransition");
        }

        internal static void InitializeTransitionTypes()
        {
            TypeCache.TypeCollection allTransitionTypes = TypeCache.GetTypesDerivedFrom<UITransition>();

            List<Type> list = new List<Type>();
            foreach (Type type in allTransitionTypes)
            {
                if(type.IsAbstract)
                {
                    continue;
                }

                if(type.FullName == null)
                {
                    continue;
                }

                list.Add(type);
            }

            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            cachedTransitionTypes = list.ToArray();
            cachedTypeNames = cachedTransitionTypes.Select(t => t.Name).ToArray();
            cachedTypeDisplayNames = cachedTransitionTypes
                .Select(t => $"{t.Name}")
                .ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 绘制所有属性，直到遇到defaultTransition
            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true);

            while (property.NextVisible(false))
            {
                if(property.name == "defaultTransition")
                {
                    DrawDefaultTransitionProperty(property);
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            serializedObject.ApplyModifiedProperties();

            // Draw the "Scan Addressables for UI" button at the bottom
            DrawScanButton();
        }

        private void DrawScanButton()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("扫描可寻址资源中UI预制体", MessageType.Info);
            if(GUILayout.Button("扫描可寻址UI", GUILayout.Height(30)))
            {
                ScanAddressablesForUI();
            }
        }

        private void ScanAddressablesForUI()
        {
            UIConfig config = target as UIConfig;
            if(config == null) return;

            // Get AddressableAssetSettings
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if(settings == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到 AddressableAssetSettings", "确定");
                return;
            }

            List<UiInfo> uiList = new List<UiInfo>();
            var foundCount = 0;

            // Iterate through all Addressable entries
            foreach (AddressableAssetGroup group in settings.groups)
            {
                if(group == null) continue;

                foreach (AddressableAssetEntry entry in group.entries)
                {
                    if(entry == null || string.IsNullOrEmpty(entry.address)) continue;

                    // Check if the main asset is a GameObject
                    if(entry.MainAsset == null || entry.MainAssetType != typeof(GameObject))
                        continue;

                    GameObject prefab = entry.MainAsset as GameObject;
                    if(prefab == null) continue;

                    // Check if it has IUIView component
                    IUIView hasController = prefab.GetComponent<IUIView>();
                    if(hasController != null)
                    {
                        string uiName = prefab.name;
                        string uiAddress = entry.address;
                        MonoBehaviour viewComponent = prefab.GetComponent<MonoBehaviour>();
                        string viewTypeName = viewComponent?.GetType().FullName;

                        if(string.IsNullOrEmpty(viewTypeName))
                        {
                            Debug.LogWarning($"预制体 {uiName} 的视图组件类型为空，已跳过");
                            continue;
                        }

                        uiList.Add(new UiInfo
                        {
                            uiName = uiName,
                            uiAddress = uiAddress,
                            viewTypeName = viewTypeName
                        });

                        foundCount++;
                    }
                    // 可选：提示未找到IUIView组件的预制体
                    // Debug.Log($"预制体 {prefab.name} 不包含 IUIView 组件，已跳过");
                }
            }

            if(foundCount > 0)
            {
                config.uis = uiList.ToArray();
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("扫描完成", $"找到 {foundCount} 个 UI 资源", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("扫描完成", "未找到包含 IUIViewController 组件的 UI 资源", "确定");
            }
        }

        private void DrawDefaultTransitionProperty(SerializedProperty property)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(property.displayName);

            if(cachedTransitionTypes == null || cachedTransitionTypes.Length == 0)
            {
                EditorGUILayout.HelpBox("未找到可用的过渡类型。", MessageType.Info);
                EditorGUILayout.EndHorizontal();
                return;
            }

            string currentName = property.stringValue;
            int currentIndex = -1;

            if(!string.IsNullOrEmpty(currentName))
            {
                currentIndex = Array.IndexOf(cachedTypeNames, currentName);
            }

            int newIndex = EditorGUILayout.Popup(currentIndex, cachedTypeDisplayNames);

            if(newIndex >= 0 && newIndex < cachedTypeNames.Length && newIndex != currentIndex)
            {
                property.stringValue = cachedTypeNames[newIndex];
            }

            EditorGUILayout.EndHorizontal();
        }

        #region Transition Type 下拉

        private static void RefreshTransitionTypes()
        {
            TypeCache.TypeCollection allTransitionTypes = TypeCache.GetTypesDerivedFrom<UITransition>();

            List<Type> list = new List<Type>();
            foreach (Type type in allTransitionTypes)
            {
                if(type.IsAbstract)
                {
                    continue;
                }

                if(type.FullName == null)
                {
                    continue;
                }

                list.Add(type);
            }

            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            cachedTransitionTypes = list.ToArray();
            cachedTypeNames = cachedTransitionTypes.Select(t => t.Name).ToArray();
            cachedTypeDisplayNames = cachedTransitionTypes
                .Select(t => $"{t.Name}")
                .ToArray();
        }

        #endregion
    }
}