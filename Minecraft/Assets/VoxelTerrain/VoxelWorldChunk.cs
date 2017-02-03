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
        Width = width;
        Height = height;
        Depth = depth;

        Voxels = new Voxel[Width, Height, Depth];

        float voxelSize = VoxelWorld.Inst.PhysicalVoxelSize;
        float halfVoxel = voxelSize * 0.5f;

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int z = 0; z < Depth; ++z)
                {
                    IntVec3 pos = new IntVec3(startx + x, starty + y, startz + z);
                    BoxCollider collider = gameObject.AddComponent<BoxCollider>();
                    collider.center = new Vector3(pos.X + halfVoxel, pos.Y + halfVoxel, pos.Z + halfVoxel);
                    collider.size = new Vector3(voxelSize, voxelSize, voxelSize);
                    Voxel voxel = new Voxel(this, VoxelType.Air, pos, collider);
                    Voxels[x, y, z] = voxel;
                    ColliderToVoxel[collider] = voxel;
                }
            }
        }
    }

    public void Refresh()
    {
        UpdateExposedVoxels();
        RefreshMesh();
        IsDirty = false;
    }

    public void RefreshMesh()
    {
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

        foreach (Voxel voxel in exposedVoxels)
        {
            IntVec3 index = voxel.Position;

            Vector3 bsw = new Vector3(index.X * voxelSize, index.Y * voxelSize, index.Z * voxelSize);
            Vector3 bnw = bsw + new Vector3(0, 0, voxelSize);
            Vector3 bne = bsw + new Vector3(voxelSize, 0, voxelSize);
            Vector3 bse = bsw + new Vector3(voxelSize, 0, 0);
            Vector3 tsw = bsw + new Vector3(0, voxelSize, 0);
            Vector3 tnw = bsw + new Vector3(0, voxelSize, voxelSize);
            Vector3 tne = bsw + new Vector3(voxelSize, voxelSize, voxelSize);
            Vector3 tse = bsw + new Vector3(voxelSize, voxelSize, 0);

            Color AOTopNorthWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopNorthWest) ? aoMod : 1.0f);
            Color AOTopNorth = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopNorth) ? aoMod : 1.0f);
            Color AOTopNorthEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopNorthEast) ? aoMod : 1.0f);
            Color AOTopWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopWest) ? aoMod : 1.0f);
            Color AOTop = Color.white * (voxel.IsNeighborSolid(VoxelDirection.Top) ? aoMod : 1.0f);
            Color AOTopEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopEast) ? aoMod : 1.0f);
            Color AOTopSouthWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopSouthWest) ? aoMod : 1.0f);
            Color AOTopSouth = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopSouth) ? aoMod : 1.0f);
            Color AOTopSouthEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.TopSouthEast) ? aoMod : 1.0f);
            Color AONorthWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.NorthWest) ? aoMod : 1.0f);
            Color AONorth = Color.white * (voxel.IsNeighborSolid(VoxelDirection.North) ? aoMod : 1.0f);
            Color AONorthEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.NorthEast) ? aoMod : 1.0f);
            Color AOWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.West) ? aoMod : 1.0f);
            Color AOEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.East) ? aoMod : 1.0f);
            Color AOSouthWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.SouthWest) ? aoMod : 1.0f);
            Color AOSouth = Color.white * (voxel.IsNeighborSolid(VoxelDirection.South) ? aoMod : 1.0f);
            Color AOSouthEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.SouthEast) ? aoMod : 1.0f);
            Color AOBottomNorthWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomNorthWest) ? aoMod : 1.0f);
            Color AOBottomNorth = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomNorth) ? aoMod : 1.0f);
            Color AOBottomNorthEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomNorthEast) ? aoMod : 1.0f);
            Color AOBottomWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomWest) ? aoMod : 1.0f);
            Color AOBottom = Color.white * (voxel.IsNeighborSolid(VoxelDirection.Bottom) ? aoMod : 1.0f);
            Color AOBottomEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomEast) ? aoMod : 1.0f);
            Color AOBottomSouthWest = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomSouthWest) ? aoMod : 1.0f);
            Color AOBottomSouth = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomSouth) ? aoMod : 1.0f);
            Color AOBottomSouthEast = Color.white * (voxel.IsNeighborSolid(VoxelDirection.BottomSouthEast) ? aoMod : 1.0f);

            Color  TopA     = AOTopWest * AOTopSouth * AOTopSouthWest;
            Color  TopB     = AOTopWest * AOTopNorth * AOTopNorthWest;
            Color  TopC     = AOTopEast * AOTopNorth * AOTopNorthEast;
            Color  TopD     = AOTopEast * AOTopSouth * AOTopSouthEast;
            Color  NorthA   = AONorthEast * AOBottomNorth * AOBottomNorthEast;
            Color  NorthB   = AONorthEast * AOTopNorth * AOTopNorthEast;
            Color  NorthC   = AONorthWest * AOTopNorth * AOTopNorthWest;
            Color  NorthD   = AONorthWest * AOBottomNorth * AOBottomNorthWest;
            Color  EastA    = AOSouthEast * AOBottomEast * AOBottomSouthEast;
            Color  EastB    = AOSouthEast * AOTopEast * AOTopSouthEast;
            Color  EastC    = AONorthEast * AOTopEast * AOTopNorthEast;
            Color  EastD    = AONorthEast * AOBottomEast * AOBottomNorthEast;
            Color  SouthA   = AOSouthWest * AOBottomSouth * AOBottomSouthWest;
            Color  SouthB   = AOSouthWest * AOTopSouth * AOTopSouthWest;
            Color  SouthC   = AOSouthEast * AOTopSouth * AOTopSouthEast;
            Color  SouthD   = AOSouthEast * AOBottomSouth * AOBottomSouthEast;
            Color  WestA    = AONorthWest * AOBottomWest * AOBottomNorthWest;
            Color  WestB    = AONorthWest * AOTopWest * AOTopNorthWest;
            Color  WestC    = AOSouthWest * AOTopWest * AOTopSouthWest;
            Color  WestD    = AOSouthWest * AOBottomWest * AOBottomSouthWest;
            Color  BottomA  = AOBottomWest * AOBottomNorth * AOBottomNorthWest;
            Color  BottomB  = AOBottomWest * AOBottomSouth * AOBottomSouthWest;
            Color  BottomC  = AOBottomEast * AOBottomSouth * AOBottomSouthEast;
            Color  BottomD  = AOBottomEast * AOBottomNorth * AOBottomNorthEast;

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
                Colors[vertIndex + 0] = BottomA;
                Colors[vertIndex + 1] = BottomB;
                Colors[vertIndex + 2] = BottomC;
                Colors[vertIndex + 3] = BottomD;
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
                Colors[vertIndex + 0] = SouthA;
                Colors[vertIndex + 1] = SouthB;
                Colors[vertIndex + 2] = SouthC;
                Colors[vertIndex + 3] = SouthD;
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
                Colors[vertIndex + 0] = WestA;
                Colors[vertIndex + 1] = WestB;
                Colors[vertIndex + 2] = WestC;
                Colors[vertIndex + 3] = WestD;
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
                Colors[vertIndex + 0] = NorthA;
                Colors[vertIndex + 1] = NorthB;
                Colors[vertIndex + 2] = NorthC;
                Colors[vertIndex + 3] = NorthD;
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
                Colors[vertIndex + 0] = EastA;
                Colors[vertIndex + 1] = EastB;
                Colors[vertIndex + 2] = EastC;
                Colors[vertIndex + 3] = EastD;
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
                Colors[vertIndex + 0] = TopA;
                Colors[vertIndex + 1] = TopB;
                Colors[vertIndex + 2] = TopC;
                Colors[vertIndex + 3] = TopD;
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
    }

    public void UpdateExposedVoxels()
    {
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