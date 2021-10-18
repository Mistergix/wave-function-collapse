using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(menuName = "WFC/Tile Data/Simple", order = 0)]
    public class SimpleTileData : ScriptableObject
    {
        [SerializeField, Min(1)] private int size;
        [SerializeField] private bool unique;
        [SerializeField] private List<Tile> tiles;
        [SerializeField] private List<Subset> subsets;
        [SerializeField] private List<Neighbor> neighbors;

        public int Size => size;
        public bool Unique => unique;
        public List<Tile> Tiles => tiles;

        public List<Neighbor> Neighbors => neighbors;

        public Subset GetSubset(string subsetName)
        {
            if (subsetName == null)
            {
                return new Subset();
            }

            var subset = subsets.FirstOrDefault(s => s.name == subsetName);
            return subset;
        }
    }
}