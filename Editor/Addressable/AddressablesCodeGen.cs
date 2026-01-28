using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CFramework.Core.Editor.Base;
using CNoom.UnityCodeGen.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace CFramework.Editor.AddressablesTools
{
    public static class AddressablesCodeGen
    {
        private static readonly string[] BuiltInGroupNames =
        {
            "Built In Data",
            "Built_In_Data"
        };

        public static void Generate(AddressablesFolderRegistry reg)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if(!settings)
                throw new InvalidOperationException("未找到 AddressableAssetSettings，请先在 Addressables 窗口创建设置。");

            string ns = string.IsNullOrWhiteSpace(reg.codeGenNamespace) ? "CFramework.Addressables" : reg.codeGenNamespace;
            string className = string.IsNullOrWhiteSpace(reg.codeGenClassName) ? "AddressKeys" : reg.codeGenClassName;
            string labelsClassName = className + "Labels";
            string outputPath = CFDirectoryKey.FrameworkGenerate;
            List<string> overridePaths = reg.records
                .Select(r => r.codeGenOutputPathOverride)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if(overridePaths.Count == 1)
            {
                outputPath = overridePaths[0];
            }
            else if(overridePaths.Count > 1)
            {
                Debug.LogWarning("[CFramework][Addressables][CodeGen] 检测到多个不同的输出路径覆盖，已忽略并使用默认输出路径。");
            }

            HashSet<string> generatedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            SortedDictionary<string, List<AddressableAssetEntry>> groupToEntries = CollectGroupEntries(settings);

            switch(reg.codeGenStructure)
            {
                case CodeGenStructure.SingleClass:
                    GenerateSingleClass(outputPath, ns, className, groupToEntries, generatedFiles);
                    break;
                case CodeGenStructure.NestedByGroup:
                    GenerateNestedByGroup(outputPath, ns, className, groupToEntries, generatedFiles);
                    break;
                case CodeGenStructure.SplitFilesByGroup:
                    GenerateSplitFilesByGroup(outputPath, ns, className, groupToEntries, generatedFiles);
                    break;
                case CodeGenStructure.PartialCombined:
                    GeneratePartialCombined(outputPath, ns, className, groupToEntries, generatedFiles);
                    break;
                default:
                    GenerateNestedByGroup(outputPath, ns, className, groupToEntries, generatedFiles);
                    break;
            }

            List<string> allLabels = settings.GetLabels()?.ToList() ?? new List<string>();
            GenerateLabelsClass(outputPath, ns, labelsClassName, allLabels, generatedFiles);
            CleanupObsoleteGeneratedFiles(outputPath, className, labelsClassName, generatedFiles);

            AssetDatabase.Refresh();
        }

        private static SortedDictionary<string, List<AddressableAssetEntry>> CollectGroupEntries(
            AddressableAssetSettings settings)
        {
            SortedDictionary<string, List<AddressableAssetEntry>> groupToEntries =
                new SortedDictionary<string, List<AddressableAssetEntry>>(StringComparer.OrdinalIgnoreCase);
            foreach (AddressableAssetGroup g in settings.groups.Where(g => g))
            {
                if(IsBuiltInGroup(g))
                {
                    continue;
                }

                List<AddressableAssetEntry> list = g.entries?.ToList() ?? new List<AddressableAssetEntry>();
                if(!groupToEntries.ContainsKey(g.Name)) groupToEntries[g.Name] = new List<AddressableAssetEntry>();
                groupToEntries[g.Name].AddRange(list);
            }

            return groupToEntries;
        }

        private static List<Dictionary<string, object>> PrepareGroupData(
            SortedDictionary<string, List<AddressableAssetEntry>> groups,
            bool shareUsedNames = false)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            HashSet<string> globalKeywords = IdentifierUtility.CSharpKeywords;
            HashSet<string> sharedUsedNames = shareUsedNames ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) : null;

            foreach (KeyValuePair<string, List<AddressableAssetEntry>> kv in groups)
            {
                string groupClass = IdentifierUtility.ToTypeIdentifier(kv.Key);
                HashSet<string> usedNames = sharedUsedNames ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                List<Dictionary<string, object>> entries = new List<Dictionary<string, object>>();

                foreach (AddressableAssetEntry e in kv.Value.OrderBy(e => e.address, StringComparer.Ordinal))
                {
                    string unique = GenerateUniqueIdentifier(e.address, globalKeywords, usedNames);
                    entries.Add(new Dictionary<string, object>
                    {
                        {
                            "Address", e.address
                        },
                        {
                            "Identifier", unique
                        }
                    });
                }

                result.Add(new Dictionary<string, object>
                {
                    {
                        "GroupName", kv.Key
                    },
                    {
                        "GroupClassName", groupClass
                    },
                    {
                        "Entries", entries
                    }
                });
            }

            return result;
        }

        private static void GenerateSingleClass(string outputDir, string ns, string className,
            SortedDictionary<string, List<AddressableAssetEntry>> groups, ISet<string> generatedFiles)
        {
            string file = Path.Combine(outputDir, className + ".cs");
            CodeGenTemplate template = new CodeGenTemplate("SingleClass", Templates.TemplateSingleClass);
            CodeGenContext context = new CodeGenContext()
                .Set("Namespace", ns)
                .Set("ClassName", className)
                .Set("Groups", PrepareGroupData(groups));

            CodeGenRequest request = new CodeGenRequest
            {
                OutputPath = file,
                Template = template,
                Context = context,
                WriteMode = CodeGenWriteMode.Overwrite
            };

            CodeGen.Generate(request);
            TrackGeneratedFile(file, generatedFiles);
        }

        private static void GenerateNestedByGroup(string outputDir, string ns, string className,
            SortedDictionary<string, List<AddressableAssetEntry>> groups, ISet<string> generatedFiles)
        {
            string file = Path.Combine(outputDir, className + ".cs");
            CodeGenTemplate template =
                new CodeGenTemplate("NestedByGroup", Templates.TemplateNestedByGroup);
            CodeGenContext context = new CodeGenContext()
                .Set("Namespace", ns)
                .Set("ClassName", className)
                .Set("Groups", PrepareGroupData(groups));

            CodeGenRequest request = new CodeGenRequest
            {
                OutputPath = file,
                Template = template,
                Context = context,
                WriteMode = CodeGenWriteMode.Overwrite
            };

            CodeGen.Generate(request);
            TrackGeneratedFile(file, generatedFiles);
        }

        private static void GenerateSplitFilesByGroup(string outputDir, string ns, string className,
            SortedDictionary<string, List<AddressableAssetEntry>> groups, ISet<string> generatedFiles)
        {
            string mainFile = Path.Combine(outputDir, className + ".cs");
            CodeGenTemplate mainTemplate =
                new CodeGenTemplate("SplitByGroupMain", Templates.TemplateSplitByGroupMain);
            CodeGenContext mainContext = new CodeGenContext()
                .Set("Namespace", ns)
                .Set("ClassName", className);

            CodeGen.Generate(new CodeGenRequest
            {
                OutputPath = mainFile,
                Template = mainTemplate,
                Context = mainContext,
                WriteMode = CodeGenWriteMode.Overwrite
            });
            TrackGeneratedFile(mainFile, generatedFiles);

            foreach (KeyValuePair<string, List<AddressableAssetEntry>> kv in groups)
            {
                string groupClass = IdentifierUtility.ToTypeIdentifier(kv.Key);
                string file = Path.Combine(outputDir, $"{className}_{groupClass}.cs");
                HashSet<string> usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                List<Dictionary<string, object>> entries = new List<Dictionary<string, object>>();

                foreach (AddressableAssetEntry e in kv.Value.OrderBy(e => e.address, StringComparer.Ordinal))
                {
                    string unique = GenerateUniqueIdentifier(e.address, IdentifierUtility.CSharpKeywords, usedNames);
                    entries.Add(new Dictionary<string, object>
                    {
                        {
                            "Address", e.address
                        },
                        {
                            "Identifier", unique
                        }
                    });
                }

                CodeGenTemplate template =
                    new CodeGenTemplate("SplitByGroup", Templates.TemplateSplitByGroup);
                CodeGenContext context = new CodeGenContext()
                    .Set("Namespace", ns)
                    .Set("ClassName", className)
                    .Set("GroupClassName", groupClass)
                    .Set("Entries", entries);

                CodeGen.Generate(new CodeGenRequest
                {
                    OutputPath = file,
                    Template = template,
                    Context = context,
                    WriteMode = CodeGenWriteMode.Overwrite
                });
                TrackGeneratedFile(file, generatedFiles);
            }
        }

        private static void GeneratePartialCombined(string outputDir, string ns, string className,
            SortedDictionary<string, List<AddressableAssetEntry>> groups, ISet<string> generatedFiles)
        {
            string mainFile = Path.Combine(outputDir, className + ".cs");
            CodeGenTemplate mainTemplate =
                new CodeGenTemplate("SplitByGroupMain", Templates.TemplateSplitByGroupMain);
            CodeGenContext mainContext = new CodeGenContext()
                .Set("Namespace", ns)
                .Set("ClassName", className);

            CodeGen.Generate(new CodeGenRequest
            {
                OutputPath = mainFile,
                Template = mainTemplate,
                Context = mainContext,
                WriteMode = CodeGenWriteMode.Overwrite
            });
            TrackGeneratedFile(mainFile, generatedFiles);

            HashSet<string> globalKeywords = IdentifierUtility.CSharpKeywords;
            HashSet<string> globalUsedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, List<AddressableAssetEntry>> kv in groups)
            {
                string groupClass = IdentifierUtility.ToTypeIdentifier(kv.Key);
                string file = Path.Combine(outputDir, $"{className}.{groupClass}.cs");
                List<Dictionary<string, object>> entries = new List<Dictionary<string, object>>();

                foreach (AddressableAssetEntry e in kv.Value.OrderBy(e => e.address, StringComparer.Ordinal))
                {
                    string unique = GenerateUniqueIdentifier(e.address, globalKeywords, globalUsedNames);
                    entries.Add(new Dictionary<string, object>
                    {
                        {
                            "Address", e.address
                        },
                        {
                            "Identifier", unique
                        }
                    });
                }

                CodeGenTemplate template =
                    new CodeGenTemplate("PartialCombined", Templates.TemplatePartialCombined);
                CodeGenContext context = new CodeGenContext()
                    .Set("Namespace", ns)
                    .Set("ClassName", className)
                    .Set("GroupClassName", groupClass)
                    .Set("Entries", entries);

                CodeGen.Generate(new CodeGenRequest
                {
                    OutputPath = file,
                    Template = template,
                    Context = context,
                    WriteMode = CodeGenWriteMode.Overwrite
                });
                TrackGeneratedFile(file, generatedFiles);
            }
        }

        private static void GenerateLabelsClass(string outputDir, string ns, string labelsClassName,
            List<string> labels, ISet<string> generatedFiles)
        {
            string file = Path.Combine(outputDir, labelsClassName + ".cs");
            HashSet<string> usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<Dictionary<string, object>> labelList = new List<Dictionary<string, object>>();

            foreach (string label in labels.OrderBy(l => l, StringComparer.Ordinal))
            {
                if(string.IsNullOrWhiteSpace(label)) continue;
                string unique = GenerateUniqueIdentifier(label, IdentifierUtility.CSharpKeywords, usedNames);
                labelList.Add(new Dictionary<string, object>
                {
                    {
                        "Label", label
                    },
                    {
                        "Identifier", unique
                    }
                });
            }

            CodeGenTemplate template = new CodeGenTemplate("Labels", Templates.TemplateLabels);
            CodeGenContext context = new CodeGenContext()
                .Set("Namespace", ns)
                .Set("ClassName", labelsClassName)
                .Set("Labels", labelList);

            CodeGen.Generate(new CodeGenRequest
            {
                OutputPath = file,
                Template = template,
                Context = context,
                WriteMode = CodeGenWriteMode.Overwrite
            });
            TrackGeneratedFile(file, generatedFiles);
        }

        private static bool IsBuiltInGroup(AddressableAssetGroup group)
        {
            if(group == null)
            {
                return false;
            }

            return BuiltInGroupNames.Any(name =>
                string.Equals(group.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static void CleanupObsoleteGeneratedFiles(string outputDir, string className, string labelsClassName,
            ISet<string> generatedFiles)
        {
            if(string.IsNullOrWhiteSpace(outputDir) || !Directory.Exists(outputDir))
            {
                return;
            }

            ISet<string> trackedFiles = generatedFiles ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string file in Directory.GetFiles(outputDir, "*.cs", SearchOption.TopDirectoryOnly))
            {
                if(!IsAddressablesGeneratedFile(file, className, labelsClassName))
                {
                    continue;
                }

                string normalizedFile = Path.GetFullPath(file);
                if(trackedFiles.Contains(normalizedFile))
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"删除旧的 Addressables 常量文件失败：{file}\n{ex}");
                }
            }
        }

        private static bool IsAddressablesGeneratedFile(string filePath, string className, string labelsClassName)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if(string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            if(!string.IsNullOrWhiteSpace(className))
            {
                if(fileName.Equals(className, StringComparison.OrdinalIgnoreCase) ||
                   fileName.StartsWith(className + "_", StringComparison.OrdinalIgnoreCase) ||
                   fileName.StartsWith(className + ".", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return !string.IsNullOrWhiteSpace(labelsClassName) &&
                   fileName.Equals(labelsClassName, StringComparison.OrdinalIgnoreCase);
        }

        private static void TrackGeneratedFile(string filePath, ISet<string> generatedFiles)
        {
            if(generatedFiles == null || string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                string normalizedPath = Path.GetFullPath(filePath);
                generatedFiles.Add(normalizedPath);
            }
            catch (Exception)
            {
                generatedFiles.Add(filePath);
            }
        }

        private static string GenerateUniqueIdentifier(string input, HashSet<string> keywords,
            HashSet<string> usedNames)
        {
            string desired = IdentifierUtility.ToIdentifier(input);
            if(keywords.Contains(desired)) desired = "_" + desired;
            return IdentifierUtility.MakeUnique(desired, usedNames);
        }
    }
}