using System;
using System.Linq;
using UnityEngine;

namespace ESGI.WFC.ThreeDimensions
{
    public class Orientations
    {
        public const int Left = 0;
        public const int Down = 1;
        public const int Back = 2;
        public const int Right = 3;
        public const int Up = 4;
        public const int Forward = 5;
        
        public static readonly int[] HorizontalDirections = { 0, 2, 3, 5 };
        
        public static readonly string[] Names = { "-Red (Left)", "-Green (Down)", "-Blue (Back)", "+Red (Right)", "+Green (Up)", "+Blue (Forward)" };

        private static Quaternion[] rotations;
        private static Vector3[] vectors;
        private static Vector3Int[] directions; 
        
        public static Quaternion[] Rotations {
            get {
                if (rotations == null) {
                    Initialize();
                }
                return rotations;
            }
        }
        
        public static Vector3Int[] Direction {
            get {
                if (directions == null) {
                    Initialize();
                }
                return directions;
            }
        }
        
        public static int Rotate(int direction, int amount) {
            if (direction == 1 || direction == 4) {
                return direction;
            }
            return HorizontalDirections[(Array.IndexOf(HorizontalDirections, direction) + amount) % 4];
        }
        
        public static bool IsHorizontal(int orientation) {
            return orientation != 1 && orientation != 4;
        }
        
        public static int GetIndex(Vector3 direction)
        {
            if (direction.x < 0) {
                return 0;
            }

            if (direction.y < 0) {
                return 1;
            }

            if (direction.z < 0) {
                return 2;
            }

            if (direction.x > 0) {
                return 3;
            }

            if (direction.y > 0) {
                return 4;
            }

            return 5;
        }
        
        private static void Initialize() {
            vectors = new[] {
                Vector3.left,
                Vector3.down,
                Vector3.back,
                Vector3.right,
                Vector3.up,
                Vector3.forward
            };

            rotations = vectors.Select(Quaternion.LookRotation).ToArray();
            directions = vectors.Select(Vector3Int.RoundToInt).ToArray();
        }
    }
}