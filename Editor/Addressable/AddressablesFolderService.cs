using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace CFramework.Editor.AddressablesTools
{
    public static class AddressablesFolderService
    {
        public static AddressableAssetSettings Settings => AddressableAssetSettingsDefaultObject.Settings;

        public static AddressableAssetGroup EnsureGroup(string groupName)
        {
            if(!Settings) throw new InvalidOperationException("AddressableAssetSettings not found. Please create Addressables settings (Window > Asset Management > Addressables > Groups).");
            if(string.IsNullOrWhiteSpace(groupName)) groupName = "Default Local Group";
            AddressableAssetGroup grp = Settings.FindGroup(groupName);
            if(!grp)
            {
                grp = Settings.CreateGroup(groupName, false, false, false, null,
                    typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            }
            return grp;
        }

        public static IEnumerable<string> EnumerateAssetsUnderFolder(string folderGUID, FolderRecord record)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(folderGUID);
            if(string.IsNullOrEmpty(folderPath)) yield break;

            string absoluteFolder = folderPath.Replace('\\', '/');
            foreach (string path in Directory.EnumerateFiles(absoluteFolder, "*", SearchOption.AllDirectories))
            {
                if(path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                string normalized = path.Replace('\\', '/');

                string ext = Path.GetExtension(normalized)?.ToLowerInvariant();
                if(!string.IsNullOrEmpty(ext) && record.includeExtensions.Count > 0 && !record.includeExtensions.Contains(ext))
                    continue;

                if(record.excludePatterns.Any(p => MatchPattern(normalized, p))) continue;

                yield return normalized;
            }
        }

        private static bool MatchPattern(string path, string pattern)
        {
            // 简易通配符处理，满足 *.meta / */Editor/* / */Tests/*
            if(pattern == "*.meta") return path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase);
            if(pattern.Contains("*/Editor/*")) return path.IndexOf("/Editor/", StringComparison.OrdinalIgnoreCase) >= 0;
            if(pattern.Contains("*/Tests/*")) return path.IndexOf("/Tests/", StringComparison.OrdinalIgnoreCase) >= 0;
            return false;
        }

        public static string MakeAddress(string assetPath, FolderRecord record)
        {
            string relPathNoExt = GetRelativeWithoutExtension(assetPath, record.folderGUID);
            string prefix = string.IsNullOrEmpty(record.addressPrefix) ? string.Empty : record.addressPrefix.TrimEnd('/') + "/";
            return record.namingRule switch
            {
                AddressNamingRule.FileName => prefix + Path.GetFileNameWithoutExtension(assetPath),
                AddressNamingRule.FolderRelative => prefix + relPathNoExt,
                AddressNamingRule.PathBased => prefix + assetPath.TrimStart('/'),
                AddressNamingRule.Custom => ApplyTemplate(record.customTemplate, assetPath, relPathNoExt, record.groupName),
                _ => prefix + relPathNoExt
            };
        }

        private static string GetRelativeWithoutExtension(string assetPath, string folderGUID)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(folderGUID).Replace('\\', '/');
            string rel = assetPath.Replace('\\', '/');
            if(rel.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase))
                rel = rel.Substring(folderPath.Length).TrimStart('/');
            string noExt = Path.ChangeExtension(rel, null);
            return noExt;
        }

        private static string ApplyTemplate(string tpl, string assetPath, string relNoExt, string group)
        {
            string fileNameNoExt = Path.GetFileNameWithoutExtension(assetPath);
            string folderName = new DirectoryInfo(Path.GetDirectoryName(assetPath) ?? string.Empty).Name;
            return (tpl ?? string.Empty)
                .Replace("{Path}", assetPath)
                .Replace("{RelPathNoExt}", relNoExt)
                .Replace("{Group}", group ?? string.Empty)
                .Replace("{FileNameNoExt}", fileNameNoExt ?? string.Empty)
                .Replace("{FolderName}", folderName ?? string.Empty)
                .Trim();
        }

        public static void AddOrUpdateEntry(string assetPath, FolderRecord record)
        {
            if(record == null) return;
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if(string.IsNullOrEmpty(guid)) return;

            AddressableAssetGroup group = EnsureGroup(string.IsNullOrWhiteSpace(record.groupName) ? "Default Local Group" : record.groupName);
            AddressableAssetEntry entry = Settings.CreateOrMoveEntry(guid, group);
            entry.address = MakeAddress(assetPath, record);

            foreach (string label in record.labels)
            {
                if(!string.IsNullOrEmpty(label))
                {
                    Settings.AddLabel(label);
                    entry.SetLabel(label, true, true);
                }
            }
        }

        public static void RemoveEntryByPath(string assetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if(string.IsNullOrEmpty(guid)) return;
            AddressableAssetEntry entry = Settings.FindAssetEntry(guid);
            if(entry != null)
            {
                Settings.RemoveAssetEntry(guid);
            }
        }
    }
}