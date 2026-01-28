using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CFramework.Extensions
{
    public static class RandomExtensions
    {
        /// <summary>
        ///     重新排列集合,使用的是 Fisher-Yates 随机排序
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShuffleExtension<T>(this ICollection<T> self)
        {
            T[] array = self.ToArray();
            int n = array.Length;
            for(var i = 0; i < n; i++)
            {
                int r = i + (n - i).ToRandom();
                (array[r], array[i]) = (array[i], array[r]);
            }

            self.Clear();
            foreach (T item in array) self.Add(item);
        }

        /// <summary>
        ///     获取随机元素
        /// </summary>
        public static T GetRandom<T>(this IEnumerable<T> self)
        {
            List<T> enumerable = self.ToList();
            return !enumerable.Any() ? default : enumerable[enumerable.Count.ToRandom()];
        }

        /// <returns>[0,self)</returns>
        public static int ToRandom(this int self)
        {
            return Random.Range(0, self);
        }
    }
}