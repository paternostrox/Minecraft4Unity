using System;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

public partial class TerrainManager : Singleton<TerrainManager>
{
    [SerializeField] Transform target; // Player
    Vector3Int chunkSize = Consts.ChunkSize;
    [SerializeField] Vector2Int chunkSpawnSize = Vector2Int.one * 3;
    [SerializeField] Material chunkMaterial;
    [SerializeField] int maxGenerateChunksInFrame = 5;
    [SerializeField] ChunkMeshBuilder.SimplifyingMethod simplifyingMethod;

    bool initialized;
    public event EmptyDel OnInitializeTerrain;

    class ChunkNode : FastPriorityQueueNode
    {
        public Vector3Int chunkPosition;
    } 
    
    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    Vector3Int currentTargetChunkPosition = new Vector3Int(int.MinValue, int.MaxValue, int.MinValue);
    //Queue<ChunkNode> generateChunkQueue = new Queue<ChunkNode>();
    FastPriorityQueue<ChunkNode> generateChunkQueue = new FastPriorityQueue<ChunkNode>(100000);
    int updatingChunks;

    public Vector3Int ChunkSize => chunkSize;
    public Material ChunkMaterial => chunkMaterial;
    public ChunkMeshBuilder.SimplifyingMethod SimplifyingMethod => simplifyingMethod;

    public int UpdatingChunks
    {
        get => updatingChunks;
        set => updatingChunks = value;
    }

    public bool CanUpdate => updatingChunks <= maxGenerateChunksInFrame;

    void Awake()
    {
        ChunkMeshBuilder.InitializeShaderParameter();
    }

    void Update()
    {
        if (target == null)
            return;

        Vector3Int targetChunkPosition = VoxelUtil.WorldToChunk(target.position, chunkSize);

        //print("TARGET POS: " + target.position + " TARGET LOCALPOS: " + target.localPosition + " CHUNK POS: " + targetChunkPosition);

        if (currentTargetChunkPosition.Equals(targetChunkPosition))
            return;

        // Player changed chunks! Generate new chunks and save old chunks!
        GenerateNearbyChunks(targetChunkPosition);
        SaveFarawayChunks(targetChunkPosition);
    }

    void LateUpdate()
    {
        ProcessGenerateChunkQueue();
        ProcessSaveChunkQueue();
    }

    void GenerateNearbyChunks(Vector3Int targetChunkPosition)
    {
        
        // If there are still chunks to generate, check if they are worth keeping.
        foreach (ChunkNode chunkNode in generateChunkQueue)
        {
            Vector3Int deltaPosition = targetChunkPosition - chunkNode.chunkPosition;
            if (chunkSpawnSize.x < Mathf.Abs(deltaPosition.x) || chunkSpawnSize.y < Mathf.Abs(deltaPosition.y) || chunkSpawnSize.x < Mathf.Abs(deltaPosition.z))
            {
                generateChunkQueue.Remove(chunkNode);
                continue;
            }
            
            generateChunkQueue.UpdatePriority(chunkNode, (targetChunkPosition - chunkNode.chunkPosition).sqrMagnitude); // Near player = Higher priority
        }

        // Queue every chunk under the new radius to generate
        for (int x = targetChunkPosition.x - chunkSpawnSize.x; x <= targetChunkPosition.x + chunkSpawnSize.x; x++)
        {
            for (int z = targetChunkPosition.z - chunkSpawnSize.x; z <= targetChunkPosition.z + chunkSpawnSize.x; z++)
            {
                for (int y = targetChunkPosition.y - chunkSpawnSize.y; y <= targetChunkPosition.y + chunkSpawnSize.y; y++) 
                {
                    Vector3Int chunkPosition = new Vector3Int(x, y, z);
                    ChunkNode chunkNode = new ChunkNode { chunkPosition = chunkPosition };

                    // Tries to get chunk from dict
                    if (chunks.ContainsKey(chunkPosition))
                    {
                        // Chunk is on dict! It should not be destroyed.
                        if (saveChunkQueue.Contains(chunkNode))
                            saveChunkQueue.Remove(chunkNode);

                        continue;
                    }

                    // If chunk is already on queue, it's inside SpawnSize and the priority was already updated
                    if (generateChunkQueue.Contains(chunkNode))
                        continue;

                    generateChunkQueue.Enqueue(chunkNode, (targetChunkPosition - chunkPosition).sqrMagnitude);
                }
            }
        }

        currentTargetChunkPosition = targetChunkPosition;
    }

    void ProcessGenerateChunkQueue()
    {
        int generatedChunks = 0;
        while (generateChunkQueue.Count != 0)
        {
            if (generatedChunks >= maxGenerateChunksInFrame)
                return;

            Vector3Int chunkPosition = generateChunkQueue.Dequeue().chunkPosition;
            GenerateChunk(chunkPosition);
            generatedChunks++;
        }
        if(!initialized)
        {
            initialized = true;
            OnInitializeTerrain?.Invoke();
        }
    }

    Chunk GenerateChunk(Vector3Int chunkPosition)
    {
        // Creates and initializes chunk, if it doesn't exists already
        if (chunks.ContainsKey(chunkPosition))
            return chunks[chunkPosition];

        GameObject chunkGameObject = new GameObject(chunkPosition.ToString());
        chunkGameObject.transform.SetParent(transform);
        chunkGameObject.transform.position = VoxelUtil.ChunkToWorld(chunkPosition, chunkSize);

        Chunk newChunk = chunkGameObject.AddComponent<Chunk>();

        chunks.Add(chunkPosition, newChunk);
        newChunk.Init(chunkPosition, this);

        // If all neighbours are initialized, the chunk can render (update).
        // Makes chunks on the edge of the world wait.
        // Chunk needs who are neighbouring voxels before it can render!
        //newChunk.CanUpdate += delegate
        //{
        //    for (int x = chunkPosition.x - 1; x <= chunkPosition.x + 1; x++)
        //    {
        //        for (int z = chunkPosition.z - 1; z <= chunkPosition.z + 1; z++)
        //        {
        //            for (int y = chunkPosition.y - 1; y <= chunkPosition.y + 1; y++)
        //            {
        //                Vector3Int neighborChunkPosition = new Vector3Int(x, y, z);
        //                if (chunks.TryGetValue(neighborChunkPosition, out Chunk neighborChunk))
        //                {
        //                    if (!neighborChunk.Initialized)
        //                    {
        //                        return false;
        //                    }
        //                }
        //                else
        //                {
        //                    return false;
        //                }
        //            }
        //        }
        //    }
        //    return true;
        //};

        return newChunk;
    }

    public bool GetChunk(Vector3 worldPosition, out Chunk chunk)
    {
        Vector3Int chunkPosition = VoxelUtil.WorldToChunk(worldPosition, chunkSize);
        return chunks.TryGetValue(chunkPosition, out chunk);
    }

    public bool GetVoxel(Vector3 worldPosition, out Voxel voxel)
    {
        if (GetChunk(worldPosition, out Chunk chunk))
        {
            Vector3Int chunkPosition = VoxelUtil.WorldToChunk(worldPosition, chunkSize);
            Vector3Int gridPosition = VoxelUtil.WorldToGrid(worldPosition, chunkPosition, chunkSize);
            if(chunk.GetVoxel(gridPosition, out voxel))
                return true;
        }
        
        voxel = VoxelUtil.Empty;
        return false;
    }

    // TO DO: Check for colliders before allowing voxel to be set
    public bool SetVoxel(Vector3 worldPosition, Voxel type)
    {
        if (GetChunk(worldPosition, out Chunk chunk))
        {
            // Get positions for voxel and chunk containing voxel
            Vector3Int chunkPosition = VoxelUtil.WorldToChunk(worldPosition, chunkSize);
            Vector3Int gridPosition = VoxelUtil.WorldToGrid(worldPosition, chunkPosition, chunkSize);
            if (chunk.SetVoxel(gridPosition, type))
            {
                // Check if neighbour voxels are in other chunks 
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            if (VoxelUtil.BoundaryCheck(gridPosition + new Vector3Int(x, y, z), chunkSize))
                                continue;

                            Vector3Int neighborChunkPosition = VoxelUtil.WorldToChunk(worldPosition + new Vector3(x, y, z), chunkSize);
                            if (chunkPosition == neighborChunkPosition)
                                continue;
                            
                            if (chunks.TryGetValue(neighborChunkPosition, out Chunk neighborChunk))
                            {
                                neighborChunk.NeighborChunkIsChanged();
                            }
                        }
                    }
                }

                return true;
            }
        }
        return false;
    }

    // This is shit
    public bool SetVoxelVolume(Vector3 worldPosition, Voxel type, Vector3Int volume, bool overwriteSolid = true)
    {

        Vector3 fixedHalfVolume = ( (Vector3) volume / 2.0f) - (0.5f * Vector3.one);

        Vector3[] allVoxelPos = new Vector3[volume.x * volume.y * volume.z];
        Vector3 voxelPos;
        int i = 0;

        for (float x = -fixedHalfVolume.x; x <= fixedHalfVolume.x; x++)
        {
            for (float y = -fixedHalfVolume.y; y <= fixedHalfVolume.y; y++)
            {
                for (float z = -fixedHalfVolume.z; z <= fixedHalfVolume.z; z++)
                {
                    voxelPos = worldPosition + new Vector3(x, y, z);
                    print("ObjPos" + i + " = " + voxelPos);
                    allVoxelPos[i++] = voxelPos;
                    if (!overwriteSolid) {
                        GetVoxel(voxelPos, out Voxel voxel);
                        if (voxel != VoxelUtil.Empty)
                            return false;
                    }
                }
            }
        }

        for(int x = 0; x < allVoxelPos.Length; x++)
        {
            SetVoxel(allVoxelPos[x], type);
        }

        return true;
    }

    public List<Voxel[]> GetNeighborVoxels(Vector3Int chunkPosition, int numNeighbor)
    {
        List<Voxel[]> neighborVoxels = new List<Voxel[]>();
        
        for (int x = chunkPosition.x - numNeighbor; x <= chunkPosition.x + numNeighbor; x++)
        {
            for (int y = chunkPosition.y - numNeighbor; y <= chunkPosition.y + numNeighbor; y++)
            {
                for (int z = chunkPosition.z - numNeighbor; z <= chunkPosition.z + numNeighbor; z++)
                {
                    Vector3Int neighborChunkPosition = new Vector3Int(x, y, z);
                    if (chunks.TryGetValue(neighborChunkPosition, out Chunk chunk))
                    {
                        neighborVoxels.Add(chunk.Voxels);
                    }
                    else
                    {
                        neighborVoxels.Add(null);
                    }
                }
            }
        }

        return neighborVoxels;
    }

    // Marks chunk player is currently at
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 pos = VoxelUtil.ChunkToWorld(currentTargetChunkPosition, chunkSize);
        Gizmos.DrawWireCube(pos + new Vector3(chunkSize.x / 2f, chunkSize.y / 2f, chunkSize.z / 2f), chunkSize);
    }
}
