using System;
using System.Collections.Generic;
using CFramework.Core.Editor.Attributes;
using CFramework.Core.Editor.Base;
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
    public class AddressablesFolderRegistry : ScriptableObject
    {

        public List<FolderRecord> records = new List<FolderRecord>();

        // 全局默认配置
        public string defaultGroupName = "Default Group";
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
}