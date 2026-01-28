using CFramework.Core.CommandSystem;

namespace CFramework.Systems.TimerSystem
{
    public static class TimerCommands
    {
        /// <summary>
        ///     启动一个定时器的命令。
        /// </summary>
        public readonly struct StartTimer : ICommandData
        {
            /// <summary>定时器唯一标识。</summary>
            public readonly string Id;
            /// <summary>单次计时时长（秒）。</summary>
            public readonly float DurationSeconds;
            /// <summary>是否循环触发。</summary>
            public readonly bool Loop;
            /// <summary>循环次数（0 或以下表示无限循环，仅在 Loop 为 true 时生效）。</summary>
            public readonly int RepeatCount;
            /// <summary>是否使用不受 Time.timeScale 影响的时间。</summary>
            public readonly bool UseUnscaledTime;
            /// <summary>业务标签（可选），用于批量操作。</summary>
            public readonly string Tag;

            /// <param name="id">定时器唯一标识。</param>
            /// <param name="durationSeconds">单次计时时长（秒）。</param>
            /// <param name="loop">是否循环触发。</param>
            /// <param name="repeatCount">循环次数（0 或以下表示无限循环，仅在 Loop 为 true 时生效）。</param>
            /// <param name="useUnscaledTime">是否使用不受 Time.timeScale 影响的时间。</param>
            /// <param name="tag">业务标签（可选），用于批量操作。</param>
            public StartTimer(
                string id,
                float durationSeconds,
                bool loop = false,
                int repeatCount = 0,
                bool useUnscaledTime = false,
                string tag = null)
            {
                Id = id;
                DurationSeconds = durationSeconds;
                Loop = loop;
                RepeatCount = repeatCount;
                UseUnscaledTime = useUnscaledTime;
                Tag = tag;
            }
        }


        /// <summary>
        ///     按 Id 停止定时器。
        /// </summary>
        public readonly struct StopTimer : ICommandData
        {
            /// <summary>要停止的定时器唯一标识。</summary>
            public readonly string Id;

            /// <param name="id">要停止的定时器唯一标识。</param>
            public StopTimer(string id)
            {
                Id = id;
            }
        }


        /// <summary>
        ///     按标签停止所有匹配的定时器。
        /// </summary>
        public readonly struct StopTimersByTag : ICommandData
        {
            /// <summary>用于筛选目标定时器的标签。</summary>
            public readonly string Tag;

            /// <param name="tag">用于筛选目标定时器的标签。</param>
            public StopTimersByTag(string tag)
            {
                Tag = tag;
            }
        }


        /// <summary>
        ///     按 Id 暂停定时器。
        /// </summary>
        public readonly struct PauseTimer : ICommandData
        {
            /// <summary>要暂停的定时器唯一标识。</summary>
            public readonly string Id;

            /// <param name="id">要暂停的定时器唯一标识。</param>
            public PauseTimer(string id)
            {
                Id = id;
            }
        }


        /// <summary>
        ///     按 Id 恢复定时器。
        /// </summary>
        public readonly struct ResumeTimer : ICommandData
        {
            /// <summary>要恢复的定时器唯一标识。</summary>
            public readonly string Id;

            /// <param name="id">要恢复的定时器唯一标识。</param>
            public ResumeTimer(string id)
            {
                Id = id;
            }
        }


        /// <summary>
        ///     按标签暂停所有匹配的定时器。
        /// </summary>
        public readonly struct PauseTimersByTag : ICommandData
        {
            /// <summary>用于筛选目标定时器的标签。</summary>
            public readonly string Tag;

            /// <param name="tag">用于筛选目标定时器的标签。</param>
            public PauseTimersByTag(string tag)
            {
                Tag = tag;
            }
        }


        /// <summary>
        ///     按标签恢复所有匹配的定时器。
        /// </summary>
        public readonly struct ResumeTimersByTag : ICommandData
        {
            /// <summary>用于筛选目标定时器的标签。</summary>
            public readonly string Tag;

            /// <param name="tag">用于筛选目标定时器的标签。</param>
            public ResumeTimersByTag(string tag)
            {
                Tag = tag;
            }
        }
    }
}