using System;
using UnityEngine.Serialization;

namespace ESGI.WFC.ThreeDimensions
{
    [Serializable]
    public class Module
    {
        public ModulePrototype prototype;
        public int rotation;
        public int Index { get; set; }
        public float pLogP;
    }
}