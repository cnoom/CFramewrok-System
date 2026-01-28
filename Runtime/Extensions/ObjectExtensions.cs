using UnityEngine;

namespace CFramework.Extensions
{
    public static class ObjectExtensions
    {
        public static T As<T>(this object obj)
        {
            if(obj is T t) return t;
            Debug.LogWarning("物体不能转换为对应类型: " + typeof(T).Name);
            return default;
        }
    }
}