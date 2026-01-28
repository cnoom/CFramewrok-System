using CFramework.Systems.UISystem.Internal;

namespace CFramework.Systems.UISystem
{
    /// <summary>
    ///     UI视图接口，所有视图组件需实现此接口。
    /// </summary>
    public interface IUIView
    {
        /// <summary>
        ///     内部使用：设置视图实例引用，供视图访问状态信息。
        /// </summary>
        void SetViewInstance(IViewInstance instance);
    }
}