using CFramework.Core.BroadcastSystem;

namespace CFramework.Systems.TimerSystem
{
    /// <summary>
    ///     TimerSystem 相关广播消息定义。
    /// </summary>
    public static class TimerBroadcasts
    {
        /// <summary>
        ///     定时器到点时广播的消息。对于循环定时器，每次触发都会广播一次。
        /// </summary>
        public readonly struct TimerCompleted : IBroadcastData
        {
            /// <summary>定时器的唯一标识。</summary>
            public readonly string Id;
            /// <summary>定时器标签，用于业务分组（可选）。</summary>
            public readonly string Tag;
            /// <summary>截至本次触发，定时器已触发的次数。</summary>
            public readonly int FiredCount;
            /// <summary>在本次触发后是否仍会重复触发。</summary>
            public readonly bool WillRepeat;

            /// <param name="id">定时器的唯一标识。</param>
            /// <param name="tag">定时器标签，用于业务分组（可选）。</param>
            /// <param name="firedCount">截至本次触发，定时器已触发的次数。</param>
            /// <param name="willRepeat">在本次触发后是否仍会重复触发。</param>
            public TimerCompleted(string id, string tag, int firedCount, bool willRepeat)
            {
                Id = id;
                Tag = tag;
                FiredCount = firedCount;
                WillRepeat = willRepeat;
            }
        }


        /// <summary>
        ///     定时器被显式取消时广播的消息（Stop 命令触发）。
        /// </summary>
        public readonly struct TimerCancelled : IBroadcastData
        {
            /// <summary>被取消的定时器唯一标识。</summary>
            public readonly string Id;
            /// <summary>定时器标签，用于业务分组（可选）。</summary>
            public readonly string Tag;

            /// <param name="id">被取消的定时器唯一标识。</param>
            /// <param name="tag">定时器标签，用于业务分组（可选）。</param>
            public TimerCancelled(string id, string tag)
            {
                Id = id;
                Tag = tag;
            }
        }
    }
}