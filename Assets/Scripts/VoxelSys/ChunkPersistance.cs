using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public partial class Chunk : MonoBehaviour
{

    public bool modified = false;

    public void Save()
    {
        string path = SaveUtil.GetChunkPath(chunkPosition);

        ChunkData data = new ChunkData(this);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public bool TryInitByLoad()
    {
        // Tries to get save file
        string path = SaveUtil.GetChunkPath(chunkPosition);

        if (!File.Exists(path))
        {
            return false;
        }

        // If it succeeds, loads the file

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);
        ChunkData data = formatter.Deserialize(stream) as ChunkData;
        this.voxels = (Voxel[])(object)data.voxelData;
        foreach (WorldObjectData wid in data.worldItemData)
        {
            SpawnManager.Main.SpawnByLoad(wid, out WorldObject worldItem);
            AddItem(worldItem);
        }
        stream.Close();

        Dirty = true;
        Initialized = true;
        Itemized = true;
        return true;
    }

    HashSet<WorldObject> worldItems = new HashSet<WorldObject>();

    public void AddItem(WorldObject worldItem)
    {
        worldItems.Add(worldItem);
        modified = true;
    }

    public void RemoveItem(WorldObject worldItem)
    {
        worldItems.Remove(worldItem);
        modified = true;
    }

    public void DestroyAllItems()
    {
        foreach (WorldObject wi in worldItems)
        {
            Destroy(wi.gameObject);
        }
        worldItems.Clear();
    }

    [Serializable]
    public class ChunkData // Maybe do nested data classes for all serializables?
    {
        public byte[] voxelData;

        public WorldObjectData[] worldItemData;

        public ChunkData(Chunk chunk)
        {
            voxelData = Array.ConvertAll(chunk.voxels, new Converter<Voxel, byte>(SaveUtil.VoxelToByte));
            worldItemData = new WorldObjectData[chunk.worldItems.Count];
            int i = 0;
            foreach (WorldObject wi in chunk.worldItems)
            {
                worldItemData[i++] = wi.GetData();
            }
        }
    }
}