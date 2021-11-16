using System.Collections;
using System.Collections.Generic;
using Tessera;
using UnityEngine;


// This script simply kicks off the generator.
// By default the generator doesn't do anything, which is not useful for a sample
public class GenerateOnStart : MonoBehaviour
{
    void Start()
    {
        Generate();
    }

    public void Clear()
    {
        var multipassGenerator = GetComponent<TesseraMultipassGenerator>();
        if (multipassGenerator != null)
        {
            multipassGenerator.Clear();
        }
        else
        {

            var generators = GetComponents<TesseraGenerator>();

            generators[0].Clear();
        }
    }

    public void Generate()
    { 
        var multipassGenerator = GetComponent<TesseraMultipassGenerator>();
        if (multipassGenerator != null)
        {
            multipassGenerator.Generate();
        }
        else
        {

            var generators = GetComponents<TesseraGenerator>();

            foreach (var generator in generators)
            {
                generator.Generate();
            }
        }
    }
}

