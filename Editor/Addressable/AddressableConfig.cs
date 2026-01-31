using System;
using System.Collections.Generic;
using CFramework.Core.Editor.Attributes;
using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor.AddressablesTools
{
    public enum AddressNamingRule
    {
        FolderRelative,
        FileName,
        PathBased,
        Custom
    }

    public enum CodeGenStructure
    {
        [InspectorName("单文件")]
        SingleClass,
        [InspectorName("嵌套分组")]
        NestedByGroup,
        [InspectorName("按组拆分")]
        SplitFilesByGroup
    }

    [Serializable]
    public class FolderRecord
    {
        public string folderGUID;
        public string groupName = "Default Group";
        public List<string> labels = new List<string>();
        public string addressPrefix = string.Empty;

        public List<string> includeExtensions = new List<string>
        {
            ".prefab",
            ".png",
            ".jpg",
            ".jpeg",
            ".mat",
            ".asset",
            ".controller",
            ".anim",
            ".unity",
            ".wav",
            ".mp3",
            ".ogg",
            ".txt",
            ".json",
            ".xml",
            ".bytes",
            ".csv"
        };

        public List<string> excludePatterns = new List<string>
        {
            "*.meta",
            "*/Editor/*",
            "*/Tests/*"
        };
        public bool autoSync = true;
        public AddressNamingRule namingRule = AddressNamingRule.FolderRelative;
        public string customTemplate = "{Group}/{RelPathNoExt}";
        public string codeGenOutputPathOverride = string.Empty; // 为空时使用全局默认
    }

    [EditorConfig]
    public class AddressableConfig : ScriptableObject
    {

        public List<FolderRecord> records = new List<FolderRecord>();

        // 全局默认配置
        public string defaultGroupName = "Default Local Group";
        public string codeGenNamespace = "CFramework.Generate";
        public string codeGenClassName = "AddressKeys";
        public CodeGenStructure codeGenStructure = CodeGenStructure.NestedByGroup;

        public bool IsRecorded(string folderGUID)
        {
            return records.Exists(r => r.folderGUID == folderGUID);
        }
        public FolderRecord GetRecord(string folderGUID)
        {
            return records.Find(r => r.folderGUID == folderGUID);
        }
    }

    [CustomEditor(typeof(AddressableConfig))]
    public class AddressableConfigEditor : UnityEditor.Editor
    {
        private Vector2 _scroll;

        public override void OnInspectorGUI()
        {
            AddressableConfig config = (AddressableConfig)target;

            EditorGUILayout.LabelField("全局设置", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            config.codeGenNamespace = EditorGUILayout.TextField("命名空间", config.codeGenNamespace);
            config.codeGenClassName = EditorGUILayout.TextField("类名", config.codeGenClassName);
            config.codeGenStructure = (CodeGenStructure)EditorGUILayout.EnumPopup("生成结构", config.codeGenStructure);
            config.defaultGroupName = EditorGUILayout.TextField("默认组名", config.defaultGroupName);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("同步全部", GUILayout.Height(24)))
            {
                AddressablesSyncPipeline.SyncAll(config);
            }
            if(GUILayout.Button("生成常量", GUILayout.Height(24)))
            {
                AddressablesCodeGen.Generate(config);
            }
            if(GUILayout.Button("保存", GUILayout.Height(24)))
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("已记录的文件夹", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for(var i = 0; i < config.records.Count; i++)
            {
                FolderRecord r = config.records[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"记录项 #{i + 1}", EditorStyles.miniBoldLabel);
                using (new EditorGUI.DisabledScope(true))
                {
                    string path = AssetDatabase.GUIDToAssetPath(r.folderGUID);
                    EditorGUILayout.TextField("文件夹", path);
                }
                r.groupName = EditorGUILayout.TextField("组名", string.IsNullOrEmpty(r.groupName) ? config.defaultGroupName : r.groupName);
                r.addressPrefix = EditorGUILayout.TextField("地址前缀", r.addressPrefix);
                r.autoSync = EditorGUILayout.Toggle("自动同步", r.autoSync);
                r.namingRule = (AddressNamingRule)EditorGUILayout.EnumPopup("命名规则", r.namingRule);
                if(r.namingRule == AddressNamingRule.Custom)
                {
                    r.customTemplate = EditorGUILayout.TextField("模板", r.customTemplate);
                }
                r.codeGenOutputPathOverride = EditorGUILayout.TextField("生成路径覆盖", r.codeGenOutputPathOverride);

                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Sync This"))
                {
                    AddressablesSyncPipeline.SyncRecord(r);
                }
                if(GUILayout.Button("移除条目"))
                {
                    if(EditorUtility.DisplayDialog("移除条目", "是否移除该记录项下的所有 Addressables 条目?", "是", "否"))
                    {
                        AddressablesSyncPipeline.RemoveAllEntriesUnderRecord(r);
                    }
                }
                if(GUILayout.Button("Unrecord"))
                {
                    if(EditorUtility.DisplayDialog("取消记录", "是否从记录表移除并(可选)移除相关条目?", "是", "否"))
                    {
                        AddressablesSyncPipeline.RemoveAllEntriesUnderRecord(r);
                        config.records.RemoveAt(i);
                        EditorUtility.SetDirty(config);
                        AssetDatabase.SaveAssets();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        [MenuItem(CFMenuKey.Systems + "/Addressables/地址配置")]
        private static void SelectConfig()
        {
            AddressableConfig config = ConfigUtility.GetOrCreateEditorConfig<AddressableConfig>();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
    }
}