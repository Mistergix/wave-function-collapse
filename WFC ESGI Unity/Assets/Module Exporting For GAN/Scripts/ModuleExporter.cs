using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ESGI.WFC.Exporter
{
    [CreateAssetMenu(menuName = "WFC/2D/Module Exporter")]
    public class ModuleExporter : ScriptableObject
    {
        public List<ModuleGAN> modules;
    }
}
