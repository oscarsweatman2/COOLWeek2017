using UnityEngine;

public enum VoxelType
{
    Air,
    Grass,
    Dirt,
    Stone,
    NumTypes
}

[System.Flags]
public enum VoxelSide
{
    None = 0,
    Top = 1,
    North = 2,
    East = 4,
    South = 8,
    West = 16,
    Bottom = 32,
    All = Top | North | East | South | West | Bottom,
}

public enum VoxelDirection
{
    TopNorthWest    = 0,
    TopNorth        = 1,
    TopNorthEast    = 2,
    TopWest         = 3,
    Top             = 4,
    TopEast         = 5,
    TopSouthWest    = 6,
    TopSouth        = 7,
    TopSouthEast    = 8,
    NorthWest       = 9,
    North           = 10,
    NorthEast       = 11,
    West            = 12,
    East            = 13,
    SouthWest       = 14,
    South           = 15,
    SouthEast       = 16,
    BottomNorthWest = 17,
    BottomNorth     = 18,
    BottomNorthEast = 19,
    BottomWest      = 20,
    Bottom          = 21,
    BottomEast      = 22,
    BottomSouthWest = 23,
    BottomSouth     = 24,
    BottomSouthEast = 25,
    NumDirections   = 26,
}

public static class VoxelDirectionOffsets
{
    public static IntVec3[] Offset = new IntVec3[(int)VoxelDirection.NumDirections]
    {
        new IntVec3( -1,  1,  1),
        new IntVec3(  0,  1,  1),
        new IntVec3(  1,  1,  1),
        new IntVec3( -1,  1,  0),
        new IntVec3(  0,  1,  0),
        new IntVec3(  1,  1,  0),
        new IntVec3( -1,  1, -1),
        new IntVec3(  0,  1, -1),
        new IntVec3(  1,  1, -1),
        new IntVec3( -1,  0,  1),
        new IntVec3(  0,  0,  1),
        new IntVec3(  1,  0,  1),
        new IntVec3( -1,  0,  0),
        new IntVec3(  1,  0,  0),
        new IntVec3( -1,  0, -1),
        new IntVec3(  0,  0, -1),
        new IntVec3(  1,  0, -1),
        new IntVec3( -1, -1,  1),
        new IntVec3(  0, -1,  1),
        new IntVec3(  1, -1,  1),
        new IntVec3( -1, -1,  0),
        new IntVec3(  0, -1,  0),
        new IntVec3(  1, -1,  0),
        new IntVec3( -1, -1, -1),
        new IntVec3(  0, -1, -1),
        new IntVec3(  1, -1, -1),
    };
}