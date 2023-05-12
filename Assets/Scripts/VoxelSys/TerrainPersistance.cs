using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Priority_Queue;

public partial class TerrainManager : Singleton<TerrainManager>
{
    [SerializeField]
    [Tooltip("Chunks outside keep size are saved and destroyed.")]
    Vector2Int chunkKeepSize = Vector2Int.one * 3;
    [SerializeField]
    int maxSaveChunksInFrame = 5;

    FastPriorityQueue<ChunkNode> saveChunkQueue = new FastPriorityQueue<ChunkNode>(100000);

    public void SaveFarawayChunks(Vector3Int targetPosition)
    {
        foreach (KeyValuePair<Vector3Int, Chunk> kv in chunks)
        {
            Vector3Int deltaPosition = targetPosition - kv.Key;
            if (Mathf.Abs(deltaPosition.x) > chunkKeepSize.x || Mathf.Abs(deltaPosition.y) > chunkKeepSize.y || Mathf.Abs(deltaPosition.z) > chunkKeepSize.x)
            {
                // Chunk is outside keepSize! Check if it must be saved
                if (!kv.Value.modified)
                    continue;

                kv.Value.modified = false;
                // Then, if it's on queue, update priority, if it's not, enqueue it.
                ChunkNode chunkNode = new ChunkNode { chunkPosition = kv.Key };
                if (saveChunkQueue.Contains(chunkNode))
                {
                    saveChunkQueue.UpdatePriority(chunkNode, -deltaPosition.sqrMagnitude);
                    continue;
                }
                saveChunkQueue.Enqueue(chunkNode, -deltaPosition.sqrMagnitude);
            }
        }
    }

    private void ProcessSaveChunkQueue()
    {
        int savedChunks = 0;
        while (saveChunkQueue.Count != 0)
        {
            if (savedChunks > maxSaveChunksInFrame)
                return;

            Vector3Int chunkPosition = saveChunkQueue.Dequeue().chunkPosition;
            // Save and get rid of chunk terrain
            Chunk chunk = chunks[chunkPosition];
            chunk.Save(); // Saves item and voxel data

            chunks.Remove(chunkPosition);
            Destroy(chunk.gameObject);
            savedChunks++;
        }
    }

    public void SaveAll()
    {
        Vector3Int targetPosition = VoxelUtil.WorldToChunk(target.position, chunkSize);

        // AutoSave all current chunks
        foreach (KeyValuePair<Vector3Int, Chunk> kv in chunks)
        {
            kv.Value.Save();
        }

        // Destroy all chunks in save queue (clearing it)
        while (saveChunkQueue.Count != 0)
        {
            Vector3Int chunkPosition = saveChunkQueue.Dequeue().chunkPosition;
            Chunk chunk = chunks[chunkPosition];
            chunks.Remove(chunkPosition);
            Destroy(chunk.gameObject);
        }
    }

    #region ITEM PERSISTENCE

    public void AddItem(Vector3 worldPosition, WorldObject worldItem)
    {
        Vector3Int chunkPosition = VoxelUtil.WorldToChunk(worldPosition, Consts.ChunkSize);
        chunks[chunkPosition].AddItem(worldItem);
    }

    public void RemoveItem(WorldObject worldItem)
    {
        Vector3Int chunkPosition = VoxelUtil.WorldToChunk(worldItem.transform.position, Consts.ChunkSize);
        chunks[chunkPosition].RemoveItem(worldItem);
    }

    public void DestroyChunkItems(Vector3Int chunkPosition)
    {
        chunks[chunkPosition].DestroyAllItems();
    }

    #endregion
}
