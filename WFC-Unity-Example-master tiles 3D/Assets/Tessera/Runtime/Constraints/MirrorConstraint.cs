using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeBroglie;
using DeBroglie.Constraints;
using DeBroglie.Models;
using DeBroglie.Rot;
using DeBroglie.Topo;
using DeBroglie.Trackers;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// Ensures that the generation is symmetric when x-axis mirrored.
    /// If there are any tile constraints, they will not be mirrored.
    /// > [!Note]
    /// > This class is available only in Tessera Pro
    /// </summary>
    [AddComponentMenu("Tessera/Mirror Constraint", 20)]
    [RequireComponent(typeof(TesseraGenerator))]
    public class MirrorConstraint : TesseraConstraint
    {
        // Unused legacy field
        [SerializeField]
        private bool hasSymmetricTiles;

        // Unused legacy field
        [SerializeField]
        private List<TesseraTileBase> symmetricTilesX = new List<TesseraTileBase>();

        // Unused legacy field
        [SerializeField]
        private List<TesseraTileBase> symmetricTilesY = new List<TesseraTileBase>();

        // Unused legacy field
        [SerializeField]
        private List<TesseraTileBase> symmetricTilesZ = new List<TesseraTileBase>();

        public Axis axis;

        public enum Axis
        {
            X,
            Y,
            Z,
            W,
        }

        internal override IEnumerable<ITileConstraint> GetTileConstraint(TileModelInfo tileModelInfo, IGrid grid)
        {
            var generator = GetComponent<TesseraGenerator>();
            if (generator.surfaceMesh != null)
            {
                throw new Exception("Mirror constraint not supported on surface meshes");
            }

            var cellType = generator.CellType;
            var mirrorOps = MirrorUtils.GetMirrorOps(cellType);
            var modelTiles = new HashSet<ModelTile>(tileModelInfo.AllTiles.Select(x => (ModelTile)x.Item1.Value));

            if (cellType is CubeCellType)
            {
                var axisDir = axis == Axis.X ? CubeFaceDir.Right : axis == Axis.Y ? CubeFaceDir.Up : CubeFaceDir.Forward;

                yield return new InnerMirrorConstraint
                {
                    mirrorOps = mirrorOps,
                    axis = (CellFaceDir)axisDir,
                    cellType = cellType,
                    canonicalization = tileModelInfo.Canonicalization,
                };

            }
            else if (cellType is SquareCellType)
            {
                var axisDir = axis == Axis.X ? SquareFaceDir.Right : SquareFaceDir.Up;

                yield return new InnerMirrorConstraint
                {
                    mirrorOps = mirrorOps,
                    axis = (CellFaceDir)axisDir,
                    cellType = cellType,
                    canonicalization = tileModelInfo.Canonicalization,
                };

            }
            else if (cellType is HexPrismCellType)
            {
                var axisDir = axis == Axis.X ? HexPrismFaceDir.Right : axis == Axis.Y ? HexPrismFaceDir.Up : axis == Axis.Z ? HexPrismFaceDir.ForwardLeft : HexPrismFaceDir.ForwardRight;

                yield return new InnerMirrorConstraint
                {
                    grid = grid,
                    mirrorOps = mirrorOps,
                    axis = (CellFaceDir)axisDir,
                    cellType = cellType,
                    canonicalization = tileModelInfo.Canonicalization,
                };
            }
            else if (cellType is TrianglePrismCellType)
            {
                // TODO
                var axisDir = TrianglePrismFaceDir.Up;
                yield return new InnerMirrorConstraint
                {
                    grid = grid,
                    mirrorOps = mirrorOps,
                    axis = (CellFaceDir)axisDir,
                    cellType = cellType,
                    canonicalization = tileModelInfo.Canonicalization,
                };
            }
            else
            {
                throw new Exception();
            }
        }

        private class InnerMirrorConstraint : SymmetryConstraint
        {
            public IGrid grid;
            public MirrorUtils.IMirrorOps mirrorOps;
            public CellFaceDir axis;
            public ICellType cellType;
            public Dictionary<Tile, Tile> canonicalization;

            protected override bool TryMapIndex(TilePropagator propagator, int i, out int i2)
            {
                return mirrorOps.ReflectIndex(axis, grid, propagator.Topology, i, out i2);
            }

            protected override bool TryMapTile(Tile tile, out Tile tile2)
            {
                var rotation = mirrorOps.GetRotation(axis);

                var modelTile = (ModelTile)tile.Value;

                var newRotation = cellType.Multiply(rotation, modelTile.Rotation);
                var modelTile2 = new Tile(new ModelTile
                {
                    Tile = modelTile.Tile,
                    Rotation = newRotation,
                    Offset = modelTile.Offset,
                });
                return canonicalization.TryGetValue(modelTile2, out tile2);
            }
        }
    }
}
