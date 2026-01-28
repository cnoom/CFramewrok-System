using CFramework.Core.CommandSystem;

namespace CFramework.Systems.SceneSystem
{
    public static class SceneCommands
    {
        /// <summary>
        ///     加载场景命令。
        /// </summary>
        public readonly struct LoadScene : ICommandData
        {
            /// <summary>要加载的逻辑场景 Key。</summary>
            public readonly string SceneKey;

            /// <param name="sceneKey">要加载的逻辑场景 Key。</param>
            public LoadScene(string sceneKey)
            {
                SceneKey = sceneKey;
            }
        }
    }

}