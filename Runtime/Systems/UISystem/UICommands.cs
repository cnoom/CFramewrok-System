using System;
using CFramework.Core.CommandSystem;

namespace CFramework.Systems.UISystem
{
    public static class UICommands
    {
        /// <summary>
        ///     打开视图。
        /// </summary>
        public readonly struct OpenView : ICommandData
        {
            /// <summary>视图地址键。</summary>
            public readonly string Key;

            /// <summary>所在层名（可选，为空时由系统决定默认层）。</summary>
            public readonly string Layer;

            /// <summary>传入视图的初始化数据。</summary>
            public readonly IViewData Data;

            /// <summary>采用的过渡方式名（可选）。</summary>
            public readonly string TransitionName;

            /// <summary>过渡时间（秒，可选）。</summary>
            public readonly float? Seconds;

            /// <summary>如果视图已经打开，是否将其置于同层顶部。</summary>
            public readonly bool BringToTop;

            /// <param name="key">视图地址键。</param>
            /// <param name="layer">所在层名（可选，为空时由系统决定默认层）。</param>
            /// <param name="data">传入视图的初始化数据。</param>
            /// <param name="transitionName">采用的过渡方式名（可选）。</param>
            /// <param name="seconds">过渡时间（秒，可选）。</param>
            /// <param name="bringToTop">如果视图已经打开，是否将其置于同层顶部。</param>
            public OpenView(string key, string layer = null, IViewData data = null,
                string transitionName = null, float? seconds = null, bool bringToTop = true)
            {
                Key = key;
                Layer = layer;
                Data = data;
                TransitionName = transitionName;
                Seconds = seconds;
                BringToTop = bringToTop;
            }
        }

        /// <summary>
        ///     通过View类型打开视图。
        /// </summary>
        /// <remarks>
        ///     需要在UIConfig中配置UiInfo的viewType字段。
        /// </remarks>
        public readonly struct OpenViewByType : ICommandData
        {
            /// <summary>View组件类型。</summary>
            public readonly Type ViewType;

            /// <summary>所在层名（可选，为空时由UIConfig配置决定默认层）。</summary>
            public readonly string Layer;

            /// <summary>传入视图的初始化数据。</summary>
            public readonly IViewData Data;

            /// <summary>采用的过渡方式名（可选）。</summary>
            public readonly string TransitionName;

            /// <summary>过渡时间（秒，可选）。</summary>
            public readonly float? Seconds;

            /// <summary>如果视图已经打开，是否将其置于同层顶部。</summary>
            public readonly bool BringToTop;

            /// <param name="viewType">View组件类型。</param>
            /// <param name="layer">所在层名（可选，为空时由UIConfig配置决定默认层）。</param>
            /// <param name="data">传入视图的初始化数据。</param>
            /// <param name="transitionName">采用的过渡方式名（可选）。</param>
            /// <param name="seconds">过渡时间（秒，可选）。</param>
            /// <param name="bringToTop">如果视图已经打开，是否将其置于同层顶部。</param>
            public OpenViewByType(Type viewType, string layer = null, IViewData data = null,
                string transitionName = null, float? seconds = null, bool bringToTop = true)
            {
                ViewType = viewType;
                Layer = layer;
                Data = data;
                TransitionName = transitionName;
                Seconds = seconds;
                BringToTop = bringToTop;
            }

            public static OpenViewByType Open<T>(string layer = null, IViewData data = null,
                string transitionName = null, float? seconds = null, bool bringToTop = true) where T : IUIView
            {
                return new OpenViewByType(typeof(T), layer, data, transitionName, seconds, bringToTop);
            }
        }


        /// <summary>
        ///     通过 Id 关闭视图。
        /// </summary>
        public readonly struct CloseViewById : ICommandData
        {
            /// <summary>视图 Id。</summary>
            public readonly string Id;

            /// <summary>过渡时间。</summary>
            public readonly float? Seconds; // Optional close transition duration

            /// <param name="id">视图 Id。</param>
            /// <param name="seconds">过渡时间。</param>
            public CloseViewById(string id, float? seconds = null)
            {
                Id = id;
                Seconds = seconds;
            }
        }


        /// <summary>
        ///     通过 Key 关闭视图。
        /// </summary>
        public readonly struct CloseViewByKey : ICommandData
        {
            /// <summary>视图地址键。</summary>
            public readonly string Key;

            /// <summary>过渡时间。</summary>
            public readonly float? Seconds;

            /// <param name="key">视图地址键。</param>
            /// <param name="seconds">过渡时间。</param>
            public CloseViewByKey(string key, float? seconds = null)
            {
                Key = key;
                Seconds = seconds;
            }
        }


        /// <summary>
        ///     关闭指定层的顶部视图。
        /// </summary>
        public readonly struct CloseTop : ICommandData
        {
            /// <summary>层名。</summary>
            public readonly string Layer;

            /// <summary>过渡时间。</summary>
            public readonly float? Seconds;

            /// <param name="layer">层名。</param>
            /// <param name="seconds">过渡时间。</param>
            public CloseTop(string layer, float? seconds = null)
            {
                Layer = layer;
                Seconds = seconds;
            }
        }


        /// <summary>
        ///     通过 Id 隐藏视图。
        /// </summary>
        public readonly struct HideView : ICommandData
        {
            /// <summary>视图 Id。</summary>
            public readonly string Id;

            /// <summary>过渡时间。</summary>
            public readonly float? Seconds;

            /// <param name="id">视图 Id。</param>
            /// <param name="seconds">过渡时间。</param>
            public HideView(string id, float? seconds = null)
            {
                Id = id;
                Seconds = seconds;
            }
        }


        /// <summary>
        ///     通过 Id 显示视图。
        /// </summary>
        public readonly struct ShowView : ICommandData
        {
            /// <summary>视图 Id。</summary>
            public readonly string Id;

            /// <summary>过渡时间。</summary>
            public readonly float? Seconds;

            /// <param name="id">视图 Id。</param>
            /// <param name="seconds">过渡时间。</param>
            public ShowView(string id, float? seconds = null)
            {
                Id = id;
                Seconds = seconds;
            }
        }
    }
}