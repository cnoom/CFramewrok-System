using System;
using UnityEngine;

namespace CFramework.Extensions
{
    /// <summary>
    ///     各种数值类型来回转换的拓展类
    /// </summary>
    public static class NumberConverterExtensions
    {
        /// <summary>
        ///     将整数转换为绝对值
        /// </summary>
        /// <param name="value">要转换的整数</param>
        /// <returns>转换后的绝对值</returns>
        public static int ToAbs(this int value)
        {
            return Math.Abs(value);
        }

        /// <summary>
        ///     将浮点数转换为绝对值
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换后的绝对值</returns>
        public static float ToAbs(this float value)
        {
            return Mathf.Abs(value);
        }

        /// <summary>
        ///     将双精度浮点数转换为绝对值
        /// </summary>
        /// <param name="value">要转换的双精度浮点数</param>
        /// <returns>转换后的绝对值</returns>
        public static double ToAbs(this double value)
        {
            return Math.Abs(value);
        }


        /// <summary>
        ///     将浮点数转换为整数
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <param name="isRound">是否四舍五入</param>
        /// <returns>转换后的整数</returns>
        public static int ToInt(this float value, bool isRound = false)
        {
            if(isRound)
            {
                return Mathf.Round(value).ToInt();
            }

            return (int)value;
        }

        /// <summary>
        ///     将双精度浮点数转换为整数
        /// </summary>
        /// <param name="value">要转换的双精度浮点数</param>
        /// <param name="isRound">是否四舍五入</param>
        /// <returns>转换后的整数</returns>
        public static int ToInt(this double value, bool isRound = false)
        {
            if(isRound)
            {
                return Math.Round(value).ToInt();
            }

            return (int)value;
        }

        /// <summary>
        ///     将对象转换为整数
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的整数</returns>
        /// <exception cref="InvalidCastException">如果对象不能转换为整数</exception>
        public static int ToInt(this object obj)
        {
            return obj switch
            {
                int intValue => intValue,
                float floatValue => floatValue.ToInt(),
                double doubleValue => doubleValue.ToInt(),
                string stringValue when int.TryParse(stringValue, out int v) => v,
                bool boolValue => boolValue ? 1 : 0,
                Enum => (int)obj,
                _ => throw new InvalidCastException($"无法将 {obj} 转换为整数。")
            };
        }

        /// <summary>
        ///     将对象转换为浮点数
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的浮点数</returns>
        /// <exception cref="InvalidCastException">如果对象不能转换为浮点数</exception>
        public static float ToFloat(this object obj)
        {
            return obj switch
            {
                float floatValue => floatValue,
                int intValue => intValue,
                double doubleValue => (float)doubleValue,
                string stringValue when float.TryParse(stringValue, out float v) => v,
                _ => throw new InvalidCastException($"无法将 {obj} 转换为浮点数。")
            };
        }

        /// <summary>
        ///     将对象转换为双精度浮点数
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的双精度浮点数</returns>
        /// <exception cref="InvalidCastException">如果对象不能转换为双精度浮点数</exception>
        public static double ToDouble(this object obj)
        {
            return obj switch
            {
                double doubleValue => doubleValue,
                int intValue => intValue,
                float floatValue => floatValue,
                string stringValue when double.TryParse(stringValue, out double v) => v,
                _ => throw new InvalidCastException($"无法将 {obj} 转换为双精度浮点数。")
            };
        }
    }
}