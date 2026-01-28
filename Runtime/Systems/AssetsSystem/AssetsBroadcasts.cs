using CFramework.Core.BroadcastSystem;

namespace CFramework.Systems.AssetsSystem
{
    public static class AssetsBroadcasts
    {
        /// <summary>
        ///     选定地址集合加载进度广播数据
        /// </summary>
        /// <remarks>通过广播通知按需跟踪的选定地址集合整体进度</remarks>
        public struct SelectedAssetsProgress : IBroadcastData
        {
            /// <summary>本次进度跟踪的 Id。</summary>
            public string TrackId;
            /// <summary>0-1 的整体进度值。</summary>
            public float Progress;
            /// <summary>当前已完成的资源数量。</summary>
            public int Count;
            /// <summary>是否已完成本次选定地址集合的全部加载。</summary>
            public bool IsDone;

        }
    }
}