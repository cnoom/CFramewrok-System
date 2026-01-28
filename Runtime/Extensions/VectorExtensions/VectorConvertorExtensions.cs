using UnityEngine;

namespace CFramework.Extensions.VectorExtensions
{
    public static class VectorConvertorExtensions
    {
        public enum VectorRounding
        {
            Floor,
            Ceil,
            Round
        }

        // Vector2 -> Vector2Int
        public static Vector2Int ToVector2Int(this Vector2 v, VectorRounding mode = VectorRounding.Round)
        {
            return new Vector2Int(Apply(v.x, mode), Apply(v.y, mode));
        }

        // Vector2 -> Vector3
        public static Vector3 ToVector3(this Vector2 v, float z = 0f)
        {
            return new Vector3(v.x, v.y, z);
        }

        // Vector2 -> Vector3Int
        public static Vector3Int ToVector3Int(this Vector2 v, VectorRounding mode = VectorRounding.Round, int z = 0)
        {
            return new Vector3Int(Apply(v.x, mode), Apply(v.y, mode), z);
        }

        // Vector3 -> Vector3Int
        public static Vector3Int ToVector3Int(this Vector3 v, VectorRounding mode = VectorRounding.Round)
        {
            return new Vector3Int(Apply(v.x, mode), Apply(v.y, mode), Apply(v.z, mode));
        }

        // Vector3 -> Vector2
        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        // Vector3 -> Vector2Int
        public static Vector2Int ToVector2Int(this Vector3 v, VectorRounding mode = VectorRounding.Round)
        {
            return new Vector2Int(Apply(v.x, mode), Apply(v.y, mode));
        }

        // Vector2Int -> Vector2
        public static Vector2 ToVector2(this Vector2Int v)
        {
            return new Vector2(v.x, v.y);
        }

        // Vector2Int -> Vector3
        public static Vector3 ToVector3(this Vector2Int v, float z = 0f)
        {
            return new Vector3(v.x, v.y, z);
        }

        // Vector2Int -> Vector3Int
        public static Vector3Int ToVector3Int(this Vector2Int v, int z = 0)
        {
            return new Vector3Int(v.x, v.y, z);
        }

        // Vector3Int -> Vector3
        public static Vector3 ToVector3(this Vector3Int v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        // Vector3Int -> Vector2
        public static Vector2 ToVector2(this Vector3Int v)
        {
            return new Vector2(v.x, v.y);
        }

        // Vector3Int -> Vector2Int
        public static Vector2Int ToVector2Int(this Vector3Int v)
        {
            return new Vector2Int(v.x, v.y);
        }

        private static int Apply(float value, VectorRounding mode)
        {
            switch(mode)
            {
                case VectorRounding.Floor:
                    return Mathf.FloorToInt(value);
                case VectorRounding.Ceil:
                    return Mathf.CeilToInt(value);
                case VectorRounding.Round:
                default:
                    return Mathf.RoundToInt(value);
            }
        }
    }
}