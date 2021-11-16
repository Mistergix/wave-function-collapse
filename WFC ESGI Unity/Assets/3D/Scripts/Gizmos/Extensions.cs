using UnityEngine;

namespace ESGI.WFC.ThreeDimensions
{
    public static class Extensions
    {
        public static Vector3 ToVector3(this Vector3Int vector) {
            return (Vector3)(vector);
        }
    }
}