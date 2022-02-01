namespace ESGI.WFC
{
    public class EdgeFilter
    {
        private readonly Socket filterType;
       
        private readonly bool isInclusive;

        private readonly Directions edgeDirection;
        
        /// <summary>
        /// Should the cells contains this type of edge
        /// </summary>

        public delegate bool CheckModuleMatchFunction(Module module, int edge);
        
        public EdgeFilter(Directions edgeDirection, Socket filterType, bool isInclusive)
        {
            this.edgeDirection = edgeDirection;
            this.filterType = filterType;
            this.isInclusive = isInclusive;
        }
        
        public EdgeFilter(int dir, Socket filterType, bool isInclusive)
        {
            edgeDirection = ToEdgeDirection(dir);
            this.filterType = filterType;
            this.isInclusive = isInclusive;
        }
        
        public bool CheckModule(Module module, CheckModuleMatchFunction matchEquality)
        {
            var edge = ((int)edgeDirection + 2) % 4;
            var match = matchEquality(module, edge);

            return isInclusive ? !match : match;
        }

        public bool MatchEquality(Module module, int edge)
        {
            var match = module.sockets[edge] == filterType;
            return match;
        }
        
        public static bool MatchType(Module module, int edge)
        {
            var socket = module.sockets[edge];
            var match = socket.GetType() == typeof(SocketBlock) || socket.GetType().IsSubclassOf(typeof(SocketBlock));
            return match;
        }

        public enum Directions
        {
            Bottom = 0,
            Right = 1,
            Top = 2,
            Left = 3
        }
        
        private static Directions ToEdgeDirection(int i)
        {
            return i switch
            {
                0 => Directions.Bottom,
                1 => Directions.Right,
                2 => Directions.Top,
                3 => Directions.Left,
                _ => default
            };
        }
    }
}