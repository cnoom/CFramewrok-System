using UnityEngine;

namespace CFramework.Systems.UISystem.Internal
{
    /// <summary>
    ///     视图实例的抽象接口，用于内部组件通信
    /// </summary>
    public interface IViewInstance
    {
        string Id { get; }
        string Key { get; }
        string Layer { get; set; }
        string TransitionName { get; set; }
        float TransitionSeconds { get; set; }
        bool Visible { get; set; }
        IUIView Controller { get; }
        GameObject Root { get; }
    }
}