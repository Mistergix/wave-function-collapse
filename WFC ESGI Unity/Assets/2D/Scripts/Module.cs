using System;
using UnityEngine;

namespace ESGI.WFC
{
    [CreateAssetMenu(menuName = "WFC/2D/Module", order = 0)]
    public class Module : ScriptableObject
    {
        public GameObject modulePrefab;
        public Neighbours<Socket> sockets;
    }
}