using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public class ChunkObjectSpawner : Singleton<ChunkObjectSpawner>
{

    Queue<Chunk> chunksToItemize = new Queue<Chunk>();

    int maxItemizeChunksInFrame = 10;

    public void Enqueue(Chunk chunk)
    {
        chunksToItemize.Enqueue(chunk);
    }

    public void Itemize()
    {
        StartCoroutine(ProcessItemization());
    }

    IEnumerator ProcessItemization()
    {
        int itemizedChunks = 0;
        while (chunksToItemize.Count > 0)
        {
            if (itemizedChunks >= maxItemizeChunksInFrame)
                yield return null;
            Chunk chunk = chunksToItemize.Dequeue();
            ChunkRandomSpawn(chunk);
            chunk.Itemized = true;
            itemizedChunks++;
        }
    }

    // Assumes all items are snapped on center
    public void ChunkRandomSpawn(Chunk chunk)
    {
        HashSet<int3> occupiedGround = new HashSet<int3>();

        // Spawn each item
        RandomSpawnItem(ItemDatabase.Main.GetCopy("Hobbit"), 3, 7, chunk.chunkPosition, chunk.groundVoxels, occupiedGround);
    }

    public void RandomSpawnItem(Item item, int minAmount, int maxAmount, Vector3Int chunkPos, int3[] groundVoxels, HashSet<int3> occupiedGround, int minStack = 1, int maxStack = 1)
    {
        int amountToSpawn = UnityEngine.Random.Range(minAmount, maxAmount + 1);

        int spawnedAmount = 0;
        while (spawnedAmount < amountToSpawn)
        {
            int r = UnityEngine.Random.Range(0, groundVoxels.Length);
            if (occupiedGround.Contains(groundVoxels[r]))
                continue;

            Vector3Int gridPos = new Vector3Int(groundVoxels[r].x, groundVoxels[r].y, groundVoxels[r].z);
            Vector3 worldPos = VoxelUtil.GridToWorld(gridPos, chunkPos, Consts.ChunkSize);
            worldPos += ItemUtil.Corner2TopMid;

            // Voxel isn't occupied, will try to spawn!
            int stack = UnityEngine.Random.Range(minStack, maxStack + 1);
            ItemStack itemStack = new ItemStack(item, stack);

            if (SpawnManager.Main.TryPopulate(Vector3.zero, worldPos, itemStack))
            {
                spawnedAmount++;
                occupiedGround.Add(groundVoxels[r]);
            }
        }
    }
}
