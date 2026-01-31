using System.IO;
using CFramework.Core.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor.AddressablesTools
{
    public static class AddressablesContextMenu
    {
        [MenuItem("Assets/CFramework/Addressables/记录文件夹", true)]
        private static bool ValidateRecord()
        {
            Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            foreach (Object obj in selectedObjects)
            {
                if(AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    AddressableConfig reg = ConfigUtility.GetOrCreateEditorConfig<AddressableConfig>();
                    if(!reg.IsRecorded(guid)) continue;
                }

                return false;
            }

            return true;
        }

        [MenuItem("Assets/CFramework/Addressables/记录文件夹")]
        private static void RecordFolder()
        {
            Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            foreach (Object obj in selectedObjects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                RecordFolder(path);
            }
        }

        /// <summary>
        ///     记录当前选中的文件夹（公共接口，供Module调用）
        /// </summary>
        public static void RecordSelectedFolders()
        {
            Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            foreach (Object obj in selectedObjects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                RecordFolder(path);
            }
        }

        private static void RecordFolder(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            AddressableConfig reg = ConfigUtility.GetOrCreateEditorConfig<AddressableConfig>();
            if(reg.IsRecorded(guid))
            {
                EditorUtility.DisplayDialog("已记录", $"该文件夹已被记录：\n{path}", "确定");
                return;
            }

            FolderRecord record = new FolderRecord
            {
                folderGUID = guid,
                groupName = reg.defaultGroupName
            };

            // 根据文件夹名称生成一个同名标签（去除非法字符）
            string folderName = Path.GetFileName(path.TrimEnd('/', '\\'));
            if(!string.IsNullOrWhiteSpace(folderName))
            {
                string label = SanitizeLabel(folderName);
                if(!record.labels.Contains(label))
                {
                    record.labels.Add(label);
                }
            }

            reg.records.Add(record);
            EditorUtility.SetDirty(reg);
            AssetDatabase.SaveAssets();

            AddressablesSyncPipeline.SyncRecord(record);
            EditorUtility.DisplayDialog("已记录", $"已记录并完成同步：\n{path}", "确定");
        }

        private static string SanitizeLabel(string name)
        {
            // Addressables 标签允许任意字符串，但为便于统一，这里将空白替换为下划线并去掉两端空格
            string trimmed = (name ?? string.Empty).Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                trimmed = trimmed.Replace(c, '_');
            }

            return string.IsNullOrEmpty(trimmed) ? "Label" : trimmed.Replace(' ', '_');
        }

        [MenuItem("Assets/CFramework/Addressables/取消记录", true)]
        private static bool ValidateUnrecord()
        {
            Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            foreach (Object obj in selectedObjects)
            {
                if(AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if(!AssetDatabase.IsValidFolder(path)) return false;
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    AddressableConfig reg = ConfigUtility.GetOrCreateEditorConfig<AddressableConfig>();
                    if(reg.IsRecorded(guid)) continue;
                }

                return false;
            }

            return true;
        }

        [MenuItem("Assets/CFramework/Addressables/取消记录")]
        private static void UnrecordFolder()
        {
            Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            foreach (Object obj in selectedObjects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(path);
            AddressableConfig reg = ConfigUtility.GetOrCreateEditorConfig<AddressableConfig>();
            FolderRecord rec = reg.GetRecord(guid);
                if(rec != null)
                {
                    if(EditorUtility.DisplayDialog("取消记录", "是否从记录列表移除并删除该文件夹下的 Addressables 条目？", "是", "否"))
                    {
                        AddressablesSyncPipeline.RemoveAllEntriesUnderRecord(rec);
                    }

                    reg.records.Remove(rec);
                    EditorUtility.SetDirty(reg);
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("已取消记录", $"已取消记录：\n{path}", "确定");
                }
            }
        }

        [MenuItem("Assets/CFramework/Addressables/Sync Recorded Folders")]
        private static void SyncAll()
        {
            AddressableConfig reg = ConfigUtility.GetOrCreateEditorConfig<AddressableConfig>();
            AddressablesSyncPipeline.SyncAll(reg);
            EditorUtility.DisplayDialog("同步完成", "已同步所有记录的文件夹。", "确定");
        }

        [MenuItem("Assets/CFramework/Addressables/Generate Address Keys")]
        private static void GenerateKeys()
        {
            AddressableConfig reg = ConfigUtility.GetOrCreateEditorConfig<AddressableConfig>();
            AddressablesCodeGen.Generate(reg);
            EditorUtility.DisplayDialog("生成完成", "已生成地址常量。", "确定");
        }

        [MenuItem("Assets/CFramework/Addressables/Sync Single Address Assets")]
        private static void SyncSingleAddressAssets()
        {
            SingleAddressAssetProcessor.ProcessAll();
            EditorUtility.DisplayDialog("同步完成", "已同步所有带有 SingleAddressAssetAttribute 的资源。", "确定");
        }
    }
}