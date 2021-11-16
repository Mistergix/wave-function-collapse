using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace ESGI.WFC.ThreeDimensions
{
    public class PrototypesParent : MonoBehaviour
    {
        public float space = 4;

        [Button]
        private void Arrange()
        {
            var i = 0;
            foreach (Transform t in transform)
            {
                t.localPosition = new Vector3(i * space, 0, 0);
                i++;
            }
        }
    }
}
