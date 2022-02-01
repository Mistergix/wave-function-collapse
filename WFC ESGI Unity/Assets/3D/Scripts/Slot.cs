using UnityEngine;

namespace ESGI.WFC.ThreeDimensions
{
    public class Slot
    {
        public Module module;
        public GameObject gameObject;
        public Vector3Int position;
        public bool Collapsed => module != null;
    }
}