using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using Sirenix.OdinInspector;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace ESGI.WFC
{
    public class WaveFunctionCollapse : GridGenerator
    {
        public List<Module> modules;
        public Module startModule;
        public Module goalModule;
        [Tooltip("If set to -1 a random seed will be selected for every level generation.")]
        public int seed;

        [SerializeField] private float offsetMin = 2;
        [SerializeField] private float offsetMax = 4;
        [SerializeField] private float moveDurationMin = 0.25f;
        [SerializeField] private float moveDurationMax = 0.75f;

        public Heap<Cell> OrderedCells { get; set; }

        private void Start()
        {
            Generate();
        }

        [Button, DisableInEditorMode]
        public void Generate()
        {
            RemoveGrid();
            GenerateGrid(this);
            
            var finalSeed = seed != -1 ? seed : Environment.TickCount;
            Random.InitState(finalSeed);
            
            CreateHeap();
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            ApplyInitialConstraints();
            WaveFunctionCollapseRun();
            
            stopwatch.Stop();
            Debug.Log(
                $"Wave-function-collapse algorithm finished in {stopwatch.Elapsed.TotalMilliseconds}ms (Seed: {finalSeed})");
            
            InstantiateGameObjects();
        }

        private void InstantiateGameObjects()
        {
            foreach (var cell in cells)
            {
                var t = cell.transform;
                var offset = Vector3.down * Random.Range(offsetMin, offsetMax);
                var go = Instantiate(cell.PossibleModules[0].modulePrefab, t.position + offset, Quaternion.identity, t);
                go.transform.DOMove(t.position, Random.Range(moveDurationMin, moveDurationMax)).SetEase(Ease.OutBack);
            }
        }

        private void WaveFunctionCollapseRun()
        {
            while (OrderedCells.Count > 0)
            {
                var cell = OrderedCells.GetFirst();
                if (cell.PossibleModules.Count == 1)
                {
                    cell.IsFinal = true;
                    OrderedCells.RemoveFirst();
                }
                else
                {
                    cell.SetModule(cell.PossibleModules[Random.Range(0, cell.PossibleModules.Count)]);
                }
            }
        }

        /// <summary>
        /// Resolve all initial constraints.
        /// </summary>
        private void ApplyInitialConstraints()
        {
            PlaceStartAndGoalConstraint();
            BorderOnlyOnOutsideConstraint();
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

                    cells[x, z].FilterCell(filter, filter.MatchType);
                }
            }

            // filter left and right cells for only border
            for (var i = 0; i < 2; i++)
            {
                var x = i * (width - 1);

                var filter = i == 0 ? leftFilter : rightFilter;

                for (var z = 0; z < height; z++)
                {
                    cells[x, z].FilterCell(filter, filter.MatchType);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="other"></param>
        /// <returns>The cell with the least entropy</returns>
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
    }
}
