using UnityEngine;

namespace WFC
{
    public class OverlappingModel : Model
    {
        public OverlappingModel(int N, int width, int height, bool periodicInput, bool periodic,
            int symmetry, int ground, WFCParams.Heuristic heuristicInstance) : base(N, width, height, periodic, heuristicInstance)
        {
            
        }

        public override Texture2D GetGraphics()
        {
            throw new System.NotImplementedException();
        }
    }
}