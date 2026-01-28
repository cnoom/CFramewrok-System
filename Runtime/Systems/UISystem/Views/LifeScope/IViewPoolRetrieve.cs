namespace CFramework.Systems.UISystem.LifeScope
{
    /// <summary>
    ///     视图从对象池中取出时的生命周期接口。
    /// </summary>
    /// <remarks>
    ///     适用于清理上次残留的状态、重置UI组件默认值等。
    ///     在OnViewCreate之后、OnShowBefore之前调用。
    /// </remarks>
    public interface IViewPoolRetrieve
    {
        /// <summary>
        ///     视图从对象池中取出时调用。
        /// </summary>
        void OnPoolRetrieve();
    }
}