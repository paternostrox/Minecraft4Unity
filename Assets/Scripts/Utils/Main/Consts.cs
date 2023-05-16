using UnityEngine;
using System.Collections;

public static class Consts
{

    public const float InteractionDistance = 4f;

    public const int InteractableStaticLayer = 9;

    public const int InteractableDynamicLayer = 11;

    public const int InteractableMask = 1 << InteractableStaticLayer | 1 << InteractableDynamicLayer;

    public const int VoxelMask = 1 << 8;


    // Remember to replace TerrainManager variables for these:

    public const int ChunkSizeHorizontal = 32;

    public const int ChunkSizeVertical = 64;

    public static readonly Vector3Int ChunkSize = new Vector3Int(ChunkSizeHorizontal, ChunkSizeVertical, ChunkSizeHorizontal);

    public static readonly Vector2Int ChunkSpawnSize = new Vector2Int(2, 1);

    public static readonly Vector2Int ChunkKeepSize = new Vector2Int(3, 2);

    public const int MaxGenerateChunksInFrame = 1;

    public const int MaxSaveChunksInFrame = 5;
}
