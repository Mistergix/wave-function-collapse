using System;

namespace ESGI.WFC.ThreeDimensions
{
    [Serializable]
    public abstract class FaceDetails
    {
        public bool walkable;
        public int connector;
        public ModulePrototype[] excludedNeighbours;
        public bool enforceWalkableNeighbours;
        public bool isOcclusionPortal;
        
        public virtual void ResetConnector()
        {
            connector = 0;
        }
    }
}