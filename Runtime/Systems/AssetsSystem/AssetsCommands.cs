using System;
using CFramework.Core.CommandSystem;

namespace CFramework.Systems.AssetsSystem
{
    public static class AssetsCommands
    {
        /// <summary>
        ///     跟踪的选定地址集合进度
        /// </summary>
        public struct StartTrackSelectedProgress : ICommandData
        {
            /// <summary>
            ///     本次进度跟踪的会话 Id（与广播中 SessionId 对应）。
            /// </summary>
            public string TrackId;
            /// <summary>
            ///     要跟踪的选定地址集合。
            /// </summary>
            public string[] Ids;
            public StartTrackSelectedProgress(string trackId, string[] ids)
            {
                (TrackId, Ids) = (trackId, ids);
            }
        }

        /// <summary>
        ///     停止跟踪选定地址集合进度
        /// </summary>
        public struct StopTrackSelectedProgress : ICommandData
        {
            /// <summary>要停止跟踪的进度 Id。</summary>
            public string TrackId;
            public StopTrackSelectedProgress(string trackId)
            {
                TrackId = trackId;
            }

        }

        /// <summary>
        ///     注册资源接收器
        /// </summary>
        public struct RegisterAssetReceiver : ICommandData
        {
            /// <summary>接收器处理的资源类型。</summary>
            public Type Type;
            public RegisterAssetReceiver(Type type)
            {
                Type = type;
            }

        }

        /// <summary>
        ///     释放指定的资源
        /// </summary>
        /// <typeparam name="TReceiverType">接收器处理的资源类型</typeparam>
        public struct ReleaseAsset<TReceiverType> : ICommandData
        {
            /// <summary>资源的地址。</summary>
            public string Path;
            public ReleaseAsset(string path)
            {
                Path = path;
            }

        }

        /// <summary>
        ///     释放指定的资源
        /// </summary>
        /// <typeparam name="TReceiverType">接收器处理的资源类型</typeparam>
        public struct ReleaseAssets<TReceiverType> : ICommandData
        {
            /// <summary>资源集合的标签。</summary>
            public string Label;
            public ReleaseAssets(string label)
            {
                Label = label;
            }

        }
    }
}