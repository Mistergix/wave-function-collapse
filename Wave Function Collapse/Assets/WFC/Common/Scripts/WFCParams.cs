using UnityEngine;

namespace WFC
{
    public abstract class WFCParams : ScriptableObject
    {
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private int screenshots;
        [SerializeField] private Heuristic heuristic;
        [SerializeField] private int limit = -1;
        
        [SerializeField] private bool periodic;

        public enum Heuristic
        {
            Entropy,
            MRV,
            Scanline
        }

        protected bool Periodic => periodic;

        protected int Width => width;

        protected int Height => height;

        protected Heuristic HeuristicInstance => heuristic;

        public int Screenshots => screenshots;
        public int Limit => limit;

        public abstract Model GetModel();
    }
}