using System;
using System.Collections.Generic;

namespace ESGI.WFC.ThreeDimensions
{
    [Serializable]
    public abstract class FaceDetails
    {
        public bool walkable;
        public int connector;
        public List<ModulePrototype> excludedNeighbours;
        public bool enforceWalkableNeighbours;
        public bool isOcclusionPortal;
        
        public virtual void ResetConnector()
        {
            connector = 0;
        }
    }
}