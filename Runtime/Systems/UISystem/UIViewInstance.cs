using CFramework.Systems.UISystem.Internal;
using UnityEngine;

namespace CFramework.Systems.UISystem
{
    public sealed class UIViewInstance : IViewInstance
    {
        public string Id { get; set; } // guid
        public string Key { get; set; } // addressables key
        public string Layer { get; set; } // layer name
        public GameObject Root { get; set; } // instantiated object
        public IUIView Controller { get; set; } // optional controller
        public string TransitionName { get; set; } // used transition
        public float TransitionSeconds { get; set; } // used seconds
        public bool Visible { get; set; } // visible state
    }
}