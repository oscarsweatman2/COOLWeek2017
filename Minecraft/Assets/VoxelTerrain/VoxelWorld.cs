
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelWorld : MonoBehaviour
{
    public static VoxelWorld Inst = null;

    public VoxelWorldChunk VoxelWorldChunkPrefab = null;

    public bool UseHeightMap = true;
    public Texture2D HeightMap = null;

    public int ChunksWide       = 2;
    public int ChunksDeep       = 2;
    public int ChunksHigh       = 2;

    public int ChunkVoxelSize   = 8;

    public float PhysicalVoxelSize = 1.0f;

    public int VoxelTextureTilesAcross = 8;
    public float VoxelTextureUVBiasPercent = 0.02f;

    public VoxelTypeDefinition VoxelTypeDefAir      = new VoxelTypeDefinition(VoxelType.Air, 1, false, false);
    public VoxelTypeDefinition VoxelTypeDefGrass    = new VoxelTypeDefinition(VoxelType.Grass, 1, true, true);
    public VoxelTypeDefinition VoxelTypeDefDirt     = new VoxelTypeDefinition(VoxelType.Dirt, 1, true, true);
    public VoxelTypeDefinition VoxelTypeDefStone    = new VoxelTypeDefinition(VoxelType.Stone, 2, true, true);

    public Dictionary<VoxelType, VoxelTypeDefinition> VoxelTypeDefs;

    private VoxelWorldChunk[,,] Chunks = null;

    public int VoxelWidth   = 0;
    public int VoxelHeight  = 0;
    public int VoxelDepth   = 0;

    public bool Initialized = false;

    public void Start()
    {
        Inst = this;

        VoxelWidth = ChunksWide * ChunkVoxelSize;
        VoxelHeight = ChunksHigh * ChunkVoxelSize;
        VoxelDepth = ChunksDeep * ChunkVoxelSize;

        VoxelTypeDefs = new Dictionary<VoxelType, VoxelTypeDefinition>();
        VoxelTypeDefs[VoxelType.Air] = VoxelTypeDefAir;
        VoxelTypeDefs[VoxelType.Grass] = VoxelTypeDefGrass;
        VoxelTypeDefs[VoxelType.Dirt] = VoxelTypeDefDirt;
        VoxelTypeDefs[VoxelType.Stone] = VoxelTypeDefStone;

        VoxelTypeDefs.Values.ToList().ForEach(def => def.RecalcUVSet());

        InstantiateChunks();

        GenerateWorldVoxels(GenerateVoxel_Pass1);
        GenerateWorldVoxels(GenerateVoxel_Pass2);

        Initialized = true;

        Refresh();
    }

    public void InstantiateChunks()
    {
        Chunks = new VoxelWorldChunk[ChunksWide, ChunksHigh, ChunksDeep];

        for (int x = 0; x < ChunksWide; ++x)
        {
            for (int y = 0; y < ChunksHigh; ++y)
            {
                for (int z = 0; z < ChunksDeep; ++z)
                {
                    string chunkName = string.Concat("Chunk ", x, ", ", y, ", ", z);
                    VoxelWorldChunk chunk = GameObject.Instantiate<VoxelWorldChunk>(VoxelWorldChunkPrefab);
                    chunk.name = chunkName;
                    chunk.transform.parent = transform;
                    chunk.ChunkPosition = new IntVec3(x, y, z);
                    int worldx = x * ChunkVoxelSize;
                    int worldy = y * ChunkVoxelSize;
                    int worldz = z * ChunkVoxelSize;
                    chunk.InstantiateVoxels(worldx, worldy, worldz, ChunkVoxelSize, ChunkVoxelSize, ChunkVoxelSize);
                    Chunks[x, y, z] = chunk;
                }
            }
        }
    }

    public void GenerateWorldVoxels(System.Action<Voxel> action)
    {
        for (int x = 0; x < ChunksWide; ++x)
        {
            for (int y = 0; y < ChunksHigh; ++y)
            {
                for (int z = 0; z < ChunksDeep; ++z)
                {
                    VoxelWorldChunk chunk = Chunks[x, y, z];

                    GenerateChunkVoxels(chunk, action);
                }
            }
        }
    }

    public void GenerateChunkVoxels(VoxelWorldChunk chunk, System.Action<Voxel> action)
    {
        for (int x = 0; x < chunk.Width; ++x)
        {
            for (int y = 0; y < chunk.Height; ++y)
            {
                for (int z = 0; z < chunk.Depth; ++z)
                {
                    Voxel voxel = chunk.GetVoxel(x, y, z);

                    action(voxel);
                }
            }
        }
    }

    public void GenerateVoxel_Pass1(Voxel voxel)
    {
        voxel.SetType(VoxelType.Dirt);

        if (UseHeightMap && HeightMap != null)
        {
            float worldRatioX = (float)voxel.Position.X / VoxelWidth;
            float worldRatioY = (float)voxel.Position.Y / VoxelHeight;
            float worldRatioZ = (float)voxel.Position.Z / VoxelDepth;

            Color texel = HeightMap.GetPixel((int)(worldRatioX * HeightMap.width), (int)(worldRatioZ * HeightMap.height));

            if (worldRatioY > texel.r)
                voxel.SetType(VoxelType.Air);
        }
    }

    public void GenerateVoxel_Pass2(Voxel voxel)
    {
        if (voxel.TypeDef.Type == VoxelType.Dirt)
        {
            Voxel above = voxel.NeighborOrNull(VoxelDirection.Top);
            if (!Voxel.IsSolid(above))
                voxel.SetType(VoxelType.Grass);
            else
            {
                // Get the voxel 4 spaces above me... if it's solid...then I will be stone
                IntVec3 testVoxelPos = new IntVec3(voxel.Position.X, voxel.Position.Y + 2, voxel.Position.Z);
                if (IsVoxelWorldIndexValid(testVoxelPos.X, testVoxelPos.Y, testVoxelPos.Z))
                {
                    Voxel testVoxel = GetVoxel(testVoxelPos);
                    if (Voxel.IsSolid(testVoxel))
                    {
                        voxel.SetType(VoxelType.Stone);
                    }
                }
            }
        }
    }

    public void Refresh()
    {
        foreach (VoxelWorldChunk chunk in Chunks)
            if (chunk.IsDirty)
                chunk.Refresh();
    }

    public void UpdateExposedVoxels()
    {
        for (int x = 0; x < ChunksWide; ++x)
        {
            for (int y = 0; y < ChunksHigh; ++y)
            {
                for (int z = 0; z < ChunksDeep; ++z)
                {
                    Chunks[x, y, z].UpdateExposedVoxels();
                }
            }
        }
    }

    public List<Voxel> GetVoxels(Vector3 center, float radius)
    {
        List<Voxel> returnVoxels = new List<Voxel>();

        int iRadius = Mathf.Max(0, (int)radius + 1);

        List<Voxel> testVoxels = GetVoxels(
                                    (int)center.x - iRadius,
                                    (int)center.y - iRadius,
                                    (int)center.z - iRadius,
                                    (int)center.x + iRadius,
                                    (int)center.y + iRadius,
                                    (int)center.z + iRadius);

        Vector3 halfVoxel = new Vector3(PhysicalVoxelSize * 0.5f, PhysicalVoxelSize * 0.5f, PhysicalVoxelSize * 0.5f);

        returnVoxels.AddRange(testVoxels.Where(vox => (vox.Position.ToVec3() + halfVoxel - center).magnitude <= radius));
        return returnVoxels;
    }

    public List<Voxel> GetVoxels(int minx, int miny, int minz, int maxx, int maxy, int maxz)
    {
        List<Voxel> returnVoxels = new List<Voxel>();

        // Early out if the request is out of bounds
        if (minx >= VoxelWidth || miny >= VoxelHeight || minz >= VoxelDepth || maxx < 0 || maxy < 0 || maxz < 0)
            return returnVoxels;

        minx = Mathf.Clamp(minx, 0, VoxelWidth - 1);
        miny = Mathf.Clamp(miny, 0, VoxelHeight - 1);
        minz = Mathf.Clamp(minz, 0, VoxelDepth - 1);
        maxx = Mathf.Clamp(maxx, 0, VoxelWidth - 1);
        maxy = Mathf.Clamp(maxy, 0, VoxelHeight - 1);
        maxz = Mathf.Clamp(maxz, 0, VoxelDepth - 1);

        //int _minx = Mathf.Min(minx, maxx);
        //int _miny = Mathf.Min(miny, maxy);
        //int _minz = Mathf.Min(minz, maxz);
        //int _maxx = Mathf.Max(minx, maxx);
        //int _maxy = Mathf.Max(miny, maxy);
        //int _maxz = Mathf.Max(minz, maxz);

        for (int x = minx; x <= maxx; ++x)
        {
            for (int y = miny; y <= maxy; ++y)
            {
                for (int z = minz; z <= maxz; ++z)
                {
                    returnVoxels.Add(GetVoxel(x, y, z));
                }
            }
        }

        return returnVoxels;
    }

    public Voxel GetVoxel(IntVec3 pos)
    {
        return GetVoxel(pos.X, pos.Y, pos.Z);
    }

    public Voxel GetVoxel(int x, int y, int z)
    {
        int chunkx = x / ChunkVoxelSize;
        int chunky = y / ChunkVoxelSize;
        int chunkz = z / ChunkVoxelSize;
        
        int localx = x % ChunkVoxelSize;
        int localy = y % ChunkVoxelSize;
        int localz = z % ChunkVoxelSize;
        
        return Chunks[chunkx, chunky, chunkz].GetVoxel(localx, localy, localz);
    }

    public VoxelWorldChunk GetChunk(IntVec3 chunkPosition)
    {
        return Chunks[chunkPosition.X, chunkPosition.Y, chunkPosition.Z];
    }

    public bool IsChunkIndexValid(IntVec3 chunkPosition)
    {
        return IsChunkIndexValid(chunkPosition.X, chunkPosition.Y, chunkPosition.Z);
    }

    public bool IsChunkIndexValid(int x, int y, int z)
    {
        return x >= 0 && y >= 0 && z >= 0 && x < ChunksWide && y < ChunksHigh && z < ChunksDeep;
    }
    
    public bool IsVoxelWorldIndexValid(int x, int y, int z)
    {
        return x >= 0 && y >= 0 && z >= 0 && x < VoxelWidth && y < VoxelHeight && z < VoxelDepth;
    }

    public Voxel GetVoxelFromCollider(BoxCollider collider)
    {
        VoxelWorldChunk chunk = collider.gameObject.GetComponent<VoxelWorldChunk>();
        return chunk.GetVoxelFromCollider(collider);
    }
}