using UnityEngine;
using System.Collections;
using System;

public enum ItemPivot : byte { Bottom, MidCenter, MidCorner, Top };

public enum SnapType : byte { Center, Edge, Corner };
public abstract class Item : ScriptableObject
{
    [HideInInspector]
    public string id = System.Guid.NewGuid().ToString();

    [Tooltip("Inventory thumb")]
    public Sprite thumbnail;
    [Tooltip("Item prefab (world rep)")]
    public GameObject prefab;
    [Tooltip("MUST BE UNIQUE!")]
    public new string name;
    [TextArea] public string description;
    public bool reusable;
    public int maxStack = 1; // Customizable items must have maxStack = 1.
    //public bool isSolid;
    public Vector3 volume = Vector3.one;
    public ItemPivot pivot = ItemPivot.Bottom;
    public SnapType snapType = SnapType.Center;

    public bool isUnique;

    public virtual Item GetCopy()
    {
        return this;
    }

    public override string ToString()
    {
        return string.Concat(name, " - ", GetType().FullName, "\n", description);
    }

    public virtual ItemData GetData()
    {
        return new ItemData(this);
    }

    public virtual void SetData(ItemData data)
    {
        // Do absolutely nothing
    }

    public abstract bool Use();

    public virtual bool ValidatePlacement(Vector3 volumeCenter)
    {
        // No orientation or layermask for now
        if (Physics.OverlapBox(volumeCenter, (volume / 2f) - ItemUtil.CollisionAllowance).Length == 0)
        {
            return true;
        }

        return false;
    }

    // REVIEW THIS
    public virtual bool ValidatePopulation(Vector3 volumeCenter)
    {
        Vector3 fixedVolumeCenter = volumeCenter + Vector3.one * .1f;

        for (float x = -volume.x / 2f; x < volume.x / 2f; x++)
        {
            for (float y = -volume.y / 2f; y < volume.y / 2f; y++)
            {
                for (float z = -volume.z / 2f; z < volume.z / 2f; z++)
                {
                    Voxel v = Voxel.Object;
                    TerrainManager.Main.GetVoxel(fixedVolumeCenter + new Vector3(x, y, z), out v);
                    if (v != Voxel.Air)
                        return false;
                }
            }
        }

        return true;
    }

    public bool SpawnPrefab()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Consts.InteractionDistance, Consts.VoxelMask))
        {
            if (SpawnManager.Main.TrySpawnItem(ray.direction, hit.point - ray.direction * 0.01f, ContainerManager.Main.GetSelectedSlotID()))
                return true;
        }
        return false;
    }
}

[Serializable]
public class ItemData
{
    public string itemName;

    public ItemData(Item item)
    {
        this.itemName = item.name;
    }
}