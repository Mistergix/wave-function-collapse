using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using ESGI.WFC.Exporter;
using Random = UnityEngine.Random;

namespace ESGI.WFC
{
    public class WaveFunctionCollapse : MonoBehaviour
    {
        public List<Module> modules;
        public Module startModule;
        public Module goalModule;
        [Tooltip("If set to -1 a random seed will be selected for every level generation.")]
        public int seed;

        public List<Socket> sockets;

        [SerializeField] private float offsetMin = 2;
        [SerializeField] private float offsetMax = 4;
        [SerializeField] private float moveDurationMin = 0.25f;
        [SerializeField] private float moveDurationMax = 0.75f;

        private Heap<Cell> OrderedCells { get; set; }

        private void Awake()
        {
            _gan = FindObjectOfType<GANInterface>();
        }

        private void Start()
        {
            Generate();
        }

        //[Button, DisableInEditorMode]
        public void Generate()
        {
            RemoveGrid();
            GenerateGrid(this);
            
            var finalSeed = seed != -1 ? seed : Environment.TickCount;
            Random.InitState(finalSeed);
            
            CreateHeap();
            
            
            //ApplyInitialConstraints();
            
            WaveFunctionCollapseRun();
            
            
            InstantiateGameObjects();
        }

        private void InstantiateGameObjects()
        {
            foreach (var cell in cells)
            {
                var t = cell.transform;
                var offset = Vector3.down * Random.Range(offsetMin, offsetMax);
                cell.meshFilter.sharedMesh = cell.Mesh;
                cell.meshFilter.transform.position = t.position + offset;
                //var go = Instantiate(cell.MainModule.modulePrefab, t.position + offset, Quaternion.identity, t);
                cell.meshFilter.transform.DOMove(t.position, Random.Range(moveDurationMin, moveDurationMax)).SetEase(Ease.OutBack);
            }
        }

        private void WaveFunctionCollapseRun()
        {
            while (OrderedCells.Count > 0)
            {
                var cell = OrderedCells.GetFirst();
                if (cell.HasMesh)
                {
                    cell.IsFinal = true;
                    OrderedCells.RemoveFirst();
                }
                else
                {
                    //cell.SetModule(cell.PossibleModules[Random.Range(0, cell.PossibleModules.Count)]);
                    var neighbours = GetNeighbours(cell);
                    var module = ScriptableObject.CreateInstance<Module>();
                    module.sockets = new Neighbours<Socket>()
                    {
                        bottom = neighbours.bottom, top = neighbours.top, left = neighbours.left,
                        right = neighbours.right
                    };

                    cell.MainModule = module;
                    // TODO should be done by batch, not one by one
                    cell.Mesh = _gan.GenerateNewMesh(module);
                }
            }
        }

        private SocketNeighbours GetNeighbours(Cell cell)
        {
            var neighbour = new SocketNeighbours();
            
            if (cell.Neighbours.bottom && cell.Neighbours.bottom.HasMesh)
            {
                neighbour.bottom = cell.Neighbours.bottom.MainModule.sockets.top;
            }
            
            if (cell.Neighbours.top && cell.Neighbours.top.HasMesh)
            {
                neighbour.top = cell.Neighbours.top.MainModule.sockets.bottom;
            }
            
            if (cell.Neighbours.left && cell.Neighbours.left.HasMesh)
            {
                neighbour.left = cell.Neighbours.left.MainModule.sockets.right;
            }
            
            if (cell.Neighbours.right && cell.Neighbours.right.HasMesh)
            {
                neighbour.right = cell.Neighbours.right.MainModule.sockets.left;
            }

            if (neighbour.bottom == null)
            {
                neighbour.bottom = sockets[Random.Range(0, sockets.Count)];
            }
            
            if (neighbour.top == null)
            {
                neighbour.top = sockets[Random.Range(0, sockets.Count)];
            }
            
            if (neighbour.left == null)
            {
                neighbour.left = sockets[Random.Range(0, sockets.Count)];
            }
            
            if (neighbour.right == null)
            {
                neighbour.right = sockets[Random.Range(0, sockets.Count)];
            }

            return neighbour;
        }

        /// <summary>
        /// Resolve all initial constraints.
        /// </summary>
        private void ApplyInitialConstraints()
        {
            //PlaceStartAndGoalConstraint();
            //BorderOnlyOnOutsideConstraint();
        }

        private void BorderOnlyOnOutsideConstraint()
        {
            var blockSocket = ScriptableObject.CreateInstance<SocketBlock>();
            var bottomFilter = new EdgeFilter(EdgeFilter.Directions.Top, blockSocket, true);
            var topFilter = new EdgeFilter(EdgeFilter.Directions.Bottom , blockSocket, true);
            var leftFilter = new EdgeFilter( EdgeFilter.Directions.Right , blockSocket, true);
            var rightFilter = new EdgeFilter(EdgeFilter.Directions.Left, blockSocket, true);

            // filter bottom and top cells for only border
            for (var i = 0; i < 2; i++)
            {
                var z = i * (height - 1);
                var filter = i == 0 ? bottomFilter : topFilter;

                for (var x = 0; x < width; x++)
                {
                    cells[x, z].FilterCell(filter, EdgeFilter.MatchType);
                }
            }

            // filter left and right cells for only border
            for (var i = 0; i < 2; i++)
            {
                var x = i * (width - 1);

                var filter = i == 0 ? leftFilter : rightFilter;

                for (var z = 0; z < height; z++)
                {
                    cells[x, z].FilterCell(filter, EdgeFilter.MatchType);
                }
            }
        }

        private void PlaceStartAndGoalConstraint()
        {
            var startCell = cells[Random.Range(0, width), Random.Range(0, height)];
            startCell.SetModule(startModule);
            
            Cell goalCell;
            do
            {
                goalCell = cells[Random.Range(0, width), Random.Range(0, height)];
            } while (goalCell == startCell);
            goalCell.SetModule(goalModule);
        }

        private void CreateHeap()
        {
            OrderedCells = new Heap<Cell>(width * height);
            for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
            {
                OrderedCells.Add(cells[i, j]);
            }
        }


        public void UpdateCell(Cell cell)
        {
            OrderedCells.UpdateItem(cell);
        }
        
        public int Compare(Cell cell, Cell other)
        {
            var compare = cell.PossibleModules.Count.CompareTo(other.PossibleModules.Count);
            if (compare == 0)
            {
                // Cells have the same entropy
                var r = Random.Range(1, 3);
                return r == 1 ? -1 : 1;
            }

            return -compare;
        }
        
        [Min(2)] public int width = 2;
        [Min(2)] public int height = 2;
        
        public Cell cellPrefab;

        private Cell[,] cells;
        private GANInterface _gan;

        private void GenerateGrid(WaveFunctionCollapse wfc)
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

        private void RemoveGrid()
        {
            foreach (Transform child in gameObject.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    [Serializable]
    public class SocketNeighbours : Neighbours<Socket> { }
}
