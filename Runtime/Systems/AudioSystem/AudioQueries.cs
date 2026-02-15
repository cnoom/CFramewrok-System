using CFramework.Core.QuerySystem;

namespace CFramework.Systems.AudioSystem
{
    public static class AudioQueries
    {
        /// <summary>
        ///     查询音量
        /// </summary>
        public readonly struct Volume : IQueryData<float>
        {
            /// <summary>要查询的音频类别。</summary>
            public readonly AudioCategory Category;

            public Volume(AudioCategory category)
            {
                Category = category;
            }
        }

        /// <summary>
        ///     查询静音状态
        /// </summary>
        public readonly struct Mute : IQueryData<bool>
        {
            /// <summary>要查询的音频类别。</summary>
            public readonly AudioCategory Category;

            public Mute(AudioCategory category)
            {
                Category = category;
            }
        }

        /// <summary>
        ///     查询当前激活背景音乐的请求
        /// </summary>
        public readonly struct GetActiveMusic : IQueryData<AudioSystemModule.ActiveMusicInfo>
        {
        }
    }
}