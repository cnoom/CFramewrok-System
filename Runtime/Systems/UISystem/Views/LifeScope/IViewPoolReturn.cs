namespace CFramework.Systems.UISystem.LifeScope
{
    /// <summary>
    ///     视图归还到对象池时的生命周期接口。
    /// </summary>
    /// <remarks>
    ///     适用于清理事件订阅、释放临时资源、保存必要状态等。
    ///     在OnViewClose之后调用。
    /// </remarks>
    public interface IViewPoolReturn
    {
        /// <summary>
        ///     视图归还到对象池时调用。
        /// </summary>
        void OnPoolReturn();
    }
}