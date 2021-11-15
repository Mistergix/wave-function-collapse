using System;
using UnityEngine;

namespace ESGI.WFC
{
    [CreateAssetMenu(fileName = "Module 2D", menuName = "WFC/2D/Module", order = 0)]
    public class Module : ScriptableObject
    {
        /// <summary>
        /// Different edge connection types.
        /// </summary>
        public enum EdgeConnectionTypes
        {
            Block,
            Open,
        }
        
        /// <summary>
        /// The module`s game object.
        /// </summary>
        public GameObject modulePrefab;
        
        [Serializable]
        public class EdgeConnectionTypesNeighbours : Neighbours<EdgeConnectionTypes> { }
        
        public EdgeConnectionTypesNeighbours edgeConnections;
    }
}