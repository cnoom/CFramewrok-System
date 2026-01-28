using CFramework.Core.CommandSystem;

namespace CFramework.Systems.AudioSystem
{
    public static class AudioCommands
    {
        /// <summary>
        ///     播放背景音乐
        /// </summary>
        public readonly struct PlayMusic : ICommandData
        {
            /// <summary>要播放的音乐资源 Key。</summary>
            public readonly string ClipKey;
            /// <summary>是否循环播放。</summary>
            public readonly bool Loop;
            /// <summary>初始音量（0-1）。</summary>
            public readonly float Volume;
            /// <summary>新音乐淡入时间（秒）。</summary>
            public readonly float FadeInSeconds;
            /// <summary>在替换现有音乐时，旧音乐淡出时间（秒）。</summary>
            public readonly float FadeOutSeconds; // 用于替换现有音乐时旧曲淡出
            /// <summary>如果与当前播放的音乐相同，是否仍然重新播放。</summary>
            public readonly bool AllowReplaySame;

            public PlayMusic(string clipKey, bool loop = true, float volume = 1f,
                float fadeInSeconds = 0.2f, float fadeOutSeconds = 0.2f, bool allowReplaySame = false)
            {
                ClipKey = clipKey;
                Loop = loop;
                Volume = volume;
                FadeInSeconds = fadeInSeconds;
                FadeOutSeconds = fadeOutSeconds;
                AllowReplaySame = allowReplaySame;
            }
        }

        /// <summary>
        ///     停止背景音乐
        /// </summary>
        public readonly struct StopMusic : ICommandData
        {
            /// <summary>停止时淡出的持续时间（秒）。</summary>
            public readonly float FadeOutSeconds;

            public StopMusic(float fadeOutSeconds = 0.2f)
            {
                FadeOutSeconds = fadeOutSeconds;
            }
        }

        /// <summary>
        ///     播放音效
        /// </summary>
        public readonly struct PlaySfx : ICommandData
        {
            /// <summary>要播放的音效资源 Key。</summary>
            public readonly string ClipKey;
            /// <summary>播放使用的音量（0-1）。</summary>
            public readonly float Volume;
            /// <summary>音高倍率，1 表示原始音高。</summary>
            public readonly float Pitch;
            /// <summary>是否循环播放。</summary>
            public readonly bool Loop;
            /// <summary>播放优先级，用于在通道受限时决策。</summary>
            public readonly int Priority;

            public PlaySfx(string clipKey, float volume = 1f, float pitch = 1f, bool loop = false,
                int priority = 0)
            {
                ClipKey = clipKey;
                Volume = volume;
                Pitch = pitch;
                Loop = loop;
                Priority = priority;
            }
        }

        /// <summary>
        ///     停止所有音效
        /// </summary>
        public readonly struct StopAllSfx : ICommandData
        {
        }

        /// <summary>
        ///     设置音量
        /// </summary>
        public readonly struct SetVolume : ICommandData
        {
            /// <summary>要设置的音频类别。</summary>
            public readonly AudioCategory Category;
            /// <summary>目标音量值（0-1）。</summary>
            public readonly float Volume; // 0-1

            public SetVolume(AudioCategory category, float volume)
            {
                Category = category;
                Volume = volume;
            }
        }

        /// <summary>
        ///     设置静音状态
        /// </summary>
        public readonly struct SetMute : ICommandData
        {
            /// <summary>要设置的音频类别。</summary>
            public readonly AudioCategory Category;
            /// <summary>静音开关状态，true 表示静音。</summary>
            public readonly bool Mute;

            public SetMute(AudioCategory category, bool mute)
            {
                Category = category;
                Mute = mute;
            }
        }
    }
}