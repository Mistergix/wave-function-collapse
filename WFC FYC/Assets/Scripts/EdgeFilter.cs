using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeFilter
{
    private readonly Socket _filterType;
    private readonly bool _isInclusive;
    private readonly Directions _edgeDirection;

    public delegate bool CheckModuleMatchFunction(Module module, int edge);
    
    public EdgeFilter(Directions direction, SocketBlock filterType, bool isInclusive)
    {
        _edgeDirection = direction;
        _filterType = filterType;
        _isInclusive = isInclusive;
    }

    public EdgeFilter(int direction, Socket filterType, bool isInclusive)
    {
        _edgeDirection = (Directions)direction;
        _filterType = filterType;
        _isInclusive = isInclusive;
    }

    public enum Directions
    {
        Bottom = 0,
        Right = 1,
        Top = 2,
        Left = 3
    }

    public static bool MatchType(Module module, int edge)
    {
        var socket = module.sockets[edge];
        var match = socket.GetType() == typeof(SocketBlock) || socket.GetType().IsSubclassOf(typeof(SocketBlock));
        return match;
    }
    
    public bool MatchEquality(Module module, int edge)
    {
        return module.sockets[edge] == _filterType;
    }

    public bool CheckModule(Module module, CheckModuleMatchFunction matchFunction)
    {
        var edge = ((int) _edgeDirection + 2) % 4;
        var match = matchFunction(module, edge);
        return _isInclusive ? !match : match;
    }

    
}
