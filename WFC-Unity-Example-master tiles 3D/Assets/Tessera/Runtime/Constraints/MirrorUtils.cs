using DeBroglie.Topo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tessera
{
    internal static class MirrorUtils
    {
        // TODO: Don't use CellFaceDir as the axis enum
        public interface IMirrorOps
        {
            // Does the given direction represent a reflectable axis
            bool CanReflect(CellFaceDir axis);

            // Gets the CellRotation corresponding to this reflection
            // We don't support grids where this isn't a constant.
            CellRotation GetRotation(CellFaceDir axis);

            // Reflect a grid cell
            // This uses the grid bounds, it doesn't reflect over a fixed point.
            bool ReflectIndex(CellFaceDir axis, IGrid grid, ITopology topology, int i, out int i2);

            // Do two faces exactly equal each other, except reflection?
            // Probably redundant, given TesseraPalette.Match works for all cells types.
            bool ReflectedEquals(CellFaceDir axis, FaceDetails a, FaceDetails b);
        }

        private class CubeMirrorOps : IMirrorOps
        {
            public bool CanReflect(CellFaceDir axis)
            {
                return true;
            }


            public CellRotation GetRotation(CellFaceDir axis)
            {
                switch ((CubeFaceDir)axis)
                {
                    case CubeFaceDir.Left:
                    case CubeFaceDir.Right:
                        return CubeRotation.ReflectX;
                    case CubeFaceDir.Up:
                    case CubeFaceDir.Down:
                        return CubeRotation.ReflectY;
                    case CubeFaceDir.Forward:
                    case CubeFaceDir.Back:
                        return CubeRotation.ReflectZ;
                    default:
                        throw new Exception();
                }
            }

            public bool ReflectedEquals(CellFaceDir faceDir, FaceDetails a, FaceDetails b) => SquareReflectedEquals(a, b);

            public bool ReflectIndex(CellFaceDir axis, IGrid grid, ITopology topology, int i, out int i2)
            {
                topology.GetCoord(i, out var x, out var y, out var z);
                switch ((CubeFaceDir)axis)
                {
                    case CubeFaceDir.Left:
                    case CubeFaceDir.Right:
                        x = topology.Width - 1 - x;
                        break;
                    case CubeFaceDir.Up:
                    case CubeFaceDir.Down:
                        y = topology.Height - 1 - y;
                        break;

                    case CubeFaceDir.Forward:
                    case CubeFaceDir.Back:
                        z = topology.Depth - 1 - z;
                        break;
                }
                i2 = topology.GetIndex(x, y, z);
                return topology.ContainsIndex(i2);
            }

            public Vector3Int ReflectOffset(CellFaceDir axis, TesseraTileBase tile, Vector3Int offset)
            {
                var bounds = ((TesseraTile)tile).GetBounds();
                switch ((CubeFaceDir)axis)
                {
                    case CubeFaceDir.Left:
                    case CubeFaceDir.Right:
                        return new Vector3Int(bounds.xMin + bounds.xMax - offset.x, offset.y, offset.z);
                    case CubeFaceDir.Up:
                    case CubeFaceDir.Down:
                        return new Vector3Int(offset.x, bounds.yMin + bounds.yMax - offset.y, offset.z);
                    case CubeFaceDir.Forward:
                    case CubeFaceDir.Back:
                        return new Vector3Int(offset.x, offset.y, bounds.zMin + bounds.zMax - offset.z);
                    default:
                        throw new Exception();
                }
            }
        }

        private class SquareMirrorOps : IMirrorOps
        {
            public bool CanReflect(CellFaceDir axis)
            {
                return true;
            }


            public CellRotation GetRotation(CellFaceDir axis)
            {
                switch ((SquareFaceDir)axis)
                {
                    case SquareFaceDir.Left:
                    case SquareFaceDir.Right:
                        return SquareRotation.ReflectX;
                    case SquareFaceDir.Up:
                    case SquareFaceDir.Down:
                        return SquareRotation.ReflectY;
                    default:
                        throw new Exception();
                }
            }

            public bool ReflectedEquals(CellFaceDir faceDir, FaceDetails a, FaceDetails b) => SquareReflectedEquals(a, b);

            public bool ReflectIndex(CellFaceDir axis, IGrid grid, ITopology topology, int i, out int i2)
            {
                topology.GetCoord(i, out var x, out var y, out var z);
                switch ((SquareFaceDir)axis)
                {
                    case SquareFaceDir.Left:
                    case SquareFaceDir.Right:
                        x = topology.Width - 1 - x;
                        break;
                    case SquareFaceDir.Up:
                    case SquareFaceDir.Down:
                        y = topology.Height - 1 - y;
                        break;
                }
                i2 = topology.GetIndex(x, y, z);
                return topology.ContainsIndex(i2);
            }

            public Vector3Int ReflectOffset(CellFaceDir axis, TesseraTileBase tile, Vector3Int offset)
            {
                var bounds = ((TesseraSquareTile)tile).GetBounds();
                switch ((SquareFaceDir)axis)
                {
                    case SquareFaceDir.Left:
                    case SquareFaceDir.Right:
                        return new Vector3Int(bounds.xMin + bounds.xMax - offset.x, offset.y, offset.z);
                    case SquareFaceDir.Up:
                    case SquareFaceDir.Down:
                        return new Vector3Int(offset.x, bounds.yMin + bounds.yMax - offset.y, offset.z);
                    default:
                        throw new Exception();
                }
            }
        }

        // Always reflects in x axis for now
        private class TrianglePrismMirrorOps : IMirrorOps
        {
            public bool CanReflect(CellFaceDir axis)
            {
                throw new NotImplementedException("Mirror constraint doesn't work for Triangle Prism cell type");
            }

            public CellRotation GetRotation(CellFaceDir axis)
            {
                return TriangleRotation.ReflectX;
            }

            public bool ReflectedEquals(CellFaceDir faceDir, FaceDetails a, FaceDetails b) => ((HexPrismFaceDir)faceDir).IsUpDown() ? TriReflectedEquals(a, b) : SquareReflectedEquals(a, b);

            public bool ReflectIndex(CellFaceDir axis, IGrid grid, ITopology topology, int i, out int i2)
            {
                topology.GetCoord(i, out var x, out var y, out var z);

                var q = x + z;
                var min = 0;
                var max = (topology.Width - 1) + (topology.Depth - 1);
                var q2 = min + max - q;
                x += Mathf.FloorToInt((q2 - q));

                if (x < 0 || x >= topology.Width || z < 0 || z >= topology.Depth)
                {
                    i2 = default;
                    return false;
                }
                i2 = topology.GetIndex(x, y, z);
                return topology.ContainsIndex(i2);
            }

            public Vector3Int ReflectOffset(CellFaceDir axis, TesseraTileBase tile, Vector3Int offset)
            {
                var bounds = ((TesseraTrianglePrismTile)tile).GetBounds();
                var x = offset.x;
                var y = offset.y;
                var z = offset.z;

                var q = x + z;
                var min = bounds.min.x + bounds.min.z;
                var max = bounds.max.x + bounds.max.z;
                var q2 = min + max - q;
                x += Mathf.FloorToInt((q2 - q));

                return new Vector3Int(x, y, z);
            }
        }

        private class HexPrismMirrorOps : IMirrorOps
        {
            public bool CanReflect(CellFaceDir axis)
            {
                return !((HexPrismFaceDir)axis).IsUpDown();
            }

            public CellRotation GetRotation(CellFaceDir axis)
            {
                switch ((HexPrismFaceDir)axis)
                {
                    case HexPrismFaceDir.Left:
                    case HexPrismFaceDir.Right:
                        return HexRotation.ReflectX;
                    case HexPrismFaceDir.Up:
                    case HexPrismFaceDir.Down:
                        throw new Exception("HexPrisms cannot be mirrored in vertical axis");
                    case HexPrismFaceDir.ForwardLeft:
                    case HexPrismFaceDir.BackRight:
                        return HexRotation.ReflectForwardLeft;
                    case HexPrismFaceDir.BackLeft:
                    case HexPrismFaceDir.ForwardRight:
                        return HexRotation.ReflectForwardRight;
                    default:
                        throw new Exception();
                }
            }

            public bool ReflectedEquals(CellFaceDir faceDir, FaceDetails a, FaceDetails b) => ((HexPrismFaceDir)faceDir).IsUpDown() ? HexReflectedEquals(a, b) : SquareReflectedEquals(a, b);

            public bool ReflectIndex(CellFaceDir axis, IGrid grid, ITopology topology, int i, out int i2)
            {
                topology.GetCoord(i, out var x, out var y, out var z);
                switch ((HexPrismFaceDir)axis)
                {
                    case HexPrismFaceDir.Left:
                    case HexPrismFaceDir.Right:
                        {
                            var q = x * 2 - z;
                            var min = 0 - topology.Depth - 1;
                            var max = (topology.Width - 1) * 2;
                            var q2 = min + max - q;
                            x += Mathf.FloorToInt((q2 - q) / 2f);
                            break;
                        }
                    case HexPrismFaceDir.Up:
                    case HexPrismFaceDir.Down:
                        throw new Exception("HexPrisms cannot be mirrored in vertical axis");
                    case HexPrismFaceDir.ForwardLeft:
                    case HexPrismFaceDir.BackRight:
                        {
                            var q = 2 * z - x;
                            var min = 0 - (topology.Width - 1);
                            var max = 2 * (topology.Depth - 1) - 0;
                            var q2 = min + max - q;
                            z += Mathf.FloorToInt((q2 - q) / 2f);
                            break;
                        }
                    case HexPrismFaceDir.BackLeft:
                    case HexPrismFaceDir.ForwardRight:
                        {
                            var q = z + x;
                            var min = 0;
                            var max = (topology.Depth - 1) + (topology.Width - 1);
                            var q2 = min + max - q;
                            x += Mathf.FloorToInt((q2 - q) / 2f);
                            z += Mathf.FloorToInt((q2 - q) / 2f);
                            break;
                        }
                }
                if (x < 0 || x >= topology.Width || z < 0 || z >= topology.Depth)
                {
                    i2 = default;
                    return false;
                }
                i2 = topology.GetIndex(x, y, z);
                return topology.ContainsIndex(i2);
            }

            public Vector3Int ReflectOffset(CellFaceDir axis, TesseraTileBase tile, Vector3Int offset)
            {
                var bounds = ((TesseraHexTile)tile).GetBounds();
                var x = offset.x;
                var y = offset.y;
                var z = offset.z;
                switch ((HexPrismFaceDir)axis)
                {
                    case HexPrismFaceDir.Left:
                    case HexPrismFaceDir.Right:
                        {
                            var q = x * 2 - z;
                            var min = 2 * bounds.min.x - bounds.max.z;
                            var max = 2 * bounds.max.x - bounds.min.z;
                            var q2 = min + max - q;
                            x += Mathf.FloorToInt((q2 - q) / 2f);
                            break;
                        }
                    case HexPrismFaceDir.Up:
                    case HexPrismFaceDir.Down:
                        throw new Exception("HexPrisms cannot be mirrored in vertical axis");
                    case HexPrismFaceDir.ForwardLeft:
                    case HexPrismFaceDir.BackRight:
                        {
                            var q = 2 * z - x;
                            var min = 2 * bounds.max.z - bounds.min.x;
                            var max = 2 * bounds.min.z - bounds.max.x;
                            var q2 = min + max - q;
                            z += Mathf.FloorToInt((q2 - q) / 2f);
                            break;
                        }
                    case HexPrismFaceDir.BackLeft:
                    case HexPrismFaceDir.ForwardRight:
                        {
                            var q = z + x;
                            var min = bounds.min.x + bounds.min.z;
                            var max = bounds.max.x + bounds.max.z;
                            var q2 = min + max - q;
                            x += Mathf.FloorToInt((q2 - q) / 2f);
                            z += Mathf.FloorToInt((q2 - q) / 2f);
                            break;
                        }
                }
                return new Vector3Int(x, y, z);
            }
        }


        public static IMirrorOps GetMirrorOps(ICellType cellType)
        {
            if(cellType is CubeCellType)
            {
                return new CubeMirrorOps();
            }
            else if (cellType is SquareCellType)
            {
                return new SquareMirrorOps();
            }
            else if (cellType is HexPrismCellType)
            {
                return new HexPrismMirrorOps();
            }
            else if (cellType is TrianglePrismCellType)
            {
                return new TrianglePrismMirrorOps();
            }
            else
            {
                throw new Exception($"No MirrorOps for {cellType.GetType()}");
            }
        }

        public static bool SquareReflectedEquals(FaceDetails a, FaceDetails b)
        {
            return (a.topLeft == b.topRight) &&
                (a.top == b.top) &&
                (a.topRight == b.topLeft) &&
                (a.left == b.right) &&
                (a.center == b.center) &&
                (a.right == b.left) &&
                (a.bottomLeft == b.bottomRight) &&
                (a.bottom == b.bottom) &&
                (a.bottomRight == b.bottomLeft);
        }

        public static bool HexReflectedEquals(FaceDetails a, FaceDetails b)
        {
            return
                (a.hexRight, a.hexLeft) == (b.hexLeft, b.hexRight) &&
                (a.hexRightAndTopRight, a.hexTopLeftAndLeft) == (b.hexTopLeftAndLeft, b.hexRightAndTopRight) &&
                (a.hexTopRight, a.hexTopLeft) == (b.hexTopLeft, b.hexTopRight) &&
                (a.hexLeftAndBottomLeft, a.hexBottomRightAndRight) == (b.hexBottomRightAndRight, b.hexLeftAndBottomLeft) &&
                (a.hexBottomLeft, a.hexBottomRight) == (b.hexBottomRight, b.hexBottomLeft) &&
                a.hexTopRightAndTopLeft == b.hexTopRightAndTopLeft &&
                a.hexBottomLeftAndBottomRight == b.hexBottomLeftAndBottomRight;
        }

        public static bool TriReflectedEquals(FaceDetails a, FaceDetails b)
        {
            return (a.topLeft == b.topRight) &&
                (a.top == b.top) &&
                (a.topRight == b.topLeft) &&
                (a.center == b.center) &&
                (a.bottomLeft == b.bottomRight) &&
                (a.bottom == b.bottom) &&
                (a.bottomRight == b.bottomLeft);
        }
    }
}
