using CFramework.Core.QuerySystem;

namespace CFramework.Systems.TimerSystem
{
    public static class TimerQueries
    {
        /// <summary>
        ///     查询指定 Id 的定时器是否存在。
        /// </summary>
        /// <returns>是否存在具有指定 Id 的定时器。</returns>
        public readonly struct HasTimer : IQueryData
        {
            /// <summary>要检查的定时器唯一标识。</summary>
            public readonly string Id;

            /// <param name="id">要检查的定时器唯一标识。</param>
            public HasTimer(string id)
            {
                Id = id;
            }
        }


        /// <summary>
        ///     查询指定 Id 的定时器剩余秒数。
        /// </summary>
        /// <returns>定时器剩余的秒数。</returns>
        public readonly struct GetRemainingSeconds : IQueryData
        {
            /// <summary>目标定时器唯一标识。</summary>
            public readonly string Id;

            /// <param name="id">目标定时器唯一标识。</param>
            public GetRemainingSeconds(string id)
            {
                Id = id;
            }
        }


        /// <summary>
        ///     查询指定 Id 的定时器详细信息。
        /// </summary>
        /// <returns>定时器运行时信息 <see cref="TimerInfo" />。</returns>
        public readonly struct GetTimerInfo : IQueryData
        {
            /// <summary>目标定时器唯一标识。</summary>
            public readonly string Id;

            /// <param name="id">目标定时器唯一标识。</param>
            public GetTimerInfo(string id)
            {
                Id = id;
            }
        }
    }


    /// <summary>
    ///     定时器的运行时信息快照。
    /// </summary>
    public readonly struct TimerInfo
    {
        /// <summary>定时器唯一标识。</summary>
        public readonly string Id;

        /// <summary>单次计时时长（秒）。</summary>
        public readonly float DurationSeconds;

        /// <summary>当前剩余秒数。</summary>
        public readonly float RemainingSeconds;

        /// <summary>是否循环触发。</summary>
        public readonly bool Loop;

        /// <summary>剩余循环次数。</summary>
        public readonly int RemainingLoops;

        /// <summary>是否使用不受 Time.timeScale 影响的时间。</summary>
        public readonly bool UseUnscaledTime;

        /// <summary>业务标签（可选）。</summary>
        public readonly string Tag;

        /// <summary>当前是否处于暂停状态。</summary>
        public readonly bool Paused;

        /// <param name="id">定时器唯一标识。</param>
        /// <param name="durationSeconds">单次计时时长（秒）。</param>
        /// <param name="remainingSeconds">当前剩余秒数。</param>
        /// <param name="loop">是否循环触发。</param>
        /// <param name="remainingLoops">剩余循环次数。</param>
        /// <param name="useUnscaledTime">是否使用不受 Time.timeScale 影响的时间。</param>
        /// <param name="tag">业务标签（可选）。</param>
        /// <param name="paused">当前是否处于暂停状态。</param>
        public TimerInfo(
            string id,
            float durationSeconds,
            float remainingSeconds,
            bool loop,
            int remainingLoops,
            bool useUnscaledTime,
            string tag,
            bool paused)
        {
            Id = id;
            DurationSeconds = durationSeconds;
            RemainingSeconds = remainingSeconds;
            Loop = loop;
            RemainingLoops = remainingLoops;
            UseUnscaledTime = useUnscaledTime;
            Tag = tag;
            Paused = paused;
        }
    }

}