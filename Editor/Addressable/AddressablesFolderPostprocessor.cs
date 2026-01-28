using System.Collections.Generic;
using System.Linq;
using CFramework.Core.Editor.Utilities;
using UnityEditor;

namespace CFramework.Editor.AddressablesTools
{
    /// <summary>
    ///     Addressables 资源后处理器
    ///     监听资源变化并自动同步到 Addressables 系统
    /// </summary>
    public class AddressablesFolderPostprocessor : AssetPostprocessor
    {
        private static readonly List<string> s_AddedAssets = new List<string>();
        private static readonly List<string> s_DeletedAssets = new List<string>();
        private static readonly List<string> s_MovedAssets = new List<string>();
        private static readonly List<string> s_MovedFromAssets = new List<string>();
        private static bool s_PendingProcess;

        static AddressablesFolderPostprocessor()
        {
            // 在构造函数中初始化
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if(ShouldSkipProcessing(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths))
                return;

            CollectAssetChanges(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);

            ScheduleProcessChanges();
        }

        private static bool ShouldSkipProcessing(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            return (importedAssets?.Length ?? 0) == 0
                   && (deletedAssets?.Length ?? 0) == 0
                   && (movedAssets?.Length ?? 0) == 0
                   && (movedFromAssetPaths?.Length ?? 0) == 0;
        }

        private static void CollectAssetChanges(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if(importedAssets != null) s_AddedAssets.AddRange(importedAssets);
            if(deletedAssets != null) s_DeletedAssets.AddRange(deletedAssets);
            if(movedAssets != null) s_MovedAssets.AddRange(movedAssets);
            if(movedFromAssetPaths != null) s_MovedFromAssets.AddRange(movedFromAssetPaths);
        }

        private static void ScheduleProcessChanges()
        {
            if(!s_PendingProcess)
            {
                s_PendingProcess = true;
                EditorApplication.delayCall += ProcessAssetChanges;
            }
        }

        private static void ProcessAssetChanges()
        {
            s_PendingProcess = false;
            EditorApplication.delayCall -= ProcessAssetChanges;

            AddressablesFolderRegistry registry = ConfigUtility.GetEditorConfig<AddressablesFolderRegistry>();
            List<FolderRecord> autoSyncRecords = GetAutoSyncRecords(registry);

            if(autoSyncRecords.Count == 0)
            {
                ClearAssetChanges();
                return;
            }

            ProcessAddedAssets(autoSyncRecords);
            ProcessDeletedAssets();
            ProcessMovedAssets(autoSyncRecords);

            ClearAssetChanges();
        }

        private static List<FolderRecord> GetAutoSyncRecords(AddressablesFolderRegistry registry)
        {
            return registry.records.Where(r => r.autoSync).ToList();
        }

        private static void ProcessAddedAssets(List<FolderRecord> autoSyncRecords)
        {
            foreach (string assetPath in s_AddedAssets)
            {
                FolderRecord record = FindRecordForAssetPath(assetPath, autoSyncRecords);
                if(record != null)
                {
                    AddressablesFolderService.AddOrUpdateEntry(assetPath, record);
                }
            }
        }

        private static void ProcessDeletedAssets()
        {
            foreach (string assetPath in s_DeletedAssets)
            {
                AddressablesFolderService.RemoveEntryByPath(assetPath);
            }
        }

        private static void ProcessMovedAssets(List<FolderRecord> autoSyncRecords)
        {
            for(var i = 0; i < s_MovedAssets.Count; i++)
            {
                string newAssetPath = s_MovedAssets[i];
                FolderRecord record = FindRecordForAssetPath(newAssetPath, autoSyncRecords);
                if(record != null)
                {
                    AddressablesFolderService.AddOrUpdateEntry(newAssetPath, record);
                }
            }
        }

        private static FolderRecord FindRecordForAssetPath(string assetPath, List<FolderRecord> records)
        {
            foreach (FolderRecord record in records)
            {
                string folderPath = NormalizePath(AssetDatabase.GUIDToAssetPath(record.folderGUID));
                if(!string.IsNullOrEmpty(folderPath) && NormalizePath(assetPath).StartsWith(folderPath + "/"))
                {
                    return record;
                }
            }
            return null;
        }

        private static string NormalizePath(string path)
        {
            return path?.Replace('\\', '/') ?? string.Empty;
        }

        private static void ClearAssetChanges()
        {
            s_AddedAssets.Clear();
            s_DeletedAssets.Clear();
            s_MovedAssets.Clear();
            s_MovedFromAssets.Clear();
        }
    }
}