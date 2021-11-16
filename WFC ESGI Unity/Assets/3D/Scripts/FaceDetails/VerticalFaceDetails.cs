using System;
using System.Linq;
using UnityEngine;

namespace ESGI.WFC.ThreeDimensions
{
    [Serializable]
    public class VerticalFaceDetails : FaceDetails
    {
        public bool invariant;
        [Range(0,3)] public int rotation;
        public override string ToString()
        {
            var suffix = "";
            if (invariant)
            {
                suffix = "i";
            }

            if (rotation != 0)
            {
                suffix = "_abc".ElementAt(rotation).ToString();
            }

            return $"{connector}{suffix}";
        }

        public override void ResetConnector()
        {
            base.ResetConnector();
            invariant = false;
            rotation = 0;
        }
    }
}