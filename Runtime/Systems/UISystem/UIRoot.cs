using System;
using System.Collections.Generic;
using CFramework.Core.Log;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CFramework.Systems.UISystem
{
    internal sealed class UIRoot
    {
        public readonly Dictionary<string, RectTransform> Layers = new Dictionary<string, RectTransform>();
        public Canvas Canvas { get; private set; }
        public CanvasScaler Scaler { get; private set; }
        public GameObject EventSystemGO { get; private set; }

        public void Build(UIConfig config, CFLogger logger)
        {
            // Canvas
            if(!config.canvas)
            {
                logger.LogError("未指定UI画布");
                throw new NullReferenceException("未指定UI画布");
            }
            Canvas = Object.Instantiate(config.canvas);
            Object.DontDestroyOnLoad(Canvas.gameObject);

            // Layers
            for(var i = 0; i < config.layerOrder.Length; i++)
            {
                string layer = config.layerOrder[i];
                GameObject layerGo = new GameObject($"CF_UI_{layer}", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                RectTransform rt = layerGo.GetComponent<RectTransform>();
                layerGo.transform.SetParent(Canvas.transform, false);
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Canvas layerCanvas = layerGo.GetComponent<Canvas>();
                layerCanvas.overrideSorting = true;
                layerCanvas.sortingOrder = Canvas.sortingOrder + i + 1;

                Layers[layer] = rt;
            }

        }
    }
}