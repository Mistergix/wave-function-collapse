using System;
using UnityEngine;

namespace ESGI.WFC
{
    public class Controller : MonoBehaviour
    {
        public WaveFunctionCollapse wfc;

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                wfc.Generate();
            }
        }
    }
}