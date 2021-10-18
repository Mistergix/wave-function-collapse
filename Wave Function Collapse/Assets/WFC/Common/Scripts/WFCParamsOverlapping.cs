using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(menuName = "WFC/Params/Overlapping Model")]
    public class WFCParamsOverlapping : WFCParams
    {
        [SerializeField] private int symmetry;
        [SerializeField] private int ground;
        [SerializeField] private bool periodicInput;
        [SerializeField] private int n;

        public int Symmetry => symmetry;

        public int Ground => ground;
        
        public override Model GetModel()
        {
            return new OverlappingModel( n, Width, Height, periodicInput, Periodic, Symmetry, Ground, HeuristicInstance);
        }
    }
}