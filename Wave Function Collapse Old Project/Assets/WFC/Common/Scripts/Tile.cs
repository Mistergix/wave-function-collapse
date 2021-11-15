using System;
using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(menuName = "WFC/Tile")]
    public class Tile : ScriptableObject
    {
        public enum Symmetry
        {
            L,
            T,
            I,
            F,
            Slash,
            X
        }
        
        public string Name => sprite.name;
        public Symmetry symmetry = Symmetry.X;
        public Sprite sprite;
        public float weight = 1.0f;
        
        public int GetCardinality()
        {
            switch (symmetry)
            {
                case Symmetry.L:
                case Symmetry.T:
                    return 4;
                case Symmetry.I: 
                case Symmetry.Slash :
                    return 2;
                case Symmetry.F :
                    return 8;
                case Symmetry.X:
                default:
                    return 1;
            }
        }
        
        public (Func<int, int> a, Func<int, int> b) GetFunctions()
        {
            switch (symmetry)
            {
                case Symmetry.L:
                    return (i => (i + 1) % 4, i => i % 2 == 0 ? i + 1 : i - 1);
                case Symmetry.T:
                    return (i => (i + 1) % 4, i => i % 2 == 0 ? i : 4 - i);
                case Symmetry.I:
                    return (i => 1 - i, i => i);
                case Symmetry.Slash:
                    return (i => 1 - i, i => 1 - i);
                case Symmetry.F:
                    return (i => i < 4 ? (i + 1) % 4 : 4 + (i - 1) % 4, i => i < 4 ? i + 4 : i - 4);
                case Symmetry.X:
                default:
                    return (i => i, i => i);
            }
        }
    }
}