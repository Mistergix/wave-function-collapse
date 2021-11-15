using System;
using UnityEngine;

namespace ESGI.WFC
{
    [CreateAssetMenu(fileName = "Module 2D", menuName = "WFC/2D/Module", order = 0)]
    public class Module : ScriptableObject
    {
        /// <summary>
        /// The module`s game object.
        /// </summary>
        public GameObject modulePrefab;
        

        public Neighbours<Socket> sockets;

    }
}