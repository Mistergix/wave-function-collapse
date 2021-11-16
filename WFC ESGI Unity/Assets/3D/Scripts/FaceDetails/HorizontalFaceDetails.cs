using System;

namespace ESGI.WFC.ThreeDimensions
{
    [Serializable]
    public class HorizontalFaceDetails : FaceDetails
    {
        public bool symmetric;
        public bool flipped;

        public override string ToString()
        {
            var suffix = "";
            if (symmetric)
            {
                suffix = "s";
            }

            if (flipped)
            {
                suffix = "F";
            }

            return $"{connector}{suffix}";
        }

        public override void ResetConnector()
        {
            base.ResetConnector();
            symmetric = false;
            flipped = false;
        }
    }
}