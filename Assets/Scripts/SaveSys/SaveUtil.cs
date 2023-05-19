using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public static class SaveUtil
{
    public static string GetSavedataPath()
    {
        return String.Concat(String.Concat(Application.persistentDataPath, "/savedata/"));
    }

    public static string[] GetAllSaveNames(bool includeAutosave = false)
    {
        List<string> paths = Directory.GetDirectories(String.Concat(Application.persistentDataPath, "/savedata/")).Select(Path.GetFileName).ToList();
        if(!includeAutosave)
            paths.RemoveAll(path => path.Contains("autosave"));

        return paths.ToArray();
    }

    public static string GetSavePath(string saveName)
    {
        return String.Concat(Application.persistentDataPath, "/savedata/", saveName);
    }

    public static string GetSaveFileName()
    {
        return Regex.Replace(DateTime.Now.ToString(), @"[^a-zA-Z0-9]+", "-").Trim('-');
    }

    public static string GetAutosavePath()
    {
        return String.Concat(Application.persistentDataPath, "/savedata/autosave/");
    }

    public static string GetChunkVoxelPath(Vector3Int chunkPosition)
    {
        return String.Concat(GetAutosavePath(), GetChunkFileName(chunkPosition), "(V).chunk");
    }

    public static string GetChunkObjectPath(Vector3Int chunkPosition)
    {
        return String.Concat(GetAutosavePath(), GetChunkFileName(chunkPosition), "(O).chunk");
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
