using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LevelGeneration
{
    /// <summary>
    /// A cell inside the level`s grid.
    /// </summary>
    public class Cell : MonoBehaviour, IHeapItem<Cell>
    {
        /// <summary>
        /// Has the decision of the final module already been propagated.
        ///
        /// NOTE:
        /// This is in fact different then just having <code>Count</code> of <see cref="possibleModules"/> 1!
        ///
        /// For example a cell's possibility space can be given only one module from the start.
        /// In this case <code>Count</code> of <see cref="possibleModules"/> is already 1 but the decision to chose
        /// this only model`s object as object for the cell has not been propagated yet.
        /// </summary>
        public bool isFinal;

        /// <summary>
        /// The possible <see cref="Module"/> objects. (Possibility space)
        /// </summary>
        public List<Module> possibleModules;

        /// <summary>
        /// The adjacent <see cref="Cell"/> objects inside the grid.
        /// Element can be null if the cell is on the grid`s edge.
        /// 
        /// [bottom, right, top, left]
        /// </summary>
        public Cell[] neighbours = new Cell[4];

        public LevelGenerator levelGenerator;

        public int HeapIndex { get; set; }

        private void Awake()
        {
            possibleModules = new List<Module>();
        }

        public void PopulateCell()
        {
            // at the beginning every module is possible
            foreach (var module in levelGenerator.modules)
            {
                possibleModules.Add(module);
            }
        }

        /// <summary>
        /// Applies an <see cref="EdgeFilter"/> to this cell.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        public void FilterCell(EdgeFilter filter)
        {
            if (possibleModules.Count == 1) return;

            var removingModules = possibleModules.Where(filter.CheckModule).ToList();

            // filter possible modules list

            // remove filtered modules
            foreach (var module in removingModules)
            {
                RemoveModule(module);
            }
        }

        /// <summary>
        /// Removes a <see cref="Module"/> from <see cref="possibleModules"/>
        /// checking if it was the last one on any edge of a specific edge type and
        /// if so propagating the changes to the affected neighbour.
        /// </summary>
        /// <param name="module">The <see cref="Module"/> to remove.</param>
        public void RemoveModule(Module module)
        {
            // remove module from possibility space
            possibleModules.Remove(module);

            // update item on the heap
            levelGenerator.orderedCells.UpdateItem(this);

            for (var j = 0; j < neighbours.Length; j++)
            {
                // only check if cell has a neighbour on this edge
                if (neighbours[j] == null) continue;

                var edgeType = module.edgeConnections[j];
                var lastWithEdgeType = true;

                // search in other possible modules for the same edge type
                foreach (var t in possibleModules)
                {
                    if (t.edgeConnections[j] == edgeType)
                    {
                        lastWithEdgeType = false;
                        break;
                    }
                }

                if (lastWithEdgeType)
                {
                    // populate edge changes to neighbour cell
                    var edgeFilter = new EdgeFilter(j, edgeType, false);
                    neighbours[j].FilterCell(edgeFilter);
                }
            }
        }

        /// <summary>
        /// Assigns this cell a specific <see cref="Module"/> removing others.
        /// </summary>
        /// <param name="module">The <see cref="Module"/>.</param>
        public void SetModule(Module module)
        {
            possibleModules = new List<Module> {module};

            // update item on the heap
            levelGenerator.orderedCells.UpdateItem(this);

            // check if it fits to already set neighbour cells
            for (var i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] == null || !neighbours[i].isFinal) continue;

                if (module.edgeConnections[i] != neighbours[i].possibleModules[0].edgeConnections[(i + 2) % 4])
                    Debug.LogError(
                        $"Setting module {module} would not fit already set neighbour {neighbours[i].gameObject}!",
                        gameObject);
            }

            // propagate changes to neighbours
            for (var i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] == null) continue;

                // populate edge changes to neighbour cell
                neighbours[i].FilterCell(new EdgeFilter(i, module.edgeConnections[i], true));
            }

            isFinal = true;
        }

        /// <summary>
        /// Compares two cells using their solved score.
        /// TODO: Refactor. Is the extra randomness necessary?
        /// </summary>
        /// <param name="other">Cell to compare.</param>
        /// <returns>Comparison value.</returns>
        public int CompareTo(Cell other)
        {
            var compare = possibleModules.Count.CompareTo(other.possibleModules.Count);
            if (compare == 0)
            {
                var r = Random.Range(1, 3);
                return r == 1 ? -1 : 1;
            }

            return -compare;
        }
    }
}