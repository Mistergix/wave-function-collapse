namespace ESGI.WFC
{
    public class EdgeFilter
    {
        private readonly Module.EdgeConnectionTypes filterType;
        /// <summary>
        /// Should the cells contains this type of edge
        /// </summary>
        private readonly bool isInclusive;

        private readonly Directions edgeDirection;
        
        public EdgeFilter(Directions edgeDirection, Module.EdgeConnectionTypes filterType, bool isInclusive)
        {
            this.edgeDirection = edgeDirection;
            this.filterType = filterType;
            this.isInclusive = isInclusive;
        }
        
        public EdgeFilter(int dir, Module.EdgeConnectionTypes filterType, bool isInclusive)
        {
            this.edgeDirection = ToEdgeDirection(dir);
            this.filterType = filterType;
            this.isInclusive = isInclusive;
        }
        
        public bool CheckModule(Module module)
        {
            var edge = ((int)edgeDirection + 2) % 4;
            var match = module.edgeConnections[edge] == filterType;

            return isInclusive ? !match : match;
        }
        
        public enum Directions
        {
            Bottom = 0,
            Right = 1,
            Top = 2,
            Left = 3
        }
        
        private EdgeFilter.Directions ToEdgeDirection(int i)
        {
            return i switch
            {
                0 => EdgeFilter.Directions.Bottom,
                1 => EdgeFilter.Directions.Right,
                2 => EdgeFilter.Directions.Top,
                3 => EdgeFilter.Directions.Left,
                _ => default
            };
        }
    }
}