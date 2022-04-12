using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Neighbours<T>
{
    public T bottom;
    public T right;
    public T top;
    public T left;
    public int Length => 4;

    public T this[int index]
    {
        get
        {
            return index switch
            {
                0 => bottom,
                1 => right,
                2 => top,
                3 => left,
                _ => default
            };
        }
    }

    public T GetNeighbour(int i)
    {
        return i switch
        {
            0 => top,
            1 => left,
            2 => bottom,
            3 => right,
            _ => default
        };
    }
}
