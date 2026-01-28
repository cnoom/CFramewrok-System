using CFramework.Core.QuerySystem;

namespace CFramework.Systems.UISystem
{
    public static class UIQueries
    {
        /// <summary>
        ///     查询视图是否打开的请求。
        /// </summary>
        /// <returns>是否满足条件的视图存在。</returns>
        public readonly struct IsOpen : IQueryData
        {
            /// <summary>视图地址键（可选）。</summary>
            public readonly string Key; // optional

            /// <summary>视图实例 Id（可选）。</summary>
            public readonly string Id; // optional

            /// <param name="key">视图地址键（可选）。</param>
            /// <param name="id">视图实例 Id（可选）。</param>
            public IsOpen(string key = null, string id = null)
            {
                Key = key;
                Id = id;
            }
        }


        /// <summary>
        ///     查询某一层顶部视图的请求。
        /// </summary>
        /// <returns>指定层栈顶视图的实例信息。</returns>
        public readonly struct GetTop : IQueryData
        {
            /// <summary>目标层名（可选，为空时查询默认层）。</summary>
            public readonly string Layer; // optional

            /// <param name="layer">目标层名（可选，为空时查询默认层）。</param>
            public GetTop(string layer = null)
            {
                Layer = layer;
            }
        }


        /// <summary>
        ///     查询已打开视图列表的请求。
        /// </summary>
        /// <returns>符合条件的已打开视图集合。</returns>
        public readonly struct GetOpenViews : IQueryData
        {
            /// <summary>用于筛选的层名（可选）。</summary>
            public readonly string Layer; // optional filter

            /// <param name="layer">用于筛选的层名（可选）。</param>
            public GetOpenViews(string layer = null)
            {
                Layer = layer;
            }
        }
    }

    /// <summary>
    ///     视图查询结果。
    /// </summary>
    public readonly struct ViewInfo
    {
        public readonly string Id;
        public readonly string Key;
        public readonly string Layer;
        public readonly bool IsVisible;

        public ViewInfo(string id, string key, string layer, bool isVisible)
        {
            Id = id;
            Key = key;
            Layer = layer;
            IsVisible = isVisible;
        }
    }

    /// <summary>
    ///     打开视图操作的结果数据结构。
    /// </summary>
    public readonly struct OpenViewResult
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
        public OpenViewResult(string id, string key, string layer)
        {
            Id = id;
            Key = key;
            Layer = layer;
        }
    }

}