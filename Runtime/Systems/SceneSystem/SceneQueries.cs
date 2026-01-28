using CFramework.Core.QuerySystem;

namespace CFramework.Systems.SceneSystem
{
    public static class SceneQueries
    {
        /// <summary>
        ///     查询当前逻辑场景信息。
        /// </summary>
        /// <returns>当前逻辑场景的信息快照 <see cref="SceneInfo" /></returns>
        public readonly struct GetCurrentSceneInfo : IQueryData
        {
        }

        /// <summary>
        ///     查询指定场景加载进度（0~1）。
        /// </summary>
        /// <returns>场景加载进度（0-1）。</returns>
        public readonly struct GetSceneProgress : IQueryData
        {
            /// <summary>要查询的逻辑场景 Key。</summary>
            public readonly string SceneKey;

            /// <param name="sceneKey">要查询的逻辑场景 Key。</param>
            public GetSceneProgress(string sceneKey)
            {
                SceneKey = sceneKey;
            }
        }


        /// <summary>
        ///     当前场景信息快照。
        /// </summary>
        public readonly struct SceneInfo
        {
            /// <summary>逻辑场景 Key。</summary>
            public readonly string SceneKey;

            /// <summary>对应 Unity 场景名称。</summary>
            public readonly string UnitySceneName;

            /// <summary>当前场景加载状态。</summary>
            public readonly SceneLoadState State;

            /// <summary>最近一次记录的加载进度（0-1）。</summary>
            public readonly float LastProgress;

            /// <summary>最近一次加载失败的错误信息（如无错误可能为空）。</summary>
            public readonly string LastError;

            /// <param name="sceneKey">逻辑场景 Key。</param>
            /// <param name="unitySceneName">对应 Unity 场景名称。</param>
            /// <param name="state">当前场景加载状态。</param>
            /// <param name="lastProgress">最近一次记录的加载进度（0-1）。</param>
            /// <param name="lastError">最近一次加载失败的错误信息（如无错误可能为空）。</param>
            public SceneInfo(string sceneKey, string unitySceneName, SceneLoadState state,
                float lastProgress, string lastError)
            {
                SceneKey = sceneKey;
                UnitySceneName = unitySceneName;
                State = state;
                LastProgress = lastProgress;
                LastError = lastError;
            }
        }
    }
}