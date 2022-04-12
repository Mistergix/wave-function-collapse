using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ESGI.WFC.Exporter
{
    [CreateAssetMenu(menuName = "WFC/2D/Module GAN")]
    public class ModuleGAN : ScriptableObject
    {
        public float rotation;
        public Mesh mesh;
        public Neighbours<Socket> sockets;
    }
}
