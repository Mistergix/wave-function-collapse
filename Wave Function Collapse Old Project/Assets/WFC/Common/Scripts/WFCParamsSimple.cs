using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(menuName = "WFC/Params/Simple Model")]

    public class WFCParamsSimple : WFCParams
    {
        [SerializeField] private Subset subset;
        [SerializeField] private bool blackBackground;
        [SerializeField] private SimpleTileData tileData;

        public SimpleTileData TileData => tileData;

        public override Model GetModel()
        {
            return new SimpleModel(this, subset, Width, Height, Periodic, blackBackground, HeuristicInstance);
        }
    }
}