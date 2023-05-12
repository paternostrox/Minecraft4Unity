using Unity.Mathematics;
using UnityEngine;

public static class VoxelUtil
{

    public static readonly int2 AtlasSize = new int2(8, 8);

    public static Voxel Empty => Voxel.Air;

    public static int3 To3DIndex(int index, int3 chunkSize)
    {
        return new int3 { z = index % chunkSize.z, y = (index / chunkSize.z) % chunkSize.y, x = index / (chunkSize.y * chunkSize.z) };
    }

    public static int To1DIndex(int3 index, int3 chunkSize)
    {
        return index.z + index.y * chunkSize.z + index.x * chunkSize.y * chunkSize.z;
    }

    public static int To1DIndex(Vector3Int index, Vector3Int chunkSize)
    {
        return To1DIndex(new int3(index.x, index.y, index.z), new int3(chunkSize.x, chunkSize.y, chunkSize.z));
    }

    public static int3 WorldToChunk(int3 worldGridPosition, int3 chunkSize)
    {
        return new int3
        {
            x = Floor((float)worldGridPosition.x / chunkSize.x),
            y = Floor((float)worldGridPosition.y / chunkSize.y),
            z = Floor((float)worldGridPosition.z / chunkSize.z)
        };
    }

    public static Vector3Int WorldToChunk(Vector3 worldPosition, Vector3Int chunkSize)
    {
        // Chunk position is indexed as a matrix including negative indexes (does not reflect world position)
        return new Vector3Int
        {
            x = Floor(worldPosition.x / chunkSize.x),
            y = Floor(worldPosition.y / chunkSize.y),
            z = Floor(worldPosition.z / chunkSize.z)
        };
    }

    public static Vector3 ChunkToWorld(Vector3Int chunkPosition, Vector3Int chunkSize)
    {
        return chunkPosition * chunkSize;
    }

    public static Vector3 GridToWorld(Vector3Int gridPosition, Vector3Int chunkPosition, Vector3Int chunkSize)
    {
        return ChunkToWorld(chunkPosition, chunkSize) + gridPosition;
    }

    public static Vector3Int WorldToGrid(Vector3 worldPosition, Vector3Int chunkPosition, Vector3Int chunkSize)
    {
        return ToVector3Int(WorldToGrid(Floor(worldPosition), ToInt3(chunkPosition), ToInt3(chunkSize)));
    }

    public static int3 WorldToGrid(int3 worldGridPosition, int3 chunkPosition, int3 chunkSize)
    {
        // Position relative to chunk bottom-left corner
        int3 posRelChunk = worldGridPosition - chunkPosition * chunkSize;

        return Mod(posRelChunk, chunkSize);
    }

    public static bool BoundaryCheck(int3 position, int3 chunkSize)
    {
        return chunkSize.x > position.x && chunkSize.y > position.y && chunkSize.z > position.z && position.x >= 0 && position.y >= 0 && position.z >= 0;
    }

    public static bool BoundaryCheck(Vector3Int position, Vector3Int chunkSize)
    {
        return chunkSize.x > position.x && chunkSize.y > position.y && chunkSize.z > position.z && position.x >= 0 && position.y >= 0 && position.z >= 0;
    }

    public static Vector3Int ToVector3Int(int3 v) => new Vector3Int(v.x, v.y, v.z);

    public static int3 ToInt3(Vector3Int v) => new int3(v.x, v.y, v.z);

    public static int InvertDirection(int direction)
    {
        int axis = direction / 2; // 0(+x,-x), 1(+y,-y), 2(+z,-z)
        int invDirection = Mathf.Abs(direction - (axis * 2 + 1)) + (axis * 2);

        /*
            direction    x0    abs(x0)    abs(x) + axis * 2 => invDirection
            0            -1    1          1  
            1            0     0          0
            2            -1    1          3
            3            0     0          2
            4            -1    1          5
            5            0     0          4
            */

        return invDirection;
    }

    public static int Mod(int v, int m)
    {
        int r = v % m;
        return r < 0 ? r + m : r;
    }

    public static int3 Mod(int3 v, int3 m)
    {
        return new int3
        {
            x = Mod(v.x, m.x),
            y = Mod(v.y, m.y),
            z = Mod(v.z, m.z)
        };
    }

    public static int Floor(float x)
    {
        int xi = (int)x;
        return x < xi ? xi - 1 : xi;
    }

    public static int3 Floor(float3 v)
    {
        return new int3
        {
            x = Floor(v.x),
            y = Floor(v.y),
            z = Floor(v.z)
        };
    }

    public static readonly int[] DirectionAlignedX = { 2, 2, 0, 0, 0, 0 };
    public static readonly int[] DirectionAlignedY = { 1, 1, 2, 2, 1, 1 };
    public static readonly int[] DirectionAlignedZ = { 0, 0, 1, 1, 2, 2 };
    public static readonly int[] DirectionAlignedSign = { 1, -1, 1, -1, 1, -1 };

    public static readonly int3[] VoxelDirectionOffsets =
    {
        new int3(1, 0, 0), // right
        new int3(-1, 0, 0), // left
        new int3(0, 1, 0), // top
        new int3(0, -1, 0), // bottom
        new int3(0, 0, 1), // front
        new int3(0, 0, -1), // back
    };

    public static readonly float3[] CubeVertices =
    {
        new float3(0f, 0f, 0f),
        new float3(1f, 0f, 0f),
        new float3(1f, 0f, 1f),
        new float3(0f, 0f, 1f),
        new float3(0f, 1f, 0f),
        new float3(1f, 1f, 0f),
        new float3(1f, 1f, 1f),
        new float3(0f, 1f, 1f)
    };

    public static readonly int[] CubeFaces =
    {
        1, 2, 5, 6, // right
        0, 3, 4, 7, // left
        4, 5, 7, 6, // top
        0, 1, 3, 2, // bottom
        3, 2, 7, 6, // front
        0, 1, 4, 5, // back
    };

    public static readonly float2[] CubeUVs =
    {
        new float2(0f, 0f), new float2(1.0f, 0f), new float2(0f, 1.0f), new float2(1.0f, 1.0f)
    };

    public static readonly int[] CubeIndices =
    {
        0, 3, 1,
        0, 2, 3, //face right
        1, 3, 0,
        3, 2, 0, //face left
        0, 3, 1,
        0, 2, 3, //face top
        1, 3, 0,
        3, 2, 0, //face bottom
        1, 3, 0,
        3, 2, 0, //face front
        0, 3, 1,
        0, 2, 3, //face back
    };

    public static readonly int[] CubeFlipedIndices =
    {
        0, 2, 1,
        1, 2, 3, //face right
        1, 2, 0,
        3, 2, 1, //face left
        0, 2, 1,
        1, 2, 3, //face top
        1, 2, 0,
        3, 2, 1, //face bottom
        1, 2, 0,
        3, 2, 1, //face front
        0, 2, 1,
        1, 2, 3, //face back
    };

    public static readonly int[] AONeighborOffsets =
    {
        0, 1, 2,
        6, 7, 0,
        2, 3, 4,
        4, 5, 6,
    };
}