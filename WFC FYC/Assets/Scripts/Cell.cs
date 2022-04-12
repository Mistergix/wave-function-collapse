using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Cell : MonoBehaviour, IHeapItem<Cell>
{
    [Serializable] public class CellNeighbours : Neighbours<Cell> { }
    
    public WaveFunctionCollapse WFC { get; set; }
    public List<Module> PossibleModules { get; set; }
    public CellNeighbours Neighbours { get; set; }

    private void Awake()
    {
        PossibleModules = new List<Module>();
        Neighbours = new CellNeighbours();
    }

    public void PopulateCell()
    {
        foreach (var module in WFC.modules)
        {
            PossibleModules.Add(module);
        }
    }

    public int CompareTo(Cell other)
    {
        return WFC.CompareCells(this, other);
    }

    public int HeapIndex { get; set; }
    public bool IsFinal { get; set; }

    public void FilterCell(EdgeFilter filter, EdgeFilter.CheckModuleMatchFunction matchFunction)
    {
        if (PossibleModules.Count == 1)
        {
            // only one module left, can"t filter the cell
            return;
        }

        var toRemove = PossibleModules.Where(module => filter.CheckModule(module, matchFunction)).ToList();
        foreach (var module in toRemove)
        {
            RemoveModule(module);
        }
    }
    
    public void FilterCell(EdgeFilter filter)
    {
        if (PossibleModules.Count == 1)
        {
            // only one module left, can"t filter the cell
            return;
        }

        var toRemove = PossibleModules.Where(module => filter.CheckModule(module, filter.MatchEquality)).ToList();
        foreach (var module in toRemove)
        {
            RemoveModule(module);
        }
    }

    private void RemoveModule(Module module)
    {
        PossibleModules.Remove(module);
        WFC.UpdateCell(this);
        PropagateRemovalToNeighbours(module);
    }

    private void PropagateRemovalToNeighbours(Module module)
    {
        for (int i = 0; i < Neighbours.Length; i++)
        {
            if (Neighbours[i] == null)
            {
                continue;
            }

            var edgeType = module.sockets[i];
            var lastWithEdgeType = PossibleModules.All(mod => mod.sockets[i] != edgeType);
            
            if(!lastWithEdgeType) {continue;}

            var edgeFilter = new EdgeFilter(i, edgeType, false);
            Neighbours[i].FilterCell(edgeFilter);
        }
    }

    public void SetRandomModule()
    {
        SetModule(PossibleModules[Random.Range(0, PossibleModules.Count)]);
    }

    private void SetModule(Module module)
    {
        PossibleModules = new List<Module> {module};
        WFC.UpdateCell(this);
        CheckNeighbourCells(module);
        PropagateSettingsToNeighbours(module);
        IsFinal = true;
    }

    private void PropagateSettingsToNeighbours(Module module)
    {
        for (int i = 0; i < Neighbours.Length; i++)
        {
            if(Neighbours[i] == null){continue;}

            var edgeFilter = new EdgeFilter(i, module.sockets[i], true);
            Neighbours[i].FilterCell(edgeFilter);
        }
    }

    private void CheckNeighbourCells(Module module)
    {
        for (int i = 0; i < Neighbours.Length; i++)
        {
            if (Neighbours[i] == null || !Neighbours[i].IsFinal)
            {
                continue;
            }
            
            // neighbour is final, check if there is a conflict
            if (module.sockets[i] != Neighbours[i].MainModule.sockets.GetNeighbour(i))
            {
                throw new Exception($"Setting module {module} would not fit already set neighbour !");
            }
        }
    }

    public Module MainModule => PossibleModules[0];
}
