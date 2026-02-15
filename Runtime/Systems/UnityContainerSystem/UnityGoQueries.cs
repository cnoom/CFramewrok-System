using System;
using CFramework.Core.QuerySystem;
using UnityEngine;

namespace CFramework.Systems.UnityContainerSystem
{
    public static class UnityGoQueries
    {
        /// <summary>
        ///     获取 GameObject 查询
        /// </summary>
        public readonly struct GetGameObject : IQueryData<GameObject>
        {
            public readonly string Key;
            public readonly string Scope;
            public GetGameObject(string key, string scope = null)
            {
                Key = key;
                Scope = scope;
            }
        }

        /// <summary>
        ///     获取 Transform 查询
        /// </summary>
        public readonly struct GetTransform : IQueryData<Transform>
        {
            public readonly string Key;
            public readonly string Scope;
            public GetTransform(string key, string scope = null)
            {
                Key = key;
                Scope = scope;
            }
        }

        /// <summary>
        ///     获取 RectTransform 查询
        /// </summary>
        public readonly struct GetRectTransform : IQueryData<RectTransform>
        {
            public readonly string Key;
            public readonly string Scope;
            public GetRectTransform(string key, string scope = null)
            {
                Key = key;
                Scope = scope;
            }
        }

        /// <summary>
        ///     获取 Component 查询
        /// </summary>
        public readonly struct GetComponent : IQueryData<Component>
        {
            public readonly Type ComponentType;
            public readonly string Key;
            public readonly string Scope;
            public GetComponent(Type componentType, string key, string scope = null)
            {
                ComponentType = componentType;
                Key = key;
                Scope = scope;
            }
        }
    }
}