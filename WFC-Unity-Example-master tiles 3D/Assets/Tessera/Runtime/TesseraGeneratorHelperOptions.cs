using DeBroglie.Constraints;
using DeBroglie.Topo;
using System;
using System.Collections.Generic;
using System.Threading;



namespace Tessera
{
    internal class TesseraGeneratorHelperOptions
    {
        // Basic configuration
        public IGrid grid;
        public TesseraPalette palette;
        public TileModelInfo tileModelInfo;
        // Constraints
        public List<ITesseraInitialConstraint> initialConstraints;
        public List<ITileConstraint> constraints;
        public TesseraInitialConstraint skyBox;
        // Run control
        public bool backtrack;
        public int stepLimit;
        public TesseraWfcAlgorithm algorithm;
        public Action<string, float> progress;
        public Action<ITopoArray<ISet<ModelTile>>> progressTiles;
        public XoRoRNG xororng;
        public CancellationToken cancellationToken;
        public FailureMode failureMode;
        public TesseraStats stats;

    }
}
