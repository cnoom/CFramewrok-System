using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor.AddressablesTools
{
    public class AddressablesManagerWindow : EditorWindow
    {
        private Vector2 _scroll;

        private void OnGUI()
        {
            AddressablesFolderRegistry reg = ConfigUtility.GetEditorConfig<AddressablesFolderRegistry>();
            EditorGUILayout.LabelField("全局设置", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            reg.codeGenNamespace = EditorGUILayout.TextField("命名空间", reg.codeGenNamespace);
            reg.codeGenClassName = EditorGUILayout.TextField("类名", reg.codeGenClassName);
            reg.codeGenStructure = (CodeGenStructure)EditorGUILayout.EnumPopup("生成结构", reg.codeGenStructure);
            reg.defaultGroupName = EditorGUILayout.TextField("默认组名", reg.defaultGroupName);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("同步全部", GUILayout.Height(24)))
            {
                AddressablesSyncPipeline.SyncAll(reg);
            }
            if(GUILayout.Button("生成常量", GUILayout.Height(24)))
            {
                AddressablesCodeGen.Generate(reg);
            }
            if(GUILayout.Button("保存", GUILayout.Height(24)))
            {
                EditorUtility.SetDirty(reg);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("已记录的文件夹", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for(var i = 0; i < reg.records.Count; i++)
            {
                FolderRecord r = reg.records[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"记录项 #{i + 1}", EditorStyles.miniBoldLabel);
                using (new EditorGUI.DisabledScope(true))
                {
                    string path = AssetDatabase.GUIDToAssetPath(r.folderGUID);
                    EditorGUILayout.TextField("文件夹", path);
                }
                r.groupName = EditorGUILayout.TextField("组名", string.IsNullOrEmpty(r.groupName) ? reg.defaultGroupName : r.groupName);
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
                    if(EditorUtility.DisplayDialog("移除条目", "是否移除该记录项下的所有 Addressables 条目？", "是", "否"))
                    {
                        AddressablesSyncPipeline.RemoveAllEntriesUnderRecord(r);
                    }
                }
                if(GUILayout.Button("Unrecord"))
                {
                    if(EditorUtility.DisplayDialog("取消记录", "是否从记录表移除并（可选）移除相关条目？", "是", "否"))
                    {
                        AddressablesSyncPipeline.RemoveAllEntriesUnderRecord(r);
                        reg.records.RemoveAt(i);
                        EditorUtility.SetDirty(reg);
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

        [MenuItem(CFMenuKey.Systems + "/Addressables/地址管理器")]
        private static void Open()
        {
            GetWindow<AddressablesManagerWindow>().Show();
        }
    }
}