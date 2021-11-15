using System;
using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(menuName = "WFC/Neighbor")]
    public class Neighbor : ScriptableObject
    {
        public Data left;
        public Data right;
        [Serializable]
        public struct Data
        {
            public Tile tile;
            public int id;
        }
    }
}