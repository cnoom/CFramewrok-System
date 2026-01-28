using System;
using System.Collections.Generic;
using System.Linq;
using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.Utilities;
using CFramework.Systems.UISystem.Transitions;
using CNoom.UnityCodeGen.Editor;

namespace CFramework.Editor.UISystem
{
    public static class UITransitionCodeGen
    {
        private const string DefaultFileName = "UITransitions.cs";
        private const string DefaultNamespace = "CFramework.Systems.UISystem";
        private const string DefaultClassName = "UITransitions";

        internal static void GenerateCodeInternal()
        {
            GenerateCode();
        }

        private static void GenerateCode()
        {
            CFDirectoryUtility.EnsureFolder(CFDirectoryKey.FrameworkGenerate);

            // 扫描UITransition子类
            Type[] transitionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(UITransition)) && !t.IsAbstract)
                .ToArray();

            HashSet<string> used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> keywords = IdentifierUtility.CSharpKeywords;
            List<TransitionItem> transitionItems = new List<TransitionItem>();

            foreach (Type type in transitionTypes)
            {
                string identifier = type.Name;
                if(keywords.Contains(identifier)) identifier = "_" + identifier;
                string unique = IdentifierUtility.MakeUnique(identifier, used);

                transitionItems.Add(new TransitionItem
                {
                    Name = unique,
                    Value = type.Name
                });
            }

            if(used.Count == 0)
            {
                transitionItems.Add(new TransitionItem
                {
                    Name = "Fade",
                    Value = "Fade"
                });
            }

            CodeGenContext context = new CodeGenContext()
                .Set("Namespace", DefaultNamespace)
                .Set("ClassName", DefaultClassName)
                .Set("Transitions", transitionItems);

            CodeGenTemplate template = new CodeGenTemplate("UITransitionTemplate", Templates.UITransitionTemplate);
            CodeGenRequest request = new CodeGenRequest
            {
                OutputPath = GetOutputPath(),
                Template = template,
                Context = context,
                WriteMode = CodeGenWriteMode.Overwrite
            };

            CodeGen.Generate(request);
        }

        private static string GetOutputPath()
        {
            return $"{CFDirectoryKey.FrameworkGenerate}/{DefaultFileName}";
        }

        private class TransitionItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}