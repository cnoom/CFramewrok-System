using CFramework.Core.BroadcastSystem;

namespace CFramework.Systems.UISystem
{
    public static class UIBroadcasts
    {
        /// <summary>
        ///     视图打开成功广播。
        /// </summary>
        /// <remarks>当一个 UI 视图成功打开并完成初始化时发送。</remarks>
        public readonly struct ViewOpened : IBroadcastData
        {
            /// <summary>该次打开视图实例的唯一 Id。</summary>
            public readonly string Id;
            /// <summary>视图地址键。</summary>
            public readonly string Key;
            /// <summary>视图所在层名。</summary>
            public readonly string Layer;

            /// <param name="id">该次打开视图实例的唯一 Id。</param>
            /// <param name="key">视图地址键。</param>
            /// <param name="layer">视图所在层名。</param>
            public ViewOpened(string id, string key, string layer)
            {
                Id = id;
                Key = key;
                Layer = layer;
            }
        }

        /// <summary>
        ///     视图关闭广播。
        /// </summary>
        /// <remarks>当一个 UI 视图被关闭（销毁）时发送。</remarks>
        public readonly struct ViewClosed : IBroadcastData
        {
            /// <summary>被关闭视图实例的 Id。</summary>
            public readonly string Id;
            /// <summary>视图地址键。</summary>
            public readonly string Key;
            /// <summary>视图所在层名。</summary>
            public readonly string Layer;

            /// <param name="id">被关闭视图实例的 Id。</param>
            /// <param name="key">视图地址键。</param>
            /// <param name="layer">视图所在层名。</param>
            public ViewClosed(string id, string key, string layer)
            {
                Id = id;
                Key = key;
                Layer = layer;
            }
        }


        /// <summary>
        ///     视图显示广播。
        /// </summary>
        /// <remarks>当一个已打开的视图被显示（例如从隐藏状态切换为可见）时发送。</remarks>
        public readonly struct ViewShown : IBroadcastData
        {
            /// <summary>视图实例 Id。</summary>
            public readonly string Id;
            /// <summary>视图地址键。</summary>
            public readonly string Key;
            /// <summary>视图所在层名。</summary>
            public readonly string Layer;

            /// <param name="id">视图实例 Id。</param>
            /// <param name="key">视图地址键。</param>
            /// <param name="layer">视图所在层名。</param>
            public ViewShown(string id, string key, string layer)
            {
                Id = id;
                Key = key;
                Layer = layer;
            }
        }


        /// <summary>
        ///     视图隐藏广播。
        /// </summary>
        /// <remarks>当一个视图被隐藏（但未关闭销毁）时发送。</remarks>
        public readonly struct ViewHidden : IBroadcastData
        {
            /// <summary>视图实例 Id。</summary>
            public readonly string Id;
            /// <summary>视图地址键。</summary>
            public readonly string Key;
            /// <summary>视图所在层名。</summary>
            public readonly string Layer;

            /// <param name="id">视图实例 Id。</param>
            /// <param name="key">视图地址键。</param>
            /// <param name="layer">视图所在层名。</param>
            public ViewHidden(string id, string key, string layer)
            {
                Id = id;
                Key = key;
                Layer = layer;
            }
        }


        /// <summary>
        ///     视图打开失败广播。
        /// </summary>
        /// <remarks>当尝试打开视图时遇到错误或无法创建视图时发送。</remarks>
        public readonly struct ViewOpenFailed : IBroadcastData
        {
            /// <summary>视图地址键。</summary>
            public readonly string Key;
            /// <summary>目标层名。</summary>
            public readonly string Layer;
            /// <summary>错误信息描述。</summary>
            public readonly string Error;

            /// <param name="key">视图地址键。</param>
            /// <param name="layer">目标层名。</param>
            /// <param name="error">错误信息描述。</param>
            public ViewOpenFailed(string key, string layer, string error)
            {
                Key = key;
                Layer = layer;
                Error = error;
            }
        }
    }
}