using System;
using CFramework.Core.BroadcastSystem;

namespace CFramework.Systems.SceneSystem
{
    public static class SceneBroadcasts
    {
        /// <summary>
        ///     场景加载开始广播。
        /// </summary>
        /// <remarks>当开始加载指定逻辑场景时发送。</remarks>
        public readonly struct SceneLoadingStarted : IBroadcastData
        {
            /// <summary>正在加载的逻辑场景 Key。</summary>
            public readonly string SceneKey;

            /// <summary>开始加载的 UTC 时间戳。</summary>
            public readonly DateTime Timestamp;

            /// <param name="sceneKey">正在加载的逻辑场景 Key。</param>
            public SceneLoadingStarted(string sceneKey)
            {
                SceneKey = sceneKey;
                Timestamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     场景加载进度广播。
        /// </summary>
        /// <remarks>在异步加载场景过程中周期性发送，用于报告当前进度。</remarks>
        public readonly struct SceneLoadingProgress : IBroadcastData
        {
            /// <summary>正在加载的逻辑场景 Key。</summary>
            public readonly string SceneKey;

            /// <summary>当前加载进度（0-1）。</summary>
            public readonly float Progress;

            /// <summary>本次进度报告的 UTC 时间戳。</summary>
            public readonly DateTime Timestamp;

            /// <param name="sceneKey">正在加载的逻辑场景 Key。</param>
            /// <param name="progress">当前加载进度（0-1）。</param>
            public SceneLoadingProgress(string sceneKey, float progress)
            {
                SceneKey = sceneKey;
                Progress = progress;
                Timestamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     场景加载完成广播。
        /// </summary>
        /// <remarks>当场景成功加载并完成必要初始化时发送。</remarks>
        public readonly struct SceneLoaded : IBroadcastData
        {
            /// <summary>已加载完成的逻辑场景 Key。</summary>
            public readonly string SceneKey;

            /// <summary>加载完成的 UTC 时间戳。</summary>
            public readonly DateTime Timestamp;

            /// <param name="sceneKey">已加载完成的逻辑场景 Key。</param>
            public SceneLoaded(string sceneKey)
            {
                SceneKey = sceneKey;
                Timestamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     场景加载失败广播。
        /// </summary>
        /// <remarks>当场景加载过程中发生错误时发送。</remarks>
        public readonly struct SceneLoadFailed : IBroadcastData
        {
            /// <summary>尝试加载的逻辑场景 Key。</summary>
            public readonly string SceneKey;

            /// <summary>错误信息描述。</summary>
            public readonly string ErrorMessage;

            /// <summary>失败发生时的 UTC 时间戳。</summary>
            public readonly DateTime Timestamp;

            /// <param name="sceneKey">尝试加载的逻辑场景 Key。</param>
            /// <param name="errorMessage">错误信息描述。</param>
            public SceneLoadFailed(string sceneKey, string errorMessage)
            {
                SceneKey = sceneKey;
                ErrorMessage = errorMessage;
                Timestamp = DateTime.UtcNow;
            }
        }
    }
}