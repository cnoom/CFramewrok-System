namespace CFramework.Systems.SceneSystem
{
    /// <summary>
    ///     场景加载状态。
    /// </summary>
    public enum SceneLoadState
    {
        NotLoaded = 0,
        Loading = 1,
        Loaded = 2,
        Activated = 3,
        Unloading = 4
    }
}