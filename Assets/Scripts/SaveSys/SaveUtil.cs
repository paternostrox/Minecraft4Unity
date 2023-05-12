using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Linq;

public static class SaveUtil
{
    public static string GetSavedataPath()
    {
        return String.Concat(String.Concat(Application.persistentDataPath, "/savedata/"));
    }

    public static string[] GetAllSaveNames()
    {
        return Directory.GetDirectories(String.Concat(Application.persistentDataPath, "/savedata/")).Select(Path.GetFileName).ToArray();
    }

    public static string GetSavePath(string saveName)
    {
        return String.Concat(Application.persistentDataPath, "/savedata/", saveName);
    }

    public static string GetAutosavePath()
    {
        return String.Concat(Application.persistentDataPath, "/savedata/autosave/");
    }

    public static string GetChunkVoxelPath(Vector3Int chunkPosition)
    {
        return String.Concat(GetAutosavePath(), GetChunkFileName(chunkPosition), "(V).chunk");
    }

    public static string GetChunkItemPath(Vector3Int chunkPosition)
    {
        return String.Concat(GetAutosavePath(), GetChunkFileName(chunkPosition), "(I).chunk");
    }

    public static string GetChunkPath(Vector3Int chunkPosition)
    {
        return String.Concat(GetAutosavePath(), GetChunkFileName(chunkPosition), ".chunk");
    }

    public static string GetChunkFileName(Vector3Int chunkPosition)
    {
        return String.Concat(
            chunkPosition.x, "_",
            chunkPosition.y, "_",
            chunkPosition.z
        );
    }

    public static string GetPlayerDataPath()
    {
        return String.Concat(GetAutosavePath(), "PlayerData.xuxa");
    }

    public static bool ValidateSaveName(string saveName)
    {
        if (string.IsNullOrEmpty(saveName))
        {
            return false;
        }
        string[] oldSaveNames = GetAllSaveNames();
        foreach (string oldSaveName in oldSaveNames)
        {
            if (saveName == oldSaveName)
            {
                return false;
            }
        }
        // Passed all checks, good to go.
        return true;
    }

    public static byte VoxelToByte(Voxel v)
    {
        return (byte)v;
    }

    public static Voxel ByteToVoxel(byte b)
    {
        return (Voxel)b;
    }
}
