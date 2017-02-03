
using UnityEngine;

public class VoxelUVSet
{
    public VoxelUVTile Top = new VoxelUVTile();
    public VoxelUVTile Side = new VoxelUVTile();
    public VoxelUVTile Bottom = new VoxelUVTile();
}

public class VoxelUVTile
{
    public Vector2 A;
    public Vector2 B;
    public Vector2 C;
    public Vector2 D;
}