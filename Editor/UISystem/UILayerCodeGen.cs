using System;
using System.Collections.Generic;
using CFramework.Core.Editor.Base;
using CFramework.Core.Editor.Utilities;
using CFramework.Systems.UISystem;
using CNoom.UnityCodeGen.Editor;

namespace CFramework.Editor.UISystem
{
    public static class UILayerCodeGen
    {
        private const string DefaultFileName = "UILayers.cs";
        private const string DefaultNamespace = "CFramework.Systems.UISystem";
        private const string DefaultClassName = "UILayers";

        internal static void GenerateCodeInternal(UIConfig config)
        {
            GenerateCode(config);
        }

        private static void GenerateCode(UIConfig config)
        {
            CFDirectoryUtility.EnsureFolder(CFDirectoryKey.FrameworkGenerate);

            string[] layers = config.layerOrder ?? Array.Empty<string>();
            if(layers.Length == 0)
            {
                layers = new[]
                {
                    "Screen"
                };
            }

            HashSet<string> used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> keywords = IdentifierUtility.CSharpKeywords;
            List<LayerItem> layerItems = new List<LayerItem>();

            foreach (string layer in layers)
            {
                if(string.IsNullOrWhiteSpace(layer)) continue;
                string identifier = IdentifierUtility.ToIdentifier(layer);
                if(keywords.Contains(identifier)) identifier = "_" + identifier;

                string unique = IdentifierUtility.MakeUnique(identifier, used);
                layerItems.Add(new LayerItem
                {
                    Name = unique,
                    Value = layer
                });
            }

            if(used.Count == 0)
            {
                layerItems.Add(new LayerItem
                {
                    Name = "Screen",
                    Value = "Screen"
                });
            }

            CodeGenContext context = new CodeGenContext()
                .Set("Namespace", DefaultNamespace)
                .Set("ClassName", DefaultClassName)
                .Set("Layers", layerItems);

            CodeGenTemplate template = new CodeGenTemplate("UILayerTemplate", Templates.UILayerTemplate);
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

        private class LayerItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}