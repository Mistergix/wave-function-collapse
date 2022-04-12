using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WFC/2D/Module")]
public class Module : ScriptableObject
{
    public GameObject modulePrefab;
    public Neighbours<Socket> sockets;
}
