namespace CFramework.Systems.UISystem.Internal
{
    /// <summary>
    ///     层级管理器接口
    /// </summary>
    public interface ILayerManager
    {
        IViewInstance GetTop(string layer);
        IViewInstance FindTopByKey(string layer, string key);
        IViewInstance FindAnyByKey(string key);
        void MoveToTop(IViewInstance inst);
        void AddToLayer(IViewInstance inst);
        void RemoveFromLayer(IViewInstance inst);
        bool ContainsLayer(string layer);
        string ResolveLayer(string layer, string defaultLayer);
    }
}