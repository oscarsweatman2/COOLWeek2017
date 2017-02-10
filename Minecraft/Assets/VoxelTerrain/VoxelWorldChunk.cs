using System.Collections.Generic;
using UnityEngine;

public class VoxelWorldChunk : MonoBehaviour
{
    public IntVec3 ChunkPosition;

    private Mesh Mesh;
    private MeshFilter MeshFilter;

    public int Width;
    public int Height;
    public int Depth;

    private Voxel[,,] Voxels;

    private Vector3[] Verts = null;
    private Color[] Colors = null;
    private Vector3[] Norms = null;
    private Vector2[] Coords = null;
    private int[] Inds = null;

    public int NumVerts = 0;
    public int NumInds = 0;

    private Dictionary<BoxCollider, Voxel> ColliderToVoxel = new Dictionary<BoxCollider, Voxel>();

    public bool IsDirty = true;

    public void Start()
    {
    }

    public void MakeDirty(bool neighbors)
    {
        IsDirty = true;
        if (neighbors && VoxelWorld.Inst.Initialized)
        {
            if (VoxelWorld.Inst.IsChunkIndexValid(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.Top])))
                VoxelWorld.Inst.GetChunk(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.Top])).MakeDirty(false);
            if (VoxelWorld.Inst.IsChunkIndexValid(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.North])))
                VoxelWorld.Inst.GetChunk(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.North])).MakeDirty(false);
            if (VoxelWorld.Inst.IsChunkIndexValid(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.East])))
                VoxelWorld.Inst.GetChunk(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.East])).MakeDirty(false);
            if (VoxelWorld.Inst.IsChunkIndexValid(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.South])))
                VoxelWorld.Inst.GetChunk(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.South])).MakeDirty(false);
            if (VoxelWorld.Inst.IsChunkIndexValid(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.West])))
                VoxelWorld.Inst.GetChunk(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.West])).MakeDirty(false);
            if (VoxelWorld.Inst.IsChunkIndexValid(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.Bottom])))
                VoxelWorld.Inst.GetChunk(ChunkPosition.Offset(VoxelDirectionOffsets.Offset[(int)VoxelDirection.Bottom])).MakeDirty(false);
        }
    }

    public void InstantiateVoxels(int startx, int starty, int startz, int width, int height, int depth)
    {
        UnityEngine.Profiling.Profiler.BeginSample("Chunk.InstantiateVoxels");

        Width = width;
        Height = height;
        Depth = depth;

        Voxels = new Voxel[Width, Height, Depth];

        float voxelSize = VoxelWorld.Inst.PhysicalVoxelSize;
        float halfVoxel = voxelSize * 0.5f;

        Voxel voxel = null;
        BoxCollider collider = null;
        IntVec3 pos;
        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int z = 0; z < Depth; ++z)
                {
                    pos = new IntVec3(startx + x, starty + y, startz + z);
                    collider = gameObject.AddComponent<BoxCollider>();
                    collider.center = new Vector3(pos.X + halfVoxel, pos.Y + halfVoxel, pos.Z + halfVoxel);
                    collider.size = new Vector3(voxelSize, voxelSize, voxelSize);
                    voxel = new Voxel(this, VoxelType.Air, pos, collider);
                    Voxels[x, y, z] = voxel;
                    ColliderToVoxel[collider] = voxel;
                }
            }
        }

        UnityEngine.Profiling.Profiler.EndSample();
    }

    public void Refresh()
    {
        UpdateExposedVoxels();
        RefreshMesh();
        IsDirty = false;
    }

    public void RefreshMesh()
    {
        UnityEngine.Profiling.Profiler.BeginSample("VoxelChunk.RefreshMesh");

        if (Mesh == null)
        {
            Mesh = new Mesh();
            MeshFilter = gameObject.GetComponent<MeshFilter>();
            MeshFilter.mesh = Mesh;
        }

        Mesh.Clear();

        if (NumVerts < 3 || NumInds < 3)
            return;

        List<Voxel> exposedVoxels = new List<Voxel>();

        foreach (Voxel voxel in Voxels)
            if (voxel.Exposed)
                exposedVoxels.Add(voxel);

        Verts = new Vector3[NumVerts];
        Colors = new Color[NumVerts];
        Norms = new Vector3[NumVerts];
        Coords = new Vector2[NumVerts];
        Inds = new int[NumInds];

        int vertIndex = 0;
        int indIndex = 0;

        Vector3 up = Vector3.up;
        Vector3 north = Vector3.forward;
        Vector3 east = Vector3.right;
        Vector3 south = -north;
        Vector3 west = -east;
        Vector3 down = -up;

        float voxelSize = VoxelWorld.Inst.PhysicalVoxelSize;

        float aoMod = 0.8f;

        Vector3 bsw;
        Vector3 bnw;
        Vector3 bne;
        Vector3 bse;
        Vector3 tsw;
        Vector3 tnw;
        Vector3 tne;
        Vector3 tse;

        Color AOTopNorthWest    ;
        Color AOTopNorth        ;
        Color AOTopNorthEast    ;
        Color AOTopWest         ;
        Color AOTopEast         ;
        Color AOTopSouthWest    ;
        Color AOTopSouth        ;
        Color AOTopSouthEast    ;
        Color AONorthWest       ;
        Color AONorthEast       ;
        Color AOSouthWest       ;
        Color AOSouthEast       ;
        Color AOBottomNorthWest ;
        Color AOBottomNorth     ;
        Color AOBottomNorthEast ;
        Color AOBottomWest      ;
        Color AOBottomEast      ;
        Color AOBottomSouthWest ;
        Color AOBottomSouth     ;
        Color AOBottomSouthEast ;

        Color  TopA     ;
        Color  TopB     ;
        Color  TopC     ;
        Color  TopD     ;
        Color  NorthA   ;
        Color  NorthB   ;
        Color  NorthC   ;
        Color  NorthD   ;
        Color  EastA    ;
        Color  EastB    ;
        Color  EastC    ;
        Color  EastD    ;
        Color  SouthA   ;
        Color  SouthB   ;
        Color  SouthC   ;
        Color  SouthD   ;
        Color  WestA    ;
        Color  WestB    ;
        Color  WestC    ;
        Color  WestD    ;
        Color  BottomA  ;
        Color  BottomB  ;
        Color  BottomC  ;
        Color  BottomD  ;

        foreach (Voxel voxel in exposedVoxels)
        {
            IntVec3 index = voxel.Position;

            float voxelVerticalRatio = (float)index.Y / (float)(VoxelWorld.Inst.VoxelHeight - 1);

            Color voxelColor = Color.Lerp(VoxelWorld.Inst.BottomMostTint, VoxelWorld.Inst.TopMostTint, voxelVerticalRatio);

            bsw = new Vector3(index.X * voxelSize, index.Y * voxelSize, index.Z * voxelSize);
            bnw = bsw; bnw.z += voxelSize; // + new Vector3(0, 0, voxelSize);
            bne = bsw; bne.x += voxelSize; bne.z += voxelSize; // + new Vector3(voxelSize, 0, voxelSize);
            bse = bsw; bse.x += voxelSize; // + new Vector3(voxelSize, 0, 0);
            tsw = bsw; tsw.y += voxelSize; // + new Vector3(0, voxelSize, 0);
            tnw = bsw; tnw.y += voxelSize; tnw.z += voxelSize; // + new Vector3(0, voxelSize, voxelSize);
            tne = bsw; tne.x += voxelSize; tne.y += voxelSize; tne.z += voxelSize; // + new Vector3(voxelSize, voxelSize, voxelSize);
            tse = bsw; tse.x += voxelSize; tse.y += voxelSize; // + new Vector3(voxelSize, voxelSize, 0);

            //AOTopNorthWest    = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopNorthWest) ? aoMod : 1.0f);
            //AOTopNorth        = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopNorth) ? aoMod : 1.0f);
            //AOTopNorthEast    = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopNorthEast) ? aoMod : 1.0f);
            //AOTopWest         = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopWest) ? aoMod : 1.0f);
            //AOTopEast         = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopEast) ? aoMod : 1.0f);
            //AOTopSouthWest    = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopSouthWest) ? aoMod : 1.0f);
            //AOTopSouth        = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopSouth) ? aoMod : 1.0f);
            //AOTopSouthEast    = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopSouthEast) ? aoMod : 1.0f);
            //AONorthWest       = Color.white * (voxel.IsNeighborSolid(VoxelDirection.NorthWest) ? aoMod : 1.0f);
            //AONorthEast       = Color.white * (voxel.IsNeighborSolid(VoxelDirection.NorthEast) ? aoMod : 1.0f);
            //AOSouthWest       = Color.white * (voxel.IsNeighborSolid(VoxelDirection.SouthWest) ? aoMod : 1.0f);
            //AOSouthEast       = Color.white * (voxel.IsNeighborSolid(VoxelDirection.SouthEast) ? aoMod : 1.0f);
            //AOBottomNorthWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomNorthWest) ? aoMod : 1.0f);
            //AOBottomNorth     = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomNorth) ? aoMod : 1.0f);
            //AOBottomNorthEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomNorthEast) ? aoMod : 1.0f);
            //AOBottomWest      = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomWest) ? aoMod : 1.0f);
            //AOBottomEast      = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomEast) ? aoMod : 1.0f);
            //AOBottomSouthWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomSouthWest) ? aoMod : 1.0f);
            //AOBottomSouth     = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomSouth) ? aoMod : 1.0f);
            //AOBottomSouthEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomSouthEast) ? aoMod : 1.0f);

            TopA     = Color.white; //AOTopWest * AOTopSouth * AOTopSouthWest;
            TopB     = Color.white; //AOTopWest * AOTopNorth * AOTopNorthWest;
            TopC     = Color.white; //AOTopEast * AOTopNorth * AOTopNorthEast;
            TopD     = Color.white; //AOTopEast * AOTopSouth * AOTopSouthEast;
            NorthA   = Color.white; //AONorthEast * AOBottomNorth * AOBottomNorthEast;
            NorthB   = Color.white; //AONorthEast * AOTopNorth * AOTopNorthEast;
            NorthC   = Color.white; //AONorthWest * AOTopNorth * AOTopNorthWest;
            NorthD   = Color.white; //AONorthWest * AOBottomNorth * AOBottomNorthWest;
            EastA    = Color.white; //AOSouthEast * AOBottomEast * AOBottomSouthEast;
            EastB    = Color.white; //AOSouthEast * AOTopEast * AOTopSouthEast;
            EastC    = Color.white; //AONorthEast * AOTopEast * AOTopNorthEast;
            EastD    = Color.white; //AONorthEast * AOBottomEast * AOBottomNorthEast;
            SouthA   = Color.white; //AOSouthWest * AOBottomSouth * AOBottomSouthWest;
            SouthB   = Color.white; //AOSouthWest * AOTopSouth * AOTopSouthWest;
            SouthC   = Color.white; //AOSouthEast * AOTopSouth * AOTopSouthEast;
            SouthD   = Color.white; //AOSouthEast * AOBottomSouth * AOBottomSouthEast;
            WestA    = Color.white; //AONorthWest * AOBottomWest * AOBottomNorthWest;
            WestB    = Color.white; //AONorthWest * AOTopWest * AOTopNorthWest;
            WestC    = Color.white; //AOSouthWest * AOTopWest * AOTopSouthWest;
            WestD    = Color.white; //AOSouthWest * AOBottomWest * AOBottomSouthWest;
            BottomA  = Color.white; //AOBottomWest * AOBottomNorth * AOBottomNorthWest;
            BottomB  = Color.white; //AOBottomWest * AOBottomSouth * AOBottomSouthWest;
            BottomC  = Color.white; //AOBottomEast * AOBottomSouth * AOBottomSouthEast;
            BottomD = Color.white;  //AOBottomEast * AOBottomNorth * AOBottomNorthEast;

            // Color tnAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopNorth) ? aoMod : 1.0f);
            // Color teAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopEast) ? aoMod : 1.0f);
            // Color tsAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopSouth) ? aoMod : 1.0f);
            // Color twAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopWest) ? aoMod : 1.0f);
            // Color neAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.NorthEast) ? aoMod : 1.0f);
            // Color seAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.SouthEast) ? aoMod : 1.0f);
            // Color swAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.SouthWest) ? aoMod : 1.0f);
            // Color nwAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.NorthWest) ? aoMod : 1.0f);
            // Color bnAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomNorth) ? aoMod : 1.0f);
            // Color beAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomEast) ? aoMod : 1.0f);
            // Color bsAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomSouth) ? aoMod : 1.0f);
            // Color bwAO = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomWest) ? aoMod : 1.0f);

            if ((voxel.ExposedSides & VoxelSide.Bottom) != VoxelSide.None)
            {
                // Bottom
                Verts[vertIndex + 0] = bnw;
                Verts[vertIndex + 1] = bsw;
                Verts[vertIndex + 2] = bse;
                Verts[vertIndex + 3] = bne;
                Colors[vertIndex + 0] = BottomA * voxelColor;
                Colors[vertIndex + 1] = BottomB * voxelColor;
                Colors[vertIndex + 2] = BottomC * voxelColor;
                Colors[vertIndex + 3] = BottomD * voxelColor;
                Norms[vertIndex + 0] = down;
                Norms[vertIndex + 1] = down;
                Norms[vertIndex + 2] = down;
                Norms[vertIndex + 3] = down;
                Coords[vertIndex + 0] = voxel.TypeDef.UVSet.Bottom.A;
                Coords[vertIndex + 1] = voxel.TypeDef.UVSet.Bottom.B;
                Coords[vertIndex + 2] = voxel.TypeDef.UVSet.Bottom.C;
                Coords[vertIndex + 3] = voxel.TypeDef.UVSet.Bottom.D;
                Inds[indIndex + 0] = vertIndex + 0;
                Inds[indIndex + 1] = vertIndex + 1;
                Inds[indIndex + 2] = vertIndex + 2;
                Inds[indIndex + 3] = vertIndex + 0;
                Inds[indIndex + 4] = vertIndex + 2;
                Inds[indIndex + 5] = vertIndex + 3;
                vertIndex += 4;
                indIndex += 6;
            }

            if ((voxel.ExposedSides & VoxelSide.South) != VoxelSide.None)
            {
                // South
                Verts[vertIndex + 0] = bsw;
                Verts[vertIndex + 1] = tsw;
                Verts[vertIndex + 2] = tse;
                Verts[vertIndex + 3] = bse;
                Colors[vertIndex + 0] = SouthA * voxelColor;
                Colors[vertIndex + 1] = SouthB * voxelColor;
                Colors[vertIndex + 2] = SouthC * voxelColor;
                Colors[vertIndex + 3] = SouthD * voxelColor;
                Norms[vertIndex + 0] = south;
                Norms[vertIndex + 1] = south;
                Norms[vertIndex + 2] = south;
                Norms[vertIndex + 3] = south;
                Coords[vertIndex + 0] = voxel.TypeDef.UVSet.Side.A;
                Coords[vertIndex + 1] = voxel.TypeDef.UVSet.Side.B;
                Coords[vertIndex + 2] = voxel.TypeDef.UVSet.Side.C;
                Coords[vertIndex + 3] = voxel.TypeDef.UVSet.Side.D;
                Inds[indIndex + 0] = vertIndex + 0;
                Inds[indIndex + 1] = vertIndex + 1;
                Inds[indIndex + 2] = vertIndex + 2;
                Inds[indIndex + 3] = vertIndex + 0;
                Inds[indIndex + 4] = vertIndex + 2;
                Inds[indIndex + 5] = vertIndex + 3;
                vertIndex += 4;
                indIndex += 6;
            }

            if ((voxel.ExposedSides & VoxelSide.West) != VoxelSide.None)
            {
                // West
                Verts[vertIndex + 0] = bnw;
                Verts[vertIndex + 1] = tnw;
                Verts[vertIndex + 2] = tsw;
                Verts[vertIndex + 3] = bsw;
                Colors[vertIndex + 0] = WestA * voxelColor;
                Colors[vertIndex + 1] = WestB * voxelColor;
                Colors[vertIndex + 2] = WestC * voxelColor;
                Colors[vertIndex + 3] = WestD * voxelColor;
                Norms[vertIndex + 0] = west;
                Norms[vertIndex + 1] = west;
                Norms[vertIndex + 2] = west;
                Norms[vertIndex + 3] = west;
                Coords[vertIndex + 0] = voxel.TypeDef.UVSet.Side.A;
                Coords[vertIndex + 1] = voxel.TypeDef.UVSet.Side.B;
                Coords[vertIndex + 2] = voxel.TypeDef.UVSet.Side.C;
                Coords[vertIndex + 3] = voxel.TypeDef.UVSet.Side.D;
                Inds[indIndex + 0] = vertIndex + 0;
                Inds[indIndex + 1] = vertIndex + 1;
                Inds[indIndex + 2] = vertIndex + 2;
                Inds[indIndex + 3] = vertIndex + 0;
                Inds[indIndex + 4] = vertIndex + 2;
                Inds[indIndex + 5] = vertIndex + 3;
                vertIndex += 4;
                indIndex += 6;
            }

            if ((voxel.ExposedSides & VoxelSide.North) != VoxelSide.None)
            {
                // North
                Verts[vertIndex + 0] = bne;
                Verts[vertIndex + 1] = tne;
                Verts[vertIndex + 2] = tnw;
                Verts[vertIndex + 3] = bnw;
                Colors[vertIndex + 0] = NorthA * voxelColor;
                Colors[vertIndex + 1] = NorthB * voxelColor;
                Colors[vertIndex + 2] = NorthC * voxelColor;
                Colors[vertIndex + 3] = NorthD * voxelColor;
                Norms[vertIndex + 0] = north;
                Norms[vertIndex + 1] = north;
                Norms[vertIndex + 2] = north;
                Norms[vertIndex + 3] = north;
                Coords[vertIndex + 0] = voxel.TypeDef.UVSet.Side.A;
                Coords[vertIndex + 1] = voxel.TypeDef.UVSet.Side.B;
                Coords[vertIndex + 2] = voxel.TypeDef.UVSet.Side.C;
                Coords[vertIndex + 3] = voxel.TypeDef.UVSet.Side.D;
                Inds[indIndex + 0] = vertIndex + 0;
                Inds[indIndex + 1] = vertIndex + 1;
                Inds[indIndex + 2] = vertIndex + 2;
                Inds[indIndex + 3] = vertIndex + 0;
                Inds[indIndex + 4] = vertIndex + 2;
                Inds[indIndex + 5] = vertIndex + 3;
                vertIndex += 4;
                indIndex += 6;
            }

            if ((voxel.ExposedSides & VoxelSide.East) != VoxelSide.None)
            {
                // East
                Verts[vertIndex + 0] = bse;
                Verts[vertIndex + 1] = tse;
                Verts[vertIndex + 2] = tne;
                Verts[vertIndex + 3] = bne;
                Colors[vertIndex + 0] = EastA * voxelColor;
                Colors[vertIndex + 1] = EastB * voxelColor;
                Colors[vertIndex + 2] = EastC * voxelColor;
                Colors[vertIndex + 3] = EastD * voxelColor;
                Norms[vertIndex + 0] = east;
                Norms[vertIndex + 1] = east;
                Norms[vertIndex + 2] = east;
                Norms[vertIndex + 3] = east;
                Coords[vertIndex + 0] = voxel.TypeDef.UVSet.Side.A;
                Coords[vertIndex + 1] = voxel.TypeDef.UVSet.Side.B;
                Coords[vertIndex + 2] = voxel.TypeDef.UVSet.Side.C;
                Coords[vertIndex + 3] = voxel.TypeDef.UVSet.Side.D;
                Inds[indIndex + 0] = vertIndex + 0;
                Inds[indIndex + 1] = vertIndex + 1;
                Inds[indIndex + 2] = vertIndex + 2;
                Inds[indIndex + 3] = vertIndex + 0;
                Inds[indIndex + 4] = vertIndex + 2;
                Inds[indIndex + 5] = vertIndex + 3;
                vertIndex += 4;
                indIndex += 6;
            }

            if ((voxel.ExposedSides & VoxelSide.Top) != VoxelSide.None)
            {
                // Top
                Verts[vertIndex + 0] = tsw;
                Verts[vertIndex + 1] = tnw;
                Verts[vertIndex + 2] = tne;
                Verts[vertIndex + 3] = tse;
                Colors[vertIndex + 0] = TopA * voxelColor;
                Colors[vertIndex + 1] = TopB * voxelColor;
                Colors[vertIndex + 2] = TopC * voxelColor;
                Colors[vertIndex + 3] = TopD * voxelColor;
                Norms[vertIndex + 0] = up;
                Norms[vertIndex + 1] = up;
                Norms[vertIndex + 2] = up;
                Norms[vertIndex + 3] = up;
                Coords[vertIndex + 0] = voxel.TypeDef.UVSet.Top.A;
                Coords[vertIndex + 1] = voxel.TypeDef.UVSet.Top.B;
                Coords[vertIndex + 2] = voxel.TypeDef.UVSet.Top.C;
                Coords[vertIndex + 3] = voxel.TypeDef.UVSet.Top.D;
                Inds[indIndex + 0] = vertIndex + 0;
                Inds[indIndex + 1] = vertIndex + 1;
                Inds[indIndex + 2] = vertIndex + 2;
                Inds[indIndex + 3] = vertIndex + 0;
                Inds[indIndex + 4] = vertIndex + 2;
                Inds[indIndex + 5] = vertIndex + 3;
                vertIndex += 4;
                indIndex += 6;
            }
        }

        Mesh.vertices = Verts;
        Mesh.colors = Colors;
        Mesh.normals = Norms;
        Mesh.uv = Coords;
        Mesh.triangles = Inds;

        UnityEngine.Profiling.Profiler.EndSample();
    }

    public void UpdateExposedVoxels()
    {
        UnityEngine.Profiling.Profiler.BeginSample("VoxelChunk.UpdateExposedVoxels");

        NumVerts = 0;
        NumInds = 0;

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int z = 0; z < Depth; ++z)
                {
                    Voxel voxel = Voxels[x, y, z];

                    voxel.Exposed = false;
                    voxel.ExposedSides = VoxelSide.None;

                    if (voxel.CanBeSeen())
                    {
                        Voxel top = voxel.NeighborOrNull(VoxelDirection.Top);
                        Voxel north = voxel.NeighborOrNull(VoxelDirection.North);
                        Voxel east = voxel.NeighborOrNull(VoxelDirection.East);
                        Voxel south = voxel.NeighborOrNull(VoxelDirection.South);
                        Voxel west = voxel.NeighborOrNull(VoxelDirection.West);
                        Voxel bottom = voxel.NeighborOrNull(VoxelDirection.Bottom);

                        if (Voxel.IsSolid(top) == false)
                        {
                            // Exposed Top
                            voxel.Exposed = true;
                            voxel.ExposedSides |= VoxelSide.Top;
                            NumVerts += 4;
                            NumInds += 6;
                        }
                        if (Voxel.IsSolid(north) == false)
                        {
                            // Exposed North
                            voxel.Exposed = true;
                            voxel.ExposedSides |= VoxelSide.North;
                            NumVerts += 4;
                            NumInds += 6;
                        }
                        if (Voxel.IsSolid(east) == false)
                        {
                            // Exposed East
                            voxel.Exposed = true;
                            voxel.ExposedSides |= VoxelSide.East;
                            NumVerts += 4;
                            NumInds += 6;
                        }
                        if (Voxel.IsSolid(south) == false)
                        {
                            // Exposed South
                            voxel.Exposed = true;
                            voxel.ExposedSides |= VoxelSide.South;
                            NumVerts += 4;
                            NumInds += 6;
                        }
                        if (Voxel.IsSolid(west) == false)
                        {
                            // Exposed West
                            voxel.Exposed = true;
                            voxel.ExposedSides |= VoxelSide.West;
                            NumVerts += 4;
                            NumInds += 6;
                        }
                        if (Voxel.IsSolid(bottom) == false)
                        {
                            // Exposed Bottom
                            voxel.Exposed = true;
                            voxel.ExposedSides |= VoxelSide.Bottom;
                            NumVerts += 4;
                            NumInds += 6;
                        }
                    }

                    voxel.Collider.enabled = voxel.Exposed;
                }
            }
        }

        UnityEngine.Profiling.Profiler.EndSample();
    }

    public Voxel GetVoxel(int x, int y, int z)
    {
        return Voxels[x, y, z];
    }

    public bool IsVoxelIndexValid(int x, int y, int z)
    {
        return x >= 0 && y >= 0 && z >= 0 && x < Width && y < Height && z < Depth;
    }

    public Voxel GetVoxelFromCollider(BoxCollider collider)
    {
        return ColliderToVoxel[collider];
    }
}