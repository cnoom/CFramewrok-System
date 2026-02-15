using System;
using CFramework.Core.QuerySystem;
using UnityEngine;

namespace CFramework.Systems.AssetsSystem
{
    public static class AssetsQueries
    {

        /// <summary>
        ///     指定类型的资源接收器查询
        /// </summary>
        /// <returns>是否存在。</returns>
        public struct HasAssetsReceiver : IQueryData<bool>
        {
            /// <summary>接收器处理的资源类型。</summary>
            public Type Type;
            public HasAssetsReceiver(Type type)
            {
                Type = type;
            }

        }

        /// <summary>
        ///     指定地址的资源查询（非泛型）。
        /// </summary>
        /// <returns>UnityEngine.Object 资源。</returns>
        public struct Asset : IQueryData<UnityEngine.Object>
        {
            /// <summary>资源的地址。</summary>
            public string Address;
            public Asset(string address)
            {
                Address = address;
            }
        }

        /// <summary>
        ///     指定地址的资源查询（泛型）。
        /// </summary>
        /// <returns>对应类型资源。</returns>
        public struct Asset<TAsset> : IQueryData<TAsset>
        {
            /// <summary>资源的地址。</summary>
            public string Address;
            public Asset(string address)
            {
                Address = address;
            }

        }

        /// <summary>
        ///     指定标签的资源集合查询（非泛型）。
        /// </summary>
        /// <returns>UnityEngine.Object 资源数组。</returns>
        public struct Assets : IQueryData<UnityEngine.Object[]>
        {
            /// <summary>资源集合的标签。</summary>
            public string Label;
            public Assets(string label)
            {
                Label = label;
            }
        }

        /// <summary>
        ///     指定标签的资源集合查询（泛型）。
        /// </summary>
        /// <returns>对应类型资源集合数组。</returns>
        public struct Assets<TAsset> : IQueryData<TAsset[]>
        {
            /// <summary>资源集合的标签。</summary>
            public string Label;
            public Assets(string label)
            {
                Label = label;
            }

        }
    }
}