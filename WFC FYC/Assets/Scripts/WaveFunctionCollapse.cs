using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveFunctionCollapse : MonoBehaviour
{
    [Min(2)] public int width = 2;
    [Min(2)] public int height = 2;
    public Cell cellPrefab;
    public List<Module> modules;
    public int seed;
    
    public float offsetMin = 2;
    public float offsetMax = 4;
    public float moveDurationMin = 0.75f;
    public float moveDurationMax = 1.25f;

    private Cell[,] _cells;
    private Heap<Cell> OrderedCells { get; set; }

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        RemoveGrid();
        GenerateGrid();

        var finalSeed = seed < 0 ? Environment.TickCount : seed;
        Random.InitState(finalSeed);

        CreateHeap();
        ApplyInitialConstraints();

        RunWaveFunctionCollapse();

        InstantiateModules();
    }

    private void InstantiateModules()
    {
        foreach (var cell in _cells)
        {
            var t = cell.transform;
            var offset = Vector3.down * Random.Range(offsetMin, offsetMax);
            var go = Instantiate(cell.MainModule.modulePrefab, t.position + offset, Quaternion.identity, t);
            go.transform.DOMove(t.position, Random.Range(moveDurationMin, moveDurationMax)).SetEase(Ease.OutBack);
        }
    }

    private void RunWaveFunctionCollapse()
    {
        while (OrderedCells.Count > 0)
        {
            var cell = OrderedCells.GetFirst();
            if (cell.PossibleModules.Count == 1)
            {
                cell.IsFinal = true;
                OrderedCells.RemoveFirst();
                Debug.Log("CELL FINAL");
            }
            else
            {
                Debug.Log($"RANDOM MODULE, module count {cell.PossibleModules.Count}, {cell.name}");
                cell.SetRandomModule();
            }
        }
    }

    private void ApplyInitialConstraints()
    {
        BorderOnlyOnOutsideConstraint();
    }

    private void BorderOnlyOnOutsideConstraint()
    {
        var blockSocket = ScriptableObject.CreateInstance<SocketBlock>();
        var bottomFilter = new EdgeFilter(EdgeFilter.Directions.Top, blockSocket, true);
        var topFilter = new EdgeFilter(EdgeFilter.Directions.Bottom, blockSocket, true);
        var leftFilter = new EdgeFilter(EdgeFilter.Directions.Right, blockSocket, true);
        var rightFilter = new EdgeFilter(EdgeFilter.Directions.Left, blockSocket, true);

        for (int i = 0; i < 2; i++)
        {
            var z = i * (height - 1); // == 0 or == height - 1
            var filter = i == 0 ? bottomFilter : topFilter;
            for (int x = 0; x < width; x++)
            {
                _cells[x, z].FilterCell(filter, EdgeFilter.MatchType);
            }
        }
        
        for (int i = 0; i < 2; i++)
        {
            var x = i * (width - 1); // == 0 or == width - 1
            var filter = i == 0 ? leftFilter : rightFilter;
            for (int z = 0; z < height; z++)
            {
                _cells[x, z].FilterCell(filter, EdgeFilter.MatchType);
            }
        }
    }

    private void CreateHeap()
    {
        OrderedCells = new Heap<Cell>(width * height);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                OrderedCells.Add(_cells[i, j]);
            }
        }
    }

    private void GenerateGrid()
    {
        _cells = new Cell[width, height];

        var scale = cellPrefab.transform.localScale;
        var origin = transform.position;
        var bottomLeft = new Vector3(
            origin.x - width * scale.x / 2f + scale.x / 2f,
            origin.y,
            origin.z - height * scale.z / 2f + scale.z / 2f);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var pos = new Vector3(bottomLeft.x + i * scale.x, bottomLeft.y, bottomLeft.z + j * scale.z);
                var cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                cell.name = $"Cell {i}, {j}";
                cell.WFC = this;
                cell.PopulateCell();
                _cells[i, j] = cell;
                AssignNeighbours(i, j, cell);
            }
        }
    }

    private void AssignNeighbours(int i, int j, Cell cell)
    {
        if (i > 0)
        {
            var left = _cells[i - 1, j];
            cell.Neighbours.left = left;
            left.Neighbours.right = cell;
        }

        if (j > 0)
        {
            var bottom = _cells[i, j - 1];
            cell.Neighbours.bottom = bottom;
            bottom.Neighbours.top = cell;
        }
    }

    private void RemoveGrid()
    {
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public int CompareCells(Cell cell, Cell other)
    {
        if (cell.Equals(other))
        {
            return 0;
        }
        var compare = cell.PossibleModules.Count.CompareTo(other.PossibleModules.Count);
        if (compare == 0)
        {
            // same entropy
            var random = Random.Range(1, 3);
            return random == 0 ? -1 : 1;
        }

        return -compare;
    }

    public void UpdateCell(Cell cell)
    {
        OrderedCells.UpdateItem(cell);
    }
}
