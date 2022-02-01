using UnityEngine;

namespace ESGI.WFC.ThreeDimensions
{
    [CreateAssetMenu(menuName = "Wave Function Collapse/Module Data", fileName = "modules")]
    public class ModuleData : ScriptableObject, ISerializationCallbackReceiver
    {
        public static Module[] currentModules;
        public GameObject prototypes;
        public Module[] modules;
        
        public void OnBeforeSerialize()
        {
            throw new System.NotImplementedException();
        }

        public void OnAfterDeserialize()
        {
            throw new System.NotImplementedException();
        }
    }
}