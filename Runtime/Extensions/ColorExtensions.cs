using System.Globalization;
using UnityEngine;

namespace CFramework.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        ///     将Unity Color转换为十六进制字符串
        /// </summary>
        /// <param name="color">Unity颜色</param>
        /// <returns>十六进制颜色字符串 (#RRGGBB)</returns>
        public static string ToHex(this Color color)
        {
            Color32 color32 = color;
            return $"#{color32.r:X2}{color32.g:X2}{color32.b:X2}";
        }

        /// <summary>
        ///     将Unity Color转换为十六进制字符串（包含透明度）
        /// </summary>
        /// <param name="color">Unity颜色</param>
        /// <returns>十六进制颜色字符串 (#RRGGBBAA)</returns>
        public static string ToHexWithAlpha(this Color color)
        {
            Color32 color32 = color;
            return $"#{color32.r:X2}{color32.g:X2}{color32.b:X2}{color32.a:X2}";
        }

        /// <summary>
        ///     将十六进制字符串转换为Unity Color
        /// </summary>
        /// <param name="hex">十六进制颜色字符串 (#RRGGBB 或 #RRGGBBAA)</param>
        /// <returns>Unity颜色</returns>
        public static Color HexToColor(this string hex)
        {
            if(string.IsNullOrEmpty(hex))
                return Color.white;

            // 移除可能存在的#前缀
            if(hex.StartsWith("#"))
                hex = hex.Substring(1);

            if(hex.Length != 6 && hex.Length != 8)
                return Color.white;

            try
            {
                if(hex.Length == 6)
                {
                    int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                    int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                    int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                    return new Color(r / 255f, g / 255f, b / 255f, 1f);
                }
                else // hex.Length == 8
                {
                    int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                    int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                    int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                    int a = int.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
                    return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                }
            }
            catch
            {
                return Color.white;
            }
        }

        /// <summary>
        ///     将Color转换为HSV
        /// </summary>
        /// <param name="color">Unity颜色</param>
        /// <returns>HSV值 (色调, 饱和度, 明度)</returns>
        public static Vector3 ToHSV(this Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            return new Vector3(h, s, v);
        }

        /// <summary>
        ///     将HSV转换为Color
        /// </summary>
        /// <param name="h">色调 (0-1)</param>
        /// <param name="s">饱和度 (0-1)</param>
        /// <param name="v">明度 (0-1)</param>
        /// <returns>Unity颜色</returns>
        public static Color HSVToColor(float h, float s, float v)
        {
            return Color.HSVToRGB(h, s, v);
        }

        /// <summary>
        ///     调整颜色的亮度
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <param name="brightness">亮度调整值 (-1 到 1)</param>
        /// <returns>调整后的颜色</returns>
        public static Color AdjustBrightness(this Color color, float brightness)
        {
            Vector3 hsv = color.ToHSV();
            hsv.z = Mathf.Clamp01(hsv.z + brightness);
            return HSVToColor(hsv.x, hsv.y, hsv.z);
        }

        /// <summary>
        ///     调整颜色的饱和度
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <param name="saturation">饱和度调整值 (-1 到 1)</param>
        /// <returns>调整后的颜色</returns>
        public static Color AdjustSaturation(this Color color, float saturation)
        {
            Vector3 hsv = color.ToHSV();
            hsv.y = Mathf.Clamp01(hsv.y + saturation);
            return HSVToColor(hsv.x, hsv.y, hsv.z);
        }

        /// <summary>
        ///     调整颜色的色调
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <param name="hue">色调调整值 (-1 到 1)</param>
        /// <returns>调整后的颜色</returns>
        public static Color AdjustHue(this Color color, float hue)
        {
            Vector3 hsv = color.ToHSV();
            hsv.x = (hsv.x + hue) % 1f;
            if(hsv.x < 0) hsv.x += 1f;
            return HSVToColor(hsv.x, hsv.y, hsv.z);
        }

        /// <summary>
        ///     获取颜色的互补色
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <returns>互补色</returns>
        public static Color GetComplementary(this Color color)
        {
            return new Color(1f - color.r, 1f - color.g, 1f - color.b, color.a);
        }

        /// <summary>
        ///     获取颜色的灰度值
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <returns>灰度颜色</returns>
        public static Color ToGrayscale(this Color color)
        {
            float gray = color.grayscale;
            return new Color(gray, gray, gray, color.a);
        }

        /// <summary>
        ///     设置颜色的透明度
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <param name="alpha">透明度 (0-1)</param>
        /// <returns>设置透明度后的颜色</returns>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
        }

        /// <summary>
        ///     线性插值两种颜色
        /// </summary>
        /// <param name="from">起始颜色</param>
        /// <param name="to">目标颜色</param>
        /// <param name="t">插值系数 (0-1)</param>
        /// <returns>插值结果</returns>
        public static Color Lerp(this Color from, Color to, float t)
        {
            return Color.Lerp(from, to, Mathf.Clamp01(t));
        }

        /// <summary>
        ///     在HSV空间中进行线性插值
        /// </summary>
        /// <param name="from">起始颜色</param>
        /// <param name="to">目标颜色</param>
        /// <param name="t">插值系数 (0-1)</param>
        /// <returns>插值结果</returns>
        public static Color LerpHSV(this Color from, Color to, float t)
        {
            Vector3 fromHSV = from.ToHSV();
            Vector3 toHSV = to.ToHSV();

            // 处理色调的环形插值
            float hue = Mathf.LerpAngle(fromHSV.x * 360f, toHSV.x * 360f, t) / 360f;
            float saturation = Mathf.Lerp(fromHSV.y, toHSV.y, t);
            float value = Mathf.Lerp(fromHSV.z, toHSV.z, t);
            float alpha = Mathf.Lerp(from.a, to.a, t);

            Color result = HSVToColor(hue, saturation, value);
            result.a = alpha;
            return result;
        }

        /// <summary>
        ///     将RGBA字符串转换为Unity Color
        /// </summary>
        /// <param name="rgba">RGBA字符串，格式为 "RGBA(r, g, b, a)" 或 "r,g,b,a"</param>
        /// <returns>Unity颜色</returns>
        public static Color RGBAToColor(this string rgba)
        {
            if(string.IsNullOrEmpty(rgba))
                return Color.white;

            try
            {
                // 移除可能存在的"RGBA("和")"前缀后缀
                if(rgba.StartsWith("RGBA(") && rgba.EndsWith(")"))
                {
                    rgba = rgba.Substring(5, rgba.Length - 6);
                }

                // 分割字符串获取各个分量
                string[] parts = rgba.Split(',');
                if(parts.Length != 4)
                    return Color.white;

                float r = float.Parse(parts[0].Trim());
                float g = float.Parse(parts[1].Trim());
                float b = float.Parse(parts[2].Trim());
                float a = float.Parse(parts[3].Trim());

                // 确保值在0-1范围内
                r = Mathf.Clamp01(r);
                g = Mathf.Clamp01(g);
                b = Mathf.Clamp01(b);
                a = Mathf.Clamp01(a);

                return new Color(r, g, b, a);
            }
            catch
            {
                return Color.white;
            }
        }


        /// <summary>
        ///     获取颜色的字符串表示
        /// </summary>
        /// <param name="color">Unity颜色</param>
        /// <returns>颜色字符串 (RGBA)</returns>
        public static string ToStringRGBA(this Color color)
        {
            return $"RGBA({color.r:F2}, {color.g:F2}, {color.b:F2}, {color.a:F2})";
        }
    }
}