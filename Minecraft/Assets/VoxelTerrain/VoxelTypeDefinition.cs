using UnityEngine;

[System.Serializable]
public class VoxelTypeDefinition
{
    public readonly VoxelType   Type;
    public int                  MaxHealth; 
    public bool                 IsVisible;   
    public bool                 IsSolid;
    public int                  UVTopX;
    public int                  UVTopY;
    public int                  UVSideX;
    public int                  UVSideY;
    public int                  UVBottomX;
    public int                  UVBottomY;
    public VoxelUVSet           UVSet;

    public VoxelTypeDefinition(VoxelType type, int maxHealth, bool isSolid, bool isVisible)
    {
        Type = type;
        MaxHealth = maxHealth;
        IsSolid = isSolid;
        IsVisible = isVisible;
        UVTopX = 0;
        UVTopY = 0;
        UVSideX = 0;
        UVSideY = 0;
        UVBottomX = 0;
        UVBottomY = 0;
        UVSet = new VoxelUVSet();
    }

    public void RecalcUVSet()
    {
        float tileSize = 1.0f / VoxelWorld.Inst.VoxelTextureTilesAcross;

        float bias = tileSize * VoxelWorld.Inst.VoxelTextureUVBiasPercent;
        Vector2 aBias = new Vector2(bias, bias);
        Vector2 bBias = new Vector2(bias, -bias);
        Vector2 cBias = new Vector2(-bias, -bias);
        Vector2 dBias = new Vector2(-bias, bias);

        UVSet.Top.A = new Vector2(UVTopX * tileSize, UVTopY * tileSize) + aBias;
        UVSet.Top.B = new Vector2(UVTopX * tileSize, UVTopY * tileSize + tileSize) + bBias;
        UVSet.Top.C = new Vector2(UVTopX * tileSize + tileSize, UVTopY * tileSize + tileSize) + cBias;
        UVSet.Top.D = new Vector2(UVTopX * tileSize + tileSize, UVTopY * tileSize) + dBias;

        UVSet.Side.A = new Vector2(UVSideX * tileSize, UVSideY * tileSize) + aBias;
        UVSet.Side.B = new Vector2(UVSideX * tileSize, UVSideY * tileSize + tileSize) + bBias;
        UVSet.Side.C = new Vector2(UVSideX * tileSize + tileSize, UVSideY * tileSize + tileSize) + cBias;
        UVSet.Side.D = new Vector2(UVSideX * tileSize + tileSize, UVSideY * tileSize) + dBias;

        UVSet.Bottom.A = new Vector2(UVBottomX * tileSize, UVBottomY * tileSize) + aBias;
        UVSet.Bottom.B = new Vector2(UVBottomX * tileSize, UVBottomY * tileSize + tileSize) + bBias;
        UVSet.Bottom.C = new Vector2(UVBottomX * tileSize + tileSize, UVBottomY * tileSize + tileSize) + cBias;
        UVSet.Bottom.D = new Vector2(UVBottomX * tileSize + tileSize, UVBottomY * tileSize) + dBias;
    }
}