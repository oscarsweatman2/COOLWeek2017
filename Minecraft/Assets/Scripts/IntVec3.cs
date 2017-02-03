using UnityEngine;

public struct IntVec3
{
    public int X;
    public int Y;
    public int Z;
    public IntVec3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3 ToVec3()
    {
        return new Vector3(X, Y, Z);
    }

    public IntVec3 Offset(IntVec3 v)
    {
        return new IntVec3(X + v.X, Y + v.Y, Z + v.Z);
    }

    public bool InRange(int xlen, int ylen, int zlen)
    {
        return X >= 0 && Y >= 0 && Z >= 0 && X < xlen && Y < ylen && Z < zlen;
    }

    public override string ToString()
    {
        return string.Concat(X, ", ", Y, ", ", Z);
    }
}