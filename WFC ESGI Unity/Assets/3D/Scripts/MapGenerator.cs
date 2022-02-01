using System.Linq;
using UnityEngine;

namespace ESGI.WFC.ThreeDimensions
{
    public class MapGenerator : MonoBehaviour
    {
        public Vector3Int mapSize;
        public bool applyBoundaryConstraints = true;
        public BoundaryConstraint[] boundaryConstraints;
        public ModuleData moduleData;

        //private InfiniteMap map;

        public int MapHeight => mapSize.y;
        
        public void Generate()
        {
            Initialize();
            Collapse(Vector3Int.zero, mapSize);
            BuildSlots();
        }

        private void BuildSlots()
        {
           // while (map.BuildQueue.Count != 0) {
            //    BuildSlot(map.BuildQueue.Dequeue());
           // }
        }

        private void BuildSlot(Slot slot)
        {
            if (slot.gameObject != null)
            {
                Destroy(slot.gameObject);
            }
            
            var module = slot.module;
            if (module == null) { // Can be null due to race conditions
                return;
            }

            if (!slot.Collapsed || !module.prototype.spawn)
            {
                return;
            }

            var go = Instantiate(module.prototype.gameObject, transform, true);
            go.name = module.prototype.gameObject.name + " " + slot.position;
            DestroyImmediate(gameObject.GetComponent<ModulePrototype>());
            go.transform.position = GetWorldPosition(slot.position);
            go.transform.rotation =Quaternion.Euler(Vector3.up * 90f * module.rotation);
            slot.gameObject = go;
        }

        private Vector3 GetWorldPosition(Vector3Int slotPosition)
        {
            return transform.position
                   + Vector3.up * ModulePrototype.BlockSize / 2f
                   + slotPosition.ToVector3() * ModulePrototype.BlockSize;
        }

        private void Collapse(Vector3Int start, Vector3Int size)
        {
           // map.Collapse(start, size);
        }

        private void Initialize()
        {
            ModuleData.currentModules = moduleData.modules;
            Clear();
           // map = new InfiniteMap(MapHeight);
            if (applyBoundaryConstraints && boundaryConstraints != null && boundaryConstraints.Any())
            {
               // map.ApplyBoundaryConstraints(boundaryConstraints);
            }
        }

        private void Clear()
        {
            var children = transform.Cast<Transform>().ToList();
            foreach (var child in children)
            {
                DestroyImmediate(child.gameObject);
            }

            //map = null;
        }
    }
}