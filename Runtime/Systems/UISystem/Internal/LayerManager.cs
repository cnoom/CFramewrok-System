using System.Collections.Generic;
using CFramework.Core.Log;

namespace CFramework.Systems.UISystem.Internal
{
    /// <summary>
    ///     层级管理器实现
    /// </summary>
    internal sealed class LayerManager : ILayerManager
    {
        private readonly string[] _layerOrder;
        private readonly Dictionary<string, List<IViewInstance>> _layers;
        private readonly CFLogger _logger;

        public LayerManager(CFLogger logger, string[] layerOrder)
        {
            _logger = logger;
            _layerOrder = layerOrder ?? new[]
            {
                "Screen"
            };
            _layers = new Dictionary<string, List<IViewInstance>>();

            foreach (string layer in _layerOrder)
            {
                _layers[layer] = new List<IViewInstance>(8);
            }
        }

        public IViewInstance GetTop(string layer)
        {
            if(!_layers.ContainsKey(layer)) return null;

            List<IViewInstance> list = _layers[layer];
            if(list.Count == 0) return null;
            return list[list.Count - 1];
        }

        public IViewInstance FindTopByKey(string layer, string key)
        {
            if(!string.IsNullOrEmpty(layer))
            {
                if(!_layers.ContainsKey(layer)) return null;
                List<IViewInstance> list = _layers[layer];
                for(int i = list.Count - 1; i >= 0; i--)
                {
                    if(list[i].Key == key)
                        return list[i];
                }
                return null;
            }

            foreach (KeyValuePair<string, List<IViewInstance>> kv in _layers)
            {
                List<IViewInstance> list = kv.Value;
                for(int i = list.Count - 1; i >= 0; i--)
                {
                    if(list[i].Key == key)
                        return list[i];
                }
            }

            return null;
        }

        public IViewInstance FindAnyByKey(string key)
        {
            foreach (KeyValuePair<string, List<IViewInstance>> kv in _layers)
            {
                List<IViewInstance> list = kv.Value;
                for(var i = 0; i < list.Count; i++)
                {
                    if(list[i].Key == key)
                        return list[i];
                }
            }
            return null;
        }

        public void MoveToTop(IViewInstance inst)
        {
            string layer = inst.Layer;
            if(!_layers.ContainsKey(layer)) return;

            List<IViewInstance> list = _layers[layer];
            int idx = list.IndexOf(inst);
            if(idx >= 0 && idx != list.Count - 1)
            {
                list.RemoveAt(idx);
                list.Add(inst);
            }
        }

        public bool ContainsLayer(string layer)
        {
            return _layers.ContainsKey(layer);
        }

        public void AddToLayer(IViewInstance inst)
        {
            string layer = inst.Layer;
            if(!_layers.ContainsKey(layer))
            {
                _logger?.LogWarning($"层名未知: {layer}");
                return;
            }
            _layers[layer].Add(inst);
        }

        public void RemoveFromLayer(IViewInstance inst)
        {
            string layer = inst.Layer;
            if(_layers.ContainsKey(layer))
            {
                _layers[layer].Remove(inst);
            }
        }

        public string ResolveLayer(string layer, string defaultLayer)
        {
            if(string.IsNullOrEmpty(layer))
                return _layerOrder.Length > 0 ? _layerOrder[0] : defaultLayer ?? "Screen";

            if(!_layers.ContainsKey(layer))
            {
                _logger?.LogWarning($"层名未知: {layer}，将归入最后一层");
                return _layerOrder[_layerOrder.Length - 1];
            }

            return layer;
        }
    }
}