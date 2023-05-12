using UnityEngine;
using System.Collections.Generic;
using Priority_Queue;
using Unity.Mathematics;
using System.Collections;

public class SpawnManager : Singleton<SpawnManager>
{
    new Transform transform;

    private void Awake()
    {
        transform = GetComponent<Transform>();
    }

    public bool TryPopulate(Vector3 rayDir, Vector3 worldPosition, ItemStack itemStack)
    {
        PlacementData placementData = new PlacementData();

        placementData = ItemUtil.Snap(rayDir, worldPosition, itemStack.item.volume, itemStack.item.pivot, itemStack.item.snapType);

        // Check if it's solid here?
        bool canPlace = itemStack.item.ValidatePopulation(placementData.volumeCenter);
        if (canPlace)
        {
            GameObject g = Instantiate(itemStack.item.prefab, transform);
            WorldObject wi = g.GetComponent<WorldObject>();
            wi.Initialize(placementData.position, placementData.rotation, itemStack.item.prefab.transform.localScale, itemStack);
            TerrainManager.Main.AddItem(worldPosition, wi);

            return true;
        }
        return false;
    }

    public bool TrySpawnItem(Vector3 rayDir, Vector3 worldPosition, string slotID)
    {
        ItemStack itemStack = ContainerManager.Main.PeekSlot(slotID);
        return TrySpawnOneItem(rayDir, worldPosition, itemStack, slotID);
    }

    // MAKE A DROP FUNCTION HERE

    // Raydir = Vector3.zero gets random vector
    public bool TrySpawnOneItem(Vector3 rayDir, Vector3 worldPosition, ItemStack itemStack, string itemSlot)
    {
        PlacementData placementData = new PlacementData();

        placementData = ItemUtil.Snap(rayDir, worldPosition, itemStack.item.volume, itemStack.item.pivot, itemStack.item.snapType);

        // Check if it's solid here?
        bool canPlace = itemStack.item.ValidatePlacement(placementData.volumeCenter);
        if (canPlace)
        {
            GameObject g = Instantiate(itemStack.item.prefab, transform);
            WorldObject wi = g.GetComponent<WorldObject>();
            ItemStack newItemStack = new ItemStack(itemStack.item, 1);
            wi.Initialize(placementData.position, placementData.rotation, itemStack.item.prefab.transform.localScale, newItemStack);

            TerrainManager.Main.AddItem(worldPosition, wi);

            return true;
        }
        return false;
    }

    public void SpawnByLoad(WorldObjectData WorldObjectData, out WorldObject WorldObject)
    {
        GameObject prefab = ItemDatabase.Main.GetPrefab(WorldObjectData.itemStackData.itemData.itemName);
        GameObject g = Instantiate(prefab, transform);
        WorldObject wi = g.GetComponent<WorldObject>();
        wi.SetData(WorldObjectData);
        WorldObject = wi;
    }
}
