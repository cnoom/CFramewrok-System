using System;
using CFramework.Core.BroadcastSystem;

namespace CFramework.Systems.AudioSystem
{
    public static class AudioBroadcasts
    {
        /// <summary>
        ///     背景音乐变更广播
        /// </summary>
        /// <remarks>当当前背景音乐发生切换（播放、替换或停止）时发送。</remarks>
        public readonly struct MusicChanged : IBroadcastData
        {
            /// <summary>之前正在播放的音乐 Key（可能为空或 null 表示之前没有音乐）。</summary>
            public readonly string PrevKey;
            /// <summary>当前新播放的音乐 Key（为空或 null 表示停止播放）。</summary>
            public readonly string NewKey;
            /// <summary>切换方式，如 direct/fade/cross 等。</summary>
            public readonly string Method;
            /// <summary>触发变更的 UTC 时间戳。</summary>
            public readonly DateTime Timestamp;

            /// <param name="prevKey">之前正在播放的音乐 Key（可能为空或 null 表示之前没有音乐）。</param>
            /// <param name="newKey">当前新播放的音乐 Key（为空或 null 表示停止播放）。</param>
            /// <param name="method">切换方式，如 direct/fade/cross 等。</param>
            public MusicChanged(string prevKey, string newKey, string method)
            {
                PrevKey = prevKey;
                NewKey = newKey;
                Method = method;
                Timestamp = DateTime.UtcNow;
            }

        }

        /// <summary>
        ///     音量变更广播
        /// </summary>
        /// <remarks>当某个音频类别的音量发生变化时发送。</remarks>
        public readonly struct VolumeChanged : IBroadcastData
        {
            /// <summary>发生变化的音频类别。</summary>
            public readonly AudioCategory Category;
            /// <summary>新的音量值（0-1）。</summary>
            public readonly float NewVolume;
            /// <summary>触发变更的 UTC 时间戳。</summary>
            public readonly DateTime Timestamp;


            public VolumeChanged(AudioCategory category, float newVolume)
            {
                Category = category;
                NewVolume = newVolume;
                Timestamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     静音状态变更广播
        /// </summary>
        /// <remarks>当某个音频类别的静音状态被切换时发送。</remarks>
        public readonly struct MuteChanged : IBroadcastData
        {
            /// <summary>发生变化的音频类别。</summary>
            public readonly AudioCategory Category;
            /// <summary>当前是否静音。</summary>
            public readonly bool Mute;
            /// <summary>触发变更的 UTC 时间戳。</summary>
            public readonly DateTime Timestamp;


            public MuteChanged(AudioCategory category, bool mute)
            {
                Category = category;
                Mute = mute;
                Timestamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     音效播放广播
        /// </summary>
        /// <remarks>当播放一个音效时发送。</remarks>
        public readonly struct SfxPlayed : IBroadcastData
        {
            /// <summary>被播放音效的资源 Key。</summary>
            public readonly string ClipKey;
            /// <summary>本次播放使用的音量值。</summary>
            public readonly float Volume;
            /// <summary>触发播放的 UTC 时间戳。</summary>
            public readonly DateTime Timestamp;

            public SfxPlayed(string clipKey, float volume)
            {
                ClipKey = clipKey;
                Volume = volume;
                Timestamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     音频系统错误广播
        /// </summary>
        /// <remarks>当音频系统出现不可恢复或重要错误时发送。</remarks>
        public readonly struct AudioError : IBroadcastData
        {
            /// <summary>错误码，用于分类错误类型。</summary>
            public readonly string Code;
            /// <summary>错误的可读描述信息。</summary>
            public readonly string Message;
            /// <summary>错误发生时的上下文，例如资源 Key、调用来源等。</summary>
            public readonly string Context;

            public AudioError(string code, string message, string context)
            {
                Code = code;
                Message = message;
                Context = context;
            }
        }
    }
}