using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ItemContainer : MonoBehaviour
{

    public abstract string[] SlotIDs { get; }

    public abstract int Capacity { get; }

    Dictionary<string, ItemStack> containedItems = new Dictionary<string, ItemStack>();

    public int ItemCount { get => containedItems.Count; }

    public int FreeSlotCount { get => Capacity - ItemCount; }

    public void UseItem(string slotID)
    {
        ItemStack itemStack = Peek(slotID);
        if (itemStack != null)
        {
            bool success = itemStack.item.Use();
            if (success)
            {
                if (!itemStack.item.reusable)
                {
                    itemStack.amount--;
                    UpdateSlot(slotID, itemStack);
                }
                return;
            }
        }
        UIUtil.Main.ShowMessage("No Effect", 2f);
    }

    public bool CanAddItem(ItemStack item)
    {
        int freeSpaces = 0;

        freeSpaces += FreeSlotCount * item.item.maxStack;
        foreach (KeyValuePair<string, ItemStack> kv in containedItems)
        {
            if (kv.Value.item == item.item) // Check if they are the same SO instance
                freeSpaces += item.item.maxStack - kv.Value.amount;
        }
        return freeSpaces >= item.amount;
    }

    // Adds/removes amount out of any slot
    // Takes maxStack and number of slots into consideration
    public bool TryAlter(ItemStack item)
    {
        if (item.amount == 0)
            throw new Exception("Cannot add/remove an amount of zero items.");

        if (item.amount > 0)
        {
            // Amount is positive, must add!
            foreach (KeyValuePair<string, ItemStack> kv in containedItems)
            {
                if (kv.Value.item == item.item)
                {
                    int freeSpaces = kv.Value.item.maxStack - kv.Value.amount;
                    int amountToPlace = Mathf.Min(item.amount, freeSpaces);
                    item.amount -= amountToPlace;
                    kv.Value.amount += amountToPlace;
                    UpdateSlot(kv.Key, kv.Value);
                    if (item.amount == 0)
                        return true;
                }
            }

            foreach (string id in SlotIDs)
            {
                if (!containedItems.ContainsKey(id) && id[0] != 'E')
                {
                    int amountToPlace = Mathf.Min(item.amount, item.item.maxStack);
                    item.amount -= amountToPlace;
                    ItemStack newItem = new ItemStack(item.item, amountToPlace);
                    containedItems.Add(id, newItem);
                    UpdateSlot(id, newItem);
                }
                if (item.amount == 0)
                    return true;
            }
        }
        else
        {
            // Amount is negative, must take!
            foreach (KeyValuePair<string, ItemStack> kv in containedItems)
            {
                if (kv.Value.item == item.item)
                {
                    int amountTaken = Mathf.Max(item.amount, -kv.Value.amount);
                    item.amount -= amountTaken;
                    kv.Value.amount += amountTaken;
                    ContainerManager.Main.UpdateSlotUI(kv.Key, kv.Value);
                    if (item.amount == 0)
                        return true;
                }
            }
        }

        return false;
    }

    public void OverwriteSlot(string slotID, ItemStack itemSlot)
    {
        ClearSlot(slotID);
        if (itemSlot.item != null && itemSlot.amount > 0)
        {
            containedItems.Add(slotID, itemSlot);
            UpdateSlot(slotID, itemSlot);
        }
    }

    public ItemStack Peek(string slotID)
    {
        ItemStack item;
        containedItems.TryGetValue(slotID, out item);
        return item;
    }

    public virtual void ClearSlot(string slotID)
    {
        containedItems.Remove(slotID);
        ContainerManager.Main.UpdateSlotUI(slotID, null);
    }

    public void UpdateSlot(string slotID, ItemStack itemStack)
    {
        if (itemStack.amount < 1)
        {
            ClearSlot(slotID);
            return;
        }

        ContainerManager.Main.UpdateSlotUI(slotID, itemStack);
    }

    public override string ToString()
    {
        string allItems = string.Empty;

        foreach (KeyValuePair<string, ItemStack> kv in containedItems)
        {
            allItems += string.Concat(kv.Key, ": ", kv.Value.amount, " ", kv.Value.item.name, " / ");
        }
        return allItems;
    }

    #region SERIALIZATION

    public ItemContainerData GetData()
    {
        return new ItemContainerData(containedItems);
    }

    public virtual void SetData(ItemContainerData itemContainerData)
    {
        containedItems.Clear();

        foreach (ContainedItemEntry entry in itemContainerData.containedItemEntries)
        {
            containedItems.Add(entry.slotID, new ItemStack(entry.itemContainerData));
        }
    }
}

[Serializable]
public class ItemContainerData
{
    public ContainedItemEntry[] containedItemEntries;

    public ItemContainerData(Dictionary<string, ItemStack> containedItems)
    {
        containedItemEntries = new ContainedItemEntry[containedItems.Count];

        int i = 0;
        foreach (KeyValuePair<string, ItemStack> kv in containedItems)
        {
            containedItemEntries[i] = new ContainedItemEntry(kv.Key, kv.Value);
            i++;
        }
    }
}

[Serializable]
public class ContainedItemEntry
{
    public string slotID;

    public ItemStackData itemContainerData;

    public ContainedItemEntry(string slotID, ItemStack itemStack)
    {
        this.slotID = slotID;

        itemContainerData = new ItemStackData(itemStack);
    }
}

#endregion

