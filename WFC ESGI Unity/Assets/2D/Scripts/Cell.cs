using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ESGI.WFC
{
    public class Cell : MonoBehaviour, IHeapItem<Cell>
    {
        public CellNeighbours Neighbours { get; private set; }
        
        public WaveFunctionCollapse WaveFunctionCollapse { get; set; }
        public bool IsFinal { get; set; }
        public List<Module> PossibleModules { get; private set; }
        public int HeapIndex { get; set; }
        public Module MainModule => PossibleModules[0];
        
        private void Awake()
        {
            PossibleModules = new List<Module>();
            Neighbours = new CellNeighbours();
        }
        
        public void PopulateCell()
        {
            // at the beginning every module is possible
            foreach (var module in WaveFunctionCollapse.modules)
            {
                PossibleModules.Add(module);
            }
        }

        public void SetModule(Module module)
        {
            PossibleModules = new List<Module>() {module};
            
            WaveFunctionCollapse.UpdateCell(this);
            
            CheckNeighbourCells(module);

            PropagateSettingToNeighbours(module);

            IsFinal = true;
        }

        public void FilterCell(EdgeFilter filter, EdgeFilter.CheckModuleMatchFunction matchFunction)
        {
            if(PossibleModules.Count == 1){return;}

            var toRemove = PossibleModules.Where(module => filter.CheckModule(module, matchFunction)).ToList();
            foreach (var module in toRemove)
            {
                RemoveModule(module);
            }
        }
        
        private void FilterCell(EdgeFilter filter)
        {
            if(PossibleModules.Count == 1){return;}

            var toRemove = PossibleModules.Where(module => filter.CheckModule(module, filter.MatchEquality)).ToList();
            foreach (var module in toRemove)
            {
                RemoveModule(module);
            }
        }

        public int CompareTo(Cell other)
        {
            return WaveFunctionCollapse.Compare(this, other);
        }

        private void RemoveModule(Module module)
        {
            PossibleModules.Remove(module);
            WaveFunctionCollapse.UpdateCell(this);
            
            PropagateRemovalToNeighbours(module);
        }
        
        private void PropagateSettingToNeighbours(Module module)
        {
            for (var i = 0; i < Neighbours.Length; i++)
            {
                if (!HasNeighbour(Neighbours[i]))
                {
                    continue;
                }

                var edgeFilter = new EdgeFilter(i, module.sockets[i], true);
                Neighbours[i].FilterCell(edgeFilter);
            }
        }

        private void CheckNeighbourCells(Module module)
        {
            for (var i = 0; i < Neighbours.Length; i++)
            {
                if (Neighbours[i] == null || !Neighbours[i].IsFinal)
                {
                    continue;
                }

                if (module.sockets[i] != Neighbours[i].MainModule.sockets.GetNeighbour(i))
                {
                    Debug.LogError(
                        $"Setting module {module} would not fit already set neighbour {Neighbours[i].gameObject}!",
                        gameObject);
                }
            }
        }

        private void PropagateRemovalToNeighbours(Module module)
        {
            for (var j = 0; j < Neighbours.Length; j++)
            {
                if (!HasNeighbour(Neighbours[j]))
                {
                    continue;
                }

                var edgeType = module.sockets[j];
                var lastWithEdgeType = PossibleModules.All(mod => mod.sockets[j] != edgeType);

                if (!lastWithEdgeType) {continue;}
                var edgeFilter = new EdgeFilter(j, edgeType, false);
                Neighbours[j].FilterCell(edgeFilter);
            }
        }

        private static bool HasNeighbour(Object neighbour)
        {
            return neighbour != null;
        }

        [Serializable]
        public class CellNeighbours : Neighbours<Cell> { }
    }
}