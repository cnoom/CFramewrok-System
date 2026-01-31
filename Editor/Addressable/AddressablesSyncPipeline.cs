using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace CFramework.Editor.AddressablesTools
{
    public static class AddressablesSyncPipeline
    {
        public static void SyncRecord(FolderRecord rec)
        {
            foreach (string path in AddressablesFolderService.EnumerateAssetsUnderFolder(rec.folderGUID, rec))
            {
                AddressablesFolderService.AddOrUpdateEntry(path, rec);
            }
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if(settings != null)
            {
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            }
        }

        public static void RemoveAllEntriesUnderRecord(FolderRecord rec)
        {
            foreach (string path in AddressablesFolderService.EnumerateAssetsUnderFolder(rec.folderGUID, rec))
            {
                AddressablesFolderService.RemoveEntryByPath(path);
            }
        }

        public static void SyncAll(AddressableConfig reg)
        {
            foreach (FolderRecord rec in reg.records)
            {
                SyncRecord(rec);
            }
        }
    }
}