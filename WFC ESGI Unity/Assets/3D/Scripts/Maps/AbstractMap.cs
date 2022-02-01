using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ESGI.WFC.ThreeDimensions
{
    public abstract class AbstractMap
    {
        public readonly Queue<Slot> BuildQueue;
        public readonly QueueDictionary<Vector3Int, ModuleSet> RemovalQueue;
        public AbstractMap()
        {
            BuildQueue = new Queue<Slot>();
            RemovalQueue = new QueueDictionary<Vector3Int, ModuleSet>(() => new ModuleSet());
        }
        
        public abstract void ApplyBoundaryConstraints(IEnumerable<BoundaryConstraint> constraints);
        
        public void Collapse(Vector3Int start, Vector3Int size) {
            var targets = new List<Vector3Int>();
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    for (int z = 0; z < size.z; z++) {
                        targets.Add(start + new Vector3Int(x, y, z));
                    }
                }
            }
            this.Collapse(targets);
        }

        private void Collapse(IEnumerable<Vector3Int> targets)
        {
            try
            {
                RemovalQueue.Clear();
                //workArea = new HashSet<Slot>(targets.Select(target => GetSlot(target)).Where(slot => slot != null && !slot.Collapsed));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private Slot GetSlot<TResult>(Vector3Int target)
        {
            throw new NotImplementedException();
        }
    }
}