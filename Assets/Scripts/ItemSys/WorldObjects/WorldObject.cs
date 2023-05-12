using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WorldObject : MonoBehaviour
{

    public ItemStack itemStack;

    bool isInitialized = false;

    //protected new Transform transform;

    protected virtual void Start()
    {
        if (!isInitialized)
        {
            throw new Exception("WorldItem must be initialized.");
        }

        //transform = base.transform;
    }

    public virtual void Initialize(Vector3 position, Quaternion rotation, Vector3 scale, ItemStack itemStack)
    {
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
        this.itemStack = itemStack;
        isInitialized = true;
    }

    public virtual void Interact()
    {
        ReturnToInventory();
    }

    public bool ReturnToInventory()
    {
        if (ContainerManager.Main.TryAlterContainer(itemStack))
        {
            TerrainManager.Main.RemoveItem(this);
            Destroy(gameObject);
            return true;
        }
        UIUtil.Main.ShowMessage("Inventory is full", 1f);
        return false;
    }

    #region SERIALIZATION

    public virtual WorldObjectData GetData()
    {
        return new WorldObjectData(transform, itemStack);
    }

    public virtual void SetData(WorldObjectData data)
    {
        Vector3 position = new Vector3(data.position[0], data.position[1], data.position[2]);
        Quaternion rotation = Quaternion.Euler(data.rotation[0], data.rotation[1], data.rotation[2]);
        Vector3 scale = new Vector3(data.scale[0], data.scale[1], data.scale[2]);

        itemStack = new ItemStack(data.itemStackData);

        Initialize(position, rotation, scale, itemStack);
    }
}

[System.Serializable]
public class WorldObjectData : WorldPlacementData
{
    public ItemStackData itemStackData;

    public WorldObjectData(Transform transform, ItemStack itemStack) : base(transform)
    {
        itemStackData = new ItemStackData(itemStack);
    }
}

#endregion

