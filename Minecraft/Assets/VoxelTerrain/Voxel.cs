
using UnityEngine;

public class Voxel
{
    public VoxelTypeDefinition  TypeDef;
    public int                  Health;
    public bool                 Exposed;
    public VoxelSide            ExposedSides;

    public IntVec3              Position;

    public BoxCollider          Collider;

    public VoxelWorldChunk      ParentChunk;

    public Voxel(VoxelWorldChunk parentChunk, VoxelType type, IntVec3 pos, BoxCollider collider)
    {
        ParentChunk = parentChunk;
        Collider = collider;
        SetType(type);
        Exposed = false;
        Position = pos;
        ExposedSides = VoxelSide.All;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            SetType(VoxelType.Air);
            VoxelWorld.Inst.Refresh();
        }
    }

    public void SetType(VoxelType type)
    {
        TypeDef = VoxelWorld.Inst.VoxelTypeDefs[type];
        Health = TypeDef.MaxHealth;
        ParentChunk.MakeDirty(true);
    }

    public Voxel NeighborOrNull(VoxelDirection direction)
    {
        IntVec3 neighborPosition = Position.Offset(VoxelDirectionOffsets.Offset[(int)direction]);
        if (VoxelWorld.Inst.IsVoxelWorldIndexValid(neighborPosition.X, neighborPosition.Y, neighborPosition.Z))
            return VoxelWorld.Inst.GetVoxel(neighborPosition);
        return null;
    }

    public bool IsNeighborSolid(VoxelDirection direction)
    {
        Voxel neighbor = NeighborOrNull(direction);
        if (neighbor != null)
            return Voxel.IsSolid(neighbor);
        return false;
    }

    public bool CanBeSeen()
    {
        return TypeDef.IsVisible;
    }

    public static bool IsSolid(Voxel voxel)
    {
        if (voxel == null || voxel.TypeDef.IsSolid == false)
            return false;
        return true;
    }

    public override string ToString()
    {
        return string.Concat("Voxel ", Position.X, ", ", Position.Y, ", ", Position.Z);
    }
}