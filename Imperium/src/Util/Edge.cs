using UnityEngine;

namespace Imperium.Util;

internal struct Edge
{
    public Vector3 PointA;
    public Vector3 PointB;

    public Edge(Vector3 a, Vector3 b)
    {
        if (a.sqrMagnitude <= b.sqrMagnitude)
        {
            PointA = a;
            PointB = b;
        }
        else
        {
            PointA = b;
            PointB = a;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is not Edge other) return false;
        return PointA == other.PointA && PointB == other.PointB;
    }

    public override int GetHashCode()
    {
        return PointA.GetHashCode() ^ PointB.GetHashCode();
    }
}