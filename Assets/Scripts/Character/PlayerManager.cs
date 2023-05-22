using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerManager : Singleton<PlayerManager>
{
    public PlayerController playerController;

    public PlayerStats playerStats;

    public Inventory playerInventory;

    #region Serialization

    private void Awake()
    {
        TryLoad();
    }

    public void Save()
    {
        string path = SaveUtil.GetPlayerDataPath();

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, GetData());
        stream.Close();
    }

    public bool TryLoad()
    {
        string path = SaveUtil.GetPlayerDataPath();

        if (!File.Exists(path))
            return false;

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);
        PlayerData data = formatter.Deserialize(stream) as PlayerData;
        SetData(data);

        return true;
    }

    public PlayerData GetData()
    {
        return new PlayerData(this);
    }

    public void SetData(PlayerData playerData)
    {
        playerController.SetData(playerData.playerPlacementData);

        playerStats.SetData(playerData.playerStatsData);

        playerInventory.SetData(playerData.playerInventoryData);
    }
}

[Serializable]
public class PlayerData
{
    public WorldPlacementData playerPlacementData;
    public PlayerStatsData playerStatsData;
    public ItemContainerData playerInventoryData;

    public PlayerData(PlayerManager playerManager)
    {
        playerPlacementData = playerManager.playerController.GetData();
        playerStatsData = playerManager.playerStats.GetData();
        playerInventoryData = playerManager.playerInventory.GetData();
    }
}

#endregion
