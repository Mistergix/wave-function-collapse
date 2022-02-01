using UnityEngine;

namespace ESGI.WFC
{
    public abstract class GridGenerator : MonoBehaviour
    {
        [Min(2)] public int width = 2;
        [Min(2)] public int height = 2;
        
        public Cell cellPrefab;
        
        protected Cell[,] cells;

        protected void GenerateGrid(WaveFunctionCollapse wfc)
        {
            if (width <= 0 || height <= 0)
            {
                Debug.LogError("Impossible grid dimensions!", gameObject);
                return;
            }

            cells = new Cell[width, height];

            var scale = cellPrefab.transform.localScale;
            var origin = transform.position;
            var bottomLeft = new Vector3(
                origin.x - width * scale.x / 2f + scale.x / 2f,
                origin.y,
                origin.z - height * scale.z / 2f + scale.z / 2
            );

            for (var x = 0; x < width; x++)
            {
                for (var z = 0; z < height; z++)
                {
                    var curPos = new Vector3(bottomLeft.x + x * scale.x, bottomLeft.y, bottomLeft.z + z * scale.z);
                    var cell = Instantiate(cellPrefab, curPos, Quaternion.identity, gameObject.transform);
                    cell.name = $"Cell {x}, {z}";
                    cell.WaveFunctionCollapse = wfc;
                    cell.PopulateCell();
                    cells[x, z] = cell;

                    AssignNeighbours(x, z, cell);
                }
            }
        }

        private void AssignNeighbours(int x, int z, Cell cell)
        {
            if (x > 0)
            {
                var leftCell = cells[x - 1, z];
                cell.Neighbours.left = leftCell;
                leftCell.Neighbours.right = cell;
            }

            if (z > 0)
            {
                var bottomCell = cells[x, z - 1];
                cell.Neighbours.bottom = bottomCell;
                bottomCell.Neighbours.top = cell;
            }
        }

        protected void RemoveGrid()
        {
            foreach (Transform child in gameObject.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}