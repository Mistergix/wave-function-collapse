using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(menuName = "WFC/Subset", order = 0)]
    public class Subset : ScriptableObject
    {
        public List<Tile> tiles;
    }
}